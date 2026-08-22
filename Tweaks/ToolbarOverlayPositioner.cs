using System;
using System.Reflection;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game;
using osucc.Plugin;
using osuTK;
using OsuTweaks.UI;

namespace OsuTweaks.Tweaks
{
    /// <summary>
    /// Динамически привязывает всплывающие оверлеи (LoginOverlay и NowPlayingOverlay)
    /// прямо под соответствующие кнопки в тулбаре.
    /// </summary>
    public partial class ToolbarOverlayPositioner : Component
    {
        private readonly ModularToolbarManager manager;
        private readonly IOsuCcPluginHost host;

        private VisibilityContainer? loginOverlay;
        private VisibilityContainer? musicOverlay;

        private bool loggedLogin;
        private bool loggedMusic;

        public ToolbarOverlayPositioner(ModularToolbarManager manager, IOsuCcPluginHost host)
        {
            this.manager = manager;
            this.host = host;
            AlwaysPresent = true;
        }

        public void BindOverlays()
        {
            resolveOverlays();
        }

        private void resolveOverlays()
        {
            try
            {
                // 1. Извлекаем LoginOverlay из ToolbarUserButton
                if (loginOverlay == null && manager.AllBlocks.TryGetValue("user_profile", out var userBlock))
                {
                    loginOverlay = extractOverlayFromButton(userBlock.ContentDrawable, "Login");
                    if (loginOverlay != null && !loggedLogin)
                    {
                        loggedLogin = true;
                        TweaksLog.Info($"ToolbarOverlayPositioner: LoginOverlay extracted from ToolbarUserButton ({loginOverlay.GetType().FullName}, Hash={loginOverlay.GetHashCode()})");
                    }
                }

                // 2. Извлекаем NowPlayingOverlay из ToolbarMusicButton
                if (musicOverlay == null && manager.AllBlocks.TryGetValue("music", out var musicBlock))
                {
                    musicOverlay = extractOverlayFromButton(musicBlock.ContentDrawable, "NowPlaying");
                    if (musicOverlay != null && !loggedMusic)
                    {
                        loggedMusic = true;
                        TweaksLog.Info($"ToolbarOverlayPositioner: MusicOverlay extracted from ToolbarMusicButton ({musicOverlay.GetType().FullName}, Hash={musicOverlay.GetHashCode()})");
                    }
                }

                // 3. Если в кнопках не нашлось, ищем по дереву OsuGame
                if (host.Game is Drawable gameRoot)
                {
                    if (loginOverlay == null)
                    {
                        loginOverlay = findOverlayRecursive(gameRoot, "LoginOverlay") ?? findOverlayRecursive(gameRoot, "Login");
                        if (loginOverlay != null && !loggedLogin)
                        {
                            loggedLogin = true;
                            TweaksLog.Info($"ToolbarOverlayPositioner: LoginOverlay found in game tree ({loginOverlay.GetType().FullName}, Hash={loginOverlay.GetHashCode()})");
                        }
                    }

                    if (musicOverlay == null)
                    {
                        musicOverlay = findOverlayRecursive(gameRoot, "NowPlayingOverlay") ?? findOverlayRecursive(gameRoot, "NowPlaying");
                        if (musicOverlay != null && !loggedMusic)
                        {
                            loggedMusic = true;
                            TweaksLog.Info($"ToolbarOverlayPositioner: MusicOverlay found in game tree ({musicOverlay.GetType().FullName}, Hash={musicOverlay.GetHashCode()})");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TweaksLog.Error("resolveOverlays exception", ex);
            }
        }

        protected override void Update()
        {
            base.Update();
            if (IsDisposed) return;

            if (loginOverlay == null || musicOverlay == null)
            {
                resolveOverlays();
            }

            if (loginOverlay != null && loginOverlay.State.Value == Visibility.Visible)
            {
                updateOverlayPosition(loginOverlay, "user_profile");
            }

            if (musicOverlay != null && musicOverlay.State.Value == Visibility.Visible)
            {
                updateOverlayPosition(musicOverlay, "music");
            }
        }

        private void updateOverlayPosition(VisibilityContainer overlay, string blockId)
        {
            if (overlay.Parent == null || !overlay.IsPresent)
                return;

            if (!manager.AllBlocks.TryGetValue(blockId, out var block) || !block.IsPresent)
                return;

            var button = block.ContentDrawable;
            if (button == null || !button.IsPresent)
                return;

            float parentWidth = overlay.Parent.DrawWidth;
            if (parentWidth <= 50f)
                return;

            float buttonCenterX = button.ScreenSpaceDrawQuad.Centre.X;
            float buttonBottomY = button.ScreenSpaceDrawQuad.BottomLeft.Y;

            Vector2 parentLocal = overlay.Parent.ToLocalSpace(new Vector2(buttonCenterX, buttonBottomY));

            overlay.Anchor = Anchor.TopLeft;
            overlay.Origin = Anchor.TopCentre;
            overlay.Margin = new MarginPadding(0);

            float overlayWidth = overlay.DrawWidth > 0 ? overlay.DrawWidth : overlay.Width;
            if (overlayWidth <= 0) overlayWidth = 360f;

            float halfWidth = overlayWidth / 2f;
            float minX = halfWidth + 8f;
            float maxX = parentWidth - halfWidth - 8f;

            float targetX;
            if (minX >= maxX)
            {
                targetX = parentLocal.X;
            }
            else
            {
                targetX = Math.Clamp(parentLocal.X, minX, maxX);
            }

            overlay.X = targetX;
        }

        private static VisibilityContainer? extractOverlayFromButton(Drawable? button, string typeKeyword)
        {
            if (button == null) return null;

            var type = button.GetType();
            while (type != null && type != typeof(object))
            {
                foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                {
                    try
                    {
                        var val = field.GetValue(button);
                        if (val is VisibilityContainer vc && vc.GetType().Name.Contains(typeKeyword, StringComparison.OrdinalIgnoreCase))
                            return vc;
                    }
                    catch { }
                }

                foreach (var prop in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                {
                    if (typeof(VisibilityContainer).IsAssignableFrom(prop.PropertyType))
                    {
                        try
                        {
                            if (prop.GetValue(button) is VisibilityContainer vc && vc.GetType().Name.Contains(typeKeyword, StringComparison.OrdinalIgnoreCase))
                                return vc;
                        }
                        catch { }
                    }
                }

                type = type.BaseType;
            }

            return null;
        }

        private static VisibilityContainer? findOverlayRecursive(Drawable root, string typeKeyword)
        {
            try
            {
                foreach (var child in root.ChildrenOfType<VisibilityContainer>())
                {
                    if (child.GetType().Name.Contains(typeKeyword, StringComparison.OrdinalIgnoreCase))
                        return child;
                }
            }
            catch { }
            return null;
        }
    }
}
