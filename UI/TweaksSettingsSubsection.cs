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
using OsuTweaks.Localisation;
using OsuTweaks.Models;
using OsuTweaks.Tweaks;

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

            // ==========================================
            // РАЗДЕЛ 1: ТУЛБАР И ПРЕСЕТЫ
            // ==========================================
            var presets = ToolbarPresetManager.GetAvailablePresets();
            var activePreset = plugin?.ActivePresetName.Value ?? "Default";
            if (!presets.Contains(activePreset))
            {
                activePreset = presets.FirstOrDefault() ?? "Default";
            }

            var presetDropdown = new SettingsDropdown<string>
            {
                LabelText = OsuTweaksStrings.PresetDropdownLabel,
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
                Text = OsuTweaksStrings.ButtonEnterEditMode,
                Margin = new MarginPadding { Top = 6f },
                Action = () => ModularToolbarManager.Instance?.EnterEditMode()
            });

            Add(new SettingsButton
            {
                Text = OsuTweaksStrings.ButtonSavePreset,
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
                Text = OsuTweaksStrings.ButtonCopyCode,
                Margin = new MarginPadding { Top = 6f },
                Action = () =>
                {
                    var config = ModularToolbarManager.Instance?.GetCurrentConfig() ?? ToolbarLayoutConfig.CreateDefault();
                    string code = config.ExportCode();
                    var clipboard = plugin?.Host?.GetDependency<Clipboard>();
                    clipboard?.SetText(code);
                    plugin?.Host?.Notify(OsuTweaksStrings.NotifyClipboardCopied, NotificationKind.Success);
                }
            });

            Add(new SettingsButton
            {
                Text = OsuTweaksStrings.ButtonImportCode,
                Margin = new MarginPadding { Top = 6f },
                Action = () =>
                {
                    var clipboard = plugin?.Host?.GetDependency<Clipboard>();
                    string? code = clipboard?.GetText();
                    if (string.IsNullOrWhiteSpace(code))
                    {
                        plugin?.Host?.Notify(OsuTweaksStrings.NotifyClipboardEmpty, NotificationKind.Warning);
                        return;
                    }

                    var config = ToolbarLayoutConfig.ImportCode(code);
                    if (config != null)
                    {
                        ModularToolbarManager.Instance?.ApplyConfig(config);
                        if (plugin != null) plugin.ActivePresetName.Value = "Импортированный пресет";
                        plugin?.Host?.Notify(OsuTweaksStrings.NotifyImportSuccess, NotificationKind.Success);
                    }
                    else
                    {
                        plugin?.Host?.Notify(OsuTweaksStrings.NotifyImportInvalid, NotificationKind.Error);
                    }
                }
            });

            Add(new SettingsButton
            {
                Text = OsuTweaksStrings.ButtonOpenPresetsFolder,
                Margin = new MarginPadding { Top = 6f },
                Action = ToolbarPresetManager.OpenPresetsFolder
            });

            Add(new SettingsButton
            {
                Text = OsuTweaksStrings.ButtonResetToDefault,
                Margin = new MarginPadding { Top = 6f, Bottom = 6f },
                Action = () =>
                {
                    ModularToolbarManager.Instance?.ResetToDefault();
                    presetDropdown.Current.Value = "Default";
                }
            });

            // ==========================================
            // РАЗДЕЛ 2: ВИЗУАЛЬНЫЙ СТИЛЬ (AESTHETICS)
            // ==========================================
            if (plugin != null)
            {
                Add(new SettingsCheckbox
                {
                    LabelText = OsuTweaksStrings.FloatingIslandCheckbox,
                    Margin = new MarginPadding { Top = 10f },
                    Current = plugin.FloatingIslandMode
                });

                var cornerRadiusBindable = new BindableFloat(plugin.ToolbarCornerRadius.Value)
                {
                    MinValue = 0f,
                    MaxValue = 24f,
                    Precision = 1f
                };
                cornerRadiusBindable.BindValueChanged(e => plugin.ToolbarCornerRadius.Value = e.NewValue);
                plugin.ToolbarCornerRadius.BindValueChanged(e => cornerRadiusBindable.Value = e.NewValue);

                Add(new SettingsSlider<float>
                {
                    LabelText = OsuTweaksStrings.ToolbarCornerRadiusSlider,
                    Margin = new MarginPadding { Top = 6f },
                    Current = cornerRadiusBindable,
                    KeyboardStep = 1f
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
                    LabelText = OsuTweaksStrings.BackgroundOpacitySlider,
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
                    LabelText = OsuTweaksStrings.ToolbarHeightSlider,
                    Margin = new MarginPadding { Top = 6f },
                    Current = heightBindable,
                    KeyboardStep = 1f
                });

                Add(new SettingsCheckbox
                {
                    LabelText = OsuTweaksStrings.NeonGlowLineCheckbox,
                    Margin = new MarginPadding { Top = 6f },
                    Current = plugin.NeonGlowLine
                });

                var glowOffsetBindable = new BindableFloat(plugin.NeonGlowOffset.Value)
                {
                    MinValue = -5f,
                    MaxValue = 15f,
                    Precision = 1f
                };
                glowOffsetBindable.BindValueChanged(e => plugin.NeonGlowOffset.Value = e.NewValue);
                plugin.NeonGlowOffset.BindValueChanged(e => glowOffsetBindable.Value = e.NewValue);

                Add(new SettingsSlider<float>
                {
                    LabelText = OsuTweaksStrings.NeonGlowOffsetSlider,
                    Margin = new MarginPadding { Top = 6f },
                    Current = glowOffsetBindable,
                    KeyboardStep = 1f
                });

                Add(new SettingsEnumDropdown<ToolbarAccentColor>
                {
                    LabelText = OsuTweaksStrings.NeonAccentColorDropdown,
                    Margin = new MarginPadding { Top = 6f },
                    Current = plugin.ToolbarAccentColor
                });
            }

            // ==========================================
            // РАЗДЕЛ 3: РАЗДЕЛИТЕЛИ (СПЕЙСЕРЫ)
            // ==========================================
            if (plugin != null)
            {
                Add(new SettingsEnumDropdown<SpacerStyle>
                {
                    LabelText = OsuTweaksStrings.SpacerStyleDropdown,
                    Margin = new MarginPadding { Top = 10f },
                    Current = plugin.SpacerStyle
                });
            }

            // ==========================================
            // РАЗДЕЛ 5: ПРОФИЛЬ ПОЛЬЗОВАТЕЛЯ
            // ==========================================
            if (plugin != null)
            {
                var profileDropdown = new SettingsEnumDropdown<UserProfileDisplayMode>
                {
                    LabelText = OsuTweaksStrings.ProfileModeDropdown,
                    Margin = new MarginPadding { Top = 10f },
                    Current = plugin.UserProfileDisplayMode
                };

                profileDropdown.Current.BindValueChanged(e =>
                {
                    ModularToolbarManager.Instance?.ApplyUserProfileDisplayMode(e.NewValue);
                });
                Add(profileDropdown);
            }

            // ==========================================
            // РАЗДЕЛ 6: ГЕЙМПЛЕЙ И ЗАПУСК
            // ==========================================
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
