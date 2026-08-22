using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Overlays.Settings;
using osucc.Plugin;
using OsuTweaks.Models;
using OsuTweaks.Tweaks;

namespace OsuTweaks.UI
{
    /// <summary>
    /// Меню настроек osu!tweaks, выполненное в 100% ванильном стиле osu!lazer.
    /// Использует стандартные SettingsDropdown, SettingsButton и нативную типографику.
    /// </summary>
    public partial class TweaksSettingsSubsection : SettingsSubsection
    {
        protected override LocalisableString Header => "osu!tweaks";

        private readonly PluginSettings settings;

        public TweaksSettingsSubsection(PluginSettings settings)
        {
            this.settings = settings;

            // ==========================================
            // РАЗДЕЛ: ТУЛБАР
            // ==========================================
            var presets = ToolbarPresetManager.GetAvailablePresets();
            var presetDropdown = new SettingsDropdown<string>
            {
                LabelText = "Пресет расположения",
                Current = new Bindable<string>(presets.FirstOrDefault() ?? "Default (Ванильный)"),
                Items = presets
            };

            presetDropdown.Current.BindValueChanged(e =>
            {
                if (!string.IsNullOrEmpty(e.NewValue))
                {
                    ModularToolbarManager.Instance?.ApplyPreset(e.NewValue);
                }
            });
            Add(presetDropdown);

            Add(new SettingsButton
            {
                Text = "Настроить тулбар (Режим редактирования)",
                Margin = new MarginPadding { Top = 6f },
                Action = () => ModularToolbarManager.Instance?.EnterEditMode()
            });

            Add(new SettingsButton
            {
                Text = "Сохранить текущий тулбар как пресет...",
                Margin = new MarginPadding { Top = 6f },
                Action = () =>
                {
                    ModularToolbarManager.Instance?.ShowSavePresetDialog(savedName =>
                    {
                        var updatedPresets = ToolbarPresetManager.GetAvailablePresets();
                        presetDropdown.Items = updatedPresets;
                        presetDropdown.Current.Value = savedName;
                    });
                }
            });

            Add(new SettingsButton
            {
                Text = "Открыть папку с пресетами",
                Margin = new MarginPadding { Top = 6f },
                Action = ToolbarPresetManager.OpenPresetsFolder
            });

            Add(new SettingsButton
            {
                Text = "Сбросить расположение по умолчанию",
                Margin = new MarginPadding { Top = 6f, Bottom = 6f },
                Action = () =>
                {
                    ModularToolbarManager.Instance?.ResetToDefault();
                    presetDropdown.Current.Value = "Default (Ванильный)";
                }
            });

            // ==========================================
            // РАЗДЕЛ: ПРОФИЛЬ ПОЛЬЗОВАТЕЛЯ
            // ==========================================
            var plugin = OsuTweaksPlugin.Instance;
            var currentProfileMode = plugin?.UserProfileDisplayMode.Value ?? UserProfileDisplayMode.Default;

            var profileModeDropdown = new SettingsDropdown<string>
            {
                LabelText = "Расположение аватарки и ника",
                Margin = new MarginPadding { Top = 10f },
                Current = new Bindable<string>(getProfileModeString(currentProfileMode)),
                Items = new[]
                {
                    "По умолчанию (Ник | Аватар)",
                    "Аватар слева (Аватар | Ник)",
                    "С разделителем (Ник │ Аватар)",
                    "Аватар слева с разделителем (Аватар │ Ник)",
                    "Только аватар",
                    "Только никнейм"
                }
            };

            profileModeDropdown.Current.BindValueChanged(e =>
            {
                if (plugin != null && !string.IsNullOrEmpty(e.NewValue))
                {
                    var parsed = parseProfileMode(e.NewValue);
                    plugin.UserProfileDisplayMode.Value = parsed;
                    ModularToolbarManager.Instance?.ApplyUserProfileDisplayMode(parsed);
                }
            });
            Add(profileModeDropdown);

            // ==========================================
            // РАЗДЕЛ: ГЕЙМПЛЕЙ
            // ==========================================
            var initialAutoSkip = plugin?.AutoSkipMode.Value ?? AutoSkipMode.Disabled;

            var autoSkipDropdown = new SettingsDropdown<string>
            {
                LabelText = "Режим пропуска пауз (Автоскип)",
                Margin = new MarginPadding { Top = 10f },
                Current = new Bindable<string>(getAutoSkipModeString(initialAutoSkip)),
                Items = new[] { "Выкл", "Автоскип мид-мап брейков", "Автоскип всего (интро, брейки, аутро)" }
            };

            autoSkipDropdown.Current.BindValueChanged(e =>
            {
                if (plugin != null && !string.IsNullOrEmpty(e.NewValue))
                {
                    plugin.AutoSkipMode.Value = parseAutoSkipMode(e.NewValue);
                }
            });
            Add(autoSkipDropdown);
        }

        private static string getProfileModeString(UserProfileDisplayMode mode) => mode switch
        {
            UserProfileDisplayMode.AvatarLeft => "Аватар слева (Аватар | Ник)",
            UserProfileDisplayMode.WithSeparator => "С разделителем (Ник │ Аватар)",
            UserProfileDisplayMode.AvatarLeftWithSep => "Аватар слева с разделителем (Аватар │ Ник)",
            UserProfileDisplayMode.AvatarOnly => "Только аватар",
            UserProfileDisplayMode.UsernameOnly => "Только никнейм",
            _ => "По умолчанию (Ник | Аватар)"
        };

        private static UserProfileDisplayMode parseProfileMode(string str) => str switch
        {
            "Аватар слева (Аватар | Ник)" => UserProfileDisplayMode.AvatarLeft,
            "С разделителем (Ник │ Аватар)" => UserProfileDisplayMode.WithSeparator,
            "Аватар слева с разделителем (Аватар │ Ник)" => UserProfileDisplayMode.AvatarLeftWithSep,
            "Только аватар" => UserProfileDisplayMode.AvatarOnly,
            "Только никнейм" => UserProfileDisplayMode.UsernameOnly,
            _ => UserProfileDisplayMode.Default
        };

        private static string getAutoSkipModeString(AutoSkipMode mode) => mode switch
        {
            AutoSkipMode.BreaksOnly => "Автоскип мид-мап брейков",
            AutoSkipMode.All => "Автоскип всего (интро, брейки, аутро)",
            _ => "Выкл"
        };

        private static AutoSkipMode parseAutoSkipMode(string str) => str switch
        {
            "Автоскип мид-мап брейков" => AutoSkipMode.BreaksOnly,
            "Автоскип всего (интро, брейки, аутро)" => AutoSkipMode.All,
            _ => AutoSkipMode.Disabled
        };
    }
}
