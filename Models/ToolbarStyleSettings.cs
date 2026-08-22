using osu.Framework.Localisation;
using OsuTweaks.Localisation;

namespace OsuTweaks.Models
{
    /// <summary>
    /// Стили отображения визуальных разделителей (спейсеров).
    /// </summary>
    public enum SpacerStyle
    {
        [LocalisableDescription(typeof(OsuTweaksStrings), nameof(OsuTweaksStrings.SpacerBlank))]
        Blank,

        [LocalisableDescription(typeof(OsuTweaksStrings), nameof(OsuTweaksStrings.SpacerLine))]
        Line,

        [LocalisableDescription(typeof(OsuTweaksStrings), nameof(OsuTweaksStrings.SpacerDot))]
        Dot
    }

    /// <summary>
    /// Акцентные цвета неоновой подсветки тулбара.
    /// </summary>
    public enum ToolbarAccentColor
    {
        [LocalisableDescription(typeof(OsuTweaksStrings), nameof(OsuTweaksStrings.AccentPink))]
        Pink,

        [LocalisableDescription(typeof(OsuTweaksStrings), nameof(OsuTweaksStrings.AccentPurple))]
        Purple,

        [LocalisableDescription(typeof(OsuTweaksStrings), nameof(OsuTweaksStrings.AccentCyan))]
        Cyan,

        [LocalisableDescription(typeof(OsuTweaksStrings), nameof(OsuTweaksStrings.AccentLime))]
        Lime,

        [LocalisableDescription(typeof(OsuTweaksStrings), nameof(OsuTweaksStrings.AccentGold))]
        Gold,

        [LocalisableDescription(typeof(OsuTweaksStrings), nameof(OsuTweaksStrings.AccentWhite))]
        White
    }
}
