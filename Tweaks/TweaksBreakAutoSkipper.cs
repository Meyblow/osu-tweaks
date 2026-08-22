using System;
using System.Collections.Generic;
using System.Reflection;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Beatmaps.Timing;
using osu.Game.Screens.Play;
using OsuTweaks.Utils;

namespace OsuTweaks.Tweaks
{
    /// <summary>
    /// Автономный компонент автоскипа брейков для osu!tweaks.
    /// Внедряется в GameplayClockContainer и оптимизированно перематывает брейк к нотам.
    /// Зависит от настройки 'Skip breaks mid-map' в Specials osu!cc.
    /// При активном автоскипе скрывает визуальную полосу и кнопку SKIP только во время брейков.
    /// </summary>
    public partial class TweaksBreakAutoSkipper : Component
    {
        private const double skip_lead_in = 2000;
        private const double minimum_skip_savings = 1000;

        private readonly Player player;
        private readonly GameplayClockContainer clockContainer;
        private readonly IReadOnlyList<BreakPeriod> breaks;

        private double firstNoteTime = double.MaxValue;
        private int nextBreakIndex;

        private object? drawableRuleset;
        private PropertyInfo? frameStablePlaybackProp;
        private FieldInfo? samplePlaybackDisabledField;
        private MethodInfo? updateSampleDisabledStateMethod;
        private Bindable<bool>? osuCcSkipBreakTime;

        public TweaksBreakAutoSkipper(Player player, GameplayClockContainer clockContainer, IReadOnlyList<BreakPeriod> breaks)
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
            if (plugin == null || !plugin.AutoSkipBreaks.Value)
                return;

            // Проверяем зависимость от настройки osu!cc "Skip breaks mid-map"
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
                    overlay.Alpha = 0;
                    overlay.AlwaysPresent = false;
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
                TweaksLog.Error("TweaksBreakAutoSkipper.performBreakSkip error", ex);
            }
        }
    }
}
