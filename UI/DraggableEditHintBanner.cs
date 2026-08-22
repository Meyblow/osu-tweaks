using System;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osuTK;
using osuTK.Input;

namespace OsuTweaks.UI
{
    /// <summary>
    /// Информационная плашка режима редактирования тулбара.
    /// Перемещается в любое место экрана зажатием левой кнопки мыши (Drag).
    /// </summary>
    public sealed partial class DraggableEditHintBanner : CompositeDrawable
    {
        public Action? OnSaveAndExit { get; set; }

        public DraggableEditHintBanner(OsuColour colours, Action onSaveAndExit)
        {
            OnSaveAndExit = onSaveAndExit;

            Anchor = Anchor.BottomCentre;
            Origin = Anchor.BottomCentre;
            Y = -24;
            AutoSizeAxes = Axes.Both;
            Masking = true;
            CornerRadius = 8;
            Alpha = 0;
            Depth = float.MinValue;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Colour4.FromHex("#16161c").Opacity(0.95f)
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    CornerRadius = 8,
                    BorderThickness = 1.5f,
                    BorderColour = colours.Pink.Opacity(0.6f),
                    Child = new Box { RelativeSizeAxes = Axes.Both, Alpha = 0, AlwaysPresent = true }
                },
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(12, 0),
                    Padding = new MarginPadding { Horizontal = 16, Vertical = 9 },
                    Children = new Drawable[]
                    {
                        new SpriteIcon
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Size = new Vector2(14),
                            Icon = FontAwesome.Solid.SlidersH,
                            Colour = colours.PinkLight
                        },
                        new OsuSpriteText
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Text = Localisation.OsuTweaksStrings.EditBannerHint,
                            Font = OsuFont.GetFont(size: 13, weight: FontWeight.SemiBold),
                            Colour = Colour4.White
                        },
                        new OsuClickableContainer
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            AutoSizeAxes = Axes.Both,
                            Action = () => OnSaveAndExit?.Invoke(),
                            Child = new Container
                            {
                                AutoSizeAxes = Axes.Both,
                                Masking = true,
                                CornerRadius = 5,
                                Children = new Drawable[]
                                {
                                    new Box
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Colour = colours.Pink.Opacity(0.35f)
                                    },
                                    new OsuSpriteText
                                    {
                                        Padding = new MarginPadding { Horizontal = 10, Vertical = 4 },
                                        Text = Localisation.OsuTweaksStrings.EditBannerSaveButton,
                                        Font = OsuFont.GetFont(size: 12, weight: FontWeight.Bold),
                                        Colour = colours.PinkLight
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        public override bool HandlePositionalInput => true;

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            return e.Button == MouseButton.Left;
        }

        protected override bool OnDragStart(DragStartEvent e)
        {
            return e.Button == MouseButton.Left;
        }

        protected override void OnDrag(DragEvent e)
        {
            Position += e.Delta;
        }

        protected override void OnDragEnd(DragEndEvent e)
        {
        }
    }
}
