using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK;
using osuTK.Input;
using OsuTweaks.Models;

namespace OsuTweaks.UI
{
    /// <summary>
    /// Контейнер зоны тулбара (Лево, Центр, Право) с поддержкой Drag-and-Drop и визуальной подсветки.
    /// </summary>
    public partial class ToolbarDropZone : CompositeDrawable
    {
        public ToolbarZone Zone { get; }

        public FillFlowContainer<ToolbarItemWrapper> Flow { get; private set; } = null!;

        private Container backgroundContainer = null!;
        private Container emptyPlaceholder = null!;
        private Box insertIndicator = null!;

        private bool isEditMode;
        public bool IsEditMode
        {
            get => isEditMode;
            set
            {
                if (isEditMode == value) return;
                isEditMode = value;
                updateVisualState();
            }
        }

        public event Action<ToolbarDropZone, MouseDownEvent>? OnZoneRightClicked;

        public ToolbarDropZone(ToolbarZone zone)
        {
            Zone = zone;

            RelativeSizeAxes = Axes.Y;
            AutoSizeAxes = Axes.X;

            Anchor = zone switch
            {
                ToolbarZone.Left => Anchor.TopLeft,
                ToolbarZone.Center => Anchor.TopCentre,
                ToolbarZone.Right => Anchor.TopRight,
                _ => Anchor.TopLeft
            };

            Origin = zone switch
            {
                ToolbarZone.Left => Anchor.TopLeft,
                ToolbarZone.Center => Anchor.TopCentre,
                ToolbarZone.Right => Anchor.TopRight,
                _ => Anchor.TopLeft
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            string zoneName = Zone switch
            {
                ToolbarZone.Left => "Лево",
                ToolbarZone.Center => "Центр",
                ToolbarZone.Right => "Право",
                _ => ""
            };

            InternalChildren = new Drawable[]
            {
                backgroundContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    CornerRadius = 4,
                    Alpha = 0,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Colour4.Black.Opacity(0.45f)
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Masking = true,
                            CornerRadius = 4,
                            BorderThickness = 1.5f,
                            BorderColour = colours.Pink.Opacity(0.5f),
                            Child = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Alpha = 0,
                                AlwaysPresent = true
                            }
                        }
                    }
                },
                emptyPlaceholder = new Container
                {
                    RelativeSizeAxes = Axes.Y,
                    Width = 70,
                    Alpha = 0,
                    Child = new OsuSpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Text = zoneName,
                        Font = OsuFont.GetFont(size: 11, weight: FontWeight.Bold),
                        Colour = Colour4.White.Opacity(0.5f)
                    }
                },
                Flow = new FillFlowContainer<ToolbarItemWrapper>
                {
                    RelativeSizeAxes = Axes.Y,
                    AutoSizeAxes = Axes.X,
                    Direction = FillDirection.Horizontal,
                    LayoutDuration = 150,
                    LayoutEasing = Easing.OutQuint
                },
                insertIndicator = new Box
                {
                    Size = new Vector2(3, 36),
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.Centre,
                    Colour = colours.PinkLight,
                    Alpha = 0
                }
            };

            updateVisualState();
        }

        public void UpdatePlaceholder()
        {
            if (emptyPlaceholder == null || Flow == null)
                return;

            bool isEmpty = !Flow.Children.Any();
            if (isEditMode && isEmpty)
            {
                emptyPlaceholder.FadeIn(150);
            }
            else
            {
                emptyPlaceholder.FadeOut(150);
            }
        }

        private void updateVisualState()
        {
            if (backgroundContainer == null)
                return;

            if (isEditMode)
            {
                backgroundContainer.FadeIn(200);
            }
            else
            {
                backgroundContainer.FadeOut(200);
                insertIndicator.FadeOut(100);
            }

            UpdatePlaceholder();

            foreach (var child in Flow.Children)
            {
                child.IsEditMode = isEditMode;
            }
        }

        public void ShowInsertIndicator(int targetIndex)
        {
            if (!isEditMode) return;

            insertIndicator.FadeIn(100);

            if (Flow.Children.Count == 0 || targetIndex <= 0)
            {
                insertIndicator.X = 2;
            }
            else if (targetIndex >= Flow.Children.Count)
            {
                var last = Flow.Children[^1];
                insertIndicator.X = last.DrawPosition.X + last.DrawSize.X + 2;
            }
            else
            {
                var target = Flow.Children[targetIndex];
                insertIndicator.X = target.DrawPosition.X - 2;
            }
        }

        public void HideInsertIndicator()
        {
            insertIndicator?.FadeOut(100);
        }

        public int GetInsertionIndexForPosition(float localX)
        {
            for (int i = 0; i < Flow.Children.Count; i++)
            {
                var child = Flow.Children[i];
                float midPoint = child.DrawPosition.X + (child.DrawSize.X / 2f);
                if (localX < midPoint)
                    return i;
            }

            return Flow.Children.Count;
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            if (e.Button == MouseButton.Right)
            {
                OnZoneRightClicked?.Invoke(this, e);
                return true;
            }
            return base.OnMouseDown(e);
        }
    }
}
