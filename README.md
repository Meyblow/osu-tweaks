# osu!tweaks

[English](README.md) | [Русский](README.ru.md)

A lightweight osu!cc plugin with quality-of-life gameplay tweaks and visual enhancements for osu!lazer.

> [!NOTE]
> Looking for modular toolbar customization (Drag & Drop layout, floating island dock, geometry sliders, profile styling)?
> Those features have moved to [**Extended Toolbar**](https://github.com/Meyblow/extended-toolbar)!

---

## ✨ Features

### ⏩ Smart State Machine Auto-Skip
Automatically skips non-gameplay downtime during beatmaps so you can jump right into the action without manual interaction:
- **Intro Skip**: Skips the lead-in period before the first hit object, landing smoothly 2 seconds before the first note.
- **Mid-map Break Skip**: Detects beatmap pause sections and fast-forwards through long breaks, resuming playback 2 seconds before the next hit object.
- **Outro Skip**: Fast-forwards long outro sections after the last note directly to the results screen.
- **Configurable Modes**:
  - `Disabled`: No auto-skipping.
  - `Breaks only`: Skips only mid-map pauses.
  - `Intro only`: Skips only the start of the map.
  - `Intro & breaks`: Skips both intros and mid-map breaks.
  - `All (Intro, breaks & outro)`: Fully automated gameplay skip.

### 🌙 Dark Intro Flash on Startup
Eliminates the blinding full-screen white flash (`GameWideFlash`) when launching osu!lazer, replacing it with a soft dark fade to protect your eyes in dark environments.

---

## 📦 Installation

1. Go to the [Releases](https://github.com/Meyblow/osu-tweaks/releases) tab.
2. Download `OsuTweaks.dll` (or `plugin-osu-tweaks-1.4.0.zip`).
3. Place the file into your osu!cc plugins folder:
   - **Windows**: `%APPDATA%\osu\osu-cc\plugins\osu-tweaks\`
   - **Linux / NixOS**: `~/.local/share/osu/osu-cc/plugins/osu-tweaks/`
4. Launch the game and configure options in **Settings → osu!tweaks**.

---

## 🛠️ Building

```bash
dotnet build -c Release
```

---

## 📜 License

MIT License. See [LICENSE](LICENSE) for details.

**Meyblow** — [Telegram](https://t.me/Meyblow) · [osu! profile](https://osu.ppy.sh/users/39791134)
