# PLANS.md — AI Implementation Guide for AudioSlicer Pro

## 🎯 Project Overview

AudioSlicer Pro is a **Windows 11 native application** built with **WinUI 3 + C# + .NET 8**.
It slices audio files (MP3, WAV, FLAC, …) into 15-second segments and converts each segment to an audio-only MP4 file using FFmpeg.

---

## 🤖 AI Agent Roles

### Agent 1 — Coder
- **Responsibility**: Write all production code (XAML, C#, JSON configs, resource files)
- **Rules**:
  - Follow MVVM pattern strictly (Model → ViewModel → View)
  - Use async/await for all I/O and FFmpeg operations
  - Never block the UI thread
  - Use dependency injection via `Microsoft.Extensions.DependencyInjection`
  - Write XML doc comments for all public APIs
  - Follow C# naming conventions (PascalCase for public, camelCase for private)

### Agent 2 — Debugger
- **Responsibility**: Find, diagnose and fix bugs in existing code
- **Rules**:
  - Read error logs from `%LOCALAPPDATA%\AudioSlicerPro\logs\`
  - Check FFmpeg exit codes and stderr output
  - Validate file paths (spaces, unicode characters, long paths)
  - Test edge cases: empty files, corrupted audio, very short files (<15s), very long files
  - Verify tmp folder cleanup after successful conversion
  - Check memory usage during large file processing

### Agent 3 — Task Checker
- **Responsibility**: Validate task completion and update TASKS.md
- **Rules**:
  - Mark tasks `[x]` only after verifying build succeeds AND feature works
  - Run `dotnet build` before marking any task complete
  - Update TASKS.md after every code change
  - Ensure no regression in previously completed tasks
  - Cross-reference TASKS.md with WORKFLOW.md milestones

---

## 🛠️ Technology Stack

### Core Framework
| Component | Technology | Version | NuGet Package |
|---|---|---|---|
| UI Framework | WinUI 3 | Latest | `Microsoft.WindowsAppSDK` |
| Runtime | .NET | 8.0 | — |
| Language | C# | 12 | — |
| MVVM Toolkit | CommunityToolkit.Mvvm | Latest | `CommunityToolkit.Mvvm` |
| WinUI Controls | CommunityToolkit.WinUI | Latest | `CommunityToolkit.WinUI.UI.Controls` |

### Audio Processing
| Component | Library | NuGet |
|---|---|---|
| FFmpeg Wrapper | Xabe.FFmpeg | `Xabe.FFmpeg` |
| FFmpeg Binary Auto-download | Xabe.FFmpeg.Downloader | `Xabe.FFmpeg.Downloader` |

> **Why Xabe.FFmpeg?**
> - Managed .NET wrapper — no unsafe code needed
> - Auto-downloads FFmpeg binaries on first run
> - Fluent API for building FFmpeg conversion pipelines
> - Supports progress events and cancellation tokens
> - MIT licensed

### Settings & Persistence
| Component | Library |
|---|---|
| JSON Serialization | `System.Text.Json` (built-in .NET 8) |
| App Settings File | `%LOCALAPPDATA%\AudioSlicerPro\settings.json` |

### Localization
| Component | Approach |
|---|---|
| Resource Files | `.resw` files (standard WinUI localization) |
| Supported Languages | `en-US`, `vi-VN` |
| Runtime Switch | Custom `LocalizationService` with `INotifyPropertyChanged` |

### Logging
| Component | Library |
|---|---|
| Logger | `Microsoft.Extensions.Logging` |
| Log File | `%LOCALAPPDATA%\AudioSlicerPro\logs\app-{date}.log` |

---

## 📁 Project Structure

```
AudioSlicerPro/
├── AudioSlicerPro.sln
├── README.md
├── TASKS.md
├── .agent/
│   ├── PLANS.md          ← this file
│   ├── WORKFLOW.md
│   └── skill/
│
└── src/
    └── AudioSlicerPro/   ← WinUI 3 app project
        ├── AudioSlicerPro.csproj
        ├── App.xaml / App.xaml.cs
        ├── MainWindow.xaml / MainWindow.xaml.cs
        │
        ├── Models/
        │   ├── AppSettings.cs          ← Settings data model
        │   ├── AudioFileInfo.cs        ← Audio file metadata
        │   └── ProcessingSegment.cs   ← Segment processing state
        │
        ├── ViewModels/
        │   ├── MainViewModel.cs        ← Main window logic
        │   └── SettingsViewModel.cs    ← Settings page logic
        │
        ├── Views/
        │   ├── MainPage.xaml           ← Main processing UI
        │   ├── MainPage.xaml.cs
        │   ├── SettingsPage.xaml       ← Settings UI
        │   └── SettingsPage.xaml.cs
        │
        ├── Services/
        │   ├── IAudioService.cs        ← Interface
        │   ├── AudioService.cs         ← FFmpeg operations
        │   ├── ISettingsService.cs
        │   ├── SettingsService.cs      ← JSON settings R/W
        │   ├── ILocalizationService.cs
        │   └── LocalizationService.cs  ← Language switching
        │
        ├── Strings/
        │   ├── en-US/
        │   │   └── Resources.resw
        │   └── vi-VN/
        │       └── Resources.resw
        │
        └── Assets/
            └── *.png (app icons)
```

---

## 🔧 FFmpeg Command Reference

### Step 1: Slice audio into 15s segments
```bash
ffmpeg -i "input.mp3" -f segment -segment_time 15 -c copy "tmp/segment_%03d.mp3"
```

### Step 2: Convert each segment to audio-only MP4
```bash
ffmpeg -i "tmp/segment_001.mp3" -vn -acodec copy "dest/AudioName/segment_001.mp4"
```

> **Note**: `-vn` disables video, `-acodec copy` copies audio stream without re-encoding for speed.
> If the audio codec is not compatible with MP4 container, use `-acodec aac` instead.

---

## ⚠️ Important Implementation Rules

1. **NEVER use `Thread.Sleep` or blocking calls on the UI thread** — always use `async/await`
2. **ALWAYS validate paths** before passing to FFmpeg (check for null, empty, invalid chars)
3. **ALWAYS clean up tmp files** after successful conversion (configurable via settings)
4. **ALWAYS use `CancellationToken`** to allow user to cancel long-running operations
5. **ALWAYS log FFmpeg stderr** output for debugging purposes
6. **Use `DispatcherQueue.TryEnqueue`** to update UI from background threads
7. **Settings must persist** across app restarts — save on every change
8. **File naming**: Use zero-padded segment numbers (`segment_001`, `segment_002`, …)
9. **Last segment** may be shorter than 15s — this is expected and should be handled gracefully
10. **Unicode file names** must be supported — test with Vietnamese characters

---

## 🧪 Testing Requirements (for Debugger Agent)

- [ ] Audio file shorter than 15s → should produce single segment
- [ ] Audio file exactly 15s → should produce single segment
- [ ] Audio file with Vietnamese filename → path handling test
- [ ] Audio file with spaces in path → path handling test
- [ ] Cancelled operation mid-way → tmp files cleaned up
- [ ] Invalid/corrupted audio file → error shown gracefully
- [ ] Tmp folder not set → prompt user to configure settings
- [ ] Dest folder not writable → show permission error
- [ ] Very large file (>1 hour) → memory and progress test
- [ ] Switching language mid-processing → UI updates correctly
