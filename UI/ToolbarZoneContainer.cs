using System;
using System.Collections.Generic;
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
    /// Контейнер зоны тулбара (Лево, Центр, Право).
    /// Поддерживает точную вставку элементов по индексам в визуальных экранных координатах.
    /// </summary>
    public partial class ToolbarZoneContainer : CompositeDrawable
    {
        public ToolbarZone Zone { get; }
        public FillFlowContainer<ToolbarBlockContainer> Flow { get; private set; } = null!;

        private Container background = null!;
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

        public event Action<ToolbarZoneContainer, MouseDownEvent>? OnZoneRightClicked;

        public ToolbarZoneContainer(ToolbarZone zone)
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
            string zoneTitle = Zone switch
            {
                ToolbarZone.Left => "Лево",
                ToolbarZone.Center => "Центр",
                ToolbarZone.Right => "Право",
                _ => ""
            };

            InternalChildren = new Drawable[]
            {
                background = new Container
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
                            Colour = Colour4.Black.Opacity(0.4f)
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Masking = true,
                            CornerRadius = 4,
                            BorderThickness = 1.5f,
                            BorderColour = colours.Pink.Opacity(0.4f),
                            Child = new Box { RelativeSizeAxes = Axes.Both, Alpha = 0, AlwaysPresent = true }
                        }
                    }
                },
                emptyPlaceholder = new Container
                {
                    RelativeSizeAxes = Axes.Y,
                    Width = 80,
                    Alpha = 0,
                    Child = new OsuSpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Text = zoneTitle,
                        Font = OsuFont.GetFont(size: 11, weight: FontWeight.Bold),
                        Colour = Colour4.White.Opacity(0.5f)
                    }
                },
                Flow = new FillFlowContainer<ToolbarBlockContainer>
                {
                    RelativeSizeAxes = Axes.Y,
                    AutoSizeAxes = Axes.X,
                    Direction = FillDirection.Horizontal,
                    LayoutDuration = 120,
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
            emptyPlaceholder.FadeTo(isEditMode && isEmpty ? 1 : 0, 150);
        }

        private void updateVisualState()
        {
            if (background == null)
                return;

            background.FadeTo(isEditMode ? 1 : 0, 150);
            if (!isEditMode) insertIndicator.FadeOut(100);

            UpdatePlaceholder();

            foreach (var child in Flow.Children)
            {
                child.IsEditMode = isEditMode;
            }
        }

        public List<ToolbarBlockContainer> GetVisualOrderedChildren()
        {
            if (Flow == null) return new List<ToolbarBlockContainer>();
            return Flow.Children.OrderBy(c => Flow.GetLayoutPosition(c)).ToList();
        }

        public void ShowInsertIndicator(int targetIndex)
        {
            if (!isEditMode || Flow == null) return;

            var ordered = GetVisualOrderedChildren();
            insertIndicator.FadeIn(80);

            if (ordered.Count == 0 || targetIndex <= 0)
            {
                if (ordered.Count > 0)
                {
                    Vector2 localPos = ToLocalSpace(new Vector2(ordered[0].ScreenSpaceDrawQuad.TopLeft.X, ScreenSpaceDrawQuad.Centre.Y));
                    insertIndicator.X = localPos.X;
                }
                else
                {
                    insertIndicator.X = 2;
                }
            }
            else if (targetIndex >= ordered.Count)
            {
                var last = ordered[^1];
                Vector2 localPos = ToLocalSpace(new Vector2(last.ScreenSpaceDrawQuad.TopRight.X, ScreenSpaceDrawQuad.Centre.Y));
                insertIndicator.X = localPos.X;
            }
            else
            {
                var target = ordered[targetIndex];
                Vector2 localPos = ToLocalSpace(new Vector2(target.ScreenSpaceDrawQuad.TopLeft.X, ScreenSpaceDrawQuad.Centre.Y));
                insertIndicator.X = localPos.X;
            }
        }

        public void HideInsertIndicator()
        {
            insertIndicator?.FadeOut(80);
        }

        public int GetInsertionIndexForPosition(Vector2 screenSpacePos)
        {
            if (Flow == null || Flow.Children.Count == 0)
                return 0;

            var ordered = GetVisualOrderedChildren();

            for (int i = 0; i < ordered.Count; i++)
            {
                var child = ordered[i];
                float midPointX = child.ScreenSpaceDrawQuad.Centre.X;
                if (screenSpacePos.X < midPointX)
                    return i;
            }

            return ordered.Count;
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
