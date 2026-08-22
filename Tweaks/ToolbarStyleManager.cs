using System;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Testing;
using osu.Game.Overlays.Toolbar;
using osuTK;
using osuTK.Graphics;
using osucc.Plugin;
using OsuTweaks.Models;

namespace OsuTweaks.Tweaks
{
    /// <summary>
    /// Управляет визуальным стилем тулбара: Floating Island, прозрачность фона, неоновая линия, высота и акцентный цвет.
    /// </summary>
    public class ToolbarStyleManager : IDisposable
    {
        private readonly IOsuCcPluginHost host;
        private Toolbar? toolbar;
        private Box? neonGlowLine;
        private Drawable? toolbarBackground;

        private readonly Bindable<bool> floatingIsland = new(false);
        private readonly Bindable<float> backgroundOpacity = new(1.0f);
        private readonly Bindable<float> toolbarHeight = new(40.0f);
        private readonly Bindable<bool> neonGlow = new(false);
        private readonly Bindable<ToolbarAccentColor> accentColor = new(ToolbarAccentColor.Pink);

        public ToolbarStyleManager(IOsuCcPluginHost host)
        {
            this.host = host;
        }

        public void Attach(
            Toolbar targetToolbar,
            Bindable<bool> islandBindable,
            Bindable<float> opacityBindable,
            Bindable<float> heightBindable,
            Bindable<bool> neonBindable,
            Bindable<ToolbarAccentColor> accentBindable)
        {
            toolbar = targetToolbar;

            floatingIsland.UnbindBindings();
            floatingIsland.BindTo(islandBindable);
            floatingIsland.BindValueChanged(_ => applyStyles(), false);

            backgroundOpacity.UnbindBindings();
            backgroundOpacity.BindTo(opacityBindable);
            backgroundOpacity.BindValueChanged(_ => applyOpacity(), false);

            toolbarHeight.UnbindBindings();
            toolbarHeight.BindTo(heightBindable);
            toolbarHeight.BindValueChanged(_ => applyHeight(), false);

            neonGlow.UnbindBindings();
            neonGlow.BindTo(neonBindable);
            neonGlow.BindValueChanged(_ => applyNeonGlow(), false);

            accentColor.UnbindBindings();
            accentColor.BindTo(accentBindable);
            accentColor.BindValueChanged(_ => applyNeonColor(), false);

            findBackground();
            createOrUpdateNeonLine();
            applyAll();
        }

        private void findBackground()
        {
            if (toolbar == null) return;

            try
            {
                toolbarBackground = toolbar.Children.FirstOrDefault(c => c.GetType().Name.Contains("ToolbarBackground"));
            }
            catch (Exception ex)
            {
                TweaksLog.Error("ToolbarStyleManager: Error finding ToolbarBackground", ex);
            }
        }

        private void createOrUpdateNeonLine()
        {
            if (toolbar == null) return;

            if (neonGlowLine == null)
            {
                neonGlowLine = new Box
                {
                    Name = "osu!tweaks Neon Glow Line",
                    RelativeSizeAxes = Axes.X,
                    Height = 2f,
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    Depth = float.MinValue,
                    Alpha = neonGlow.Value ? 1f : 0f,
                    Colour = getColourForAccent(accentColor.Value)
                };

                toolbar.Add(neonGlowLine);
            }
        }

        public void ApplyAll() => applyAll();

        private void applyAll()
        {
            if (toolbar == null || !toolbar.IsAlive) return;

            host.Scheduler?.AddOnce(() =>
            {
                applyStyles();
                applyOpacity();
                applyHeight();
                applyNeonGlow();
                applyNeonColor();
            });
        }

        private void applyStyles()
        {
            if (toolbar == null) return;

            if (floatingIsland.Value)
            {
                toolbar.Masking = true;
                toolbar.CornerRadius = 12f;
                toolbar.Margin = new MarginPadding { Top = 6f, Horizontal = 14f };
                toolbar.EdgeEffect = new EdgeEffectParameters
                {
                    Type = EdgeEffectType.Shadow,
                    Radius = 10f,
                    Colour = Colour4.Black.Opacity(0.45f)
                };
            }
            else
            {
                toolbar.Masking = false;
                toolbar.CornerRadius = 0f;
                toolbar.Margin = new MarginPadding(0);
                toolbar.EdgeEffect = default;
            }
        }

        private void applyOpacity()
        {
            if (toolbar == null) return;

            if (toolbarBackground != null)
            {
                toolbarBackground.Alpha = backgroundOpacity.Value;
            }

            // Находим все фоновые плашки в тулбаре
            foreach (var child in toolbar.ChildrenOfType<Box>())
            {
                if (child == neonGlowLine) continue;

                // Если это фоновый темный бокс
                var col = child.Colour.TopLeft.Linear;
                if (col.R < 0.2f && col.G < 0.2f && col.B < 0.2f)
                {
                    child.Alpha = backgroundOpacity.Value;
                }
            }
        }

        private void applyHeight()
        {
            if (toolbar == null) return;

            float h = Math.Clamp(toolbarHeight.Value, 26f, 40f);
            toolbar.Height = h;
        }

        private void applyNeonGlow()
        {
            if (neonGlowLine != null)
            {
                neonGlowLine.Alpha = neonGlow.Value ? 1f : 0f;
            }
        }

        private void applyNeonColor()
        {
            if (neonGlowLine != null)
            {
                neonGlowLine.Colour = getColourForAccent(accentColor.Value);
            }
        }

        public static Colour4 GetAccentColour(ToolbarAccentColor color) => getColourForAccent(color);

        private static Colour4 getColourForAccent(ToolbarAccentColor color) => color switch
        {
            ToolbarAccentColor.Purple => Colour4.FromHex("#AA55FF"),
            ToolbarAccentColor.Cyan => Colour4.FromHex("#00DDFF"),
            ToolbarAccentColor.Lime => Colour4.FromHex("#55EE77"),
            ToolbarAccentColor.Gold => Colour4.FromHex("#FFCC22"),
            ToolbarAccentColor.White => Colour4.White,
            _ => Colour4.FromHex("#FF66AA") // Pink
        };

        public void Dispose()
        {
            if (neonGlowLine?.Parent != null)
            {
                toolbar?.Remove(neonGlowLine, true);
            }
            neonGlowLine = null;
            toolbar = null;
            toolbarBackground = null;
            GC.SuppressFinalize(this);
        }
    }
}
