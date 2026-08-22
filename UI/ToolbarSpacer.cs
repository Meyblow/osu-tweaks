using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osuTK;
using OsuTweaks.Models;
using OsuTweaks.Tweaks;

namespace OsuTweaks.UI
{
    /// <summary>
    /// Разделитель / отступ в тулбаре с поддержкой стилей: невидимый зазор, тонкая линия, точка.
    /// </summary>
    public partial class ToolbarSpacer : CompositeDrawable
    {
        public float SpacerWidth { get; private set; }

        private Box background = null!;
        private Container lineContainer = null!;
        private Box line = null!;
        private Circle dot = null!;
        private bool isEditMode;

        private readonly Bindable<SpacerStyle> styleBindable = new(SpacerStyle.Blank);

        public ToolbarSpacer(float width = 16f)
        {
            SpacerWidth = width;
            Size = new Vector2(width, 40);
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            InternalChildren = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Colour4.White.Opacity(0.06f),
                    Alpha = 0
                },
                lineContainer = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(2, 18),
                    Masking = true,
                    CornerRadius = 1,
                    Child = line = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Colour4.White.Opacity(0.25f)
                    }
                },
                dot = new Circle
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(4),
                    Colour = Colour4.White.Opacity(0.4f),
                    Alpha = 0
                }
            };

            if (OsuTweaksPlugin.Instance != null)
            {
                styleBindable.BindTo(OsuTweaksPlugin.Instance.SpacerStyle);
                styleBindable.BindValueChanged(_ => updateVisuals(), true);
            }
            else
            {
                updateVisuals();
            }
        }

        public void SetEditMode(bool editMode)
        {
            isEditMode = editMode;
            background.Alpha = editMode ? 1 : 0;
            updateVisuals();
        }

        public void UpdateStyle(SpacerStyle style)
        {
            styleBindable.Value = style;
            updateVisuals();
        }

        private void updateVisuals()
        {
            if (background == null || lineContainer == null || dot == null) return;

            if (isEditMode)
            {
                lineContainer.Alpha = 1;
                line.Colour = Colour4.White.Opacity(0.3f);
                dot.Alpha = 0;
                return;
            }

            switch (styleBindable.Value)
            {
                case SpacerStyle.Line:
                    lineContainer.Alpha = 1;
                    line.Colour = Colour4.White.Opacity(0.22f);
                    dot.Alpha = 0;
                    break;

                case SpacerStyle.Dot:
                    lineContainer.Alpha = 0;
                    dot.Alpha = 1;
                    dot.Colour = Colour4.White.Opacity(0.35f);
                    break;

                default: // Blank
                    lineContainer.Alpha = 0;
                    dot.Alpha = 0;
                    break;
            }
        }
    }
}
