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

**Auto-skip breaks**
Automatically skips long mid-map break periods and seeks right back to gameplay lead-in.
Hides the skip progress bar during breaks while leaving song intros completely untouched.

**Interactive edit mode**
Right-click any empty spot on the toolbar to enter edit mode. Drag blocks around, add spacers
between button groups, or pick from ready-made layout presets (Vanilla, Centered, etc.).

**Settings & Customization**
- Toggle auto-break skip on or off in game settings
- Add custom spacers to group related buttons
- Quick preset switching and one-click reset to default layout
- Dynamic hot-reloading — changes apply instantly without restarting the game

### Install

1. Go to the [Releases](https://github.com/Meyblow/osu-tweaks/releases) tab
2. Download the latest `plugin-osu-tweaks.zip`
3. Drop it into your osu-cc plugins folder — `%APPDATA%\osu\osu-cc\plugins`
4. Restart the game, then right-click the toolbar or check `Ctrl+O` → Specials to configure

### Notes

- Layout configuration is saved locally as JSON (`layout.json`).
- The ruleset selector automatically hides on screens where changing game modes is disallowed (Results, Player, Editor).
- Requires osu!cc with plugin support.

---
**Meyblow** — [Telegram](https://t.me/Meyblow) · [osu! profile](https://osu.ppy.sh/users/39791134)
