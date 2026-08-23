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
    /// Автоматически скрывает отвлекающие элементы HUD (HP, прогресс, скор, комбо, моды) во время игры,
    /// оставляя только Hit Error Bar и игровое поле, и плавно возвращает их на паузе.
    /// </summary>
    public partial class TweaksHUDCustomizer : Component
    {
        private readonly Player player;
        private readonly GameplayClockContainer clockContainer;
        private readonly Bindable<bool> isEnabled = new(false);

        private readonly HashSet<Drawable> trackedElements = new();
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
        }

        protected override void Update()
        {
            base.Update();

            if (IsDisposed || !isEnabled.Value) return;

            // Динамически сканируем дерево игрока на наличие компонентов худа
            scanHudElements();

            bool shouldHide = clockContainer.IsRunning;

            if (shouldHide != isCurrentlyHidden)
            {
                isCurrentlyHidden = shouldHide;
                applyVisibilityState();
            }
        }

        private void scanHudElements()
        {
            try
            {
                var hud = ReflectionHelper.GetPropertyValue<Drawable>(player, "HUDOverlay")
                          ?? player.ChildrenOfType<HUDOverlay>().FirstOrDefault();

                if (hud == null) return;

                foreach (var child in hud.ChildrenOfType<Drawable>())
                {
                    string name = child.GetType().Name;
                    string fullName = child.GetType().FullName ?? "";

                    // Проверяем компоненты здоровья, прогресса, очков, комбо и модов
                    if (name.Contains("Health", StringComparison.OrdinalIgnoreCase) ||
                        name.Contains("SongProgress", StringComparison.OrdinalIgnoreCase) ||
                        name.Contains("ScoreCounter", StringComparison.OrdinalIgnoreCase) ||
                        name.Contains("ComboCounter", StringComparison.OrdinalIgnoreCase) ||
                        name.Contains("ModDisplay", StringComparison.OrdinalIgnoreCase) ||
                        fullName.Contains("Health", StringComparison.OrdinalIgnoreCase) ||
                        fullName.Contains("SongProgress", StringComparison.OrdinalIgnoreCase) ||
                        fullName.Contains("ScoreCounter", StringComparison.OrdinalIgnoreCase) ||
                        fullName.Contains("ArgonScore", StringComparison.OrdinalIgnoreCase))
                    {
                        // Никогда не трогаем Hit Error Display или ноты
                        if (!name.Contains("HitError", StringComparison.OrdinalIgnoreCase) &&
                            !fullName.Contains("HitError", StringComparison.OrdinalIgnoreCase))
                        {
                            if (trackedElements.Add(child))
                            {
                                if (isCurrentlyHidden)
                                {
                                    child.FadeTo(0f, 150, Easing.OutQuint);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TweaksLog.Error("TweaksHUDCustomizer: Error scanning HUD elements", ex);
            }
        }

        private void applyVisibilityState()
        {
            if (IsDisposed) return;

            try
            {
                float targetAlpha = (isEnabled.Value && isCurrentlyHidden) ? 0f : 1f;

                foreach (var elem in trackedElements)
                {
                    if (elem.IsPresent || targetAlpha > 0)
                    {
                        elem.FadeTo(targetAlpha, 200, Easing.OutQuint);
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
            foreach (var elem in trackedElements)
            {
                try { elem.FadeIn(100); } catch { }
            }
            trackedElements.Clear();

            base.Dispose(isDisposing);
        }
    }
}
