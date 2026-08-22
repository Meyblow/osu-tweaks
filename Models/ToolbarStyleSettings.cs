using osu.Framework.Localisation;
using OsuTweaks.Localisation;

namespace OsuTweaks.Models
{
    /// <summary>
    /// Форматы отображения часов на тулбаре.
    /// </summary>
    public enum ClockDisplayFormat
    {
        [LocalisableDescription(typeof(OsuTweaksStrings), nameof(OsuTweaksStrings.ClockFormatStandard))]
        StandardWithSeconds,

        [LocalisableDescription(typeof(OsuTweaksStrings), nameof(OsuTweaksStrings.ClockFormatCompact))]
        CompactNoSeconds,

        [LocalisableDescription(typeof(OsuTweaksStrings), nameof(OsuTweaksStrings.ClockFormatWithDate))]
        WithDate,

        [LocalisableDescription(typeof(OsuTweaksStrings), nameof(OsuTweaksStrings.ClockFormatWithDateAndSeconds))]
        WithDateAndSeconds,

        [LocalisableDescription(typeof(OsuTweaksStrings), nameof(OsuTweaksStrings.ClockFormatSessionOnly))]
        SessionTimerOnly
    }

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
    /// Акцентные цвета подсветки тулбара и неоновых элементов.
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
