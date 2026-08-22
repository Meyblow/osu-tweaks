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
    /// Подсекция настроек osu!tweaks, отображаемая в меню настроек osu! и карточке плагина.
    /// Разделена на категории "User Interface" и "Gameplay".
    /// </summary>
    public partial class TweaksSettingsSubsection : SettingsSubsection
    {
        protected override LocalisableString Header => "osu!tweaks";

        private readonly PluginSettings settings;

        public TweaksSettingsSubsection(PluginSettings settings)
        {
            this.settings = settings;

            // --- Категория: User Interface ---
            Add(createSectionHeader("User Interface", FontAwesome.Solid.Desktop));

            var presets = ToolbarPresetManager.GetAvailablePresets();
            var presetDropdown = new SettingsDropdown<string>
            {
                LabelText = "Пресет тулбара (VFS плагина)",
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
                Padding = new MarginPadding { Horizontal = 14, Vertical = 6 },
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
                        new RoundedButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 38,
                            Text = "Сбросить тулбар по умолчанию",
                            BackgroundColour = Colour4.FromHex("#2a2a36"),
                            Action = () =>
                            {
                                ModularToolbarManager.Instance?.ResetToDefault();
                                presetDropdown.Current.Value = "Default (Ванильный)";
                            }
                        }
                    }
                }
            });

            // --- Категория: Gameplay ---
            Add(createSectionHeader("Gameplay", FontAwesome.Solid.Gamepad));

            var plugin = OsuTweaksPlugin.Instance;
            var initialMode = plugin?.AutoSkipMode.Value ?? AutoSkipMode.Disabled;

            var autoSkipDropdown = new SettingsDropdown<string>
            {
                LabelText = "Режим автоскипа",
                Current = new Bindable<string>(getModeString(initialMode)),
                Items = new[] { "Выкл", "Автоскип мид-мап брейков", "Автоскип всего (интро, брейки, аутро)" }
            };

            autoSkipDropdown.Current.BindValueChanged(e =>
            {
                if (plugin != null)
                {
                    plugin.AutoSkipMode.Value = parseMode(e.NewValue);
                }
            });
            Add(autoSkipDropdown);
        }

        private static string getModeString(AutoSkipMode mode) => mode switch
        {
            AutoSkipMode.BreaksOnly => "Автоскип мид-мап брейков",
            AutoSkipMode.All => "Автоскип всего (интро, брейки, аутро)",
            _ => "Выкл"
        };

        private static AutoSkipMode parseMode(string str) => str switch
        {
            "Автоскип мид-мап брейков" => AutoSkipMode.BreaksOnly,
            "Автоскип всего (интро, брейки, аутро)" => AutoSkipMode.All,
            _ => AutoSkipMode.Disabled
        };

        private static Container createSectionHeader(string text, IconUsage icon) => new Container
        {
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            Padding = new MarginPadding { Top = 14, Bottom = 6, Left = 14, Right = 14 },
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
                        Size = new Vector2(13),
                        Icon = icon,
                        Colour = Colour4.FromHex("#ff66aa")
                    },
                    new OsuSpriteText
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Text = text,
                        Font = OsuFont.GetFont(size: 14, weight: FontWeight.Bold),
                        Colour = Colour4.FromHex("#ff66aa")
                    }
                }
            }
        };
    }
}
