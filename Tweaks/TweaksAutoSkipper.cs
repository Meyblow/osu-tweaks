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
    /// Автономный компонент автоскипа (интро, брейки, аутро) на основе State Machine для osu!tweaks.
    /// </summary>
    public partial class TweaksAutoSkipper : Component
    {
        private const double skip_lead_in = 2000;
        private const double minimum_skip_savings = 1000;

        private enum SkipState
        {
            Idle,
            Seeking
        }

        private readonly Player player;
        private readonly GameplayClockContainer clockContainer;

        private SkipState state = SkipState.Idle;
        private double seekTarget;

        private double firstNoteTime = double.MaxValue;
        private double lastNoteEndTime = double.MaxValue;
        private IReadOnlyList<BreakPeriod>? breaks;
        private readonly HashSet<BreakPeriod> skippedBreaks = new();

        private bool hasSkippedIntro;
        private bool hasSkippedOutro;

        private object? drawableRuleset;
        private PropertyInfo? frameStablePlaybackProp;
        private FieldInfo? samplePlaybackDisabledField;
        private MethodInfo? updateSampleDisabledStateMethod;
        private Bindable<bool>? samplePlaybackDisabled;

        public TweaksAutoSkipper(Player player, GameplayClockContainer clockContainer)
        {
            this.player = player;
            this.clockContainer = clockContainer;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            try
            {
                var beatmap = player.GameplayState?.Beatmap;
                if (beatmap != null)
                {
                    breaks = beatmap.Breaks;

                    var hitObjects = beatmap.HitObjects;
                    if (hitObjects != null && hitObjects.Count > 0)
                    {
                        firstNoteTime = hitObjects[0].StartTime;
                        lastNoteEndTime = hitObjects.Max(h => h.GetEndTime());
                    }
                }

                drawableRuleset = ReflectionHelper.GetPropertyValue<object>(player, "DrawableRuleset");
                if (drawableRuleset != null)
                {
                    frameStablePlaybackProp = ReflectionHelper.FindProperty(drawableRuleset.GetType(), "FrameStablePlayback");
                }

                samplePlaybackDisabledField = ReflectionHelper.FindField(player.GetType(), "samplePlaybackDisabled");
                samplePlaybackDisabled = samplePlaybackDisabledField?.GetValue(player) as Bindable<bool>;
                updateSampleDisabledStateMethod = ReflectionHelper.FindMethod(player.GetType(), "updateSampleDisabledState");
            }
            catch (Exception ex)
            {
                TweaksLog.Error("TweaksAutoSkipper.load failed to resolve reflection handles", ex);
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            cleanupSkipState();
            base.Dispose(isDisposing);
        }

        protected override void Update()
        {
            base.Update();

            if (IsDisposed || clockContainer == null || !clockContainer.IsRunning)
                return;

            switch (state)
            {
                case SkipState.Idle:
                    checkForSkipOpportunity();
                    break;

                case SkipState.Seeking:
                    if (Math.Abs(clockContainer.CurrentTime - seekTarget) < 50 || clockContainer.CurrentTime >= seekTarget)
                    {
                        finishSkip();
                    }
                    break;
            }
        }

        private void checkForSkipOpportunity()
        {
            var plugin = OsuTweaksPlugin.Instance;
            if (plugin == null || plugin.AutoSkipMode.Value == AutoSkipMode.Disabled)
                return;

            var mode = plugin.AutoSkipMode.Value;
            double currentTime = clockContainer.CurrentTime;

            // 1. АВТОСКИП ИНТРО (IntroOnly, IntroAndBreaks, All)
            if ((mode == AutoSkipMode.IntroOnly || mode == AutoSkipMode.IntroAndBreaks || mode == AutoSkipMode.All) && !hasSkippedIntro)
            {
                double introSkipTarget = firstNoteTime - skip_lead_in;
                if (introSkipTarget - currentTime >= minimum_skip_savings && currentTime < firstNoteTime - 500)
                {
                    hasSkippedIntro = true;
                    TweaksLog.Info($"TweaksAutoSkipper: Auto-skipping Intro to {introSkipTarget:F0}ms");
                    beginSkip(introSkipTarget);
                    return;
                }
            }

            // 2. АВТОСКИП БРЕЙКОВ (BreaksOnly, IntroAndBreaks, All)
            if ((mode == AutoSkipMode.BreaksOnly || mode == AutoSkipMode.IntroAndBreaks || mode == AutoSkipMode.All) && breaks != null && breaks.Count > 0)
            {
                foreach (var b in breaks)
                {
                    if (skippedBreaks.Contains(b))
                        continue;

                    // Учитываем упреждение перед концом брейка
                    double breakSkipTarget = b.EndTime - skip_lead_in;

                    // Если брейк достаточно длинный и мы уже вошли в него
                    if (currentTime >= b.StartTime + 300 &&
                        breakSkipTarget - currentTime >= minimum_skip_savings &&
                        currentTime < b.EndTime - 500)
                    {
                        skippedBreaks.Add(b);
                        TweaksLog.Info($"TweaksAutoSkipper: Auto-skipping mid-map break ({b.StartTime:F0}ms -> {b.EndTime:F0}ms) to {breakSkipTarget:F0}ms");
                        beginSkip(breakSkipTarget);
                        return;
                    }
                }
            }

            // 3. АВТОСКИП АУТРО (только All)
            if (mode == AutoSkipMode.All && !hasSkippedOutro && currentTime >= lastNoteEndTime + 800)
            {
                hasSkippedOutro = true;
                double outroSkipTarget = lastNoteEndTime + 1200;
                TweaksLog.Info($"TweaksAutoSkipper: Auto-skipping Outro to {outroSkipTarget:F0}ms");
                beginSkip(outroSkipTarget);
            }
        }

        private void beginSkip(double target)
        {
            seekTarget = target;
            state = SkipState.Seeking;

            if (samplePlaybackDisabled != null)
            {
                samplePlaybackDisabled.Value = true;
            }

            frameStablePlaybackProp?.SetValue(drawableRuleset, false);
            hideSkipOverlays();

            clockContainer.Seek(target);
        }

        private void finishSkip()
        {
            frameStablePlaybackProp?.SetValue(drawableRuleset, true);

            if (samplePlaybackDisabled != null)
            {
                samplePlaybackDisabled.Value = false;
            }

            try
            {
                updateSampleDisabledStateMethod?.Invoke(player, null);
            }
            catch (Exception ex)
            {
                TweaksLog.Error("TweaksAutoSkipper.finishSkip updateSampleDisabledState error", ex);
            }

            state = SkipState.Idle;
        }

        private void hideSkipOverlays()
        {
            try
            {
                if (clockContainer == null) return;

                foreach (var overlay in clockContainer.ChildrenOfType<SkipOverlay>())
                {
                    overlay.FadeOut(100);
                }
            }
            catch { }
        }

        private void cleanupSkipState()
        {
            if (state == SkipState.Seeking)
            {
                finishSkip();
            }
        }
    }
}
