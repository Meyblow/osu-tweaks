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
    /// Автономный компонент автоскипа брейков для osu!tweaks.
    /// Внедряется в GameplayClockContainer и оптимизированно перематывает брейк к нотам.
    /// </summary>
    public partial class TweaksBreakAutoSkipper : Component
    {
        private const double skip_lead_in = 2000;
        private const double minimum_skip_savings = 1000;

        private readonly Player player;
        private readonly GameplayClockContainer clockContainer;
        private readonly IReadOnlyList<BreakPeriod>? breaks;

        private double firstNoteTime = double.MaxValue;
        private int nextBreakIndex;

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
            if (plugin == null || plugin.AutoSkipMode.Value == AutoSkipMode.Disabled)
                return;

            if (osuCcSkipBreakTime != null && !osuCcSkipBreakTime.Value)
                return;

            if (breaks == null || nextBreakIndex >= breaks.Count)
                return;

            double currentTime = clockContainer.CurrentTime;

            // Пропускаем интро в начале карты
            if (currentTime < firstNoteTime - 500)
                return;

            var currentBreak = breaks[nextBreakIndex];

            // Если время вышло за пределы текущего брейка, переходим к следующему
            if (currentTime >= currentBreak.EndTime)
            {
                nextBreakIndex++;
                return;
            }

            // Если вошли в диапазон брейка:
            if (currentBreak.HasEffect && currentTime >= currentBreak.StartTime && currentTime < currentBreak.EndTime)
            {
                hideBreakSkipOverlays();

                double skipTarget = currentBreak.EndTime - skip_lead_in;
                if (skipTarget - currentTime >= minimum_skip_savings)
                {
                    nextBreakIndex++;
                    performBreakSkip(skipTarget);
                }
            }
        }

        private void hideBreakSkipOverlays()
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

        private void performBreakSkip(double targetTime)
        {
            try
            {
                TweaksLog.Info($"TweaksBreakAutoSkipper: Event-driven auto-skipping break to {targetTime:F0}ms");

                // 1. Mute samples
                var sampleDisabled = samplePlaybackDisabledField?.GetValue(player) as Bindable<bool>;
                if (sampleDisabled != null)
                {
                    sampleDisabled.Value = true;
                    updateSampleDisabledStateMethod?.Invoke(player, null);
                }

                // 2. Disable frame stable playback
                frameStablePlaybackProp?.SetValue(drawableRuleset, false);

                // 3. Seek to targetTime
                clockContainer.Seek(targetTime);

                // 4. Restore frame stable playback
                frameStablePlaybackProp?.SetValue(drawableRuleset, true);

                // 5. Restore sample playback
                if (sampleDisabled != null)
                {
                    sampleDisabled.Value = false;
                    updateSampleDisabledStateMethod?.Invoke(player, null);
                }
            }
            catch (Exception ex)
            {
                TweaksLog.Error("TweaksBreakAutoSkipper.performBreakSkip error", ex);
            }
        }
    }
}
