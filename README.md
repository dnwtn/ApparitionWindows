# 👻 Apparition Window Manager

A lightweight Windows desktop utility for controlling window transparency, z-order, and click-through behavior — all from a sleek floating icon widget.

![Platform](https://img.shields.io/badge/platform-Windows-blue)
![Framework](https://img.shields.io/badge/.NET%20Framework-4.5-purple)
![Language](https://img.shields.io/badge/language-C%23-green)

---

## ✨ Features

- **Window Transparency Control** — Adjust the opacity of any open window from ~47% to 100% using a simple slider.
- **Window Modes** — Set any window to one of three modes:
  | Mode | Behavior |
  |------|----------|
  | **Normal** | Default window behavior |
  | **Top Most** | Window stays above all others |
  | **Ghost** | Always-on-top *and* click-through (mouse events pass through the window) |
- **Quick Toggle** — Double-click any window in the list to instantly toggle between full opacity and your configured quick-transparency level.
- **Floating Icon Widget** — A tiny, draggable desktop icon that fades to a configurable transparency when idle. Hover to reveal the control panel.
- **Auto-Refresh** — The window list updates automatically when processes start or stop (requires admin privileges).
- **Restore on Close** — Optionally reset all modified windows back to their normal state when Apparition exits.
- **Single Instance** — Only one instance of Apparition can run at a time.

---

## 🖥️ How It Works

Apparition runs as a small **55×55 pixel floating icon** on your desktop — no taskbar clutter.

1. **Hover** over the icon to reveal the main control panel.
2. **Select** a window from the list to bring it to the foreground.
3. **Adjust transparency** with the slider, or **change the mode** with the dropdown.
4. **Double-click** a window for a quick transparency toggle.
5. **Right-click** the icon to access Settings or close the app.
6. **Drag** the icon anywhere on your desktop — its position is saved between sessions.

The panel automatically fades out when you click away, keeping your workspace clean.

---

## ⚙️ Settings

| Setting | Description | Default |
|---------|-------------|---------|
| **Restore on Close** | Reset all windows to normal state when Apparition exits | `false` |
| **Quick Transparency** | Opacity percentage used for the double-click toggle | `50%` |
| **Icon Transparency** | Idle opacity of the floating desktop icon | `50%` |

Settings are persisted automatically between sessions.

---

## 🏗️ Architecture

```
AppartionWM/
├── Program.cs                    # Entry point, single-instance check
├── ApparitionIcon.cs             # Floating desktop icon widget
├── Apparition.cs                 # Main control panel (window list, slider, mode selector)
├── Apparition.Constants.cs       # Win32 constants (window styles, show commands)
├── Apparition.Imports.cs         # Win32 P/Invoke declarations & helper methods
├── Apparition.ProcessWatcher.cs  # WMI-based process start/stop monitoring
├── OpenWindowGetter.cs           # Enumerates open windows via EnumWindows
├── Settings.cs                   # Settings form with swappable panels
├── Controls/
│   └── SettingsPanel.cs          # User control for editing preferences
└── resources/                    # Icons and other assets
```

The `Apparition` class is split across multiple partial class files for organization:

- **`Apparition.cs`** — Core UI logic: loading processes, handling selection, opacity changes, mode switching, fade animations
- **`Apparition.Constants.cs`** — Win32 API constants
- **`Apparition.Imports.cs`** — P/Invoke signatures (`SetWindowLong`, `SetLayeredWindowAttributes`, `SetWindowPos`, etc.) and helper methods (`makeTransparent`, `makeNormal`), plus custom `WndProc` for borderless window resizing
- **`Apparition.ProcessWatcher.cs`** — WMI event watchers for automatic window list refresh

---

## 🔧 Building

### Prerequisites

- Visual Studio 2012 or later
- .NET Framework 4.5

### Build Steps

1. Open `ApparitionWM.sln` in Visual Studio.
2. Build the solution (`Ctrl+Shift+B`).
3. The output executable `Apparitions.exe` will be in `AppartionWM/bin/Debug/` or `AppartionWM/bin/Release/`.

---

## 📝 Notes

- **Admin privileges** are recommended for the auto-refresh feature (WMI process watching). The app will still function without admin rights, but the window list won't update automatically.
- The "Ghost" mode uses the `WS_EX_TRANSPARENT` extended window style, which makes the target window completely click-through — mouse events pass through to whatever is behind it.
- The floating icon uses `TransparencyKey` for its rounded appearance, rendering on a `LightGray` transparent background.

---

## 📄 License

This project is provided as-is for personal use.
