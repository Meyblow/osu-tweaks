using osu.Framework.Localisation;
using OsuTweaks.Localisation;

namespace OsuTweaks.Models
{
    /// <summary>
    /// Палитра цветового спектра звёзд сложности карт.
    /// </summary>
    public enum StarRatingPalette
    {
        [LocalisableDescription(typeof(OsuTweaksStrings), nameof(OsuTweaksStrings.StarRatingVanilla))]
        Vanilla,

        [LocalisableDescription(typeof(OsuTweaksStrings), nameof(OsuTweaksStrings.StarRatingClassicStable))]
        ClassicStable,

        [LocalisableDescription(typeof(OsuTweaksStrings), nameof(OsuTweaksStrings.StarRatingNeon))]
        Neon,

        [LocalisableDescription(typeof(OsuTweaksStrings), nameof(OsuTweaksStrings.StarRatingPastel))]
        Pastel
    }
}
