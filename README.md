# osu!tweaks

osu!cc plugin packed with modular UI tweaks and quality-of-life improvements for osu!lazer.
Rearrange your top toolbar, hide clutter, and automatically skip breaks mid-map.

Made this to clean up the default lazer UI and make the toolbar actually customizable.

### Features

**Modular toolbar**
Completely rearrange any element on the top bar via drag and drop. Move ruleset selectors,
profile buttons, overlays, and plugin icons between Left, Center, and Right zones.

**Hide unused buttons**
Click to hide any icon you don't use. Hidden buttons stay accessible through their native
game hotkeys (`Ctrl+O`, `F8`, `F9`, etc.), so you keep full functionality with zero visual clutter.

**Custom Presets & VFS Storage**
Save custom toolbar layouts with a named preset dialog, switch between presets on the fly,
and access preset files directly through osu!cc Virtual File System (`osucc.Data.IOsuCcStorage`).

**Profile Button Customization**
Customize your top bar user button layout: Avatar Left, Avatar Right, With Separator,
Avatar Only, or Username Only (with automatic centering).

**Auto-skip breaks**
Automatically skips long mid-map break periods and seeks right back to gameplay lead-in.
Configurable to skip breaks only or intros/outros as well.

**Interactive edit mode**
Right-click any empty spot on the toolbar to enter edit mode. Drag blocks around, add spacers
between button groups, or pick from ready-made layout presets (Vanilla, Centered, etc.).

### Install

1. Go to the [Releases](https://github.com/Meyblow/osu-tweaks/releases) tab
2. Download the latest `plugin-osu-tweaks-1.1.0.zip`
3. Place the `.zip` file into your osu!cc plugins folder:
   - **Windows**: `%APPDATA%\osu\osu-cc\plugins\`
   - **Linux / NixOS**: `~/.local/share/osu/osu-cc/plugins/`
4. *Important*: If upgrading from an older version, delete any old unpacked folders like `plugins/OsuTweaks/`.
5. Restart the game, then right-click the toolbar or check Settings → osu!tweaks to configure.

### Notes

- Layout configuration and custom presets are stored securely in osu!cc storage.
- The ruleset selector automatically hides on screens where changing game modes is disallowed (Results, Player, Editor).
- Requires osu!cc 2.2.0+.

---
**Meyblow** — [Telegram](https://t.me/Meyblow) · [osu! profile](https://osu.ppy.sh/users/39791134)
