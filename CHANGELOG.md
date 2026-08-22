# Changelog

All notable changes to osu!tweaks are documented in this file.

## [1.2.3] - 2026-08-22

### Added
- **Full Russian & English Localization**: Integrated `OsuCcLocalisation` and embedded `Localisation/ru.json` so all settings, tooltips, buttons, dialogs, banners, and context menus automatically match the game's selected language in osu! settings.

### Fixed
- **Clock Flickering & Desync**: Eliminated second-by-second clock text flickering by replacing vanilla text rendering with a dedicated custom clock layer, preventing race conditions with osu!lazer's native `DigitalClockDisplay`.
- **Toolbar Position & Reset**: Guaranteed precise top alignment (`Margin = 0`, `Y = 0`) when floating island is disabled, preventing unwanted offsets.
- **Preset Cleanliness**: Removed obsolete bundled presets (`Centered`), retaining only `Default (Vanilla)` so users can create, save, and manage their own personalized presets cleanly.

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
