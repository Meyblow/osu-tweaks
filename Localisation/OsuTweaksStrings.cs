using osu.Framework.Localisation;
using osucc.Localisation;

namespace OsuTweaks.Localisation
{
    public static class OsuTweaksStrings
    {
        private const string prefix = "osu-tweaks";
        private static string getKey(string name) => $"{prefix}:{name}";

        public static LocalisableString PluginName => OsuCcLocalisation.Get($"{prefix}:name", "osu!tweaks");
        public static LocalisableString PluginDescription => OsuCcLocalisation.Get($"{prefix}:description", "Collection of useful tweaks and quality-of-life enhancements for osu!cc.");

        // Subsection Header
        public static LocalisableString Header => OsuCcLocalisation.Get(getKey(nameof(Header)), "osu!tweaks");

        // Gameplay & Visual Tweaks
        public static LocalisableString DarkIntroFlashCheckbox => OsuCcLocalisation.Get(getKey(nameof(DarkIntroFlashCheckbox)), "Dark Intro Flash on Startup");
        public static LocalisableString AutoSkipDropdown => OsuCcLocalisation.Get(getKey(nameof(AutoSkipDropdown)), "Auto-Skip Mode (Intro & Outro)");
        public static LocalisableString AutoSkipDisabled => OsuCcLocalisation.Get(getKey(nameof(AutoSkipDisabled)), "Disabled");
        public static LocalisableString AutoSkipIntroOnly => OsuCcLocalisation.Get(getKey(nameof(AutoSkipIntroOnly)), "Auto-skip intro");
        public static LocalisableString AutoSkipAll => OsuCcLocalisation.Get(getKey(nameof(AutoSkipAll)), "Auto-skip intro and outro");
    }
}
