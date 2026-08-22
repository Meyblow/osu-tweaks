using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Testing;
using osu.Game.Graphics;
using osu.Game.Overlays.Toolbar;
using osuTK;
using osucc.Plugin;
using OsuTweaks.Models;
using OsuTweaks.Utils;

namespace OsuTweaks.Tweaks
{
    /// <summary>
    /// Управляет визуальным стилем тулбара: Floating Island (плавающий док),
    /// прозрачность фона (0-100%), компактная высота (26-40px), неоновая полоса подсветки и акцентные цвета.
    /// </summary>
    public class ToolbarStyleManager : IDisposable
    {
        private readonly IOsuCcPluginHost host;
        private Toolbar? toolbar;
        private Box? toolbarBackground;
        private Box? neonGlowLine;

        private readonly Bindable<bool> floatingIsland = new();
        private readonly Bindable<float> backgroundOpacity = new(1f);
        private readonly Bindable<float> toolbarHeight = new(40f);
        private readonly Bindable<bool> neonGlow = new(false);
        private readonly Bindable<ToolbarAccentColor> neonColor = new(ToolbarAccentColor.Pink);

        public ToolbarStyleManager(IOsuCcPluginHost host)
        {
            this.host = host;
        }

        public void Attach(
            Toolbar targetToolbar,
            Bindable<bool> floatingIslandMode,
            Bindable<float> bgOpacity,
            Bindable<float> height,
            Bindable<bool> glowLine,
            Bindable<ToolbarAccentColor> glowColor)
        {
            toolbar = targetToolbar;

            floatingIsland.UnbindBindings();
            floatingIsland.BindTo(floatingIslandMode);
            floatingIsland.BindValueChanged(_ => applyStyles(), false);

            backgroundOpacity.UnbindBindings();
            backgroundOpacity.BindTo(bgOpacity);
            backgroundOpacity.BindValueChanged(_ => applyOpacity(), false);

            toolbarHeight.UnbindBindings();
            toolbarHeight.BindTo(height);
            toolbarHeight.BindValueChanged(_ => applyHeight(), false);

            neonGlow.UnbindBindings();
            neonGlow.BindTo(glowLine);
            neonGlow.BindValueChanged(_ => applyNeonGlow(), false);

            neonColor.UnbindBindings();
            neonColor.BindTo(glowColor);
            neonColor.BindValueChanged(_ => applyNeonColor(), false);

            findToolbarBackground();
            createNeonGlowLine();

            host.Scheduler?.AddOnce(() =>
            {
                applyStyles();
                applyOpacity();
                applyHeight();
                applyNeonGlow();
                applyNeonColor();
            });
        }

        private void findToolbarBackground()
        {
            if (toolbar == null) return;

            toolbarBackground = toolbar.ChildrenOfType<Box>().FirstOrDefault(b => b.Name.Contains("background", StringComparison.OrdinalIgnoreCase))
                                ?? ReflectionHelper.GetFieldValue<Box>(toolbar, "background")
                                ?? toolbar.ChildrenOfType<Box>().FirstOrDefault();
        }

        private void createNeonGlowLine()
        {
            if (toolbar == null || neonGlowLine != null) return;

            neonGlowLine = new Box
            {
                Name = "osu!tweaks Neon Glow Line",
                RelativeSizeAxes = Axes.X,
                Height = 2f,
                Anchor = Anchor.BottomCentre,
                Origin = Anchor.BottomCentre,
                Alpha = 0,
                Colour = getColourForAccent(neonColor.Value)
            };

            var container = toolbar.ChildrenOfType<Container>().FirstOrDefault();
            if (container != null)
            {
                container.Add(neonGlowLine);
            }
            else
            {
                toolbar.Add(neonGlowLine);
            }
        }

        public void ApplyAll()
        {
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
                toolbar.Anchor = Anchor.TopCentre;
                toolbar.Origin = Anchor.TopCentre;
                toolbar.X = 0f;
                toolbar.Y = 6f;
                toolbar.Width = 0.985f;
                toolbar.Margin = new MarginPadding(0);
                toolbar.Padding = new MarginPadding(0);
                toolbar.Masking = true;
                toolbar.CornerRadius = 12f;
                toolbar.EdgeEffect = new EdgeEffectParameters
                {
                    Type = EdgeEffectType.Shadow,
                    Radius = 10f,
                    Colour = Colour4.Black.Opacity(0.45f)
                };
            }
            else
            {
                toolbar.Anchor = Anchor.TopLeft;
                toolbar.Origin = Anchor.TopLeft;
                toolbar.X = 0f;
                toolbar.Y = 0f;
                toolbar.Width = 1f;
                toolbar.Margin = new MarginPadding(0);
                toolbar.Padding = new MarginPadding(0);
                toolbar.Masking = false;
                toolbar.CornerRadius = 0f;
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
            if (neonGlowLine == null) return;

            neonGlowLine.Alpha = neonGlow.Value ? 1f : 0f;
        }

        private void applyNeonColor()
        {
            if (neonGlowLine == null) return;

            neonGlowLine.Colour = getColourForAccent(neonColor.Value);
        }

        private static Colour4 getColourForAccent(ToolbarAccentColor color) => color switch
        {
            ToolbarAccentColor.Purple => Colour4.FromHex("#aa55ff"),
            ToolbarAccentColor.Cyan => Colour4.FromHex("#00ddff"),
            ToolbarAccentColor.Lime => Colour4.FromHex("#55ee77"),
            ToolbarAccentColor.Gold => Colour4.FromHex("#ffcc22"),
            ToolbarAccentColor.White => Colour4.White,
            _ => Colour4.FromHex("#ff66aa")
        };

        public void Dispose()
        {
            if (toolbar != null)
            {
                toolbar.Anchor = Anchor.TopLeft;
                toolbar.Origin = Anchor.TopLeft;
                toolbar.X = 0f;
                toolbar.Y = 0f;
                toolbar.Width = 1f;
                toolbar.Masking = false;
                toolbar.CornerRadius = 0f;
                toolbar.Margin = new MarginPadding(0);
                toolbar.Padding = new MarginPadding(0);
                toolbar.Height = 40f;
                toolbar.EdgeEffect = default;
            }

            if (neonGlowLine != null)
            {
                neonGlowLine.Expire();
                neonGlowLine = null;
            }

            toolbar = null;
            toolbarBackground = null;
            GC.SuppressFinalize(this);
        }
    }
}
