using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Screens.Play;
using osuTK;
using OsuTweaks.Utils;

namespace OsuTweaks.Tweaks
{
    /// <summary>
    /// Отключает тряску экрана и красную вспышку виньетки при низком уровне здоровья (Low HP).
    /// Выполняет поиск контейнеров один раз при загрузке и работает по событиям.
    /// </summary>
    public partial class TweaksScreenShakeCustomizer : Component
    {
        private readonly Player player;
        private readonly Bindable<bool> isEnabled = new(false);
        private readonly List<Container> shakeContainers = new();
        private readonly List<Drawable> flashOverlays = new();
        private bool hasScanned;

        public TweaksScreenShakeCustomizer(Player player)
        {
            this.player = player;
            AlwaysPresent = true;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            var plugin = OsuTweaksPlugin.Instance;
            if (plugin != null)
            {
                isEnabled.BindTo(plugin.DisableLowHealthShake);
                isEnabled.BindValueChanged(v => applySuppression(v.NewValue), true);
            }

            Scheduler.AddDelayed(scanContainersOnce, 200);
        }

        private void scanContainersOnce()
        {
            if (IsDisposed) return;

            try
            {
                foreach (var container in player.ChildrenOfType<Container>())
                {
                    string name = container.GetType().Name;

                    if (name.Contains("ScreenShake", StringComparison.OrdinalIgnoreCase) ||
                        name.Contains("ShakeContainer", StringComparison.OrdinalIgnoreCase))
                    {
                        shakeContainers.Add(container);
                    }
                    else if (name.Contains("LowHealth", StringComparison.OrdinalIgnoreCase) ||
                             name.Contains("FlashHealth", StringComparison.OrdinalIgnoreCase) ||
                             name.Contains("HealthFlash", StringComparison.OrdinalIgnoreCase))
                    {
                        flashOverlays.Add(container);
                    }
                }

                hasScanned = true;
                applySuppression(isEnabled.Value);
            }
            catch (Exception ex)
            {
                TweaksLog.Error("TweaksScreenShakeCustomizer: Error scanning containers", ex);
            }
        }

        private void applySuppression(bool suppress)
        {
            if (IsDisposed || !hasScanned) return;

            try
            {
                if (suppress)
                {
                    foreach (var shake in shakeContainers)
                    {
                        shake.ClearTransforms();
                        shake.Position = Vector2.Zero;
                    }

                    foreach (var flash in flashOverlays)
                    {
                        flash.ClearTransforms();
                        flash.Alpha = 0f;
                    }
                }
            }
            catch
            {
                // ignore disposed exceptions
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            shakeContainers.Clear();
            flashOverlays.Clear();
            base.Dispose(isDisposing);
        }
    }
}
