using System;
using osu.Framework.Bindables;
using osucc.Plugin;
using OsuTweaks.Models;
using OsuTweaks.Patches;
using OsuTweaks.Tweaks;
using OsuTweaks.UI;

namespace OsuTweaks
{
    /// <summary>
    /// Главная точка входа плагина osu!tweaks для osu!cc.
    /// </summary>
    public class OsuTweaksPlugin : OsuCcPlugin
    {
        public static OsuTweaksPlugin? Instance { get; private set; }

        // 1. Gameplay & Restart
        public Bindable<AutoSkipMode> AutoSkipMode { get; private set; } = new(Models.AutoSkipMode.Disabled);
        public Bindable<bool> InstantQuickRetry { get; private set; } = new(false);
        public Bindable<bool> SilentFailSound { get; private set; } = new(false);

        // 2. Visual & Focus
        public Bindable<bool> DarkIntroFlash { get; private set; } = new(true);
        public Bindable<bool> SkipStartupIntro { get; private set; } = new(false);
        public Bindable<bool> MinimalistHUD { get; private set; } = new(false);
        public Bindable<bool> DisableLowHealthShake { get; private set; } = new(false);
        public Bindable<StarRatingPalette> StarRatingPalette { get; private set; } = new(Models.StarRatingPalette.Vanilla);

        // 3. Audio & Song Select
        public Bindable<double> PreviewVolumeLimit { get; private set; } = new(0.6);

        private TweaksAudioLimiter? audioLimiter;

        protected override void OnLoad()
        {
            Instance = this;
            TweaksLog.Init(Host);
            TweaksLog.Info("osu!tweaks: OnLoad() starting...");

            try
            {
                osucc.Localisation.OsuCcLocalisation.RegisterAssembly(typeof(OsuTweaksPlugin).Assembly);
            }
            catch (Exception ex)
            {
                TweaksLog.Error("Failed to register localization assembly", ex);
            }

            // Bind settings using centralized TweaksSettings constants
            AutoSkipMode = Host.GetSettings().Bind(TweaksSettings.AutoSkipMode, Models.AutoSkipMode.Disabled);
            InstantQuickRetry = Host.GetSettings().Bind(TweaksSettings.InstantQuickRetry, false);
            SilentFailSound = Host.GetSettings().Bind(TweaksSettings.SilentFailSound, false);

            DarkIntroFlash = Host.GetSettings().Bind(TweaksSettings.DarkIntroFlash, true);
            SkipStartupIntro = Host.GetSettings().Bind(TweaksSettings.SkipStartupIntro, false);
            MinimalistHUD = Host.GetSettings().Bind(TweaksSettings.MinimalistHUD, false);
            DisableLowHealthShake = Host.GetSettings().Bind(TweaksSettings.DisableLowHealthShake, false);
            StarRatingPalette = Host.GetSettings().Bind(TweaksSettings.StarRatingPalette, Models.StarRatingPalette.Vanilla);

            PreviewVolumeLimit = Host.GetSettings().Bind(TweaksSettings.PreviewVolumeLimit, 0.6);

            // Initialize audio limiter with reversibility support
            audioLimiter = new TweaksAudioLimiter(Host);
            audioLimiter.Attach(PreviewVolumeLimit);

            // Register Harmony patches
            Host.AddPatch(new PlayerBreakAutoSkipPatch(this, Host));
            Host.AddPatch(new IntroFlashPatch(this, Host));
            Host.AddPatch(new SkipStartupIntroPatch(this, Host, SkipStartupIntro));
            Host.AddPatch(new InstantRestartPatch(this, Host, InstantQuickRetry));
            Host.AddPatch(new PlayerExitingRestartPatch(this, Host));
            Host.AddPatch(new PlayerLoaderResumingPatch(this, Host));
            Host.AddPatch(new FailSoundPatch(this, Host, SilentFailSound));
            Host.AddPatch(new StarDifficultyColorPatch(this, Host, StarRatingPalette));
            Host.AddPatch(new StarDifficultyTextColorPatch(this, Host));

            TweaksLog.Info("osu!tweaks: OnLoad() complete.");
        }

        public override void AttachToGame()
        {
            TweaksLog.Info("osu!tweaks: AttachToGame() called.");
            Host.AddSettingsSubsection(() => new TweaksSettingsSubsection(Host.GetSettings()));
        }

        public override void Dispose()
        {
            TweaksLog.Info("osu!tweaks: Disposing plugin...");

            audioLimiter?.Dispose();
            audioLimiter = null;

            base.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
