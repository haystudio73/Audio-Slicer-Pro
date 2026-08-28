# TASKS.md — AudioSlicer Pro Task Tracker

> ⚡ This file is updated after EVERY code change by the Task Checker Agent.
> Format: `[x]` = done, `[/]` = in progress, `[ ]` = todo, `[-]` = blocked/skipped

**Last Updated**: 2026-07-24
**Current Phase**: Phase 5 — Polish & Release Prep

---

## 📊 Progress Summary

| Phase | Total | Done | In Progress | Remaining |
|---|---|---|---|---|
| Phase 1: Scaffolding | 7 | 7 | 0 | 0 |
| Phase 2: Settings & L10n | 10 | 10 | 0 | 0 |
| Phase 3: Audio Processing | 10 | 10 | 0 | 0 |
| Phase 4: Main UI | 9 | 9 | 0 | 0 |
| Phase 5: Polish | 7 | 7 | 0 | 0 |
| **TOTAL** | **43** | **43** | **0** | **0** |

---

## Phase 1 — Project Scaffolding

- [x] **[Coder]** Create WinUI 3 project with `dotnet new` or Visual Studio template
  - Target: `src/AudioSlicerPro/AudioSlicerPro.csproj`
  - Framework: `net10.0-windows10.0.26100.0`
- [x] **[Coder]** Add NuGet packages:
  - `Microsoft.WindowsAppSDK` (latest stable)
  - `CommunityToolkit.Mvvm` (latest)
  - `Xabe.FFmpeg` (latest)
  - `Xabe.FFmpeg.Downloader` (latest)
  - `Microsoft.Extensions.Logging` (10.x)
  - `Microsoft.Extensions.DependencyInjection` (10.x)
- [x] **[Coder]** Set up MVVM folder structure:
  - `Models/`, `ViewModels/`, `Views/`, `Services/`, `Strings/`, `Assets/`, `Converters/`
- [x] **[Coder]** Configure DI container in `App.xaml.cs`
  - Register all services as singletons/transients
- [x] **[Coder]** Set up `Microsoft.Extensions.Logging` with file sink
  - Log to: `%LOCALAPPDATA%\AudioSlicerPro\logs\app-{date}.log`
- [x] **[Debugger]** Verify clean build: `dotnet build -c Debug` → 0 errors
- [x] **[TaskChecker]** Update TASKS.md progress summary

---

## Phase 2 — Settings & Localization

- [x] **[Coder]** Create `Models/AppSettings.cs`
- [x] **[Coder]** Implement `Services/SettingsService.cs`
  - Read/write `%LOCALAPPDATA%\AudioSlicerPro\settings.json`
  - Use `System.Text.Json`
- [x] **[Coder]** Create `Strings/en-US/Resources.resw` with all UI strings
- [x] **[Coder]** Create `Strings/vi-VN/Resources.resw` with Vietnamese translations
- [x] **[Coder]** Implement `Services/LocalizationService.cs`
  - Runtime language switching without app restart
- [x] **[Coder]** Build `Views/SettingsPage.xaml`
  - Tmp folder picker (FolderPicker)
  - Dest folder picker (FolderPicker)
  - Language dropdown (ComboBox)
  - Save button
  - Trim silence toggle & threshold slider
- [x] **[Coder]** Implement `ViewModels/SettingsViewModel.cs` (CommunityToolkit.Mvvm)
- [x] **[Debugger]** Test: Settings persist after app restart
- [x] **[Debugger]** Test: Language switch updates all UI strings at runtime
- [x] **[TaskChecker]** Update TASKS.md progress summary

---

## Phase 3 — Audio Processing Core

- [x] **[Coder]** Define `Services/IAudioService.cs` interface
- [x] **[Coder]** Implement `Services/AudioService.cs` — SliceAudioAsync
  - FFmpeg command: `-f segment -segment_time 15`
  - FFmpeg silence trim filter: `silenceremove=start_periods=1:stop_periods=-1`
  - Output pattern: `segment_%03d.<ext>`
- [x] **[Coder]** Implement `Services/AudioService.cs` — ConvertToMp4Async
  - FFmpeg command: `-vn -c:a aac`
  - Output: `<DestFolder>/<AudioName>/segment_NNN.mp4`
- [x] **[Coder]** Add `IProgress<double>` reporting (0.0 to 1.0)
  - Parse FFmpeg stderr `time=` for progress calculation
- [x] **[Coder]** Add `CancellationToken` support throughout processing pipeline
- [x] **[Coder]** Create `Models/AudioFileInfo.cs` and `Models/ProcessingSegment.cs`
- [x] **[Debugger]** Test: MP3 file → slices → MP4 output (happy path)
- [x] **[Debugger]** Test: WAV file input
- [x] **[Debugger]** Test: File shorter than 15s → produces 1 segment
- [x] **[Debugger]** Test: Cancel operation → tmp files cleaned up
- [x] **[TaskChecker]** Update TASKS.md progress summary

---

## Phase 4 — Main UI

- [x] **[Coder]** Build `Views/MainPage.xaml` layout:
  - Drag-and-drop zone
  - Open File button
  - File info display (name, duration, format, size)
  - Progress bar (overall)
  - Log/output panel (ScrollViewer + TextBlock)
  - Start/Cancel button
  - Navigation to Settings
- [x] **[Coder]** Implement drag-and-drop (`DragEnter`, `Drop` events)
- [x] **[Coder]** Implement `StorageFilePicker` for Open button
- [x] **[Coder]** Implement `MainViewModel.cs`:
  - `StartProcessingCommand` (IAsyncRelayCommand)
  - `CancelCommand`
  - Progress property binding
  - Log entries ObservableCollection
- [x] **[Coder]** Wire MainPage ↔ MainViewModel (DataContext)
- [x] **[Coder]** Output folder auto-creation: `<DestRoot>/<FileNameWithoutExt>/`
- [x] **[Coder]** Navigation: MainPage ↔ SettingsPage (Frame navigation)
- [x] **[Debugger]** Test: Full end-to-end flow (open file → process → check MP4 output)
- [x] **[TaskChecker]** Update TASKS.md progress summary

---

## Phase 5 — Polish & Release

- [x] **[Coder]** Add error dialogs / status notifications for all error states
- [x] **[Coder]** Add app icon and assets
- [x] **[Coder]** Review and finalize UI aesthetics (WinUI theme)
- [x] **[Coder]** Implement tmp file cleanup
- [x] **[Debugger]** Full regression test: all phases
- [x] **[Coder]** Create build & release configuration (`dotnet build -c Release` verified)
- [x] **[TaskChecker]** Final TASKS.md review — all tasks `[x]`

---

## 🐛 Bug Log

- `[2026-07-24]` BUG-001: XAML Compiler unknown type `CardControl` | Status: fixed
- `[2026-07-24]` BUG-002: DispatcherQueue namespace in WinUI 3 | Status: fixed
- `[2026-07-24]` BUG-003: PublishTrimmed conflict in non-self-contained build | Status: fixed
- `[2026-07-24]` BUG-004: Light mode UI background rendering dark window fallback brush | Status: fixed

---

## ✅ Completed Features

- Scaffolding WinUI 3 project with MVVM pattern & DI container
- Audio file silence trimming (`silenceremove`) & 15s segment slicing
- Audio-only MP4 container conversion via FFmpeg
- Drag-and-drop file import & WinUI FileOpenPicker
- Settings persistence (`%LOCALAPPDATA%\AudioSlicerPro\settings.json`)
- Multilingual localization support (`en-US`, `vi-VN`)

---

## 📝 Change Log

| Date | Agent | Change |
|---|---|---|
| 2026-07-24 | Task Checker | Initial TASKS.md created |
| 2026-07-24 | Coder | Implemented full AudioSlicerPro WinUI 3 app with FFmpeg integration & trim silence |
| 2026-07-24 | Debugger | Fixed XAML compilation issues & verified Debug build |
| 2026-07-24 | Task Checker | Verified clean Release build and marked all 43 tasks as complete [x] |
| 2026-07-24 | Coder & Debugger | Fixed Light Mode UI background contrast & Mica backdrop theme styling |
| 2026-07-24 | Coder | Updated MP4 output generator to include H.264 video track (yuv420p + AAC + faststart) for Google Flow upload compatibility |
| 2026-07-24 | Coder | Added preview player file info header card & dynamic playing state button styles (emerald green background + active sound icon + auto reset on MediaEnded) |
