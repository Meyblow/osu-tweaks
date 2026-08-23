using System;
using osu.Framework.Graphics.Colour;
using osuTK.Graphics;
using osucc.Core;
using osucc.Plugin;
using OsuTweaks.Utils;

namespace OsuTweaks.Patches
{
    /// <summary>
    /// Патч на OsuColour.ForStarDifficultyText для обеспечения высокого контраста текста на кастомных спектрах сложности звёзд.
    /// </summary>
    public sealed class StarDifficultyTextColorPatch : PluginPatch<OsuTweaksPlugin>
    {
        public StarDifficultyTextColorPatch(OsuTweaksPlugin plugin, IOsuCcPluginHost host)
            : base(plugin, host, "osu.Game.Graphics.OsuColour", "ForStarDifficultyText", MethodType.Prefix)
        {
        }

        public static bool Prefix(double starDifficulty, ref Color4 __result)
        {
            var plugin = OsuTweaksPlugin.Instance;
            if (plugin?.StarRatingPalette.Value == Models.StarRatingPalette.Vanilla)
                return true;

            try
            {
                var bg = StarDifficultyColorPatch.GetCustomColor(starDifficulty, plugin?.StarRatingPalette.Value ?? Models.StarRatingPalette.Vanilla);
                // Относительная яркость по стандарту WCAG
                double luminance = 0.299 * bg.R + 0.587 * bg.G + 0.114 * bg.B;

                __result = luminance > 0.5f ? new Color4(20, 20, 20, 255) : Color4.White;
                return false;
            }
            catch (Exception ex)
            {
                TweaksLog.Error("StarDifficultyTextColorPatch error", ex);
                return true;
            }
        }
    }
}
