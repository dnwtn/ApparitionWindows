# 👻 Apparition Window Manager

A modern Windows desktop utility for controlling window transparency, z-order, and click-through behavior — accessible from a sleek floating icon widget or the system tray.

![Platform](https://img.shields.io/badge/platform-Windows-blue)
![Framework](https://img.shields.io/badge/.NET-8.0-purple)
![UI](https://img.shields.io/badge/UI-WPF-green)
![Architecture](https://img.shields.io/badge/pattern-MVVM-orange)

---

## ✨ Features

### Core
- **Window Transparency Control** — Adjust the opacity of any open window using a smooth slider.
- **Window Modes** — Set any window to one of three modes:
  | Mode | Behavior |
  |------|----------|
  | **Normal** | Default window behavior |
  | **Top Most** | Window stays above all others |
  | **Ghost** | Always-on-top *and* click-through (mouse events pass through) |
- **Quick Toggle** — Double-click any window to instantly toggle between full opacity and your configured quick-transparency level.

### New
- **🎹 Global Hotkeys** — Control windows from anywhere without opening the panel:
  | Hotkey | Action |
  |--------|--------|
  | `Ctrl+Shift+G` | Toggle Ghost mode on foreground window |
  | `Ctrl+Shift+T` | Toggle TopMost on foreground window |
  | `Ctrl+Shift+O` | Quick opacity toggle on foreground window |
  | `Ctrl+Shift+A` | Show/hide the control panel |
- **💾 Per-Window Presets** — Save transparency + mode combos and reapply them instantly.
- **🔔 System Tray Icon** — Full functionality accessible from the system tray, not just the floating icon.
- **🖥️ Multi-Monitor Awareness** — Panel positioning adapts to screen boundaries across all monitors.
- **🚀 Auto-Start** — Optionally launch Apparition on Windows login via registry.
- **Restore on Close** — Optionally reset all modified windows when Apparition exits.

---

## 🖥️ How It Works

Apparition runs as a **floating desktop icon** and/or **system tray application**.

1. **Hover** over the floating ghost icon to reveal the control panel.
2. **Select** a window from the list to bring it to the foreground.
3. **Adjust transparency** with the slider or **change the mode** with the dropdown.
4. **Save presets** for frequently-used window configurations.
5. Use **global hotkeys** to ghost/pin/toggle windows without touching the panel.
6. **Right-click** the floating icon or tray icon for Settings and Exit.

---

## ⚙️ Settings

| Setting | Description | Default |
|---------|-------------|---------|
| **Restore on Close** | Reset all windows to normal on exit | `Off` |
| **Quick Transparency** | Opacity % for quick toggle | `50%` |
| **Icon Idle Opacity** | Floating icon opacity when inactive | `50%` |
| **Show Floating Icon** | Toggle the desktop widget on/off | `On` |
| **Auto-Start** | Launch on Windows login | `Off` |

Settings are persisted as JSON in `%LOCALAPPDATA%\Apparition\settings.json`.

---

## 🏗️ Architecture

Built with **.NET 8 WPF** using the **MVVM** (Model-View-ViewModel) pattern.

```
src/ApparitionWM/
├── App.xaml / App.xaml.cs            # Application entry, tray icon, lifecycle
├── Helpers/
│   ├── NativeMethods.cs              # Win32 P/Invoke declarations & constants
│   ├── ObservableObject.cs           # INotifyPropertyChanged base class
│   └── RelayCommand.cs               # ICommand implementation
├── Models/
│   ├── AppSettings.cs                # Settings data model
│   ├── HotkeyBinding.cs              # Global hotkey bindings
│   ├── ManagedWindow.cs              # Window state model
│   ├── WindowMode.cs                 # Normal / TopMost / Ghost enum
│   └── WindowPreset.cs               # Saved presets
├── Services/
│   ├── AutoStartService.cs           # Registry-based auto-start
│   ├── HotkeyService.cs              # Global hotkey registration (Win32)
│   ├── ProcessWatcherService.cs      # WMI process monitoring
│   ├── SettingsService.cs            # JSON settings persistence
│   └── WindowInteropService.cs       # Window manipulation (transparency, z-order)
├── ViewModels/
│   ├── ControlPanelViewModel.cs      # Main panel logic
│   └── SettingsViewModel.cs          # Settings dialog logic
├── Views/
│   ├── ControlPanelWindow.xaml       # Main control panel UI
│   ├── FloatingIconWindow.xaml       # Desktop ghost icon widget
│   └── SettingsWindow.xaml           # Settings dialog UI
├── Themes/
│   └── DarkTheme.xaml                # Dark theme with purple accent
└── Resources/
    └── apparition-icon.ico           # Application icon
```

---

## 🔧 Building

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### Build & Run

```bash
dotnet build src/ApparitionWM/ApparitionWM.csproj
dotnet run --project src/ApparitionWM/ApparitionWM.csproj
```

### Publish (Self-Contained)

```bash
dotnet publish src/ApparitionWM/ApparitionWM.csproj -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -p:IncludeAllContentForSelfExtract=true -o publish/
```

This produces a single `Apparition.exe` (~60MB) that runs without needing the .NET runtime.

---

## 📝 Notes

- **Admin privileges** are recommended for auto-refresh (WMI process watching). The app functions without admin but won't auto-detect new windows.
- **Ghost mode** uses `WS_EX_TRANSPARENT` — the window becomes completely click-through.
- The **dark theme** uses a purple accent palette (`#7C3AED`) with Segoe UI Variable typography.
- The legacy WinForms project is preserved in the `AppartionWM/` directory for reference.

---

## 📄 License

This project is provided as-is for personal use.
