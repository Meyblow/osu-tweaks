using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Colour;
using osuTK.Graphics;
using osucc.Core;
using osucc.Plugin;
using OsuTweaks.Models;
using OsuTweaks.Utils;

namespace OsuTweaks.Patches
{
    /// <summary>
    /// Патч на OsuColour.ForStarDifficulty для применения кастомных градиентов сложности звёзд.
    /// </summary>
    public sealed class StarDifficultyColorPatch : PluginPatch<OsuTweaksPlugin>
    {
        private static Bindable<StarRatingPalette>? paletteBindable;

        public StarDifficultyColorPatch(OsuTweaksPlugin plugin, IOsuCcPluginHost host, Bindable<StarRatingPalette> palette)
            : base(plugin, host, "osu.Game.Graphics.OsuColour", "ForStarDifficulty", MethodType.Prefix)
        {
            paletteBindable = palette;
        }

        public static bool Prefix(double stars, ref Color4 __result)
        {
            if (paletteBindable == null || paletteBindable.Value == StarRatingPalette.Vanilla)
                return true; // Use default lazer palette

            try
            {
                __result = GetCustomColor(stars, paletteBindable.Value);
                return false; // Skip original
            }
            catch (Exception ex)
            {
                TweaksLog.Error("StarDifficultyColorPatch error", ex);
                return true;
            }
        }

        public static Color4 GetCustomColor(double stars, StarRatingPalette palette)
        {
            return palette switch
            {
                StarRatingPalette.ClassicStable => getClassicStableColor(stars),
                StarRatingPalette.Neon => getCyberNeonColor(stars),
                StarRatingPalette.Pastel => getSoftPastelColor(stars),
                _ => Color4.White
            };
        }

        private static Color4 getClassicStableColor(double stars)
        {
            if (stars < 1.5) return new Color4(74, 144, 226, 255);   // 1* Blue
            if (stars < 2.25) return new Color4(80, 227, 194, 255);  // 2* Cyan
            if (stars < 3.75) return new Color4(126, 211, 33, 255);  // 3* Green
            if (stars < 4.5) return new Color4(248, 231, 28, 255);   // 4* Yellow
            if (stars < 5.25) return new Color4(245, 166, 35, 255);  // 5* Orange
            if (stars < 6.0) return new Color4(208, 2, 27, 255);     // 6* Red
            if (stars < 7.0) return new Color4(144, 19, 254, 255);   // 7* Purple
            if (stars < 8.0) return new Color4(74, 74, 74, 255);     // 8* Dark Gray
            return new Color4(20, 20, 20, 255);                      // 9*+ Black
        }

        private static Color4 getCyberNeonColor(double stars)
        {
            if (stars < 2.0) return new Color4(0, 255, 240, 255);    // Neon Cyan
            if (stars < 4.0) return new Color4(57, 255, 20, 255);    // Neon Lime
            if (stars < 5.5) return new Color4(255, 255, 0, 255);    // Electric Yellow
            if (stars < 6.5) return new Color4(255, 0, 127, 255);    // Neon Pink
            if (stars < 7.5) return new Color4(180, 0, 255, 255);    // Cyber Violet
            return new Color4(255, 7, 58, 255);                      // Laser Red
        }

        private static Color4 getSoftPastelColor(double stars)
        {
            if (stars < 2.0) return new Color4(179, 205, 224, 255);  // Pastel Sky
            if (stars < 4.0) return new Color4(186, 225, 200, 255);  // Pastel Mint
            if (stars < 5.5) return new Color4(255, 243, 176, 255);  // Pastel Butter
            if (stars < 6.5) return new Color4(255, 204, 188, 255);  // Pastel Peach
            if (stars < 7.5) return new Color4(255, 179, 186, 255);  // Pastel Rose
            return new Color4(218, 182, 214, 255);                   // Pastel Lavender
        }
    }
}
