using System;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Testing;
using osu.Game;
using osu.Game.Overlays.Toolbar;
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
        public Bindable<UserProfileDisplayMode> UserProfileDisplayMode { get; private set; } = new(Models.UserProfileDisplayMode.Default);
        public Bindable<string> ActivePresetName { get; private set; } = new("Default");

        // Визуальные стили тулбара (Aesthetics)
        public Bindable<bool> FloatingIslandMode { get; private set; } = new(false);
        public Bindable<float> ToolbarCornerRadius { get; private set; } = new(12.0f);
        public Bindable<float> ToolbarBackgroundOpacity { get; private set; } = new(1.0f);
        public Bindable<float> ToolbarHeight { get; private set; } = new(40.0f);
        public Bindable<bool> NeonGlowLine { get; private set; } = new(false);
        public Bindable<float> NeonGlowOffset { get; private set; } = new(0.0f);
        public Bindable<ToolbarAccentColor> ToolbarAccentColor { get; private set; } = new(Models.ToolbarAccentColor.Pink);

        public Bindable<SpacerStyle> SpacerStyle { get; private set; } = new(Models.SpacerStyle.Blank);

        // Дополнительные твики
        public Bindable<bool> DarkIntroFlash { get; private set; } = new(true);

        private ModularToolbarManager? toolbarManager;
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
            UserProfileDisplayMode = Host.GetSettings().Bind("user_profile_display_mode", Models.UserProfileDisplayMode.Default);
            ActivePresetName = Host.GetSettings().Bind("active_preset_name", "Default");

            FloatingIslandMode = Host.GetSettings().Bind("floating_island_mode", false);
            ToolbarCornerRadius = Host.GetSettings().Bind("toolbar_corner_radius", 12.0f);
            ToolbarBackgroundOpacity = Host.GetSettings().Bind("toolbar_bg_opacity", 1.0f);
            ToolbarHeight = Host.GetSettings().Bind("toolbar_height", 40.0f);
            NeonGlowLine = Host.GetSettings().Bind("neon_glow_line", false);
            NeonGlowOffset = Host.GetSettings().Bind("neon_glow_offset", 0.0f);
            ToolbarAccentColor = Host.GetSettings().Bind("toolbar_accent_color", Models.ToolbarAccentColor.Pink);

            SpacerStyle = Host.GetSettings().Bind("spacer_style", Models.SpacerStyle.Blank);
            DarkIntroFlash = Host.GetSettings().Bind("dark_intro_flash", true);

            toolbarManager = new ModularToolbarManager(Host);
            introFlashCustomizer = new IntroFlashCustomizer(Host);
            introFlashCustomizer.Attach(DarkIntroFlash);

            Host.AddPatch(new ToolbarPatch(this, Host));
            Host.AddPatch(new PlayerBreakAutoSkipPatch(this, Host));

            TweaksLog.Info("osu!tweaks: OnLoad() complete.");
        }

        public override void AttachToGame()
        {
            TweaksLog.Info("osu!tweaks: AttachToGame() called.");

            try
            {
                if (Host.Data != null)
                {
                    ToolbarPresetManager.Init(Host.Data);
                }
            }
            catch (Exception ex)
            {
                TweaksLog.Error("Error attaching Host.Data in AttachToGame", ex);
            }

            Host.AddSettingsSubsection(() => new TweaksSettingsSubsection(Host.GetSettings()));

            if (Host.Game is OsuGame game)
            {
                Host.Scheduler?.Add(() =>
                {
                    try
                    {
                        var toolbar = game.ChildrenOfType<Toolbar>().FirstOrDefault();
                        if (toolbar != null)
                        {
                            TweaksLog.Info($"AttachToGame: Found already existing Toolbar ({toolbar.GetHashCode()}), attaching manager.");
                            toolbarManager?.AttachToolbar(toolbar);
                        }
                    }
                    catch (Exception ex)
                    {
                        TweaksLog.Error("Error checking Toolbar in AttachToGame scheduler", ex);
                    }
                });
            }
        }

        internal void OnToolbarLoaded(Toolbar toolbar)
        {
            TweaksLog.Info($"OnToolbarLoaded received Toolbar ({toolbar.GetHashCode()})");
            toolbarManager?.AttachToolbar(toolbar);
        }

        public override void Dispose()
        {
            TweaksLog.Info("osu!tweaks: Disposing plugin...");
            toolbarManager?.Dispose();
            toolbarManager = null;

            introFlashCustomizer?.Dispose();
            introFlashCustomizer = null;

            base.Dispose();
            GC.SuppressFinalize(this);
            TweaksLog.Info("osu!tweaks: Plugin disposed.");
        }
    }
}
