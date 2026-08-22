using osu.Framework.Localisation;
using osucc.Localisation;

namespace OsuTweaks.Localisation
{
    public static class OsuTweaksStrings
    {
        private const string prefix = "osu-tweaks";
        private static string getKey(string name) => $"{prefix}:{name}";

        public static LocalisableString PluginName => OsuCcLocalisation.Get($"{prefix}:name", "osu!tweaks");
        public static LocalisableString PluginDescription => OsuCcLocalisation.Get($"{prefix}:description", "Collection of useful tweaks and UI customizations for osu!cc.");

        // Subsection Header
        public static LocalisableString Header => OsuCcLocalisation.Get(getKey(nameof(Header)), "osu!tweaks");

        // Section 1: Toolbar & Presets
        public static LocalisableString PresetDropdownLabel => OsuCcLocalisation.Get(getKey(nameof(PresetDropdownLabel)), "Layout Preset");
        public static LocalisableString DefaultPresetName => OsuCcLocalisation.Get(getKey(nameof(DefaultPresetName)), "Default (Vanilla)");
        public static LocalisableString ImportedPresetName => OsuCcLocalisation.Get(getKey(nameof(ImportedPresetName)), "Imported Layout");

        public static LocalisableString ButtonEnterEditMode => OsuCcLocalisation.Get(getKey(nameof(ButtonEnterEditMode)), "Customize Toolbar (Edit Mode)");
        public static LocalisableString ButtonSavePreset => OsuCcLocalisation.Get(getKey(nameof(ButtonSavePreset)), "Save Current Toolbar as Preset...");
        public static LocalisableString ButtonCopyCode => OsuCcLocalisation.Get(getKey(nameof(ButtonCopyCode)), "Copy Layout Code to Clipboard (Share)");
        public static LocalisableString ButtonImportCode => OsuCcLocalisation.Get(getKey(nameof(ButtonImportCode)), "Import Layout from Clipboard...");
        public static LocalisableString ButtonOpenPresetsFolder => OsuCcLocalisation.Get(getKey(nameof(ButtonOpenPresetsFolder)), "Open Presets Folder");
        public static LocalisableString ButtonResetToDefault => OsuCcLocalisation.Get(getKey(nameof(ButtonResetToDefault)), "Reset to Default Layout");

        // Section 2: Aesthetics
        public static LocalisableString FloatingIslandCheckbox => OsuCcLocalisation.Get(getKey(nameof(FloatingIslandCheckbox)), "Floating Toolbar Island (Dock)");
        public static LocalisableString BackgroundOpacitySlider => OsuCcLocalisation.Get(getKey(nameof(BackgroundOpacitySlider)), "Toolbar Background Opacity");
        public static LocalisableString ToolbarHeightSlider => OsuCcLocalisation.Get(getKey(nameof(ToolbarHeightSlider)), "Toolbar Height (Compact Mode)");
        public static LocalisableString NeonGlowLineCheckbox => OsuCcLocalisation.Get(getKey(nameof(NeonGlowLineCheckbox)), "Neon Glow Underline");
        public static LocalisableString NeonAccentColorDropdown => OsuCcLocalisation.Get(getKey(nameof(NeonAccentColorDropdown)), "Accent Glow Color");

        public static LocalisableString AccentPink => OsuCcLocalisation.Get(getKey(nameof(AccentPink)), "osu! Pink");
        public static LocalisableString AccentPurple => OsuCcLocalisation.Get(getKey(nameof(AccentPurple)), "Neon Purple");
        public static LocalisableString AccentCyan => OsuCcLocalisation.Get(getKey(nameof(AccentCyan)), "Cyberpunk Cyan");
        public static LocalisableString AccentLime => OsuCcLocalisation.Get(getKey(nameof(AccentLime)), "Emerald Lime");
        public static LocalisableString AccentGold => OsuCcLocalisation.Get(getKey(nameof(AccentGold)), "Gold");
        public static LocalisableString AccentWhite => OsuCcLocalisation.Get(getKey(nameof(AccentWhite)), "White");

        // Section 3: Clock & Date
        public static LocalisableString ClockFormatDropdown => OsuCcLocalisation.Get(getKey(nameof(ClockFormatDropdown)), "Time & Date Format");
        public static LocalisableString ClockFormatStandard => OsuCcLocalisation.Get(getKey(nameof(ClockFormatStandard)), "Standard with seconds (HH:mm:ss)");
        public static LocalisableString ClockFormatCompact => OsuCcLocalisation.Get(getKey(nameof(ClockFormatCompact)), "Compact without seconds (HH:mm)");
        public static LocalisableString ClockFormatWithDate => OsuCcLocalisation.Get(getKey(nameof(ClockFormatWithDate)), "With date (dd MMM · HH:mm)");
        public static LocalisableString ClockFormatWithDateAndSeconds => OsuCcLocalisation.Get(getKey(nameof(ClockFormatWithDateAndSeconds)), "With date and seconds (dd MMM · HH:mm:ss)");
        public static LocalisableString ClockFormatSessionOnly => OsuCcLocalisation.Get(getKey(nameof(ClockFormatSessionOnly)), "Session timer only");
        public static LocalisableString ShowSessionTimerCheckbox => OsuCcLocalisation.Get(getKey(nameof(ShowSessionTimerCheckbox)), "Show Session Timer");

        // Section 4: Spacers
        public static LocalisableString SpacerStyleDropdown => OsuCcLocalisation.Get(getKey(nameof(SpacerStyleDropdown)), "Spacer Style");
        public static LocalisableString SpacerBlank => OsuCcLocalisation.Get(getKey(nameof(SpacerBlank)), "Blank gap");
        public static LocalisableString SpacerLine => OsuCcLocalisation.Get(getKey(nameof(SpacerLine)), "Thin vertical line");
        public static LocalisableString SpacerDot => OsuCcLocalisation.Get(getKey(nameof(SpacerDot)), "Dot");

        // Section 5: User Profile
        public static LocalisableString ProfileModeDropdown => OsuCcLocalisation.Get(getKey(nameof(ProfileModeDropdown)), "Avatar & Username Layout");
        public static LocalisableString ProfileDefault => OsuCcLocalisation.Get(getKey(nameof(ProfileDefault)), "Default (Username | Avatar)");
        public static LocalisableString ProfileAvatarLeft => OsuCcLocalisation.Get(getKey(nameof(ProfileAvatarLeft)), "Avatar on left (Avatar | Username)");
        public static LocalisableString ProfileWithSeparator => OsuCcLocalisation.Get(getKey(nameof(ProfileWithSeparator)), "With separator (Username │ Avatar)");
        public static LocalisableString ProfileAvatarLeftWithSep => OsuCcLocalisation.Get(getKey(nameof(ProfileAvatarLeftWithSep)), "Avatar on left with separator (Avatar │ Username)");
        public static LocalisableString ProfileAvatarOnly => OsuCcLocalisation.Get(getKey(nameof(ProfileAvatarOnly)), "Avatar only");
        public static LocalisableString ProfileUsernameOnly => OsuCcLocalisation.Get(getKey(nameof(ProfileUsernameOnly)), "Username only");

        // Section 6: Gameplay
        public static LocalisableString AutoSkipDropdown => OsuCcLocalisation.Get(getKey(nameof(AutoSkipDropdown)), "Auto-Skip Breaks Mode");
        public static LocalisableString AutoSkipDisabled => OsuCcLocalisation.Get(getKey(nameof(AutoSkipDisabled)), "Disabled");
        public static LocalisableString AutoSkipBreaksOnly => OsuCcLocalisation.Get(getKey(nameof(AutoSkipBreaksOnly)), "Auto-skip mid-map breaks");
        public static LocalisableString AutoSkipAll => OsuCcLocalisation.Get(getKey(nameof(AutoSkipAll)), "Auto-skip all (intro, breaks, outro)");

        // Edit Mode Banner
        public static LocalisableString EditBannerHint => OsuCcLocalisation.Get(getKey(nameof(EditBannerHint)), "Edit Mode | Drag blocks to reorder | RMB: hide / reset | Esc: Save");
        public static LocalisableString EditBannerSaveButton => OsuCcLocalisation.Get(getKey(nameof(EditBannerSaveButton)), "Save & Exit");

        // Context Menu Items
        public static LocalisableString ContextMenuHide => OsuCcLocalisation.Get(getKey(nameof(ContextMenuHide)), "Hide this element");
        public static LocalisableString ContextMenuMoveLeft => OsuCcLocalisation.Get(getKey(nameof(ContextMenuMoveLeft)), "Move to: Left");
        public static LocalisableString ContextMenuMoveCenter => OsuCcLocalisation.Get(getKey(nameof(ContextMenuMoveCenter)), "Move to: Center");
        public static LocalisableString ContextMenuMoveRight => OsuCcLocalisation.Get(getKey(nameof(ContextMenuMoveRight)), "Move to: Right");
        public static LocalisableString ContextMenuResetBlock => OsuCcLocalisation.Get(getKey(nameof(ContextMenuResetBlock)), "Reset to default position");
        public static LocalisableString ContextMenuAddSpacer => OsuCcLocalisation.Get(getKey(nameof(ContextMenuAddSpacer)), "Add spacer (gap)");
        public static LocalisableString ContextMenuRemoveSpacer => OsuCcLocalisation.Get(getKey(nameof(ContextMenuRemoveSpacer)), "Delete spacer");

        // Block Friendly Names
        public static LocalisableString BlockSettings => OsuCcLocalisation.Get(getKey(nameof(BlockSettings)), "Settings");
        public static LocalisableString BlockHome => OsuCcLocalisation.Get(getKey(nameof(BlockHome)), "Home");
        public static LocalisableString BlockRulesets => OsuCcLocalisation.Get(getKey(nameof(BlockRulesets)), "Game Modes");
        public static LocalisableString BlockClock => OsuCcLocalisation.Get(getKey(nameof(BlockClock)), "Clock");
        public static LocalisableString BlockNotifications => OsuCcLocalisation.Get(getKey(nameof(BlockNotifications)), "Notifications");
        public static LocalisableString BlockRankings => OsuCcLocalisation.Get(getKey(nameof(BlockRankings)), "Rankings");
        public static LocalisableString BlockNews => OsuCcLocalisation.Get(getKey(nameof(BlockNews)), "News");
        public static LocalisableString BlockChangelog => OsuCcLocalisation.Get(getKey(nameof(BlockChangelog)), "Changelog");
        public static LocalisableString BlockWiki => OsuCcLocalisation.Get(getKey(nameof(BlockWiki)), "Wiki");
        public static LocalisableString BlockBeatmaps => OsuCcLocalisation.Get(getKey(nameof(BlockBeatmaps)), "Beatmap Listing");
        public static LocalisableString BlockChat => OsuCcLocalisation.Get(getKey(nameof(BlockChat)), "Chat");
        public static LocalisableString BlockSocial => OsuCcLocalisation.Get(getKey(nameof(BlockSocial)), "Social");
        public static LocalisableString BlockMusic => OsuCcLocalisation.Get(getKey(nameof(BlockMusic)), "Music");
        public static LocalisableString BlockUserProfile => OsuCcLocalisation.Get(getKey(nameof(BlockUserProfile)), "User Profile");
        public static LocalisableString BlockSpacer => OsuCcLocalisation.Get(getKey(nameof(BlockSpacer)), "Spacer");

        // Save Preset Dialog
        public static LocalisableString DialogSavePresetTitle => OsuCcLocalisation.Get(getKey(nameof(DialogSavePresetTitle)), "Save Toolbar Layout Preset");
        public static LocalisableString DialogSavePresetPrompt => OsuCcLocalisation.Get(getKey(nameof(DialogSavePresetPrompt)), "Enter a name for the new preset:");
        public static LocalisableString DialogSaveButton => OsuCcLocalisation.Get(getKey(nameof(DialogSaveButton)), "Save");
        public static LocalisableString DialogCancelButton => OsuCcLocalisation.Get(getKey(nameof(DialogCancelButton)), "Cancel");

        // Notifications
        public static LocalisableString NotifyClipboardCopied => OsuCcLocalisation.Get(getKey(nameof(NotifyClipboardCopied)), "Layout code copied to clipboard!");
        public static LocalisableString NotifyClipboardEmpty => OsuCcLocalisation.Get(getKey(nameof(NotifyClipboardEmpty)), "Clipboard is empty!");
        public static LocalisableString NotifyImportSuccess => OsuCcLocalisation.Get(getKey(nameof(NotifyImportSuccess)), "Toolbar layout successfully imported!");
        public static LocalisableString NotifyImportInvalid => OsuCcLocalisation.Get(getKey(nameof(NotifyImportInvalid)), "No valid layout code found in clipboard (OT_LAYOUT_v1:...)!");
        public static LocalisableString NotifyLayoutSaved => OsuCcLocalisation.Get(getKey(nameof(NotifyLayoutSaved)), "Toolbar layout saved");
        public static LocalisableString NotifyBlockReset(string name) => OsuCcLocalisation.Get(getKey(nameof(NotifyBlockReset)), "Block '{0}' restored to default position", name);
    }
}
