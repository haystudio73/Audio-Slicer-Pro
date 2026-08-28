# WORKFLOW.md — Development Workflow & Process

## 🗂️ Branch Strategy

```
main          ← stable production-ready code
├── dev       ← integration branch
│   ├── feat/ui-main-page
│   ├── feat/audio-slicing
│   ├── feat/mp4-conversion
│   ├── feat/settings-page
│   └── feat/localization
└── fix/*     ← bug fixes
```

---

## 📐 Development Phases

### Phase 1 — Project Scaffolding
**Goal**: Set up WinUI 3 project, NuGet packages, folder structure, DI container

| Task | Agent | Status |
|---|---|---|
| Create WinUI 3 project (`dotnet new winui`) | Coder | ⬜ |
| Add NuGet packages (Xabe.FFmpeg, CommunityToolkit.Mvvm, etc.) | Coder | ⬜ |
| Set up MVVM folder structure | Coder | ⬜ |
| Configure DI container in `App.xaml.cs` | Coder | ⬜ |
| Set up logging infrastructure | Coder | ⬜ |
| Verify build succeeds | Debugger | ⬜ |
| Update TASKS.md | Task Checker | ⬜ |

### Phase 2 — Settings & Localization
**Goal**: Working settings persistence and language switching

| Task | Agent | Status |
|---|---|---|
| Create `AppSettings.cs` model | Coder | ⬜ |
| Implement `SettingsService` (JSON read/write) | Coder | ⬜ |
| Create `en-US/Resources.resw` | Coder | ⬜ |
| Create `vi-VN/Resources.resw` | Coder | ⬜ |
| Implement `LocalizationService` | Coder | ⬜ |
| Build `SettingsPage.xaml` UI | Coder | ⬜ |
| Wire up `SettingsViewModel` | Coder | ⬜ |
| Test settings persist after app restart | Debugger | ⬜ |
| Test language switch at runtime | Debugger | ⬜ |
| Update TASKS.md | Task Checker | ⬜ |

### Phase 3 — Audio Processing Core
**Goal**: Working FFmpeg integration for slicing and conversion

| Task | Agent | Status |
|---|---|---|
| Create `IAudioService` interface | Coder | ⬜ |
| Implement `AudioService.SliceAudioAsync()` | Coder | ⬜ |
| Implement `AudioService.ConvertToMp4Async()` | Coder | ⬜ |
| Add progress reporting (IProgress<T>) | Coder | ⬜ |
| Add CancellationToken support | Coder | ⬜ |
| Test with MP3 file | Debugger | ⬜ |
| Test with WAV file | Debugger | ⬜ |
| Test with FLAC file | Debugger | ⬜ |
| Test edge cases (short file, unicode name) | Debugger | ⬜ |
| Update TASKS.md | Task Checker | ⬜ |

### Phase 4 — Main UI
**Goal**: Complete main window with file picker, progress, log panel

| Task | Agent | Status |
|---|---|---|
| Build `MainPage.xaml` layout | Coder | ⬜ |
| Implement drag-and-drop file input | Coder | ⬜ |
| Implement file picker (Open button) | Coder | ⬜ |
| Implement progress bar binding | Coder | ⬜ |
| Implement log/output panel | Coder | ⬜ |
| Wire up `MainViewModel` | Coder | ⬜ |
| Implement Cancel button | Coder | ⬜ |
| Test full flow end-to-end | Debugger | ⬜ |
| Update TASKS.md | Task Checker | ⬜ |

### Phase 5 — Polish & Release Prep
**Goal**: Clean UI, error handling, packaging

| Task | Agent | Status |
|---|---|---|
| Add proper error dialogs (ContentDialog) | Coder | ⬜ |
| Add app icon and branding | Coder | ⬜ |
| Review and improve UI aesthetics | Coder | ⬜ |
| Ensure tmp cleanup works | Debugger | ⬜ |
| Full regression test all phases | Debugger | ⬜ |
| Create MSIX package for distribution | Coder | ⬜ |
| Final TASKS.md review | Task Checker | ⬜ |

---

## 🔄 Agent Communication Protocol

### Coder → Debugger Handoff
When Coder completes a feature:
1. Update TASKS.md marking feature as `[/]` (in progress verification)
2. Note the feature name in TASKS.md under **"Ready for Testing"** section
3. Specify exact test scenarios in TASKS.md

### Debugger → Task Checker Handoff
When Debugger verifies a feature:
1. If PASS: note `✅ PASS` with test details in TASKS.md
2. If FAIL: note `❌ FAIL` with reproduction steps — hand back to Coder
3. Task Checker marks `[x]` only on `✅ PASS` confirmation

### Error Escalation
```
Bug found → Debugger documents → Coder fixes → Debugger retests → Task Checker signs off
```

---

## 📏 Code Review Checklist (for every PR/commit)

- [ ] No blocking calls on UI thread
- [ ] All public methods have XML doc comments
- [ ] All string literals replaced by localization resources
- [ ] FFmpeg paths properly escaped
- [ ] Tmp files cleaned up in finally blocks
- [ ] CancellationToken checked at every loop iteration
- [ ] Build passes: `dotnet build -c Release`
- [ ] No hardcoded file paths (use `AppSettings`)
- [ ] Settings saved after every change
- [ ] TASKS.md updated

---

## 🚨 Known Constraints & Gotchas

1. **WinUI 3 File Picker**: Must run on UI thread — use `StorageFilePicker` via `WinRT`
2. **FFmpeg paths with spaces**: Wrap all paths in escaped quotes when building command strings
3. **Xabe.FFmpeg auto-download**: Requires internet on first run — handle offline case gracefully
4. **MP4 container audio codec**: Not all audio codecs are compatible with MP4 — may need to transcode to AAC
5. **Progress events from FFmpeg**: Parse FFmpeg stderr `time=HH:MM:SS.ms` lines for progress
6. **Language switching**: WinUI 3 does not natively support runtime locale change — implement custom `LocalizationService`
7. **Long file paths (>260 chars)**: Enable long path support in app manifest
8. **MSIX packaging**: WinUI 3 apps require MSIX or sparse package for full WinRT API access

---

## 📊 Definition of Done

A task is **Done** when ALL of the following are true:
1. ✅ Code is written and committed
2. ✅ `dotnet build -c Release` succeeds with 0 errors, 0 warnings
3. ✅ Feature tested by Debugger Agent with all specified test cases
4. ✅ TASKS.md updated with `[x]`
5. ✅ No regression in any previously completed task
