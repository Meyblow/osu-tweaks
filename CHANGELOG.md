# Changelog

All notable changes to osu!tweaks are documented in this file.

## [1.2.2] - 2026-08-22

### Fixed
- **Synchronized SDK Packages in local_feed**: Updated all `osucc.Api`, `osucc.Build`, and `osucc.Shared` nupkg binaries in `local_feed/` to the latest revision, ensuring GitHub Actions and local builds generate exact matching metadata and eliminating `MissingMethodException: Method not found: 'Void osucc.Plugin.OsuCcPluginAttribute..ctor(String, String, Int32, Int32)'` forever across all platforms.

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

---

## [1.1.1] - 2026-08-22

### Fixed
- **Reset to Default Hierarchy Safety**: Fixed `InvalidOperationException: May not add a drawable to multiple containers` when clicking "Reset to default layout" by applying the vanilla preset directly through the modular zone layout pipeline without attempting to dismantle existing drawables.

---

## [1.1.0] - 2026-08-22

### Added
- **Persistent Preset Selection**: Added persistent `ActivePresetName` bindable setting so the active preset name is saved across game restarts and properly displayed in the settings dropdown.

### Fixed
- **Plugin Metadata Binary Compatibility**: Ensured strict 3-parameter constructor metadata binding `[assembly: OsuCcPlugin(id, name, priority)]` across all builds to prevent `MissingMethodException` on Linux / NixOS / legacy installations.
- **Legacy Folder Cleanup Note**: Added instructions to delete obsolete unpacked `plugins/OsuTweaks/` directories when updating to modern `.zip` distribution.

---

## [1.0.9] - 2026-08-22

### Changed
- **Settings Spacing & Padding**: Added 6px vertical spacing between action buttons and 10px section separation in the settings subsection so buttons are clearly separated instead of merged into a single block.

---

## [1.0.8] - 2026-08-22

### Fixed
- **User Profile Statistics Overlap**: Fixed visual overlap between `Global Ranking` and `Performance` by retrieving the exact `avatar` and `usernameText` fields directly from `ToolbarUserButton` without modifying or traversing the `UserStatisticsDisplay` sub-tree.
- **Thread Safety**: Wrapped profile UI mutation logic in `Scheduler.AddOnce` for instant update synchronization.

---

## [1.0.7] - 2026-08-22

### Fixed
- **FillFlowContainer Anchor Alignment Crash**: Fixed `InvalidOperationException: All drawables in a FillFlowContainer must use the same RelativeAnchorPosition for the given FillDirection(Horizontal) (0 != 0.5)` by maintaining uniform `Anchor.CentreLeft` across all children.

---

## [1.0.6] - 2026-08-22

### Added
- **Plugin Document**: Added in-game `Changelog` tab directly inside the osu!cc plugin manager.
- **Centering for Profile Button**: When avatar is hidden (`UsernameOnly`), the username text is centered within the button. When username is hidden (`AvatarOnly`), the avatar is centered.

### Changed
- **Vanilla Settings UI**: Completely redesigned the settings subsection to 100% match standard osu!lazer settings typography, using native `SettingsButton` and `SettingsDropdown` without external fonts or neon headers.

### Fixed
- **Profile Display Layout Exception**: Fixed `InvalidOperationException: Cannot change layout position of drawable which is not contained within this FlowContainer` by safely resolving the direct child container of the user profile button before adjusting layout positions.
- **ObjectDisposedException Lifecycle Safety**: Added comprehensive `if (IsDisposed) return;` guards across all drawables, screen stack event handlers, drag-and-drop managers, and delayed schedulers.

---

## [1.0.5] - 2026-08-22

### Fixed
- **Host.Data NullReferenceException**: Resolved lifecycle timing issue where `Host.Data` was accessed during early `OnLoad()` before storage initialization, moving initialization safely to `AttachToGame()`.

---

## [1.0.4] - 2026-08-22

### Added
- **IOsuCcStorage Integration**: Presets and toolbar configurations now use the native osu!cc Virtual File System (`osucc.Data.IOsuCcStorage`).
- **Save Preset Modal Dialog**: Added in-game modal popup (`SavePresetDialog`) to save the active toolbar layout as a named `.json` preset.
- **Open Presets Folder**: Added action to instantly open the preset directory in Windows Explorer.
- **User Profile Display Modes**: Added customization for player avatar and username (Default, Avatar Left, With Separator, Avatar Left with Separator, Avatar Only, Username Only).

### Fixed
- **Hidden Icon Spacing Gaps**: Fixed unwanted 50px spacing gaps for hidden icons and screen-hidden ruleset selectors by zeroing out container width (`Width = 0` + `AutoSizeAxes = None`) while retaining hotkeys (`Ctrl+O`, `F8`, `F9`).

---

## [1.0.3] - 2026-08-22

### Added
- **GitHub Actions Automation**: Automated release workflow that builds, packages, and attaches plugin `.zip` files to GitHub releases upon pushing `v*` tags.

---

## [1.0.2] - 2026-08-22

### Fixed
- **OsuCcPluginAttribute Constructor Compatibility**: Switched local feed references to match the 3-argument constructor signature expected by osu!cc 2.2.0 runtime, fixing `MissingMethodException`.

---

## [1.0.1] - 2026-08-22

### Added
- **Draggable Edit Banner**: Toolbar edit mode floating banner can now be dragged to any screen position.
- **Keyboard Shortcuts Preservation**: Maintained global hotkey support for hidden toolbar buttons via `AlwaysPresent` sub-tree propagation.

---

## [1.0.0] - 2026-08-21

### Added
- **Modular Toolbar Manager**: Full drag-and-drop customization of top toolbar buttons across Left, Center, and Right zones.
- **Context Menu**: Right-click toolbar or buttons to enter edit mode, toggle visibility, add spacers, and reset layouts.
- **Auto-Skip Breaks**: Configurable mid-map break and intro/outro auto-skipping during gameplay.
