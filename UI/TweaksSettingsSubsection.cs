using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays.Settings;
using osucc.Plugin;
using osuTK;
using OsuTweaks.Models;
using OsuTweaks.Tweaks;

namespace OsuTweaks.UI
{
    /// <summary>
    /// Меню настроек osu!tweaks с четкой иерархией категорий и подсекций:
    /// - User Interface (Тулбар, Профиль пользователя)
    /// - Gameplay (Автоскип)
    /// </summary>
    public partial class TweaksSettingsSubsection : SettingsSubsection
    {
        protected override LocalisableString Header => "osu!tweaks";

        private readonly PluginSettings settings;

        public TweaksSettingsSubsection(PluginSettings settings)
        {
            this.settings = settings;

            // ==========================================
            // КАТЕГОРИЯ: USER INTERFACE
            // ==========================================
            Add(createCategoryHeader("User Interface", FontAwesome.Solid.Desktop));

            // --- Подсекция: Тулбар ---
            Add(createSubHeader("Тулбар", FontAwesome.Solid.SlidersH));

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

            Add(new Container
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Padding = new MarginPadding { Horizontal = 14, Vertical = 4 },
                Child = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0, 8),
                    Children = new Drawable[]
                    {
                        new RoundedButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 38,
                            Text = "Настроить тулбар (Режим редактирования)",
                            BackgroundColour = Colour4.FromHex("#ff66aa"),
                            Action = () => ModularToolbarManager.Instance?.EnterEditMode()
                        },
                        new GridContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            RowDimensions = new[] { new Dimension(GridSizeMode.AutoSize) },
                            ColumnDimensions = new[]
                            {
                                new Dimension(GridSizeMode.Relative, 0.5f),
                                new Dimension(GridSizeMode.Absolute, 8),
                                new Dimension(GridSizeMode.Relative, 0.5f)
                            },
                            Content = new[]
                            {
                                new Drawable[]
                                {
                                    new RoundedButton
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        Height = 36,
                                        Text = "Сохранить как пресет...",
                                        BackgroundColour = Colour4.FromHex("#323242"),
                                        Action = () =>
                                        {
                                            ModularToolbarManager.Instance?.ShowSavePresetDialog(savedName =>
                                            {
                                                var updatedPresets = ToolbarPresetManager.GetAvailablePresets();
                                                presetDropdown.Items = updatedPresets;
                                                presetDropdown.Current.Value = savedName;
                                            });
                                        }
                                    },
                                    Empty(),
                                    new RoundedButton
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        Height = 36,
                                        Text = "Папка пресетов",
                                        BackgroundColour = Colour4.FromHex("#323242"),
                                        Action = ToolbarPresetManager.OpenPresetsFolder
                                    }
                                }
                            }
                        },
                        new RoundedButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 36,
                            Text = "Сбросить расположение по умолчанию",
                            BackgroundColour = Colour4.FromHex("#22222c"),
                            Action = () =>
                            {
                                ModularToolbarManager.Instance?.ResetToDefault();
                                presetDropdown.Current.Value = "Default (Ванильный)";
                            }
                        }
                    }
                }
            });

            // --- Подсекция: Профиль пользователя ---
            Add(createSubHeader("Профиль пользователя", FontAwesome.Solid.UserCircle));

            var plugin = OsuTweaksPlugin.Instance;
            var currentProfileMode = plugin?.UserProfileDisplayMode.Value ?? UserProfileDisplayMode.Default;

            var profileModeDropdown = new SettingsDropdown<string>
            {
                LabelText = "Расположение аватарки и ника",
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
                if (plugin != null)
                {
                    plugin.UserProfileDisplayMode.Value = parseProfileMode(e.NewValue);
                }
            });
            Add(profileModeDropdown);

            // ==========================================
            // КАТЕГОРИЯ: GAMEPLAY
            // ==========================================
            Add(createCategoryHeader("Gameplay", FontAwesome.Solid.Gamepad));

            // --- Подсекция: Автоскип ---
            Add(createSubHeader("Автоскип", FontAwesome.Solid.Forward));

            var initialAutoSkip = plugin?.AutoSkipMode.Value ?? AutoSkipMode.Disabled;

            var autoSkipDropdown = new SettingsDropdown<string>
            {
                LabelText = "Режим пропуска пауз",
                Current = new Bindable<string>(getAutoSkipModeString(initialAutoSkip)),
                Items = new[] { "Выкл", "Автоскип мид-мап брейков", "Автоскип всего (интро, брейки, аутро)" }
            };

            autoSkipDropdown.Current.BindValueChanged(e =>
            {
                if (plugin != null)
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

        private static Container createCategoryHeader(string text, IconUsage icon) => new Container
        {
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            Padding = new MarginPadding { Top = 16, Bottom = 6, Left = 14, Right = 14 },
            Child = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Horizontal,
                Spacing = new Vector2(8, 0),
                Children = new Drawable[]
                {
                    new SpriteIcon
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Size = new Vector2(14),
                        Icon = icon,
                        Colour = Colour4.FromHex("#ff66aa")
                    },
                    new OsuSpriteText
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Text = text,
                        Font = OsuFont.GetFont(size: 15, weight: FontWeight.Black),
                        Colour = Colour4.FromHex("#ff66aa")
                    }
                }
            }
        };

        private static Container createSubHeader(string text, IconUsage icon) => new Container
        {
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            Padding = new MarginPadding { Top = 10, Bottom = 4, Left = 20, Right = 14 },
            Child = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Horizontal,
                Spacing = new Vector2(6, 0),
                Children = new Drawable[]
                {
                    new SpriteIcon
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Size = new Vector2(11),
                        Icon = icon,
                        Colour = Colour4.White.Opacity(0.6f)
                    },
                    new OsuSpriteText
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Text = text,
                        Font = OsuFont.GetFont(size: 13, weight: FontWeight.Bold),
                        Colour = Colour4.White.Opacity(0.85f)
                    }
                }
            }
        };
    }
}
