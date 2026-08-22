# Changelog

All notable changes to osu!tweaks are documented in this file.

## [1.0.7] - 2026-08-22

### Fixed
- **FillFlowContainer Anchor Alignment Crash**: Fixed `InvalidOperationException: All drawables in a FillFlowContainer must use the same RelativeAnchorPosition for the given FillDirection(Horizontal) (0 != 0.5)` by maintaining uniform `Anchor.CentreLeft` across all children and centering the container via `targetFlow.Anchor = Anchor.Centre`.

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
