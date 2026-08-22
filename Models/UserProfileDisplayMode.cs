using osu.Framework.Localisation;
using OsuTweaks.Localisation;

namespace OsuTweaks.Models
{
    /// <summary>
    /// Режим визуального отображения кнопки профиля пользователя в тулбаре.
    /// </summary>
    public enum UserProfileDisplayMode
    {
        [LocalisableDescription(typeof(OsuTweaksStrings), nameof(OsuTweaksStrings.ProfileDefault))]
        Default,

        [LocalisableDescription(typeof(OsuTweaksStrings), nameof(OsuTweaksStrings.ProfileAvatarLeft))]
        AvatarLeft,

        [LocalisableDescription(typeof(OsuTweaksStrings), nameof(OsuTweaksStrings.ProfileWithSeparator))]
        WithSeparator,

        [LocalisableDescription(typeof(OsuTweaksStrings), nameof(OsuTweaksStrings.ProfileAvatarLeftWithSep))]
        AvatarLeftWithSep,

        [LocalisableDescription(typeof(OsuTweaksStrings), nameof(OsuTweaksStrings.ProfileAvatarOnly))]
        AvatarOnly,

        [LocalisableDescription(typeof(OsuTweaksStrings), nameof(OsuTweaksStrings.ProfileUsernameOnly))]
        UsernameOnly
    }
}
