# Open Issues — Technical Scope Review

**Repo:** [sillsdev/libpalaso](https://github.com/sillsdev/libpalaso) · **Branch:** `issues` (current with `master`) · **Reviewed:** 2026-06-29
**Scope:** all 23 open issues + 1 open PR. Each issue was evaluated against its full GitHub conversation and the current checkout (≈master).

**Size key:** `XS` trivial (<1 hr) · `S` hours · `M` ~1 day · `L` multi-day · `XL` major / cross-repo / uncertain.

---

## 1. Summary table (sorted by size)

| #                                                          | Title                                           | Size | Review | Breaking  |                     Already fixed?                      | Primary file(s)                     |
| ---------------------------------------------------------- | ----------------------------------------------- | :--: | :----: | :-------: | :-----------------------------------------------------: | ----------------------------------- |
| [#1431](https://github.com/sillsdev/libpalaso/issues/1431) | nuget: hide old betas                           |  XS  |  Low   |    No     |                      nuget.org op                       |
| [#1479](https://github.com/sillsdev/libpalaso/issues/1479) | VerseRef.GetBBBCCCVVV at upper limit            |  XS  |  Low   |    No     |               `SIL.Scripture/VerseRef.cs`               |
| [#359](https://github.com/sillsdev/libpalaso/issues/359)   | Linux GlobalMutex EINTR                         |  S   | Medium |    No     |           `SIL.Core/Threading/GlobalMutex.cs`           |
| [#1516](https://github.com/sillsdev/libpalaso/issues/1516) | Flaky audio tests on CI                         |  S   | Medium |    No     |         `SIL.Media.Tests/AudioSessionTests.cs`          |
| [#959](https://github.com/sillsdev/libpalaso/issues/959)   | GetFileDistributedWithApplication under VS Test |  S   | Medium | Partially |    `ReflectionHelper.cs`, `FileLocationUtilities.cs`    |
| [#1134](https://github.com/sillsdev/libpalaso/issues/1134) | Keep protected controls hidden                  |  S   |  Low   |    No     |              `SettingsProtectionHelper.cs`              |
| [#1170](https://github.com/sillsdev/libpalaso/issues/1170) | VerseControlTests fail on Linux                 |  S   | Medium |    No     |            `LinuxClipboard.NativeMethods.cs`            |
| [#1266](https://github.com/sillsdev/libpalaso/issues/1266) | BeginMonitoring throws when Mic perm off        |  S   | Medium |    No     |           `SIL.Media/Naudio/AudioRecorder.cs`           |
| [#1275](https://github.com/sillsdev/libpalaso/issues/1275) | Crash switching Crop/Choose                     |  S   |  Low   |    No     |         `ImageToolbox/Cropping/ImageCropper.cs`         |
| [#1337](https://github.com/sillsdev/libpalaso/issues/1337) | WritingSystemSetupDialog not localizable        |  S   |  Low   |    No     |         `WritingSystemSetupDialog.Designer.cs`          |
| [#1435](https://github.com/sillsdev/libpalaso/issues/1435) | KeymanLegacyBundle should be SIL-controlled     |  S   |  Low   | Partially |         `SIL.Windows.Forms.Keyboarding.csproj`          |
| [#1468](https://github.com/sillsdev/libpalaso/issues/1468) | Tok Pisin tpi/tp lang mismatch                  |  S   | Medium |    No     |                   `l10n/crowdin.yml`                    |
| [#1474](https://github.com/sillsdev/libpalaso/issues/1474) | Inconsistent VerseRef creation                  |  S   | Medium |    No     |               `SIL.Scripture/VerseRef.cs`               |
| [#1503](https://github.com/sillsdev/libpalaso/issues/1503) | Invalid copyright encoding in PNG               |  S   | Medium |    No     |              `ClearShare/MetadataCore.cs`               |
| [#597](https://github.com/sillsdev/libpalaso/issues/597)   | Email attachments ignored                       |  M   | Medium |    No     |           `SIL.Core/Email/*EmailProvider.cs`            |
| [#1105](https://github.com/sillsdev/libpalaso/issues/1105) | Linux CopyImageToClipboard                      |  M   | Medium |    No     |            `Clipboarding/LinuxClipboard.cs`             |
| [#1272](https://github.com/sillsdev/libpalaso/issues/1272) | Scan image dialog non-modal in x64              |  M   | Medium |    No     | `lib/x64/Interop.WIA.dll`, `ImageAcquisitionService.cs` |
| [#1320](https://github.com/sillsdev/libpalaso/issues/1320) | Upgrade to .NET8                                |  M   |  High  | Yes (NRT) |                        Partially                        | `Directory.Build.props`, `*.csproj` |
| [#1411](https://github.com/sillsdev/libpalaso/issues/1411) | Calls to obsolete methods                       |  M   | Medium |    No     |        `Sldr.cs`, `AnalyticsEventSender.cs`, +5         |
| [#1428](https://github.com/sillsdev/libpalaso/issues/1428) | GlobalMutex hardcoded /var/lock                 |  M   | Medium |    No     |           `SIL.Core/Threading/GlobalMutex.cs`           |
| [#1089](https://github.com/sillsdev/libpalaso/issues/1089) | ICU collation parse/gen problems                |  L   |  High  |    No     |    `SimpleRulesParser.cs`, `LdmlCollationParser.cs`     |
| [#1505](https://github.com/sillsdev/libpalaso/issues/1505) | Linux keyboard switching broken (Ubuntu 24.04)  |  L   |  High  |    No     |       `GnomeShellIbusKeyboardSwitchingAdaptor.cs`       |
| [#1425](https://github.com/sillsdev/libpalaso/issues/1425) | Migrate away from irrKlang                      |  XL  |  High  |    No     |  `SIL.Media/WindowsAudioSession.cs`, `AudioFactory.cs`  |

---

## 2. Issues that touch the same file / area

These clusters are worth batching into a single PR (or at least coordinating) to avoid merge conflicts and duplicated regression testing.

### Strong overlaps (literally the same file)

- **`SIL.Core/Threading/GlobalMutex.cs`** → **#359** (Linux EINTR retry) + **#1428** (hardcoded `/var/lock`). Both touch the Linux adapter and a maintainer already flagged doing them together. **Note:** closed-unmerged **PR #1504** also edited this file (Windows adapter, `AbandonedMutexException`) and remains an available reference — see §3.
- **`SIL.Scripture/VerseRef.cs`** (+ `SIL.Scripture.Tests/VerseRefTests.cs`) → **#1474** (`TrySetVerse` skips validation) + **#1479** (`GetBBBCCCVVV` off-by-one at the limit). Two independent bugs in the same class; bundle the test changes.
- **`SIL.Windows.Forms/Clipboarding/LinuxClipboard.NativeMethods.cs`** → **#1170** (UTF-8 byte-length bug in `gtk_clipboard_set_text`) + **#1105** (implement `CopyImageToClipboard`). Same native-interop file; #1105 also edits `LinuxClipboard.cs`. Fixing #1170 may directly un-skip the tests blocked in #1170.

### Same project / related area

- **`SIL.Media`** → **#1266** (`Naudio/AudioRecorder.cs`, mic-permission error) + **#1425** (`WindowsAudioSession.cs`/`AudioFactory.cs`, irrKlang→NAudio) + **#1516** (flaky `AudioSessionTests.cs` on CI). Same audio project; #1425 is the big one and may subsume recorder hardening. **PR #1325** renames this whole project (see §3).
- **`SIL.Windows.Forms/ImageToolbox/`** → **#1272** (`ImageAcquisitionService.cs` + `lib/x64/Interop.WIA.dll`) + **#1275** (`Cropping/ImageCropper.cs`). Different files, same toolbox feature; recent master commits already touch ImageToolbox.
- **`SIL.Windows.Forms.WritingSystems/`** → **#1337** (`WritingSystemSetupDialog.Designer.cs`/`.cs` localization) + **#1411** (one of seven obsolete-call sites is `WSSpellingControl.cs`/`WritingSystemSetupModel.cs`). Adjacent, mild overlap.

---

## 3. In-progress PRs

One open PR remains (a **draft**). The previously-open PR #1504 was **closed unmerged** on 2026-06-18 (author: "not actually up for this right now"; added a comment to the Jira issue pointing at the PR) — it survives as a reference for the Windows-adapter fix, noted below.

### PR #1504 (closed, unmerged) — Recover from AbandonedMutexException in GlobalMutex (Windows)

- **Author:** myieye · **Closed unmerged 2026-06-18** · `+48/-1`, 3 files · opened 2026-06-02.
- `WindowsGlobalMutexAdapter.Wait()` would catch `AbandonedMutexException`, log a warning, and proceed — matching the .NET `Mutex` contract (the lock is still acquired when the exception is thrown). Targeted JIRA **LT-21834** plus a shutdown-time crash via `WritingSystemManager.Save → GlobalWritingSystemRepository.TryGet → GlobalMutex.Lock()`. The author closed it as too hairy to finish now and pointed the Jira issue at it.
- **Cross-reference:** touches the **same file** as issues **#359** and **#1428** (Windows path vs. their Linux path). Whoever picks up the Linux mutex issues can reuse this branch's diff as a starting point for the Windows-adapter hardening.

### PR #1325 — Renamed SIL.Media to SIL.Windows.Forms.Media

- **Author:** josephmyers · **Draft** · `+87/-83`, **89 files** · opened 2024-06-04, last updated 2026-02-02 (**stale ~1.5 yrs**).
- Wide-reaching project/namespace rename. Low line-count but huge file fan-out, so high merge-conflict risk against anything touching `SIL.Media`.
- **Breaking change:** Yes — renaming the assembly/namespace breaks every downstream consumer of `SIL.Media`. Needs a clear migration story and semver-major bump.
- **Cross-reference:** directly collides with the `SIL.Media` work in **#1425** (irrKlang migration) and **#1266** (audio recorder). Decide the rename's fate _before_ doing substantial `SIL.Media` work, or the rename will need re-doing.
- **See [`NET8-UPGRADE-REMAINING-PLAN.md`](NET8-UPGRADE-REMAINING-PLAN.md)** for the rename-vs-split-vs-leave decision (with pros/cons) and the recommendation to open a fresh PR off `master` rather than reviving this one.

---

## 4. Per-issue detail

### XS

#### #1431 — [nuget] Hide old betas

- **Files:** none in repo — nuget.org operation.
- **Review:** Low. **Breaking:** No. **Already fixed:** No.
- Run [SIL.NuGetCleaner](https://github.com/sillsdev/SIL.NuGetCleaner) to unlist beta versions 14.0.1–16.0.0.

#### #1479 — VerseRef.GetBBBCCCVVV does unexpected things at the upper limit

- **Files:** `SIL.Scripture/VerseRef.cs` (edit), `SIL.Scripture.Tests/VerseRefTests.cs` (edit)
- **Review:** Low — arithmetic only. **Breaking:** **Yes** — callers relying on modulus-wrap would change. **Already fixed:** No.
- `bcvMaxValue = 999`, so `% bcvMaxValue` is `% 999`: `verseNum 999 → 0` (off-by-one) and values >999 wrap nonsensically (line ~1469). Fix by clamping (`Math.Min(value, bcvMaxValue)`) or `% (bcvMaxValue + 1)`. paranext-core has a `ToComparableBCV` workaround to remove once a fixed ParatextData ships. **Bundle with #1474** (same file).

### S

#### #359 — GlobalMutex.LinuxGlobalMutexAdapter fails to handle EINTR

- **Files:** `SIL.Core/Threading/GlobalMutex.cs` (edit)
- **Review:** Medium — Unix signal semantics. **Breaking:** No. **Already fixed:** No.
- `LinuxGlobalMutexAdapter.Wait()` (~line 293) calls `flock(LOCK_EX)` with no `EINTR` retry, so a signal surfaces as an unhandled `NativeException`. Loop and retry when `GetLastWin32Error()==4`. The `SIL_CORE_MAKE_GLOBAL_MUTEX_LOCAL_ONLY` workaround (PR #1378) exists but root cause is unfixed. @rmunn planned to fix alongside **#1428**. **Bundle with #1428; reuse the closed-unmerged PR #1504 diff for the Windows-adapter piece.**

#### #959 — GetFileDistributedWithApplication under VS Test Platform

- **Files:** `SIL.Core/Reflection/ReflectionHelper.cs`, `SIL.Core/IO/FileLocationUtilities.cs`, `SIL.Core.Tests/IO/FileLocationUtilitiesTests.cs`
- **Review:** Medium. **Breaking:** Maybe. **Already fixed:** Partially.
- `ReflectionHelper.RunningFromUnitTest` already detects MSTest's `TestFramework`, but under `microsoft.testplatform.testhost` `GetEntryAssembly()` returns the host, pointing `DirectoryOfApplicationOrSolution` at the wrong place. Detect `testhost` by name or fall back to executing-assembly path. Maintainer (@tombogle, Feb 2025) suspects it may be moot (Vessel/GlyssenEngine inactive) — possible **close/needs-repro** candidate.

#### #1134 — Keep protected controls hidden under password protection

- **Files:** `SIL.Windows.Forms/SettingProtection/SettingsProtectionHelper.cs` (edit), test dialog (edit)
- **Review:** Low — well-scoped, additive. **Breaking:** No. **Already fixed:** No.
- `UpdateDisplay()` (line ~75) shows/hides all managed components uniformly via `!NormallyHidden || (Ctrl+Shift)`. No per-component override (no `remainHidden`/`keepHidden` symbol anywhere). Track a second set of always-hidden components and pass intent through `SetSettingsProtection()` as an optional bool.

#### #1170 — Investigate VerseControlTests failing on Linux

- **Files:** `SIL.Windows.Forms/Clipboarding/LinuxClipboard.NativeMethods.cs` (edit), `SIL.Windows.Forms.Scripture.Tests/VerseControlTests.cs` (re-enable tests)
- **Review:** Medium. **Breaking:** No. **Already fixed:** No.
- Three tests are `ExcludePlatform = "Linux"` (lines 74/75/78). Root cause: `gtk_clipboard_set_text(clipboard, utf8, len)` passes `len = text.Length` (UTF-16 char count) as the **byte** length of an already-UTF-8 buffer, so multi-byte text truncates. Pass the real UTF-8 byte count, or `-1` for null-terminated. **Same file as #1105.**

#### #1266 — BeginMonitoring() throws when Win 11 Mic permission is off

- **Files:** `SIL.Media/Naudio/AudioRecorder.cs` (edit)
- **Review:** Medium — NAudio `MmResult` codes + recorder state machine. **Breaking:** No. **Already fixed:** No.
- `BeginMonitoringIfNeeded` (~line 264) only catches `MmResult.AlreadyAllocated`; `MmResult.UnspecifiedError` (thrown when mic permission off) propagates to a noisy `ErrorReport.NotifyUserOfProblem`. Catch `UnspecifiedError` (and maybe `BadDeviceId`/`NoDriver`) and treat as "no device". **Same project as #1425; PR #1325 renames the project.**

#### #1275 — Crash in ImageToolbox switching between Crop and Choose

- **Files:** `SIL.Windows.Forms/ImageToolbox/Cropping/ImageCropper.cs` (edit)
- **Review:** Low — clear disposal oversights. **Breaking:** No. **Already fixed:** No.
- Two compounding bugs: (1) `Application.Idle` subscribed in ctor (line 45) but never removed in `Dispose` (line 475) → handler fires on a disposed cropper; (2) `GetCroppedImage()` (~line 424) does `Image.FromStream(stream)` inside a `using` that disposes the `MemoryStream` before the bitmap is used. Add `Application.Idle -=` in Dispose and stop disposing the stream early. **ImageToolbox area shared with #1272.**

#### #1337 — Some controls in WritingSystemSetupDialog are not localizable

- **Files:** `SIL.Windows.Forms.WritingSystems/WritingSystemSetupDialog.Designer.cs` (edit), `WritingSystemSetupDialog.cs` (edit)
- **Review:** Low — standard L10NSharp extender wiring. **Breaking:** No. **Already fixed:** No.
- The dialog's `InitializeComponent` never created an `L10NSharpExtender` (unlike sibling controls). `_closeButton`, `_openGlobal`, `_openDirectory`, `_openLabel`, form `Text`, and two `String.Format` `dlg.Text` assignments are all hardcoded. Add the extender, set localizing IDs, wrap programmatic text with `LocalizationManager.GetString`.

#### #1435 — KeymanLegacyBundle package should be SIL-controlled

- **Files:** `SIL.Windows.Forms.Keyboarding/SIL.Windows.Forms.Keyboarding.csproj` (edit) if the ID changes.
- **Review:** Low — mostly a nuget ownership/process change. **Breaking:** Maybe. **Already fixed:** Partially.
- `KeymanLegacyBundle` v1.0.0 was published under an individual account; @josephmyers added `sillsdev`/`sil-lsdev` as co-owners (May 2025), mitigating the risk. csproj still references `KeymanLegacyBundle 1.0.0`. Re-publishing under a canonical SIL ID would break pinned consumers (Transcelerator broke once). Consensus: leave the ID, keep co-ownership.

#### #1468 — Tok Pisin l10n files: tpi filename vs tp xml:lang

- **Files:** `l10n/crowdin.yml` (edit), possibly a post-process step in `.github/workflows/l10n-packaging.yml`
- **Review:** Medium. **Breaking:** Maybe — consumers keying off `xml:lang="tp"`. **Already fixed:** No.
- Crowdin's internal locale is `tp`, so downloaded XLF carries `xml:lang="tp"` despite the `Palaso.tpi.xlf` filename. Add a `language_mapping` block in `crowdin.yml` (or sed/xmllint rewrite `tp`→`tpi`). The full Crowdin-side fix risks translation loss and needs coordination outside this repo.

#### #1474 — Inconsistent VerseRef creation behavior

- **Files:** `SIL.Scripture/VerseRef.cs` (edit), `SIL.Scripture.Tests/VerseRefTests.cs` (edit)
- **Review:** Medium — confirm no Paratext path relies on the lax behavior. **Breaking:** Maybe. **Already fixed:** No.
- Constructor validates via `IsVerseParseable` (line ~705) and throws for `"LUK 1:1,"`, but `TrySetVerse` (called by the `Verse` setter, line ~255) skips that check and stores `"1,"`, so `AllVerses()` yields a degenerate ref. Add one guard in `TrySetVerse` + a setter test. Thread requests Paratext-team sign-off first. **Bundle with #1479.**

#### #1503 — Invalid copyright info written to PNG files (UTF-8 vs ISO-8859-1)

- **Files:** `SIL.Core/ClearShare/MetadataCore.cs` (edit), possibly `SIL.Windows.Forms/ClearShare/Metadata.cs`
- **Review:** Medium — TagLib# PNG tag-writing internals. **Breaking:** No (only `tEXt` bytes; XMP/iTXt unchanged). **Already fixed:** No.
- TagLib# (v2.3.0) writes PNG `tEXt` chunks as UTF-8 where the spec requires ISO-8859-1, so non-ASCII copyright/author double-encodes (`MetadataCore.cs` ~line 482 forces a `TagTypes.Png` tag). Options: skip the `TagTypes.Png` tag, post-process the bytes, or fix upstream. Reporter notes low impact (XMP iTXt is always correct; most images are ASCII/JPEG) and left it as a tracking item.

#### #1516 — Flaky tests on CI

- **Files:** `SIL.Media.Tests/AudioSessionTests.cs` (investigate/stabilize)
- **Review:** Medium — flakiness is intermittent and audio-timing dependent. **Breaking:** No. **Already fixed:** No.
- Two SIL.Media playback tests flake on CI: `Play_InvalidAudioFileThrowsBackgroundException_NonFatalErrorReported` and `PlayAndStopPlaying_NonWindows_DoesNotThrow("wav")`. Both depend on async playback/background-error timing, so a fixed wait or a missed event surfaces as an intermittent failure. Diagnose the race (poll/await the expected state instead of sleeping; ensure the background exception is observed deterministically). **Same project as #1266/#1425; PR #1325 renames the project.**

### M

#### #597 — Additional attachments are ignored when sending email

- **Files:** `SIL.Core/Email/MailToEmailProvider.cs`, `LinuxEmailProvider.cs`, `ThunderbirdEmailProvider.cs` + their tests (edit)
- **Review:** Medium — per-client protocol quirks. **Breaking:** No. **Already fixed:** No.
- `MailToEmailProvider.GetAttachments()` comma-joins into one `&attachment=` (invalid for `mailto:`); `ThunderbirdEmailProvider` similarly uses a single param; `LinuxEmailProvider` correctly repeats `--attach`; macOS correctly iterates. Fix the format per provider (repeat the key). Note: a known upstream `xdg-email` bug (Feb 2025 comment) may still drop all but the last attachment even when formatted correctly — document the limitation.

#### #1105 — Implement PortableClipboard.CopyImageToClipboard on Linux

- **Files:** `SIL.Windows.Forms/Clipboarding/LinuxClipboard.cs`, `LinuxClipboard.NativeMethods.cs`, `PortableClipboardTests.cs`
- **Review:** Medium — GTK P/Invoke. **Breaking:** No (fills a `NotImplementedException` stub). **Already fixed:** No.
- `LinuxClipboard.CopyImageToClipboard()` (lines 50–59) still throws `NotImplementedException` citing this issue. The `IClipboard` interface + `WindowsClipboard.cs` give the reference pattern. Import `gtk_clipboard_set_image` + `gdk_pixbuf_new_from_data`, convert the bitmap to a pixbuf, call set_image. **Same file as #1170** — do them together.

#### #1272 — Scan image dialog is non-modal in x64 apps

- **Files:** `lib/x64/Interop.WIA.dll` (regenerate), `SIL.Windows.Forms/ImageToolbox/ImageAcquisitionService.cs` (edit), maybe `.csproj`
- **Review:** Medium — COM interop + arch-specific DLL gen. **Breaking:** Maybe. **Already fixed:** No.
- `lib/x64/Interop.WIA.dll` is still a PE32/MSIL (AnyCPU) assembly, not a true x64 PIA, so `CommonDialog.ShowSelectDevice` loses modality on x64. Regenerate a real x64 interop (COM ref to "Microsoft Windows Image Acquisition", `Embed=false`) and copy into `lib/x64/`. Also `ShowSelectDevice` is called without an owner handle — plumb one if the API allows. x86 unaffected. **ImageToolbox area shared with #1275.**

#### #1320 — Upgrade to .NET8

- **Files:** `Directory.Build.props` (add `<Nullable>enable</Nullable>`), `SIL.Windows.Forms.Scripture.csproj` (add `net8.0-windows`), many `.cs` for NRT annotation.
- **Review:** High — NRT is a repo-wide effort. **Breaking:** **Yes** (enabling NRT changes public API nullability). **Already fixed:** Partially.
- Done: non-WinForms libs target `netstandard2.0`; major WinForms projects target `net8.0-windows`; AppVeyor removed (`cc60efbb`); GHA matrix builds net8.0 + net8.0-windows. Remaining: add `net8.0-windows` to `SIL.Windows.Forms.Scripture` (and verify GeckoBrowserAdapter — its Geckofx dependency is Framework-only), and enable NRT solution-wide (`LangVersion=8` set but `<Nullable>` absent) — the latter is the bulk and a deliberate later phase.
- **Full remaining-work plan, with the decision points (SIL.Media disposition, NRT rollout strategy, `LangVersion` bump, dropping `net462`) and pros/cons: [`NET8-UPGRADE-REMAINING-PLAN.md`](NET8-UPGRADE-REMAINING-PLAN.md).**

#### #1411 — Calls to obsolete methods

- **Files:** `SIL.WritingSystems/Sldr.cs`, `SIL.Core/Reporting/AnalyticsEventSender.cs`, `SIL.Windows.Forms/Miscellaneous/X11.cs`, `SIL.Windows.Forms.Keyboarding/Windows/Win32.cs`, `GeckoBrowserAdapter/NativeReplacements.cs`, `WSSpellingControl.cs`, `WritingSystemSetupModel.cs`
- **Review:** Medium. **Breaking:** Maybe — removing `[Obsolete]` public members is semver-major; `WebRequest`→`HttpClient` can change exception types. **Already fixed:** No (all 7 sites still present).
- Four categories: 3× `Assembly.LoadWithPartialName` are Mono-compat guards (likely deletable now); `Uri.EscapeUriString`→`EscapeDataString`; `WebRequest.Create`→`HttpClient` (the larger async refactor, in Sldr.cs + AnalyticsEventSender.cs); the `GetSpellCheckComboBoxItems`/`CurrentSpellChecker` pair already `[Obsolete]` — inline/replace the `WSSpellingControl` callsite. **Mild overlap with #1337** (WS setup area).

#### #1428 — GlobalMutex uses hardcoded /var/lock (deprecated on modern Linux)

- **Files:** `SIL.Core/Threading/GlobalMutex.cs` (edit), `SIL.Core.Tests/Threading/GlobalMutexTests.cs` (edit)
- **Review:** Medium — fallback strategy still debated. **Breaking:** Maybe (different versions wouldn't mutually exclude during upgrade). **Already fixed:** No.
- Line ~262 still `Path.Combine("/var/lock", name)`. An XDG_RUNTIME_DIR fallback added in `3301b97a` (2021) was effectively dropped by `1bbd00bb` (Jun 2024, macOS rewrite). On modern Linux `/var/lock`→`/run/lock` is root-only and systemd plans removal in v258. Options unresolved: `/dev/shm` fallback vs `XDG_RUNTIME_DIR` (user-scoped) vs `ExplicitGlobalMutexAdapter`. **Bundle with #359; reuse the closed-unmerged PR #1504 diff for the Windows-adapter piece.**

### L

#### #1089 — ICU collation parsing and generation problems

- **Files:** `SIL.WritingSystems/SimpleRulesParser.cs`, `LdmlCollationParser.cs`, `SimpleCollationRulesParserTests.cs`
- **Review:** High — deep ICU tailoring-syntax knowledge. **Breaking:** Maybe (changes generated ICU rules / stored LDML). **Already fixed:** No.
- `SimpleRulesParser.IcuEscape()` blindly escapes all non-alphanumeric ASCII including `/`, so `a/A` (meant as tertiary `a <<< A`) becomes semantically wrong; `LdmlCollationParser.GetIcuRulesFromCollationNode()` copies the `<cr>` CDATA verbatim without validation. Parse `/` and emit `<<<`; stop double-escaping on round-trip. Stored ICU rules aren't actually tested since the code regenerates from simple rules.

#### #1505 — Linux: keyboard switching broken since Ubuntu 24.04

- **Files:** `SIL.Windows.Forms.Keyboarding/Linux/GnomeShellIbusKeyboardSwitchingAdaptor.cs` (edit), possibly the retrieving adaptor; likely a new bundled GNOME Shell extension.
- **Review:** High — GNOME Shell dev, hard to CI-test. **Breaking:** Maybe (changes packaging/install). **Already fixed:** No.
- `SelectKeyboard()` still calls `--method org.gnome.Shell.Eval` with `imports.ui.status.keyboard…` (lines 36–38). `org.gnome.Shell.Eval` was deprecated in GNOME 41 and removed in the version shipping with Ubuntu 24.04+, so switching silently fails. Fix likely requires deploying a custom GNOME Shell extension exposing a replacement D-Bus method (or an alternative like `ibus engine`). No fallback exists today. Most complex of the two keyboard issues.

### XL

#### #1425 — Prepare to migrate away from irrKlang

- **Files:** `SIL.Media/WindowsAudioSession.cs`, `SIL.Media/AudioFactory.cs`, `SIL.Media/SIL.Media.csproj` (remove irrKlang ref), remove `lib/win-x64|x86/irrKlang.NET4.dll`, possibly new NAudio recorder files.
- **Review:** High — audio APIs differ; needs hardware to test. **Breaking:** Maybe (subtle recording-behavior changes; net8 Windows already throws today). **Already fixed:** No.
- irrKlang's vendor confirmed no updates/new licenses. `AudioFactory.CreateAudioSession` already throws `PlatformNotSupportedException` on .NET8+ Windows, blocking net8 Windows consumers. NAudio (1.10.0) is already a dependency and used for playback; migrate the `IrrKlang.IAudioRecorder` recording path to NAudio `WaveInEvent`/`WaveFileWriter` and drop the NET462/48 conditional blocks. **Same project as #1266; collides with the PR #1325 rename — settle the rename first.**

---

## 5. Closed PRs with undeleted branches

After `git fetch --prune`, **49** branches remain on `origin`. Cross-referencing each against the PR list, these branches are the head of a **closed (or merged) same-repo PR** yet were never deleted. (Fork PRs don't leave branches on `origin`, so they're out of scope; merged same-repo PRs normally auto-delete — none did except the three noted below.) The two **open** PRs' branches (`fix/global-mutex-abandoned-recovery` → #1504, `sil.windows.forms.media` → #1325) are covered in §3 and excluded here.

**Recommendation key:** _Delete_ = work landed elsewhere, was abandoned, or is obsolete — branch is safe to remove. _Keep_ = author asked to retain, or it's a long-lived maintenance branch.

### Closed-unmerged PRs

| Branch                                     | PR                                                       |   Closed   | Why it closed                                                                   | Issue action                                                       |                         Branch                          |
| ------------------------------------------ | -------------------------------------------------------- | :--------: | ------------------------------------------------------------------------------- | ------------------------------------------------------------------ | :-----------------------------------------------------: |
| `bug/paint-file-crash`                     | [#1254](https://github.com/sillsdev/libpalaso/pull/1254) | 2023-06-05 | Fix abandoned (never reviewed to merge)                                         | **Create issue** — crash still live (see below)                    |            Keep until triaged → then delete             |
| `task/merge60`                             | [#1141](https://github.com/sillsdev/libpalaso/pull/1141) | 2024-08-26 | libpalaso-6.0→master merge stalled on coordination                              | Optional: open a tracking issue for the remaining 6.0→master merge | **Keep** — author explicitly asked to retain the branch |
| `Use_SIL_NAudio`                           | [#943](https://github.com/sillsdev/libpalaso/pull/943)   | 2020-06-17 | SIL NAudio fork no longer relied on (author searched, found nothing)            | Optional: note the fork investigation in #1425                     |                         Delete                          |
| `add-indexof-based-on-icomparer-of-string` | [#1096](https://github.com/sillsdev/libpalaso/pull/1096) | 2021-09-17 | Author concluded the `IndexOf(IComparer)` semantics are a logical contradiction | None — dead end, don't revive                                      |                         Delete                          |
| `ParatextSplitter`                         | [#65](https://github.com/sillsdev/libpalaso/pull/65)     | 2014-07-16 | Old, abandoned (12 yrs)                                                         | None                                                               |                         Delete                          |

### The one branch that warrants a new issue

#### `bug/paint-file-crash` ([#1254](https://github.com/sillsdev/libpalaso/pull/1254)) — crash adding metadata to a Paint-authored JPG

- The fix was never merged and **no issue tracks it**. The crash path is still live: `MetadataCore.FileFormatSupportsMetadata()` ([MetadataCore.cs:435](SIL.Core/ClearShare/MetadataCore.cs#L435)) calls `TagLib.File.Create(path)` inside `RetryUtility.Retry` with no catch, so a file TagLib can't parse (the reporter's `star.jpg` from MS Paint on Win10) throws straight up through `Write()` ([MetadataCore.cs:469](SIL.Core/ClearShare/MetadataCore.cs#L469)). The PR's fix was a one-line `try/catch ⇒ return false`. The recent BL-16221 PalasoImage hardening (`cca708f4`) touched adjacent code but not this method.
- **Recommended:** open a small bug issue capturing the repro (the `star.jpg` from the PR thread) and the one-line guard, then delete the branch. Keep the branch only as long as it's the sole record of the repro.

### Note on the remaining ~35 undeleted branches (no associated PR — out of scope)

These were pushed directly (predating or bypassing the PR workflow), so they aren't "closed PRs." For completeness:

- **Keep (intentional maintenance/release branches):** `libpalaso-2.6`/`-3.1`/`-4.0`/`-4.2`/`-6.0`, `wesay1.2`/`1.4`/`1.5`, `Bloom3.7`/`Bloom-5.2`, `HearThis1.4`, `FW7.2`, `LIFT-0.15`, `DotNet35Profile`, `DotNet40ClientProfile`.
- **Stale experiment/feature branches, likely deletable** (each would need a quick owner check; not tied to any PR): `BL-16414-imagetoolbox-dpi`, `HandleLaunchesRight`, `IWritingSystemDefinition`, `ImproveGACookie`, `LT-16358a`, `LdmlDataMapperToDumpXmlReaderAndXmlWriter`, `QueryWork`, `TrimWav`-era siblings, `bugfix/master/ldata_sldr`, `feature/master/20180319alltags`, `feature/master/ethnologueFeb18update`, `feature/master/fixldata_updatesldr`, `fix/CheckVersion`, `main-copy-for-testing`, `move-metadata`, `naudioForWeSay`.

#### Resolution check on the 16 "likely deletable" branches

`TrimWav`-era siblings and `bugfix/master/ldata_sldr` no longer exist on `origin` (already gone). The remaining 15 were diffed against master's history:

| Branch                                      | Owner             | Last commit | Verdict                                                                                                                                      | Recommendation                                |
| ------------------------------------------- | ----------------- | ----------- | -------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------- |
| `BL-16414-imagetoolbox-dpi`                 | Andrew Polk       | 2026-06-11  | **Active WIP**, 20 days old. DPI-scaling fix has no equivalent in master.                                                                    | **Keep — check with Andrew Polk**, not stale  |
| `move-metadata`                             | Ariel Rorabaugh   | 2025-10-14  | Superseded by master's Metadata/LicenseInfo refactor (PR #1478); its CHANGELOG/L10NSharp-9 commits already merged separately                 | Delete                                        |
| `LT-16358a`                                 | mark-sil          | 2025-10-14  | Same goal as `move-metadata`, also superseded by PR #1478                                                                                    | Delete                                        |
| `main-copy-for-testing`                     | Jason Naylor      | 2025-06-17  | `update-language-data.yml` already exists on master in a more mature form                                                                    | Delete                                        |
| `fix/CheckVersion`                          | Hasso             | 2022-08-19  | Version-check/throw logic already present in master's `LdmlDataMapper.cs`, arrived independently                                             | Delete                                        |
| `feature/master/fixldata_updatesldr`        | Daniel Glassey    | 2018-03-22  | Payload (`alltags.txt`) later deleted from repo entirely; TLS1.2 fix never landed and SLDR lookup was since rewritten                        | Delete                                        |
| `feature/master/20180319alltags`            | Daniel Glassey    | 2018-03-23  | Data-only; the resource files it updates were later deleted repo-wide (SLDR-based lookup replaced them)                                      | Delete                                        |
| `feature/master/ethnologueFeb18update`      | Daniel Glassey    | 2018-03-19  | Same — obsolete data, files since removed                                                                                                    | Delete                                        |
| `LdmlDataMapperToDumpXmlReaderAndXmlWriter` | Randy Regnier     | 2014-01-15  | Goal (XmlReader→XElement) achieved independently in master                                                                                   | Delete                                        |
| `QueryWork`                                 | Tim Armstrong     | 2014-01-15  | `CustomFieldQuery`/`RecordToken` reimplemented independently under `SIL.Core/Data`                                                           | Delete                                        |
| `naudioForWeSay`                            | John Hatton       | 2014-01-15  | 2012 WIP NAudio recorder; superseded by `SIL.Media/Naudio` (already NAudio-based) — but is prior art for open issue #1425 (irrKlang removal) | Delete; optionally cite as prior art on #1425 |
| `HandleLaunchesRight`                       | Eberhard Beilharz | 2014-01-15  | Zero substantive commits — only hg→git migration/`.gitignore` noise                                                                          | Delete                                        |
| `IWritingSystemDefinition`                  | Eberhard Beilharz | 2014-01-15  | Same — zero substantive commits                                                                                                              | Delete                                        |
| `ImproveGACookie`                           | Eberhard Beilharz | 2014-01-15  | Same — only `.gitattributes`/`.gitignore` commits                                                                                            | Delete                                        |

**Net: All but 1 safe to delete now; only `BL-16414-imagetoolbox-dpi` needs an owner check first.**

---

_Generated by a technical review of all open issues (each read with its full conversation) plus the 1 open PR, checked against the current `issues`-branch checkout. §5 cross-references the 1,402-PR history against the surviving `origin` branches. No issues, PRs, or branches were created or modified._
