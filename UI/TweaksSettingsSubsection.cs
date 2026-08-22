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

            this.AddCheckbox(settings, "auto_skip_breaks", false, "Автоматический скип брейков", "Автоматически перематывает паузы между секциями карты без нажатия кнопки (требуется включенная опция 'Skip breaks mid-map' в Specials osu!cc)");
        }

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
