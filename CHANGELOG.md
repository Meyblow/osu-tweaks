# Changelog

All notable changes to osu!tweaks are documented in this file.

## [1.4.1] - 2026-08-23

### Added
- **Mid-map Break Auto-Skip**: Restored and improved automatic skipping of mid-map pauses with customizable modes (`Breaks only`, `Intro only`, `Intro & breaks`, `All`).
- **GameWideFlash Prefix Neutralization**: Instant zero-alpha suppression on `GameWideFlash` for seamless dark startup transition.

---

## [1.4.0] - 2026-08-23

### Changed
- **Modular Toolbar Moved to Separate Plugin**: All toolbar customization features (drag & drop layout, floating island dock, geometry sliders, profile button styling, and presets) have been moved to a dedicated plugin: [**Extended Toolbar**](https://github.com/Meyblow/extended-toolbar). Install it separately if you used those features.
- **State Machine Auto-Skip**: Reimplemented auto-skipping for intros, mid-map breaks, and outros using a robust state machine (`TweaksAutoSkipper`) with configurable modes (`Breaks only`, `Intro only`, `Intro & breaks`, `All`).

### Added
- **Dark Intro Flash on Startup**: Eliminates the blinding white additive intro flash when launching the game via a direct Prefix patch on `GameWideFlash`.

---

## [1.3.3] - 2026-08-23

### Fixed
- **Edit Mode Toolbar Interaction**: Fixed positional input queue propagation in `ToolbarBlockContainer` so that clicking, dragging, and context menu actions on toolbar buttons and game icons work smoothly in Edit Mode.
- **Native osu! Button Shielding**: Refined input shielding so native osu! buttons are not inadvertently triggered while rearranging or toggling blocks.

### Added
- **Profile Stats Position (Left / Right)**: Added setting to configure whether rank (#) and PP performance stats slide in from the right or left of the user profile button.

---

## [1.3.2] - 2026-08-23

### Fixed
- **Accidental Block Hiding in Edit Mode**: Removed left-click toggle on `IsHidden` so dragging and clicking blocks in Edit Mode never accidentally hides buttons or turns them red. Hiding and showing blocks is now exclusively done via the context menu (RMB).
- **Auto-Skip Breaks Reliability**: Decoupled `TweaksBreakAutoSkipper` from unset host configuration flags, ensuring breaks and intros are reliably auto-skipped during gameplay according to the user's `Auto-Skip Breaks Mode` setting.
- **Screen Transition Edit Mode Auto-Close**: Edit Mode now automatically saves and closes upon entering song select or gameplay screens.

---

## [1.3.1] - 2026-08-23

### Added
- **Toolbar Corner Radius Slider**: Configurable corner rounding (0px to 24px) for the Floating Island toolbar dock.
- **Neon Glow Underline Offset Slider**: Configurable vertical offset (-5px to +15px) to position the accent glow line perfectly beneath the toolbar.
- **Dark Intro Flash on Startup**: Silences the blinding additive white flash (`GameWideFlash`) when launching the game, replacing it with a soft dark fade.
- **Persistent Toolbar Spacers**: Added automatic deserialization and state persistence for spacers in presets and layout configs.
- **Top Priority Spacer Action**: Right-clicking in Edit Mode now shows `+ Add spacer (gap)` at the very top of the menu for faster workflow.

### Fixed
- **Startup Button Hover Glow**: Removed unintended background alpha mutation on toolbar button child boxes, fixing permanent button highlights on launch.
- **Restored Native Toolbar Clock**: Completely removed invasive clock injection overlays to restore 100% native osu! clock functionality, animations, click mode toggling, and clean vertical alignment.
- **Edit Mode Ghost Clipping**: Moved drag ghost container to the root game layer to prevent masking/cropping when dragging items beyond toolbar borders.
- **Edit Mode Subtree Hover**: Disabled button hover propagation in Edit Mode to prevent internal button animations and sounds while dragging.
- **Gameplay Edit Mode Auto-Exit**: Automatically closes and saves Edit Mode when entering gameplay or results screens.
- **Default Preset Protection**: Renamed default preset to `Default` and protected it from accidental overwrites.
- **Context Menu Input Leakage**: Blocked hover and scroll events from passing through context menus to background buttons.
- **Context Menu Localization**: Replaced hardcoded text with proper `OsuTweaksStrings` keys.
