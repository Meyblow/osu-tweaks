using osu.Framework.Localisation;
using osucc.Localisation;

namespace OsuTweaks.Localisation
{
    public static class OsuTweaksStrings
    {
        private const string prefix = "osu-tweaks";
        private static string getKey(string name) => $"{prefix}:{name}";

        public static LocalisableString PluginName => OsuCcLocalisation.Get($"{prefix}:name", "osu!tweaks");
        public static LocalisableString PluginDescription => OsuCcLocalisation.Get($"{prefix}:description", "Collection of useful gameplay tweaks, QoL, and visual enhancements for osu!cc.");

        // Subsection Headers
        public static LocalisableString Header => OsuCcLocalisation.Get(getKey(nameof(Header)), "osu!tweaks");
        public static LocalisableString SectionGameplay => OsuCcLocalisation.Get(getKey(nameof(SectionGameplay)), "Gameplay & Restart");
        public static LocalisableString SectionVisual => OsuCcLocalisation.Get(getKey(nameof(SectionVisual)), "Visual & Focus");
        public static LocalisableString SectionAudio => OsuCcLocalisation.Get(getKey(nameof(SectionAudio)), "Audio & Song Select");

        // 1. Auto-Skip
        public static LocalisableString AutoSkipDropdown => OsuCcLocalisation.Get(getKey(nameof(AutoSkipDropdown)), "Auto-Skip Mode");
        public static LocalisableString AutoSkipDisabled => OsuCcLocalisation.Get(getKey(nameof(AutoSkipDisabled)), "Disabled");
        public static LocalisableString AutoSkipBreaksOnly => OsuCcLocalisation.Get(getKey(nameof(AutoSkipBreaksOnly)), "Breaks only");
        public static LocalisableString AutoSkipIntroOnly => OsuCcLocalisation.Get(getKey(nameof(AutoSkipIntroOnly)), "Intro only");
        public static LocalisableString AutoSkipIntroAndBreaks => OsuCcLocalisation.Get(getKey(nameof(AutoSkipIntroAndBreaks)), "Intro & breaks");
        public static LocalisableString AutoSkipAll => OsuCcLocalisation.Get(getKey(nameof(AutoSkipAll)), "All (Intro, breaks & outro)");

        // 2. Instant Quick-Retry & Results
        public static LocalisableString InstantQuickRetryCheckbox => OsuCcLocalisation.Get(getKey(nameof(InstantQuickRetryCheckbox)), "Instant Quick-Retry (Zero-Delay Restart)");
        public static LocalisableString SkipResultsAnimationCheckbox => OsuCcLocalisation.Get(getKey(nameof(SkipResultsAnimationCheckbox)), "Skip Results Screen Animations (Instant statistics)");

        // 3. Silent Fail Sound
        public static LocalisableString SilentFailSoundCheckbox => OsuCcLocalisation.Get(getKey(nameof(SilentFailSoundCheckbox)), "Silent Fail Sound (Mute on death)");

        // 4. Dark Intro Flash & Skip Intro
        public static LocalisableString DarkIntroFlashCheckbox => OsuCcLocalisation.Get(getKey(nameof(DarkIntroFlashCheckbox)), "Dark Intro Flash on Startup");
        public static LocalisableString SkipStartupIntroCheckbox => OsuCcLocalisation.Get(getKey(nameof(SkipStartupIntroCheckbox)), "Skip Startup Intro (Circles, Triangles & Welcome)");

        // 5. Minimalist HUD
        public static LocalisableString MinimalistHUDCheckbox => OsuCcLocalisation.Get(getKey(nameof(MinimalistHUDCheckbox)), "Minimalist HUD (Clean Gameplay)");

        // 6. Disable Low HP Shake
        public static LocalisableString DisableLowHealthShakeCheckbox => OsuCcLocalisation.Get(getKey(nameof(DisableLowHealthShakeCheckbox)), "Disable Screen Shake & Red Flash on Low HP");

        // 7. Star Rating Palette
        public static LocalisableString StarRatingPaletteDropdown => OsuCcLocalisation.Get(getKey(nameof(StarRatingPaletteDropdown)), "Star Rating Color Palette");
        public static LocalisableString StarRatingVanilla => OsuCcLocalisation.Get(getKey(nameof(StarRatingVanilla)), "Vanilla (Default)");
        public static LocalisableString StarRatingClassicStable => OsuCcLocalisation.Get(getKey(nameof(StarRatingClassicStable)), "Classic osu!stable");
        public static LocalisableString StarRatingNeon => OsuCcLocalisation.Get(getKey(nameof(StarRatingNeon)), "Cyber Neon");
        public static LocalisableString StarRatingPastel => OsuCcLocalisation.Get(getKey(nameof(StarRatingPastel)), "Soft Pastel");

        // 8. Song Preview Volume Limiter
        public static LocalisableString PreviewVolumeLimiterSlider => OsuCcLocalisation.Get(getKey(nameof(PreviewVolumeLimiterSlider)), "Song Select Preview Max Volume Limit");
    }
}
