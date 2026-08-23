using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Screens.Play;
using osu.Game.Screens.Play.HUD;
using OsuTweaks.Utils;

namespace OsuTweaks.Tweaks
{
    /// <summary>
    /// Автоматически скрывает отвлекающие элементы HUD (HP, прогресс, скор, моды) во время игры,
    /// оставляя только Hit Error Bar и ноты, и возвращает их на паузе.
    /// </summary>
    public partial class TweaksHUDCustomizer : Component
    {
        private readonly Player player;
        private readonly GameplayClockContainer clockContainer;
        private readonly Bindable<bool> isEnabled = new(false);

        private Drawable? hudOverlay;
        private readonly List<Drawable> hiddenElements = new();
        private bool isCurrentlyHidden;

        public TweaksHUDCustomizer(Player player, GameplayClockContainer clockContainer)
        {
            this.player = player;
            this.clockContainer = clockContainer;
            AlwaysPresent = true;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            var plugin = OsuTweaksPlugin.Instance;
            if (plugin != null)
            {
                isEnabled.BindTo(plugin.MinimalistHUD);
                isEnabled.BindValueChanged(_ => applyVisibilityState(), true);
            }

            resolveElements();
        }

        private void resolveElements()
        {
            try
            {
                hudOverlay = ReflectionHelper.GetPropertyValue<Drawable>(player, "HUDOverlay")
                             ?? player.ChildrenOfType<HUDOverlay>().FirstOrDefault();

                if (hudOverlay == null) return;

                hiddenElements.Clear();

                foreach (var child in hudOverlay.ChildrenOfType<Drawable>())
                {
                    string name = child.GetType().Name;

                    // Прячем HP, прогресс карты, очки, комбо и моды
                    if (name.Contains("Health", StringComparison.OrdinalIgnoreCase) ||
                        name.Contains("SongProgress", StringComparison.OrdinalIgnoreCase) ||
                        name.Contains("ScoreCounter", StringComparison.OrdinalIgnoreCase) ||
                        name.Contains("ComboCounter", StringComparison.OrdinalIgnoreCase) ||
                        name.Contains("ModDisplay", StringComparison.OrdinalIgnoreCase))
                    {
                        // Не прячем HitErrorDisplay или само игровое поле
                        if (!name.Contains("HitError", StringComparison.OrdinalIgnoreCase) && !hiddenElements.Contains(child))
                        {
                            hiddenElements.Add(child);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TweaksLog.Error("TweaksHUDCustomizer: Error resolving HUD elements", ex);
            }
        }

        protected override void Update()
        {
            base.Update();

            if (IsDisposed || !isEnabled.Value) return;

            if (hiddenElements.Count == 0)
            {
                resolveElements();
            }

            bool shouldHide = clockContainer.IsRunning;

            if (shouldHide != isCurrentlyHidden)
            {
                isCurrentlyHidden = shouldHide;
                applyVisibilityState();
            }
        }

        private void applyVisibilityState()
        {
            if (IsDisposed) return;

            try
            {
                float targetAlpha = (isEnabled.Value && isCurrentlyHidden) ? 0f : 1f;

                foreach (var elem in hiddenElements)
                {
                    if (elem.IsPresent || targetAlpha > 0)
                    {
                        elem.FadeTo(targetAlpha, 250, Easing.OutQuint);
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
            foreach (var elem in hiddenElements)
            {
                try { elem.FadeIn(100); } catch { }
            }
            hiddenElements.Clear();

            base.Dispose(isDisposing);
        }
    }
}
