using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Framework.Platform;
using osu.Game.Overlays.Settings;
using osucc.Client;
using osucc.Plugin;
using OsuTweaks.Models;
using OsuTweaks.Tweaks;

namespace OsuTweaks.UI
{
    /// <summary>
    /// Меню настроек osu!tweaks, выполненное в 100% ванильном стиле osu!lazer.
    /// Использует стандартные SettingsDropdown, SettingsCheckbox, SettingsSlider, SettingsButton и нативную типографику.
    /// </summary>
    public partial class TweaksSettingsSubsection : SettingsSubsection
    {
        protected override LocalisableString Header => "osu!tweaks";

        private readonly PluginSettings settings;

        public TweaksSettingsSubsection(PluginSettings settings)
        {
            this.settings = settings;
            var plugin = OsuTweaksPlugin.Instance;

            // ==========================================
            // РАЗДЕЛ 1: ТУЛБАР И ПРЕСЕТЫ
            // ==========================================
            var presets = ToolbarPresetManager.GetAvailablePresets();
            var activePreset = plugin?.ActivePresetName.Value ?? "Default (Ванильный)";
            if (!presets.Contains(activePreset))
            {
                activePreset = presets.FirstOrDefault() ?? "Default (Ванильный)";
            }

            var presetDropdown = new SettingsDropdown<string>
            {
                LabelText = "Пресет расположения",
                Current = new Bindable<string>(activePreset),
                Items = presets
            };

            presetDropdown.Current.BindValueChanged(e =>
            {
                if (!string.IsNullOrEmpty(e.NewValue) && e.NewValue != plugin?.ActivePresetName.Value)
                {
                    if (plugin != null) plugin.ActivePresetName.Value = e.NewValue;
                    ModularToolbarManager.Instance?.ApplyPreset(e.NewValue);
                }
            });

            if (plugin != null)
            {
                plugin.ActivePresetName.BindValueChanged(e =>
                {
                    if (presetDropdown.Current.Value != e.NewValue)
                    {
                        presetDropdown.Current.Value = e.NewValue;
                    }
                });
            }
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
                Text = "Скопировать код раскладки в буфер (Шаринг)",
                Margin = new MarginPadding { Top = 6f },
                Action = () =>
                {
                    var config = ModularToolbarManager.Instance?.GetCurrentConfig() ?? ToolbarLayoutConfig.CreateDefault();
                    string code = config.ExportCode();
                    var clipboard = plugin?.Host?.GetDependency<Clipboard>();
                    clipboard?.SetText(code);
                    plugin?.Host?.Notify("Код раскладки скопирован в буфер обмена!", NotificationKind.Success);
                }
            });

            Add(new SettingsButton
            {
                Text = "Импортировать раскладку из буфера обмена...",
                Margin = new MarginPadding { Top = 6f },
                Action = () =>
                {
                    var clipboard = plugin?.Host?.GetDependency<Clipboard>();
                    string? code = clipboard?.GetText();
                    if (string.IsNullOrWhiteSpace(code))
                    {
                        plugin?.Host?.Notify("Буфер обмена пуст!", NotificationKind.Warning);
                        return;
                    }

                    var config = ToolbarLayoutConfig.ImportCode(code);
                    if (config != null)
                    {
                        ModularToolbarManager.Instance?.ApplyConfig(config);
                        if (plugin != null) plugin.ActivePresetName.Value = "Импортированный пресет";
                        plugin?.Host?.Notify("Раскладка успешно импортирована!", NotificationKind.Success);
                    }
                    else
                    {
                        plugin?.Host?.Notify("В буфере обмена нет корректного кода раскладки (OT_LAYOUT_v1:...)!", NotificationKind.Error);
                    }
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
            // РАЗДЕЛ 2: ВИЗУАЛЬНЫЙ СТИЛЬ (AESTHETICS)
            // ==========================================
            if (plugin != null)
            {
                Add(new SettingsCheckbox
                {
                    LabelText = "Парящий тулбар (Floating Island)",
                    Margin = new MarginPadding { Top = 10f },
                    Current = plugin.FloatingIslandMode
                });

                var opacityBindable = new BindableFloat(plugin.ToolbarBackgroundOpacity.Value)
                {
                    MinValue = 0f,
                    MaxValue = 1f,
                    Precision = 0.05f
                };
                opacityBindable.BindValueChanged(e => plugin.ToolbarBackgroundOpacity.Value = e.NewValue);
                plugin.ToolbarBackgroundOpacity.BindValueChanged(e => opacityBindable.Value = e.NewValue);

                Add(new SettingsSlider<float>
                {
                    LabelText = "Прозрачность фона тулбара",
                    Margin = new MarginPadding { Top = 6f },
                    Current = opacityBindable,
                    DisplayAsPercentage = true,
                    KeyboardStep = 0.05f
                });

                var heightBindable = new BindableFloat(plugin.ToolbarHeight.Value)
                {
                    MinValue = 26f,
                    MaxValue = 40f,
                    Precision = 1f
                };
                heightBindable.BindValueChanged(e => plugin.ToolbarHeight.Value = e.NewValue);
                plugin.ToolbarHeight.BindValueChanged(e => heightBindable.Value = e.NewValue);

                Add(new SettingsSlider<float>
                {
                    LabelText = "Высота тулбара (Компактный режим)",
                    Margin = new MarginPadding { Top = 6f },
                    Current = heightBindable,
                    KeyboardStep = 1f
                });

                Add(new SettingsCheckbox
                {
                    LabelText = "Неоновая линия подсветки снизу (Glow)",
                    Margin = new MarginPadding { Top = 6f },
                    Current = plugin.NeonGlowLine
                });

                var accentDropdown = new SettingsDropdown<string>
                {
                    LabelText = "Цвет неоновой подсветки",
                    Margin = new MarginPadding { Top = 6f },
                    Current = new Bindable<string>(getAccentColorString(plugin.ToolbarAccentColor.Value)),
                    Items = new[] { "osu! Розовый", "Неоновый фиолетовый", "Киберпанк циан", "Изумрудный лайм", "Золотой", "Белый" }
                };
                accentDropdown.Current.BindValueChanged(e =>
                {
                    if (!string.IsNullOrEmpty(e.NewValue))
                        plugin.ToolbarAccentColor.Value = parseAccentColor(e.NewValue);
                });
                Add(accentDropdown);
            }

            // ==========================================
            // РАЗДЕЛ 3: ЧАСЫ И ДАТА
            // ==========================================
            if (plugin != null)
            {
                var clockDropdown = new SettingsDropdown<string>
                {
                    LabelText = "Формат времени и даты",
                    Margin = new MarginPadding { Top = 10f },
                    Current = new Bindable<string>(getClockFormatString(plugin.ClockDisplayFormat.Value)),
                    Items = new[]
                    {
                        "Стандартный с секундами (HH:mm:ss)",
                        "Компактный без секунд (HH:mm)",
                        "С датой (дд MMM · HH:mm)",
                        "С датой и секундами (дд MMM · HH:mm:ss)",
                        "Только таймер сессии"
                    }
                };
                clockDropdown.Current.BindValueChanged(e =>
                {
                    if (!string.IsNullOrEmpty(e.NewValue))
                        plugin.ClockDisplayFormat.Value = parseClockFormat(e.NewValue);
                });
                Add(clockDropdown);

                Add(new SettingsCheckbox
                {
                    LabelText = "Отображать таймер сессии",
                    Margin = new MarginPadding { Top = 6f },
                    Current = plugin.ShowSessionTimer
                });
            }

            // ==========================================
            // РАЗДЕЛ 4: РАЗДЕЛИТЕЛИ (СПЕЙСЕРЫ)
            // ==========================================
            if (plugin != null)
            {
                var spacerDropdown = new SettingsDropdown<string>
                {
                    LabelText = "Стиль разделителей (Spacers)",
                    Margin = new MarginPadding { Top = 10f },
                    Current = new Bindable<string>(getSpacerStyleString(plugin.SpacerStyle.Value)),
                    Items = new[] { "Невидимый зазор", "Тонкая вертикальная линия", "Точка" }
                };
                spacerDropdown.Current.BindValueChanged(e =>
                {
                    if (!string.IsNullOrEmpty(e.NewValue))
                        plugin.SpacerStyle.Value = parseSpacerStyle(e.NewValue);
                });
                Add(spacerDropdown);
            }

            // ==========================================
            // РАЗДЕЛ 5: ПРОФИЛЬ ПОЛЬЗОВАТЕЛЯ
            // ==========================================
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
            // РАЗДЕЛ 6: ГЕЙМПЛЕЙ
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

        private static string getAccentColorString(ToolbarAccentColor color) => color switch
        {
            ToolbarAccentColor.Purple => "Неоновый фиолетовый",
            ToolbarAccentColor.Cyan => "Киберпанк циан",
            ToolbarAccentColor.Lime => "Изумрудный лайм",
            ToolbarAccentColor.Gold => "Золотой",
            ToolbarAccentColor.White => "Белый",
            _ => "osu! Розовый"
        };

        private static ToolbarAccentColor parseAccentColor(string str) => str switch
        {
            "Неоновый фиолетовый" => ToolbarAccentColor.Purple,
            "Киберпанк циан" => ToolbarAccentColor.Cyan,
            "Изумрудный лайм" => ToolbarAccentColor.Lime,
            "Золотой" => ToolbarAccentColor.Gold,
            "Белый" => ToolbarAccentColor.White,
            _ => ToolbarAccentColor.Pink
        };

        private static string getClockFormatString(ClockDisplayFormat format) => format switch
        {
            ClockDisplayFormat.CompactNoSeconds => "Компактный без секунд (HH:mm)",
            ClockDisplayFormat.WithDate => "С датой (дд MMM · HH:mm)",
            ClockDisplayFormat.WithDateAndSeconds => "С датой и секундами (дд MMM · HH:mm:ss)",
            ClockDisplayFormat.SessionTimerOnly => "Только таймер сессии",
            _ => "Стандартный с секундами (HH:mm:ss)"
        };

        private static ClockDisplayFormat parseClockFormat(string str) => str switch
        {
            "Компактный без секунд (HH:mm)" => ClockDisplayFormat.CompactNoSeconds,
            "С датой (дд MMM · HH:mm)" => ClockDisplayFormat.WithDate,
            "С датой и секундами (дд MMM · HH:mm:ss)" => ClockDisplayFormat.WithDateAndSeconds,
            "Только таймер сессии" => ClockDisplayFormat.SessionTimerOnly,
            _ => ClockDisplayFormat.StandardWithSeconds
        };

        private static string getSpacerStyleString(SpacerStyle style) => style switch
        {
            SpacerStyle.Line => "Тонкая вертикальная линия",
            SpacerStyle.Dot => "Точка",
            _ => "Невидимый зазор"
        };

        private static SpacerStyle parseSpacerStyle(string str) => str switch
        {
            "Тонкая вертикальная линия" => SpacerStyle.Line,
            "Точка" => SpacerStyle.Dot,
            _ => SpacerStyle.Blank
        };

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
