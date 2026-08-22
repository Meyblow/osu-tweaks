using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osuTK;
using osuTK.Input;
using OsuTweaks.Models;

namespace OsuTweaks.UI
{
    /// <summary>
    /// Обертка для отдельного элемента тулбара.
    /// Обрабатывает клик для скрытия/показа и Drag-and-Drop в режиме редактирования.
    /// </summary>
    public partial class ToolbarItemWrapper : CompositeDrawable
    {
        public string ItemId { get; }
        public string DisplayName { get; }
        public Drawable WrappedDrawable { get; }

        public Bindable<bool> IsHidden { get; } = new();
        public ToolbarZone CurrentZone { get; set; }

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

        private Container contentContainer = null!;
        private Box hoverOverlay = null!;
        private Container borderHighlight = null!;
        private Box hiddenIndicator = null!;
        private bool wasDragged;

        public event Action<ToolbarItemWrapper, MouseDownEvent>? OnItemRightClicked;
        public event Action<ToolbarItemWrapper, DragStartEvent>? OnItemDragStarted;
        public event Action<ToolbarItemWrapper, DragEvent>? OnItemDragged;
        public event Action<ToolbarItemWrapper, DragEndEvent>? OnItemDragEnded;

        public ToolbarItemWrapper(string id, string name, Drawable wrappedDrawable, bool isHidden = false)
        {
            ItemId = id;
            DisplayName = name;
            WrappedDrawable = wrappedDrawable;
            IsHidden.Value = isHidden;

            RelativeSizeAxes = Axes.Y;
            AutoSizeAxes = Axes.X;

            IsHidden.BindValueChanged(_ => updateVisualState());
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            InternalChildren = new Drawable[]
            {
                contentContainer = new Container
                {
                    RelativeSizeAxes = Axes.Y,
                    AutoSizeAxes = Axes.X,
                    Child = WrappedDrawable
                },
                hoverOverlay = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Colour4.White.Opacity(0.12f),
                    Alpha = 0
                },
                borderHighlight = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    CornerRadius = 4,
                    BorderThickness = 2,
                    BorderColour = colours.Pink,
                    Alpha = 0,
                    Child = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0,
                        AlwaysPresent = true
                    }
                },
                hiddenIndicator = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Colour4.Red.Opacity(0.3f),
                    Alpha = 0
                }
            };

            updateVisualState();
        }

        private void updateVisualState()
        {
            if (contentContainer == null)
                return;

            if (isEditMode)
            {
                AlwaysPresent = true;
                if (IsHidden.Value)
                {
                    this.FadeTo(0.35f, 150);
                    hiddenIndicator.FadeIn(150);
                }
                else
                {
                    this.FadeTo(1f, 150);
                    hiddenIndicator.FadeOut(150);
                }

                borderHighlight.FadeTo(0.5f, 150);
            }
            else
            {
                hiddenIndicator.FadeOut(150);
                borderHighlight.FadeOut(150);
                hoverOverlay.FadeOut(150);

                if (IsHidden.Value)
                {
                    this.FadeOut(150);
                    AlwaysPresent = false;
                }
                else
                {
                    this.FadeIn(150);
                    AlwaysPresent = true;
                }
            }

            if (WrappedDrawable is ToolbarSpacer spacer)
            {
                spacer.SetEditMode(isEditMode);
            }
        }

        protected override bool OnHover(HoverEvent e)
        {
            if (isEditMode)
            {
                hoverOverlay.FadeIn(100);
                borderHighlight.FadeTo(1f, 100);
                return true;
            }
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            if (isEditMode)
            {
                hoverOverlay.FadeOut(100);
                borderHighlight.FadeTo(0.5f, 100);
            }
            base.OnHoverLost(e);
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            if (e.Button == MouseButton.Right)
            {
                OnItemRightClicked?.Invoke(this, e);
                return true;
            }
            return base.OnMouseDown(e);
        }

        protected override bool OnClick(ClickEvent e)
        {
            if (isEditMode && e.Button == MouseButton.Left)
            {
                if (!wasDragged)
                {
                    IsHidden.Value = !IsHidden.Value;
                }
                wasDragged = false;
                return true;
            }
            return base.OnClick(e);
        }

        protected override bool OnDragStart(DragStartEvent e)
        {
            if (isEditMode && e.Button == MouseButton.Left)
            {
                wasDragged = true;
                OnItemDragStarted?.Invoke(this, e);
                return true;
            }
            return base.OnDragStart(e);
        }

        protected override void OnDrag(DragEvent e)
        {
            if (isEditMode)
            {
                OnItemDragged?.Invoke(this, e);
            }
            base.OnDrag(e);
        }

        protected override void OnDragEnd(DragEndEvent e)
        {
            if (isEditMode)
            {
                OnItemDragEnded?.Invoke(this, e);
                Schedule(() => wasDragged = false);
            }
            base.OnDragEnd(e);
        }
    }
}
