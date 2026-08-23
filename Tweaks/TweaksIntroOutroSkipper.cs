using System;
using System.Linq;
using System.Reflection;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Rulesets.Objects;
using osu.Game.Screens.Play;
using OsuTweaks.Models;
using OsuTweaks.Utils;

namespace OsuTweaks.Tweaks
{
    /// <summary>
    /// Автономный компонент автоскипа интро и аутро на основе State Machine для osu!tweaks.
    /// Перемотка пауз (брейков) делегирована хосту osu!cc.
    /// </summary>
    public partial class TweaksIntroOutroSkipper : Component
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

        private bool hasSkippedIntro;
        private bool hasSkippedOutro;

        private object? drawableRuleset;
        private PropertyInfo? frameStablePlaybackProp;
        private FieldInfo? samplePlaybackDisabledField;
        private MethodInfo? updateSampleDisabledStateMethod;
        private Bindable<bool>? samplePlaybackDisabled;

        public TweaksIntroOutroSkipper(Player player, GameplayClockContainer clockContainer)
        {
            this.player = player;
            this.clockContainer = clockContainer;
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
                samplePlaybackDisabled = samplePlaybackDisabledField?.GetValue(player) as Bindable<bool>;
                updateSampleDisabledStateMethod = ReflectionHelper.FindMethod(player.GetType(), "updateSampleDisabledState");
            }
            catch (Exception ex)
            {
                TweaksLog.Error("TweaksIntroOutroSkipper.load failed to resolve reflection handles", ex);
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
                    checkForIntroOutroSkipOpportunity();
                    break;

                case SkipState.Seeking:
                    if (Math.Abs(clockContainer.CurrentTime - seekTarget) < 50 || clockContainer.CurrentTime >= seekTarget)
                    {
                        finishSkip();
                    }
                    break;
            }
        }

        private void checkForIntroOutroSkipOpportunity()
        {
            var plugin = OsuTweaksPlugin.Instance;
            if (plugin == null || plugin.AutoSkipMode.Value == AutoSkipMode.Disabled)
                return;

            var mode = plugin.AutoSkipMode.Value;
            double currentTime = clockContainer.CurrentTime;

            // 1. АВТОСКИП ИНТРО (All или IntroOnly)
            if ((mode == AutoSkipMode.All || mode == AutoSkipMode.IntroOnly) && !hasSkippedIntro)
            {
                double introSkipTarget = firstNoteTime - skip_lead_in;
                if (introSkipTarget - currentTime >= minimum_skip_savings && currentTime < firstNoteTime - 500)
                {
                    hasSkippedIntro = true;
                    TweaksLog.Info($"TweaksIntroOutroSkipper: Auto-skipping Intro to {introSkipTarget:F0}ms");
                    beginSkip(introSkipTarget);
                    return;
                }
            }

            // 2. АВТОСКИП АУТРО (только All)
            if (mode == AutoSkipMode.All && !hasSkippedOutro && currentTime >= lastNoteEndTime + 800)
            {
                hasSkippedOutro = true;
                double outroSkipTarget = lastNoteEndTime + 1200;
                TweaksLog.Info($"TweaksIntroOutroSkipper: Auto-skipping Outro to {outroSkipTarget:F0}ms");
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
                TweaksLog.Error("TweaksIntroOutroSkipper.finishSkip updateSampleDisabledState error", ex);
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
