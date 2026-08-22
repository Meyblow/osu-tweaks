# Changelog

All notable changes to osu!tweaks are documented in this file.

## [1.3.0] - 2026-08-22

### Fixed
- **Fatal Crash in Clock FillFlowContainer**: Fixed `System.InvalidOperationException: All drawables in a FillFlowContainer must use the same RelativeAnchorPosition for the given FillDirection(Vertical)` by synchronizing all child anchors to `Anchor.TopLeft`.
- **Floating Island Horizontal Displacement**: Fixed the floating toolbar shifting to the right on the X axis by anchoring it symmetrically to `Anchor.TopCentre` with `Origin = TopCentre`, ensuring perfect center alignment and clean reset.
- **Native Localization in All Dropdowns**: Switched all settings dropdowns to `SettingsEnumDropdown<T>` with `[LocalisableDescription]`, allowing options (`Accent Glow Color`, `Time & Date Format`, `Spacer Style`, `Avatar & Username Layout`, `Auto-Skip Mode`) to automatically render in English or Russian based on the game's active language.
- **Drag Ghost & Spacer Badges**: Localized drag preview badges and spacer context menu labels.

---

## [1.2.4] - 2026-08-22

### Fixed
- **SDK Metadata Synchronization**: Internal build synchronization for package metadata.

---

## [1.2.2] - 2026-08-22

### Fixed
- **Synchronized SDK Packages in local_feed**: Updated all `osucc.Api`, `osucc.Build`, and `osucc.Shared` nupkg binaries in `local_feed/` to the latest revision, ensuring GitHub Actions and local builds generate exact matching metadata and eliminating `MissingMethodException` across all platforms.

---

## [1.2.1] - 2026-08-22

### Fixed
- **Settings Slider Type Safety**: Fixed `InvalidCastException: Unable to cast object of type 'Bindable<Single>' to type 'BindableNumber<Single>'` when opening the settings subsection by providing properly bounded `BindableFloat` instances with `MinValue`, `MaxValue`, and `Precision` to `SettingsSlider<float>` controls.

---

## [1.2.0] - 2026-08-22

### Added
- **Floating Island Toolbar**: Turn your top toolbar into a floating dock with `CornerRadius = 12px`, top/side margins, and shadow effects.
- **Background Opacity Slider**: Smooth 0% to 100% background transparency slider allowing buttons to float cleanly above game backgrounds.
- **Compact Toolbar Height**: Configurable toolbar height (26px – 40px) saving screen space on laptops and small monitors.
- **Neon Glow Underline**: Accent line beneath the toolbar with custom colors (osu! Pink, Neon Purple, Cyberpunk Cyan, Emerald Lime, Gold, White).
- **Toolbar Clock Customization**: Formats for standard seconds (`HH:mm:ss`), compact without seconds (`HH:mm`), with date (`dd MMM · HH:mm`), with date and seconds (`dd MMM · HH:mm:ss`), and in-game session timer.
- **Spacer Styles**: Choose between blank gap, thin vertical line (`│`), and minimal dot (`•`) for toolbar spacers.
- **Preset Code Sharing**: Export (`OT_LAYOUT_v1:...`) and import toolbar layouts via clipboard with one click.
- **Reset Single Block**: Right-click any block in edit mode → *«Вернуть на стандартное место»* to restore its default position.
