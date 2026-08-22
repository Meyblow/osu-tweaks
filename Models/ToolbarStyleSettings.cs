namespace OsuTweaks.Models
{
    /// <summary>
    /// Форматы отображения часов на тулбаре.
    /// </summary>
    public enum ClockDisplayFormat
    {
        StandardWithSeconds,   // HH:mm:ss
        CompactNoSeconds,      // HH:mm
        WithDate,              // dd MMM · HH:mm
        WithDateAndSeconds,    // dd MMM · HH:mm:ss
        SessionTimerOnly       // ⏳ 1ч 24м
    }

    /// <summary>
    /// Стили отображения визуальных разделителей (спейсеров).
    /// </summary>
    public enum SpacerStyle
    {
        Blank,  // Невидимый пустой зазор
        Line,   // Тонкая вертикальная разделительная линия
        Dot     // Минималистичный маркер-точка
    }

    /// <summary>
    /// Акцентные цвета подсветки тулбара и неоновых элементов.
    /// </summary>
    public enum ToolbarAccentColor
    {
        Pink,       // #FF66AA (osu! Pink)
        Purple,     // #AA55FF (Neon Purple)
        Cyan,       // #00DDFF (Cyberpunk Cyan)
        Lime,       // #55EE77 (Emerald Lime)
        Gold,       // #FFCC22 (Warm Gold)
        White       // #FFFFFF (Clean White)
    }
}
