using osu.Framework.Localisation;
using OsuTweaks.Localisation;

namespace OsuTweaks.Models
{
    /// <summary>
    /// Режимы работы автоскипа интро и аутро для osu!tweaks.
    /// </summary>
    public enum AutoSkipMode
    {
        [LocalisableDescription(typeof(OsuTweaksStrings), nameof(OsuTweaksStrings.AutoSkipDisabled))]
        Disabled,

        [LocalisableDescription(typeof(OsuTweaksStrings), nameof(OsuTweaksStrings.AutoSkipIntroOnly))]
        IntroOnly,

        [LocalisableDescription(typeof(OsuTweaksStrings), nameof(OsuTweaksStrings.AutoSkipAll))]
        All
    }
}
