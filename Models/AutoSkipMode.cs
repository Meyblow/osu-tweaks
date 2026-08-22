using System.ComponentModel;

namespace OsuTweaks.Models
{
    /// <summary>
    /// Режимы работы автоскипа для osu!tweaks.
    /// </summary>
    public enum AutoSkipMode
    {
        [Description("Выкл")]
        Disabled,

        [Description("Автоскип мид-мап брейков")]
        BreaksOnly,

        [Description("Автоскип всего (интро, брейки, аутро)")]
        All
    }
}
