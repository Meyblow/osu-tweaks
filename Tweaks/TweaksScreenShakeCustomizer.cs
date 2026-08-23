using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Screens.Play;
using OsuTweaks.Utils;

namespace OsuTweaks.Tweaks
{
    /// <summary>
    /// Отключает эффект тряски экрана и пульсирующую красную виньетку при низком уровне здоровья.
    /// </summary>
    public partial class TweaksScreenShakeCustomizer : Component
    {
        private readonly Player player;
        private readonly Bindable<bool> isEnabled = new(false);

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
                isEnabled.BindValueChanged(_ => suppressShakeAndFlash(), true);
            }
        }

        protected override void Update()
        {
            base.Update();
            if (IsDisposed || !isEnabled.Value) return;

            suppressShakeAndFlash();
        }

        private void suppressShakeAndFlash()
        {
            if (IsDisposed) return;

            try
            {
                // Ищем контейнеры с эффектами тряски экрана и виньетки
                foreach (var container in player.ChildrenOfType<Container>())
                {
                    string name = container.GetType().Name;

                    if (name.Contains("ScreenShake", StringComparison.OrdinalIgnoreCase) ||
                        name.Contains("ShakeContainer", StringComparison.OrdinalIgnoreCase))
                    {
                        container.ClearTransforms();
                        container.Position = osuTK.Vector2.Zero;
                        container.Rotation = 0f;
                    }

                    if (name.Contains("LowHealthOverlay", StringComparison.OrdinalIgnoreCase) ||
                        name.Contains("FlashHealthDisplay", StringComparison.OrdinalIgnoreCase))
                    {
                        container.Alpha = 0f;
                    }
                }
            }
            catch
            {
                // ignore disposed exceptions
            }
        }
    }
}
