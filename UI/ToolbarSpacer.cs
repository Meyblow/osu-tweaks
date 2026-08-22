using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osuTK;

namespace OsuTweaks.UI
{
    /// <summary>
    /// Разделитель / отступ в тулбаре с настраиваемой шириной.
    /// </summary>
    public partial class ToolbarSpacer : CompositeDrawable
    {
        public float SpacerWidth { get; private set; }

        private Box background = null!;
        private Box line = null!;

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
                    Colour = Colour4.White.Opacity(0.05f),
                    Alpha = 0
                },
                line = new Box
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(2, 20),
                    Colour = Colour4.White.Opacity(0.2f),
                    Alpha = 0
                }
            };
        }

        public void SetEditMode(bool isEditMode)
        {
            background.Alpha = isEditMode ? 1 : 0;
            line.Alpha = isEditMode ? 1 : 0;
        }
    }
}
