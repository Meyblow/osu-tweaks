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

        private IntroFlashCustomizer? introFlashCustomizer;
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

            // Bind settings
            AutoSkipMode = Host.GetSettings().Bind("auto_skip_mode", Models.AutoSkipMode.Disabled);
            InstantQuickRetry = Host.GetSettings().Bind("instant_quick_retry", false);
            SilentFailSound = Host.GetSettings().Bind("silent_fail_sound", false);

            DarkIntroFlash = Host.GetSettings().Bind("dark_intro_flash", true);
            SkipStartupIntro = Host.GetSettings().Bind("skip_startup_intro", false);
            MinimalistHUD = Host.GetSettings().Bind("minimalist_hud", false);
            DisableLowHealthShake = Host.GetSettings().Bind("disable_low_health_shake", false);
            StarRatingPalette = Host.GetSettings().Bind("star_rating_palette", Models.StarRatingPalette.Vanilla);

            PreviewVolumeLimit = Host.GetSettings().Bind("preview_volume_limit", 0.6);

            // Initialize customizers
            introFlashCustomizer = new IntroFlashCustomizer(Host);
            introFlashCustomizer.Attach(DarkIntroFlash);

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

            introFlashCustomizer?.Dispose();
            introFlashCustomizer = null;

            audioLimiter?.Dispose();
            audioLimiter = null;

            base.Dispose();
            GC.SuppressFinalize(this);
            TweaksLog.Info("osu!tweaks: Plugin disposed.");
        }
    }
}
