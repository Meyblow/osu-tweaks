using System;
using System.Reflection;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osuTK;
using osuTK.Input;
using OsuTweaks.Models;

namespace OsuTweaks.UI
{
    /// <summary>
    /// Универсальный модульный блок для любого элемента тулбара
    /// (кнопки, селектор режимов, профиль, часы, оверлеи, плагины, разделители).
    /// В обычном режиме: прозрачный контейнер.
    /// В режиме скрытия: схлопывается в 0px ширины и становится прозрачным (не реагирует на мышь, но сохраняет AlwaysPresent для хоткеев).
    /// В режиме редактирования: блокирует нативные клики osu! и позволяет перемещать/скрывать блоки.
    /// </summary>
    public partial class ToolbarBlockContainer : CompositeDrawable
    {
        public string ItemId { get; }
        public string DisplayName { get; }
        public Drawable ContentDrawable { get; }

        public Bindable<bool> IsHidden { get; } = new();
        public ToolbarZone CurrentZone { get; set; }

        private bool isHiddenByScreen;
        public bool IsHiddenByScreen
        {
            get => isHiddenByScreen;
            set
            {
                if (isHiddenByScreen == value) return;
                isHiddenByScreen = value;
                updateVisualState();
            }
        }

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

        private ShieldedContentContainer contentWrapper = null!;
        private Container editOverlay = null!;
        private Container borderHighlight = null!;
        private Box hoverOverlay = null!;
        private Box hiddenBackdrop = null!;
        private SpriteIcon eyeIcon = null!;

        private bool wasDragged;

        public event Action<ToolbarBlockContainer, MouseDownEvent>? OnBlockRightClicked;
        public event Action<ToolbarBlockContainer, DragStartEvent>? OnBlockDragStarted;
        public event Action<ToolbarBlockContainer, DragEvent>? OnBlockDragged;
        public event Action<ToolbarBlockContainer, DragEndEvent>? OnBlockDragEnded;

        public override bool HandlePositionalInput => isEditMode || (!IsHidden.Value && !isHiddenByScreen);
        public override bool HandleNonPositionalInput => true;
        public override bool PropagatePositionalInputSubTree => isEditMode || (!IsHidden.Value && !isHiddenByScreen);
        public override bool PropagateNonPositionalInputSubTree => true;

        public ToolbarBlockContainer(string id, string displayName, Drawable contentDrawable, bool isHidden = false)
        {
            ItemId = id;
            DisplayName = displayName;
            ContentDrawable = contentDrawable;
            IsHidden.Value = isHidden;

            RelativeSizeAxes = Axes.Y;
            AutoSizeAxes = Axes.X;
            AlwaysPresent = true;

            IsHidden.BindValueChanged(_ => updateVisualState());
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            DetachFromParent(ContentDrawable);

            ContentDrawable.AlwaysPresent = true;

            InternalChildren = new Drawable[]
            {
                contentWrapper = new ShieldedContentContainer
                {
                    RelativeSizeAxes = Axes.Y,
                    AutoSizeAxes = Axes.X,
                    AlwaysPresent = true,
                    Child = ContentDrawable
                },
                editOverlay = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Depth = float.MinValue,
                    Alpha = 0,
                    Children = new Drawable[]
                    {
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
                            Alpha = 0.4f,
                            Child = new Box { RelativeSizeAxes = Axes.Both, Alpha = 0, AlwaysPresent = true }
                        },
                        hiddenBackdrop = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Colour4.Red.Opacity(0.25f),
                            Alpha = 0
                        },
                        new Container
                        {
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                            Margin = new MarginPadding(2),
                            AutoSizeAxes = Axes.Both,
                            Child = eyeIcon = new SpriteIcon
                            {
                                Size = new Vector2(10),
                                Icon = FontAwesome.Solid.Eye,
                                Colour = Colour4.White.Opacity(0.8f)
                            }
                        }
                    }
                }
            };

            updateVisualState();
        }

        public static void DetachFromParent(Drawable drawable)
        {
            if (drawable.Parent == null)
                return;

            var parent = drawable.Parent;
            if (parent is Container container)
            {
                container.Remove(drawable, false);
                if (drawable.Parent == null)
                    return;
            }

            var removeMethod = typeof(CompositeDrawable).GetMethod("RemoveInternal", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            removeMethod?.Invoke(parent, new object[] { drawable, false });
        }

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos)
        {
            if (!isEditMode && (IsHidden.Value || isHiddenByScreen))
                return false;

            return base.ReceivePositionalInputAt(screenSpacePos);
        }

        private void updateVisualState()
        {
            if (contentWrapper == null)
                return;

            if (isEditMode)
            {
                contentWrapper.IsShielded = true;
                contentWrapper.FadeTo(IsHidden.Value ? 0.35f : 1f, 150);
                contentWrapper.AlwaysPresent = true;
                this.FadeIn(150);
                AlwaysPresent = true;
                BypassAutoSizeAxes = Axes.None;
                editOverlay.FadeIn(150);

                if (IsHidden.Value)
                {
                    hiddenBackdrop.FadeIn(150);
                    eyeIcon.Icon = FontAwesome.Solid.EyeSlash;
                    eyeIcon.Colour = Colour4.Red.Lighten(0.3f);
                }
                else
                {
                    hiddenBackdrop.FadeOut(150);
                    eyeIcon.Icon = FontAwesome.Solid.Eye;
                    eyeIcon.Colour = Colour4.White.Opacity(0.8f);
                }
            }
            else
            {
                editOverlay.FadeOut(150);
                hoverOverlay.FadeOut(100);

                if (IsHidden.Value || isHiddenByScreen)
                {
                    // В обычном режиме скрытые блоки полностью невидимы, не кликаются и не занимают место,
                    // НО AlwaysPresent = true сохраняет работу глобальных хоткеев (Ctrl+O, F8, F9)!
                    this.FadeOut(150);
                    AlwaysPresent = true;
                    BypassAutoSizeAxes = Axes.Both;
                    contentWrapper.Alpha = 0;
                    contentWrapper.AlwaysPresent = true;
                    contentWrapper.IsShielded = true;
                }
                else
                {
                    this.FadeIn(150);
                    AlwaysPresent = true;
                    BypassAutoSizeAxes = Axes.None;
                    contentWrapper.Alpha = 1;
                    contentWrapper.AlwaysPresent = true;
                    contentWrapper.IsShielded = false;
                }
            }
        }

        protected override bool OnHover(HoverEvent e)
        {
            if (isEditMode)
            {
                hoverOverlay.FadeIn(80);
                borderHighlight.FadeTo(1f, 80);
                return true;
            }
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            if (isEditMode)
            {
                hoverOverlay.FadeOut(80);
                borderHighlight.FadeTo(0.4f, 80);
            }
            base.OnHoverLost(e);
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            if (isEditMode)
            {
                if (e.Button == MouseButton.Right)
                {
                    OnBlockRightClicked?.Invoke(this, e);
                }
                return true;
            }
            return base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            if (isEditMode)
                return;

            base.OnMouseUp(e);
        }

        protected override bool OnClick(ClickEvent e)
        {
            if (isEditMode)
            {
                if (e.Button == MouseButton.Left && !wasDragged)
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
                OnBlockDragStarted?.Invoke(this, e);
                return true;
            }
            return base.OnDragStart(e);
        }

        protected override void OnDrag(DragEvent e)
        {
            if (isEditMode)
            {
                OnBlockDragged?.Invoke(this, e);
                return;
            }
            base.OnDrag(e);
        }

        protected override void OnDragEnd(DragEndEvent e)
        {
            if (isEditMode)
            {
                OnBlockDragEnded?.Invoke(this, e);
                Schedule(() => wasDragged = false);
                return;
            }
            base.OnDragEnd(e);
        }

        private sealed partial class ShieldedContentContainer : Container
        {
            public bool IsShielded { get; set; }

            public override bool HandlePositionalInput => !IsShielded;
            public override bool PropagatePositionalInputSubTree => !IsShielded;

            public override bool HandleNonPositionalInput => true;
            public override bool PropagateNonPositionalInputSubTree => true;

            public override bool ReceivePositionalInputAt(Vector2 screenSpacePos)
            {
                if (IsShielded)
                    return false;

                return base.ReceivePositionalInputAt(screenSpacePos);
            }
        }
    }
}
