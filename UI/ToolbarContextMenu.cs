using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace OsuTweaks.UI
{
    public class ContextMenuItemData
    {
        public string Title { get; set; } = string.Empty;
        public IconUsage? Icon { get; set; }
        public Action? Action { get; set; }
        public bool IsDangerous { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Всплывающее контекстное меню с анимациями и osu!-дизайном.
    /// Конструкция UI создается сразу в конструкторе, чтобы гарантировать отсутствие NRE.
    /// </summary>
    public partial class ToolbarContextMenu : CompositeDrawable
    {
        private readonly Container card;
        private readonly FillFlowContainer menuFlow;
        private readonly List<ContextMenuItemData> items = new();

        public ToolbarContextMenu()
        {
            RelativeSizeAxes = Axes.Both;
            Alpha = 0;
            Depth = float.MinValue;

            menuFlow = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical,
                Padding = new MarginPadding(4)
            };

            card = new Container
            {
                AutoSizeAxes = Axes.Both,
                Masking = true,
                CornerRadius = 6,
                EdgeEffect = new EdgeEffectParameters
                {
                    Type = EdgeEffectType.Shadow,
                    Colour = Colour4.Black.Opacity(0.5f),
                    Radius = 10,
                    Offset = new Vector2(0, 4)
                },
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Colour4.FromHex("#1c1c22").Opacity(0.96f)
                    },
                    menuFlow
                }
            };

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Colour4.Black.Opacity(0.01f),
                    AlwaysPresent = true
                },
                card
            };
        }

        public void ShowAt(Vector2 screenSpacePosition, IEnumerable<ContextMenuItemData> newItems)
        {
            if (menuFlow == null || card == null)
                return;

            items.Clear();
            items.AddRange(newItems);

            menuFlow.Clear();
            foreach (var item in items)
            {
                menuFlow.Add(new ContextMenuItemButton(item, () => Hide()));
            }

            Vector2 localPos = ToLocalSpace(screenSpacePosition);
            card.Position = new Vector2(
                Math.Max(8, Math.Min(localPos.X, Math.Max(100, DrawWidth - 210))),
                Math.Max(8, Math.Min(localPos.Y, Math.Max(100, DrawHeight - 220)))
            );

            this.FadeIn(120);
            card.ScaleTo(0.95f).ScaleTo(1f, 180, Easing.OutQuint);
        }

        public override void Hide()
        {
            this.FadeOut(100);
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            if (!card.ReceivePositionalInputAt(e.ScreenSpaceMouseDownPosition))
            {
                Hide();
                return true;
            }
            return true;
        }

        private sealed partial class ContextMenuItemButton : OsuClickableContainer
        {
            private readonly ContextMenuItemData data;
            private readonly Action closeAction;

            private Box hoverBox = null!;
            private SpriteIcon? iconSprite;
            private OsuSpriteText text = null!;

            public ContextMenuItemButton(ContextMenuItemData data, Action closeAction)
            {
                this.data = data;
                this.closeAction = closeAction;

                Size = new Vector2(190, 30);
                Action = () =>
                {
                    closeAction();
                    data.Action?.Invoke();
                };
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                Colour4 textColour = data.IsDangerous ? colours.Red : (data.IsActive ? colours.PinkLight : Colour4.White);

                Children = new Drawable[]
                {
                    hoverBox = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = (data.IsDangerous ? colours.Red : colours.Pink).Opacity(0.18f),
                        Alpha = 0
                    },
                    new FillFlowContainer
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Direction = FillDirection.Horizontal,
                        Spacing = new Vector2(8, 0),
                        Padding = new MarginPadding { Left = 10, Right = 10 },
                        AutoSizeAxes = Axes.Both,
                        Children = new Drawable[]
                        {
                            iconSprite = data.Icon.HasValue ? new SpriteIcon
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Size = new Vector2(13),
                                Icon = data.Icon.Value,
                                Colour = textColour
                            } : null!,
                            text = new OsuSpriteText
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Text = data.Title,
                                Font = OsuFont.GetFont(size: 13, weight: FontWeight.SemiBold),
                                Colour = textColour
                            }
                        }
                    }
                };
            }

            protected override bool OnHover(HoverEvent e)
            {
                hoverBox.FadeIn(80);
                return base.OnHover(e);
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                hoverBox.FadeOut(80);
                base.OnHoverLost(e);
            }
        }
    }
}
