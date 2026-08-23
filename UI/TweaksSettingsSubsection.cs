using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Settings;
using osucc.Plugin;
using OsuTweaks.Localisation;
using OsuTweaks.Models;

namespace OsuTweaks.UI
{
    /// <summary>
    /// Меню настроек osu!tweaks, выполненное в 100% ванильном стиле osu!lazer.
    /// Поддерживает автоматическую локализацию (русский / английский) на основе выбранного языка игры.
    /// </summary>
    public partial class TweaksSettingsSubsection : SettingsSubsection
    {
        protected override LocalisableString Header => OsuTweaksStrings.Header;

        private readonly PluginSettings settings;

        public TweaksSettingsSubsection(PluginSettings settings)
        {
            this.settings = settings;
            var plugin = OsuTweaksPlugin.Instance;

            if (plugin != null)
            {
                // ==========================================
                // 1. GAMEPLAY & RESTART
                // ==========================================
                Add(new SettingsEnumDropdown<AutoSkipMode>
                {
                    LabelText = OsuTweaksStrings.AutoSkipDropdown,
                    Margin = new MarginPadding { Top = 10f },
                    Current = plugin.AutoSkipMode
                });

                Add(new SettingsCheckbox
                {
                    LabelText = OsuTweaksStrings.InstantQuickRetryCheckbox,
                    Margin = new MarginPadding { Top = 6f },
                    Current = plugin.InstantQuickRetry
                });

                Add(new SettingsCheckbox
                {
                    LabelText = OsuTweaksStrings.SilentFailSoundCheckbox,
                    Margin = new MarginPadding { Top = 6f },
                    Current = plugin.SilentFailSound
                });

                // ==========================================
                // 2. VISUAL & FOCUS
                // ==========================================
                Add(new SettingsCheckbox
                {
                    LabelText = OsuTweaksStrings.DarkIntroFlashCheckbox,
                    Margin = new MarginPadding { Top = 16f },
                    Current = plugin.DarkIntroFlash
                });

                Add(new SettingsCheckbox
                {
                    LabelText = OsuTweaksStrings.MinimalistHUDCheckbox,
                    Margin = new MarginPadding { Top = 6f },
                    Current = plugin.MinimalistHUD
                });

                Add(new SettingsCheckbox
                {
                    LabelText = OsuTweaksStrings.DisableLowHealthShakeCheckbox,
                    Margin = new MarginPadding { Top = 6f },
                    Current = plugin.DisableLowHealthShake
                });

                Add(new SettingsEnumDropdown<StarRatingPalette>
                {
                    LabelText = OsuTweaksStrings.StarRatingPaletteDropdown,
                    Margin = new MarginPadding { Top = 6f },
                    Current = plugin.StarRatingPalette
                });

                // ==========================================
                // 3. AUDIO & SONG SELECT
                // ==========================================
                Add(new SettingsSlider<double>
                {
                    LabelText = OsuTweaksStrings.PreviewVolumeLimiterSlider,
                    Margin = new MarginPadding { Top = 16f, Bottom = 10f },
                    DisplayAsPercentage = true,
                    KeyboardStep = 0.05f,
                    Current = plugin.PreviewVolumeLimit
                });
            }
        }
    }
}
