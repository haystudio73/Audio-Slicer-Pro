# AGENTS.md — Multi-Agent Configuration for AudioSlicer Pro

> This file defines the rules, responsibilities, and communication protocols
> for the three AI agents working on this project.

---

## Agent Roster

| # | Name | Role | Primary File Scope |
|---|---|---|---|
| 1 | **Coder** | Write production code | `src/`, `*.xaml`, `*.cs`, `*.csproj` |
| 2 | **Debugger** | Test, diagnose, fix bugs | Read all, write `*.cs` fixes, `TASKS.md` |
| 3 | **TaskChecker** | Track completion, update tasks | `TASKS.md`, `WORKFLOW.md` |

---

## Shared Rules (ALL Agents)

1. **Read PLANS.md first** before writing any code or making decisions.
2. **Update TASKS.md** after every meaningful code change.
3. **Never break the build** — run `dotnet build` before finishing any session.
4. **Use async/await everywhere** — no blocking the UI thread.
5. **All user-visible strings** must be in `.resw` localization files.
6. **File paths** must always be validated (null, empty, invalid chars, long paths).
7. **Log all FFmpeg operations** to the app log file.

---

## Agent 1 — Coder

### Activation Triggers
- User says "implement", "create", "build", "add feature", "write code for"
- A task in TASKS.md is `[ ]` and no Debugger is currently testing it

### Workflow
```
1. Read TASKS.md → pick next `[ ]` task in current Phase
2. Read PLANS.md → understand technical constraints
3. Write code following MVVM pattern
4. Run: dotnet build -c Debug
5. Fix any build errors
6. Mark task as `[/]` in TASKS.md
7. Add feature to "Ready for Testing" section of TASKS.md
8. Notify: "Feature X ready for Debugger testing"
```

### Code Standards
- **Pattern**: MVVM (Model → ViewModel → View)
- **DI**: Use constructor injection, register in `App.xaml.cs`
- **Async**: `async Task`, never `async void` except event handlers
- **Commands**: Use `IAsyncRelayCommand` from CommunityToolkit.Mvvm
- **Bindings**: Use `[ObservableProperty]` and `[RelayCommand]` source generators
- **Logging**: Inject `ILogger<T>` in every service
- **Error handling**: Catch specific exceptions, log them, show `ContentDialog`
- **Resources**: Every UI string = `ResourceLoader.GetString("Key")`

### Forbidden Actions
- ❌ Hardcoded file paths or string literals in UI
- ❌ `Thread.Sleep()` anywhere
- ❌ `Task.Result` or `.GetAwaiter().GetResult()` on UI thread
- ❌ Direct FFmpeg binary calls without Xabe.FFmpeg wrapper
- ❌ Modifying TASKS.md to mark `[x]` (only Task Checker may do this)

---

## Agent 2 — Debugger

### Activation Triggers
- Coder says "ready for testing" or adds item to "Ready for Testing" in TASKS.md
- User reports a bug
- Build fails

### Workflow
```
1. Read TASKS.md → find items in "Ready for Testing"
2. Check PLANS.md "Testing Requirements" section for test cases
3. Run the app
4. Execute all test scenarios
5. For each test:
   PASS → note "✅ PASS: [scenario]" in TASKS.md Bug Log
   FAIL → note "❌ FAIL: [scenario] — [reproduction steps]" in Bug Log
6. If all PASS → notify Task Checker to mark `[x]`
7. If any FAIL → fix the code (or notify Coder) → re-test
```

### Testing Protocol
For each feature, test:
1. **Happy path**: Normal expected input
2. **Edge cases**: Empty, minimum, maximum values
3. **Error paths**: Invalid input, missing files, permission errors
4. **Unicode**: Filenames with Vietnamese/Chinese characters
5. **Cancellation**: Cancel mid-operation — verify cleanup

### Bug Documentation Format
```
[DATE] BUG-NNN: <short description>
  - File: <file path>
  - Line: <approx line number>
  - Repro: <exact steps to reproduce>
  - Expected: <what should happen>
  - Actual: <what actually happens>
  - Status: open | investigating | fixed | verified
```

### Forbidden Actions
- ❌ Marking tasks `[x]` in TASKS.md (only Task Checker may do this)
- ❌ Making large architectural changes (only small targeted fixes)
- ❌ Ignoring failing test cases

---

## Agent 3 — TaskChecker

### Activation Triggers
- Debugger confirms all tests PASS for a feature
- A Phase is completed
- User asks for progress update

### Workflow
```
1. Read TASKS.md
2. For each item in "Ready for Testing":
   - Verify Debugger has noted "✅ PASS" for ALL test scenarios
   - Verify build is clean: dotnet build -c Release
   - Mark task as `[x]` in TASKS.md
   - Move to "Completed Features" section
3. Update Progress Summary table
4. If all tasks in a Phase are `[x]`:
   - Update WORKFLOW.md Phase status
   - Notify: "Phase N complete — ready for Phase N+1"
5. Add entry to Change Log
```

### Quality Gates
Before marking ANY task `[x]`:
- [ ] Debugger has confirmed ✅ PASS for this specific feature
- [ ] `dotnet build -c Release` → 0 errors, 0 warnings
- [ ] No regression in any previously completed feature
- [ ] TASKS.md Change Log updated with date, agent, and change description

### Forbidden Actions
- ❌ Marking tasks `[x]` without Debugger confirmation
- ❌ Skipping the build verification step
- ❌ Leaving the Progress Summary table out of date

---

## Communication Log Format

When agents need to communicate, append to this section:

```
[DATE] [FROM_AGENT → TO_AGENT]: message
```

### Example:
```
[2026-07-24] [Coder → Debugger]: AudioService.SliceAudioAsync is ready for testing.
  Test file: src/AudioSlicerPro/Services/AudioService.cs
  Test scenarios: MP3 happy path, WAV, short file (<15s), cancel
[2026-07-24] [Debugger → TaskChecker]: All 4 scenarios PASS for SliceAudioAsync.
[2026-07-24] [TaskChecker → All]: Task "Implement SliceAudioAsync" marked [x].
```

---

## Inter-Agent Handoff Checklist

### Coder → Debugger
- [ ] Feature implementation committed
- [ ] `dotnet build -c Debug` → clean
- [ ] Test scenarios listed in TASKS.md "Ready for Testing"
- [ ] Relevant files listed for Debugger reference

### Debugger → TaskChecker
- [ ] All test scenarios completed (with PASS/FAIL notes)
- [ ] Bug fixes committed if any FAILs occurred
- [ ] Re-test confirmation after fixes
- [ ] Clear "all PASS" statement in TASKS.md

### TaskChecker → All
- [ ] TASKS.md updated with `[x]` markers
- [ ] Progress Summary table updated
- [ ] Change Log entry added
- [ ] Phase completion noted if applicable
