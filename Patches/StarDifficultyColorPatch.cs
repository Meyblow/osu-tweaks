using System;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osuTK.Graphics;
using osucc.Core;
using osucc.Plugin;
using OsuTweaks.Models;

namespace OsuTweaks.Patches
{
    /// <summary>
    /// Патч на OsuColour.ForStarDifficulty для кастомизации цветового спектра фона плашек звёзд сложности карт.
    /// </summary>
    public sealed class StarDifficultyColorPatch : PluginPatch<OsuTweaksPlugin>
    {
        public static Bindable<StarRatingPalette>? PaletteBindable { get; private set; }

        public StarDifficultyColorPatch(OsuTweaksPlugin plugin, IOsuCcPluginHost host, Bindable<StarRatingPalette> palette)
            : base(plugin, host, "osu.Game.Graphics.OsuColour", "ForStarDifficulty", MethodType.Prefix)
        {
            PaletteBindable = palette;
        }

        public static bool Prefix(double starDifficulty, ref Color4 __result)
        {
            if (PaletteBindable == null || PaletteBindable.Value == StarRatingPalette.Vanilla)
                return true;

            try
            {
                __result = GetCustomStarColour(starDifficulty, PaletteBindable.Value);
                return false;
            }
            catch
            {
                return true;
            }
        }

        public static Color4 GetCustomStarColour(double stars, StarRatingPalette palette)
        {
            float s = (float)Math.Max(0, stars);

            return palette switch
            {
                StarRatingPalette.ClassicStable => getClassicStableColour(s),
                StarRatingPalette.Neon => getNeonColour(s),
                StarRatingPalette.Pastel => getPastelColour(s),
                _ => Color4.White
            };
        }

        private static Color4 getClassicStableColour(float stars)
        {
            if (stars < 1.5f) return Color4Extensions.FromHex("#4fc0ff"); // Easy
            if (stars < 2.25f) return Color4Extensions.FromHex("#4fffcb"); // Normal
            if (stars < 3.75f) return Color4Extensions.FromHex("#f6f05c"); // Hard
            if (stars < 5.25f) return Color4Extensions.FromHex("#ff5454"); // Insane
            if (stars < 6.5f) return Color4Extensions.FromHex("#8e5eff"); // Expert
            if (stars < 8.0f) return Color4Extensions.FromHex("#ff5ee2"); // Master
            return Color4Extensions.FromHex("#2a2a35"); // 8+ Stars
        }

        private static Color4 getNeonColour(float stars)
        {
            if (stars < 2.0f) return Color4Extensions.FromHex("#00f5d4");
            if (stars < 3.5f) return Color4Extensions.FromHex("#70e000");
            if (stars < 5.0f) return Color4Extensions.FromHex("#fee440");
            if (stars < 6.5f) return Color4Extensions.FromHex("#f72585");
            if (stars < 8.0f) return Color4Extensions.FromHex("#7209b7");
            return Color4Extensions.FromHex("#3a0ca3");
        }

        private static Color4 getPastelColour(float stars)
        {
            if (stars < 2.0f) return Color4Extensions.FromHex("#a0c4ff");
            if (stars < 3.5f) return Color4Extensions.FromHex("#caffbf");
            if (stars < 5.0f) return Color4Extensions.FromHex("#fdffb6");
            if (stars < 6.5f) return Color4Extensions.FromHex("#ffadad");
            if (stars < 8.0f) return Color4Extensions.FromHex("#bdb2ff");
            return Color4Extensions.FromHex("#ffc6ff");
        }
    }

    /// <summary>
    /// Патч на OsuColour.ForStarDifficultyText для обеспечения идеальной читаемости текста поверх кастомных цветов.
    /// </summary>
    public sealed class StarDifficultyTextColorPatch : PluginPatch<OsuTweaksPlugin>
    {
        public StarDifficultyTextColorPatch(OsuTweaksPlugin plugin, IOsuCcPluginHost host)
            : base(plugin, host, "osu.Game.Graphics.OsuColour", "ForStarDifficultyText", MethodType.Prefix)
        {
        }

        public static bool Prefix(double starDifficulty, ref Color4 __result)
        {
            var palette = StarDifficultyColorPatch.PaletteBindable;
            if (palette == null || palette.Value == StarRatingPalette.Vanilla)
                return true;

            try
            {
                // Для тёмных фонов (высокая сложность 6.5+) - белый текст, для светлых фонов - темный текст
                if (starDifficulty >= 6.5)
                {
                    __result = Color4.White;
                }
                else
                {
                    __result = Color4.Black.Opacity(0.85f);
                }

                return false;
            }
            catch
            {
                return true;
            }
        }
    }
}
