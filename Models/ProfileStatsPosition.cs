using osu.Framework.Localisation;
using OsuTweaks.Localisation;

namespace OsuTweaks.Models
{
    /// <summary>
    /// Расположение блока статистики (ранг # и PP) в профиле: справа (по умолчанию) или слева.
    /// </summary>
    public enum ProfileStatsPosition
    {
        [LocalisableDescription(typeof(OsuTweaksStrings), nameof(OsuTweaksStrings.ProfileStatsRight))]
        Right,

        [LocalisableDescription(typeof(OsuTweaksStrings), nameof(OsuTweaksStrings.ProfileStatsLeft))]
        Left
    }
}
