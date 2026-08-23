using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Beatmaps.Timing;
using osu.Game.Rulesets.Objects;
using osu.Game.Screens.Play;
using OsuTweaks.Models;
using OsuTweaks.Utils;

namespace OsuTweaks.Tweaks
{
    /// <summary>
    /// Автономный компонент автоскипа для osu!tweaks.
    /// Поддерживает режимы:
    /// - Disabled (Выкл)
    /// - BreaksOnly (Автоскип мид-мап брейков)
    /// - All (Автоскип всего: интро, брейки, аутро)
    /// </summary>
    public partial class TweaksBreakAutoSkipper : Component
    {
        private const double skip_lead_in = 2000;
        private const double minimum_skip_savings = 1000;

        private readonly Player player;
        private readonly GameplayClockContainer clockContainer;
        private readonly IReadOnlyList<BreakPeriod>? breaks;

        private double firstNoteTime = double.MaxValue;
        private double lastNoteEndTime = double.MaxValue;

        private int nextBreakIndex;
        private bool hasSkippedIntro;
        private bool hasSkippedOutro;

        private object? drawableRuleset;
        private PropertyInfo? frameStablePlaybackProp;
        private FieldInfo? samplePlaybackDisabledField;
        private MethodInfo? updateSampleDisabledStateMethod;
        private Bindable<bool>? osuCcSkipBreakTime;

        public TweaksBreakAutoSkipper(Player player, GameplayClockContainer clockContainer, IReadOnlyList<BreakPeriod>? breaks)
        {
            this.player = player;
            this.clockContainer = clockContainer;
            this.breaks = breaks;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            try
            {
                var hitObjects = player.GameplayState?.Beatmap?.HitObjects;
                if (hitObjects != null && hitObjects.Count > 0)
                {
                    firstNoteTime = hitObjects[0].StartTime;
                    lastNoteEndTime = hitObjects.Max(h => h.GetEndTime());
                }

                drawableRuleset = ReflectionHelper.GetPropertyValue<object>(player, "DrawableRuleset");
                if (drawableRuleset != null)
                {
                    frameStablePlaybackProp = ReflectionHelper.FindProperty(drawableRuleset.GetType(), "FrameStablePlayback");
                }

                samplePlaybackDisabledField = ReflectionHelper.FindField(player.GetType(), "samplePlaybackDisabled");
                updateSampleDisabledStateMethod = ReflectionHelper.FindMethod(player.GetType(), "updateSampleDisabledState");

                var clientConfigType = Type.GetType("osucc.Client.ClientConfig, osucc.Host");
                if (clientConfigType != null)
                {
                    var field = ReflectionHelper.FindField(clientConfigType, "SkipBreakTime");
                    osuCcSkipBreakTime = field?.GetValue(null) as Bindable<bool>;
                }
            }
            catch (Exception ex)
            {
                TweaksLog.Error("TweaksBreakAutoSkipper.load failed to resolve reflection handles", ex);
            }
        }

        protected override void Update()
        {
            base.Update();

            var plugin = OsuTweaksPlugin.Instance;
            if (plugin == null)
                return;

            var mode = plugin.AutoSkipMode.Value;
            if (mode == AutoSkipMode.Disabled)
                return;

            double currentTime = clockContainer.CurrentTime;

            // 1. АВТОСКИП ИНТРО (только в режиме All)
            if (mode == AutoSkipMode.All && !hasSkippedIntro)
            {
                double introSkipTarget = firstNoteTime - skip_lead_in;
                if (introSkipTarget - currentTime >= minimum_skip_savings)
                {
                    hasSkippedIntro = true;
                    performSkip(introSkipTarget, "Intro");
                    hideSkipOverlays();
                    return;
                }
            }

            // 2. АВТОСКИП МИД-МАП БРЕЙКОВ (в режимах BreaksOnly и All)
            if (breaks != null && nextBreakIndex < breaks.Count && currentTime >= firstNoteTime - 500)
            {
                var currentBreak = breaks[nextBreakIndex];

                if (currentTime >= currentBreak.EndTime)
                {
                    nextBreakIndex++;
                }
                else if (currentBreak.HasEffect && currentTime >= currentBreak.StartTime && currentTime < currentBreak.EndTime)
                {
                    hideSkipOverlays();

                    double breakSkipTarget = currentBreak.EndTime - skip_lead_in;
                    if (breakSkipTarget - currentTime >= minimum_skip_savings)
                    {
                        nextBreakIndex++;
                        performSkip(breakSkipTarget, "Break");
                        return;
                    }
                }
            }

            // 3. АВТОСКИП АУТРО (только в режиме All)
            if (mode == AutoSkipMode.All && !hasSkippedOutro && currentTime >= lastNoteEndTime + 800)
            {
                hasSkippedOutro = true;
                double trackEndTime = player.GameplayState?.Beatmap?.HitObjects.Count > 0
                    ? lastNoteEndTime + 1200
                    : clockContainer.CurrentTime;

                TweaksLog.Info($"TweaksBreakAutoSkipper: Outro auto-complete at {currentTime:F0}ms");
                hideSkipOverlays();
            }
        }

        private void hideSkipOverlays()
        {
            try
            {
                if (clockContainer == null) return;

                foreach (var overlay in clockContainer.ChildrenOfType<SkipOverlay>())
                {
                    overlay.Alpha = 0;
                    overlay.AlwaysPresent = false;
                }
            }
            catch { }
        }

        private void performSkip(double targetTime, string reason)
        {
            try
            {
                TweaksLog.Info($"TweaksBreakAutoSkipper: Auto-skipping {reason} to {targetTime:F0}ms");

                // 1. Mute samples
                var sampleDisabled = samplePlaybackDisabledField?.GetValue(player) as Bindable<bool>;
                if (sampleDisabled != null)
                    sampleDisabled.Value = true;

                // 2. Temporarily disable frame-stable playback
                bool wasFrameStable = false;
                if (drawableRuleset != null && frameStablePlaybackProp != null)
                {
                    wasFrameStable = (bool)(frameStablePlaybackProp.GetValue(drawableRuleset) ?? true);
                    frameStablePlaybackProp.SetValue(drawableRuleset, false);
                }

                // 3. Seek
                clockContainer.Seek(targetTime);

                // 4. Re-enable frame-stable playback after one frame
                if (drawableRuleset != null && frameStablePlaybackProp != null && wasFrameStable)
                {
                    var capturedProp = frameStablePlaybackProp;
                    var capturedRuleset = drawableRuleset;
                    Scheduler.AddDelayed(() =>
                    {
                        if (IsDisposed) return;
                        capturedProp.SetValue(capturedRuleset, true);
                    }, 0);
                }

                // 5. Restore sample playback
                updateSampleDisabledStateMethod?.Invoke(player, null);
            }
            catch (Exception ex)
            {
                TweaksLog.Error($"TweaksBreakAutoSkipper.performSkip ({reason}) error", ex);
            }
        }
    }
}
