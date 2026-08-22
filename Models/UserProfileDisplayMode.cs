namespace OsuTweaks.Models
{
    /// <summary>
    /// Режим визуального отображения кнопки профиля пользователя в тулбаре.
    /// </summary>
    public enum UserProfileDisplayMode
    {
        Default,            // Ник | Аватар (По умолчанию в игре)
        AvatarLeft,         // Аватар | Ник
        WithSeparator,      // Ник │ Аватар
        AvatarLeftWithSep,  // Аватар │ Ник
        AvatarOnly,         // Только аватар
        UsernameOnly        // Только никнейм
    }
}
