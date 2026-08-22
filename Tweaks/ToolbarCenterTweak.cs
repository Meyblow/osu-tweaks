using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Overlays.Toolbar;
using osucc.Plugin;

namespace OsuTweaks.Tweaks
{
    /// <summary>
    /// Твик центрирования иконок оверлеев и навигации в тулбаре.
    /// Переносит иконки из правого блока в отдельный центральный контейнер,
    /// оставляя кнопку профиля справа.
    /// </summary>
    public class ToolbarCenterTweak : IDisposable
    {
        public static ToolbarCenterTweak? Instance { get; private set; }

        private readonly IOsuCcPluginHost host;
        private readonly Bindable<bool> isEnabled;

        private Toolbar? toolbar;
        private FillFlowContainer? rightFlow;
        private FillFlowContainer? centerFlow;
        private ToolbarUserButton? userButton;
        private readonly List<Drawable> movedButtons = new();

        public ToolbarCenterTweak(IOsuCcPluginHost host, Bindable<bool> isEnabled)
        {
            Instance = this;
            this.host = host;
            this.isEnabled = isEnabled;

            isEnabled.BindValueChanged(onToggleChanged, false);
            TweaksLog.Info($"ToolbarCenterTweak instantiated. Current setting CenterToolbarIcons = {isEnabled.Value}");
        }

        public void AttachToolbar(Toolbar newToolbar)
        {
            toolbar = newToolbar;
            TweaksLog.Info($"ToolbarCenterTweak.AttachToolbar called with toolbar HashCode: {newToolbar.GetHashCode()}");

            host.Scheduler?.Add(() =>
            {
                try
                {
                    initContainers();
                    applyState(isEnabled.Value);
                }
                catch (Exception ex)
                {
                    TweaksLog.Error("Error during AttachToolbar on scheduler", ex);
                }
            });
        }

        private void onToggleChanged(ValueChangedEvent<bool> e)
        {
            TweaksLog.Info($"CenterToolbarIcons setting changed from {e.OldValue} to {e.NewValue}");
            host.Scheduler?.Add(() =>
            {
                try
                {
                    applyState(e.NewValue);
                }
                catch (Exception ex)
                {
                    TweaksLog.Error("Error in onToggleChanged scheduler", ex);
                }
            });
        }

        private void initContainers()
        {
            if (toolbar == null)
            {
                TweaksLog.Warn("initContainers: toolbar is null");
                return;
            }

            if (rightFlow != null && rightFlow.IsLoaded)
            {
                TweaksLog.Info("initContainers: rightFlow is already resolved and loaded.");
                return;
            }

            TweaksLog.Info("initContainers: Searching for Right buttons container in Toolbar hierarchy...");

            foreach (var child in getChildren(toolbar))
            {
                TweaksLog.Info($"  Toolbar visual child: Type={child.GetType().Name}, Name='{child.Name}', Depth={child.Depth}");
            }

            rightFlow = findRightFlow(toolbar);

            if (rightFlow == null)
            {
                TweaksLog.Error("initContainers: Could NOT find right buttons FillFlowContainer in Toolbar!");
                return;
            }

            TweaksLog.Info($"initContainers: Found rightFlow: Type={rightFlow.GetType().Name}, Name='{rightFlow.Name}', ChildrenCount={rightFlow.Children.Count}");

            userButton = findUserButton(rightFlow);

            if (userButton != null)
            {
                TweaksLog.Info($"initContainers: Found userButton: Type={userButton.GetType().Name}, Name='{userButton.Name}'");
            }
            else
            {
                TweaksLog.Warn("initContainers: ToolbarUserButton not found inside rightFlow. Listing all rightFlow children:");
                int idx = 0;
                foreach (var child in rightFlow.Children)
                {
                    TweaksLog.Info($"    [{idx++}] Child Type={child.GetType().Name}, Name='{child.Name}'");
                }
            }
        }

        private static ToolbarUserButton? findUserButton(Drawable container)
        {
            if (container is ToolbarUserButton direct)
                return direct;

            foreach (var child in getChildren(container))
            {
                if (child is ToolbarUserButton ub)
                    return ub;

                var nested = findUserButton(child);
                if (nested != null)
                    return nested;
            }

            return null;
        }

        private static FillFlowContainer? findRightFlow(Drawable root)
        {
            foreach (var child in getChildren(root))
            {
                if (child.Name == "Right buttons" || child.Name == "Right flow")
                {
                    var flow = getChildren(child).OfType<FillFlowContainer>().FirstOrDefault() ?? (child as FillFlowContainer);
                    if (flow != null)
                        return flow;
                }

                if (child is FillFlowContainer f && (f.Anchor == Anchor.TopRight || f.Origin == Anchor.TopRight))
                    return f;

                var found = findRightFlow(child);
                if (found != null)
                    return found;
            }

            return null;
        }

        private static IEnumerable<Drawable> getChildren(Drawable drawable)
        {
            if (drawable == null) yield break;

            var childrenProp = drawable.GetType().GetProperty("Children", BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
            if (childrenProp?.GetValue(drawable) is IEnumerable<Drawable> children)
            {
                foreach (var child in children)
                    yield return child;
            }

            var contentProp = drawable.GetType().GetProperty("Content", BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
            if (contentProp?.GetValue(drawable) is IEnumerable content)
            {
                foreach (var row in content)
                {
                    if (row is Drawable cellDrawable)
                    {
                        yield return cellDrawable;
                    }
                    else if (row is IEnumerable rowContent)
                    {
                        foreach (var cell in rowContent)
                        {
                            if (cell is Drawable inner)
                                yield return inner;
                        }
                    }
                }
            }
        }

        private void applyState(bool enabled)
        {
            if (toolbar == null)
            {
                TweaksLog.Warn("applyState: toolbar is null, skipping.");
                return;
            }

            initContainers();

            if (rightFlow == null)
            {
                TweaksLog.Warn("applyState: rightFlow is null, cannot apply tweak.");
                return;
            }

            TweaksLog.Info($"applyState({enabled}) starting. rightFlow children count = {rightFlow.Children.Count}");

            if (enabled)
            {
                if (centerFlow != null && centerFlow.Parent != null)
                {
                    TweaksLog.Info("applyState(true): centerFlow is already attached to toolbar.");
                    return;
                }

                var buttonsToMove = rightFlow.Children
                                             .Where(c => c != userButton && c != centerFlow)
                                             .ToList();

                TweaksLog.Info($"applyState(true): Found {buttonsToMove.Count} buttons to move into centerFlow.");

                if (buttonsToMove.Count == 0 && movedButtons.Count == 0)
                {
                    TweaksLog.Warn("applyState(true): No buttons to move!");
                    return;
                }

                if (centerFlow == null)
                {
                    centerFlow = new FillFlowContainer
                    {
                        Name = "OsuTweaks_CenterToolbarIcons",
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        RelativeSizeAxes = Axes.Y,
                        AutoSizeAxes = Axes.X,
                        Direction = FillDirection.Horizontal
                    };
                    TweaksLog.Info("applyState(true): Created new centerFlow container.");
                }

                movedButtons.Clear();

                foreach (var button in buttonsToMove)
                {
                    TweaksLog.Info($"  Moving button '{button.GetType().Name}' ({button.Name}) from rightFlow -> centerFlow");
                    rightFlow.Remove(button, false);
                    centerFlow.Add(button);
                    movedButtons.Add(button);
                }

                if (centerFlow.Parent == null)
                {
                    toolbar.Add(centerFlow);
                    TweaksLog.Info("applyState(true): Added centerFlow to Toolbar successfully!");
                }
            }
            else
            {
                if (centerFlow == null || centerFlow.Parent == null)
                {
                    TweaksLog.Info("applyState(false): centerFlow is not attached, nothing to restore.");
                    return;
                }

                var buttonsToRestore = centerFlow.Children.ToList();
                TweaksLog.Info($"applyState(false): Restoring {buttonsToRestore.Count} buttons back to rightFlow.");

                foreach (var button in buttonsToRestore)
                {
                    TweaksLog.Info($"  Restoring button '{button.GetType().Name}' ({button.Name}) -> rightFlow");
                    centerFlow.Remove(button, false);

                    if (userButton != null && rightFlow.Children.Contains(userButton))
                    {
                        rightFlow.Insert(Math.Max(0, rightFlow.IndexOf(userButton)), button);
                    }
                    else
                    {
                        rightFlow.Add(button);
                    }
                }

                movedButtons.Clear();
                toolbar.Remove(centerFlow, true);
                centerFlow = null;
                TweaksLog.Info("applyState(false): centerFlow removed from Toolbar. Standard layout restored.");
            }
        }

        public void Dispose()
        {
            isEnabled.ValueChanged -= onToggleChanged;
            host.Scheduler?.Add(() => applyState(false));
            GC.SuppressFinalize(this);
        }
    }
}
