# osu!tweaks

[English](README.md) | [Русский](README.ru.md)

A lightweight osu!cc plugin with quality-of-life gameplay tweaks, focus enhancements, and audio/visual polish for osu!lazer.

> [!NOTE]
> Looking for modular toolbar customization (Drag & Drop layout, floating island dock, geometry sliders, profile styling)?
> Those features have moved to [**Extended Toolbar**](https://github.com/Meyblow/extended-toolbar)!

---

## ✨ Features

### 🎮 Gameplay & Restart
- **⏩ Smart State Machine Auto-Skip**:
  Automatically skips non-gameplay downtime during beatmaps (intros, mid-map breaks, and outros) so you can jump right into the action without manual interaction.
  - Modes: `Disabled`, `Breaks only`, `Intro only`, `Intro & breaks`, `All`.
- **⚡ Instant Quick-Retry (Zero-Delay Restart)**:
  Eliminates transition delays when pressing `Ctrl+R` or restarting a map, dropping you right back into the beatmap in 1 frame.
- **🔇 Silent Fail Sound**:
  Suppresses the harsh fail audio on death, making retrying smooth and quiet.

### 🎯 Visual & Focus
- **🌙 Dark Intro Flash on Startup**:
  Eliminates the blinding full-screen white flash (`GameWideFlash`) when launching osu!lazer, replacing it with a soft dark fade.
- **👓 Minimalist HUD (Clean Gameplay)**:
  Automatically hides HP bar, score, progress bar, combo counter, and mod icons during active gameplay, keeping only notes and the Hit Error Bar visible (restores instantly on pause).
- **🛡️ Disable Screen Shake & Red Flash on Low HP**:
  Stops the screen from shaking and eliminates red pulsating vignette when health is critical, maintaining pure aim stability.
- **🌈 Custom Star Rating Gradients**:
  Customize the star difficulty colors across Song Select:
  - `Vanilla`: Default osu!lazer spectrum.
  - `Classic osu!stable`: Iconic Blue $\to$ Green $\to$ Yellow $\to$ Orange $\to$ Red $\to$ Purple $\to$ Black palette.
  - `Cyber Neon`: High-contrast electric cyberpunk gradient.
  - `Soft Pastel`: Calming aesthetic pastel shades.

### 🎵 Audio & Song Select
- **🔉 Song Preview Volume Limiter**:
  Set a maximum volume ceiling for beatmap previews in Song Select (10% to 100%) to protect your ears against unexpectedly loud songs.

---

## 📦 Installation

1. Go to the [Releases](https://github.com/Meyblow/osu-tweaks/releases) tab.
2. Download `OsuTweaks.dll` (or `plugin-osu-tweaks-1.5.0.zip`).
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
