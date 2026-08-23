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

        public Bindable<AutoSkipMode> AutoSkipMode { get; private set; } = new(Models.AutoSkipMode.Disabled);
        public Bindable<bool> DarkIntroFlash { get; private set; } = new(true);

        private IntroFlashCustomizer? introFlashCustomizer;

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

            AutoSkipMode = Host.GetSettings().Bind("auto_skip_mode", Models.AutoSkipMode.Disabled);
            DarkIntroFlash = Host.GetSettings().Bind("dark_intro_flash", true);

            introFlashCustomizer = new IntroFlashCustomizer(Host);
            introFlashCustomizer.Attach(DarkIntroFlash);

            Host.AddPatch(new PlayerBreakAutoSkipPatch(this, Host));
            Host.AddPatch(new IntroFlashPatch(this, Host));

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

            base.Dispose();
            GC.SuppressFinalize(this);
            TweaksLog.Info("osu!tweaks: Plugin disposed.");
        }
    }
}
