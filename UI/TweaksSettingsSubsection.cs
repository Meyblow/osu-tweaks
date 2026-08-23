using osu.Framework.Graphics;
using osu.Framework.Localisation;
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
                Add(new SettingsCheckbox
                {
                    LabelText = OsuTweaksStrings.DarkIntroFlashCheckbox,
                    Margin = new MarginPadding { Top = 10f },
                    Current = plugin.DarkIntroFlash
                });

                Add(new SettingsEnumDropdown<AutoSkipMode>
                {
                    LabelText = OsuTweaksStrings.AutoSkipDropdown,
                    Margin = new MarginPadding { Top = 6f },
                    Current = plugin.AutoSkipMode
                });
            }
        }
    }
}
