# Open Issues — Technical Scope Review

**Repo:** [sillsdev/libpalaso](https://github.com/sillsdev/libpalaso) · **Branch:** `issues` (current with `master`) · **Reviewed:** 2026-06-16
**Scope:** all 34 open issues + 2 open PRs. Each issue was evaluated against its full GitHub conversation and the current checkout (≈master).

**Size key:** `XS` trivial (<1 hr) · `S` hours · `M` ~1 day · `L` multi-day · `XL` major / cross-repo / uncertain.

---

## 1. Summary table (sorted by size)

| # | Title | Size | Review | Breaking | Already fixed? | Primary file(s) |
|---|-------|:----:|:------:|:--------:|:--------------:|-----------------|
| [#761](https://github.com/sillsdev/libpalaso/issues/761) | Corruption in Windows language crashes | XS | Low | No | No | `WritingSystemFromWindowsLocaleProvider.cs` |
| [#1016](https://github.com/sillsdev/libpalaso/issues/1016) | feature/nuget → main dev branch | XS | Low | Likely (done) | — | none (close-out) |
| [#1037](https://github.com/sillsdev/libpalaso/issues/1037) | pdfdroplet consume/publish nuget | XS | Low | Partially | `CopyTo pdfDroplet.bat` (remove) |
| [#1392](https://github.com/sillsdev/libpalaso/issues/1392) | GetDocumentationUri test fails | XS | Low | No | `AccessProtocolList.cs` |
| [#1430](https://github.com/sillsdev/libpalaso/issues/1430) | Wiki: Release Procedure out of date | XS | Low | No | wiki only |
| [#1431](https://github.com/sillsdev/libpalaso/issues/1431) | nuget: hide old betas | XS | Low | No | nuget.org op |
| [#1448](https://github.com/sillsdev/libpalaso/issues/1448) | Date wrong in generated PR commit msg | XS | Low | No | `.github/workflows/update-language-data.yml` |
| [#1479](https://github.com/sillsdev/libpalaso/issues/1479) | VerseRef.GetBBBCCCVVV at upper limit | XS | Low | No | `SIL.Scripture/VerseRef.cs` |
| [#359](https://github.com/sillsdev/libpalaso/issues/359) | Linux GlobalMutex EINTR | S | Medium | No | `SIL.Core/Threading/GlobalMutex.cs` |
| [#959](https://github.com/sillsdev/libpalaso/issues/959) | GetFileDistributedWithApplication under VS Test | S | Medium | Partially | `ReflectionHelper.cs`, `FileLocationUtilities.cs` |
| [#1010](https://github.com/sillsdev/libpalaso/issues/1010) | Document nuget package changes | S | Low | Partially | `README.md`, wiki |
| [#1021](https://github.com/sillsdev/libpalaso/issues/1021) | Always get latest nuget version | S | Low | No | `dependabot.yml` (add) / docs |
| [#1134](https://github.com/sillsdev/libpalaso/issues/1134) | Keep protected controls hidden | S | Low | No | `SettingsProtectionHelper.cs` |
| [#1170](https://github.com/sillsdev/libpalaso/issues/1170) | VerseControlTests fail on Linux | S | Medium | No | `LinuxClipboard.NativeMethods.cs` |
| [#1266](https://github.com/sillsdev/libpalaso/issues/1266) | BeginMonitoring throws when Mic perm off | S | Medium | No | `SIL.Media/Naudio/AudioRecorder.cs` |
| [#1275](https://github.com/sillsdev/libpalaso/issues/1275) | Crash switching Crop/Choose | S | Low | No | `ImageToolbox/Cropping/ImageCropper.cs` |
| [#1337](https://github.com/sillsdev/libpalaso/issues/1337) | WritingSystemSetupDialog not localizable | S | Low | No | `WritingSystemSetupDialog.Designer.cs` |
| [#1435](https://github.com/sillsdev/libpalaso/issues/1435) | KeymanLegacyBundle should be SIL-controlled | S | Low | Partially | `SIL.Windows.Forms.Keyboarding.csproj` |
| [#1468](https://github.com/sillsdev/libpalaso/issues/1468) | Tok Pisin tpi/tp lang mismatch | S | Medium | No | `l10n/crowdin.yml` |
| [#1474](https://github.com/sillsdev/libpalaso/issues/1474) | Inconsistent VerseRef creation | S | Medium | No | `SIL.Scripture/VerseRef.cs` |
| [#1503](https://github.com/sillsdev/libpalaso/issues/1503) | Invalid copyright encoding in PNG | S | Medium | No | `ClearShare/MetadataCore.cs` |
| [#597](https://github.com/sillsdev/libpalaso/issues/597) | Email attachments ignored | M | Medium | No | `SIL.Core/Email/*EmailProvider.cs` |
| [#1007](https://github.com/sillsdev/libpalaso/issues/1007) | Update/test LfMerge packaging | M | Medium | Unclear | LfMerge repo |
| [#1008](https://github.com/sillsdev/libpalaso/issues/1008) | Update/test FieldWorks packaging | M | Medium | Unclear | FieldWorks repo |
| [#1105](https://github.com/sillsdev/libpalaso/issues/1105) | Linux CopyImageToClipboard | M | Medium | No | `Clipboarding/LinuxClipboard.cs` |
| [#1272](https://github.com/sillsdev/libpalaso/issues/1272) | Scan image dialog non-modal in x64 | M | Medium | No | `lib/x64/Interop.WIA.dll`, `ImageAcquisitionService.cs` |
| [#1320](https://github.com/sillsdev/libpalaso/issues/1320) | Upgrade to .NET8 | M | High | Yes (NRT) | `Directory.Build.props`, `*.csproj` |
| [#1411](https://github.com/sillsdev/libpalaso/issues/1411) | Calls to obsolete methods | M | Medium | No | `Sldr.cs`, `AnalyticsEventSender.cs`, +5 |
| [#1428](https://github.com/sillsdev/libpalaso/issues/1428) | GlobalMutex hardcoded /var/lock | M | Medium | No | `SIL.Core/Threading/GlobalMutex.cs` |
| [#1001](https://github.com/sillsdev/libpalaso/issues/1001) | Use nuget packages in FieldWorks | L | High | No | FieldWorks repo |
| [#1089](https://github.com/sillsdev/libpalaso/issues/1089) | ICU collation parse/gen problems | L | High | No | `SimpleRulesParser.cs`, `LdmlCollationParser.cs` |
| [#1505](https://github.com/sillsdev/libpalaso/issues/1505) | Linux keyboard switching broken (Ubuntu 24.04) | L | High | No | `GnomeShellIbusKeyboardSwitchingAdaptor.cs` |
| [#993](https://github.com/sillsdev/libpalaso/issues/993) | Create/use nuget packages (umbrella) | XL | High | Partially (libpalaso done) | cross-repo meta |
| [#1425](https://github.com/sillsdev/libpalaso/issues/1425) | Migrate away from irrKlang | XL | High | No | `SIL.Media/WindowsAudioSession.cs`, `AudioFactory.cs` |

---

## 2. Issues that touch the same file / area

These clusters are worth batching into a single PR (or at least coordinating) to avoid merge conflicts and duplicated regression testing.

### Strong overlaps (literally the same file)

- **`SIL.Core/Threading/GlobalMutex.cs`** → **#359** (Linux EINTR retry) + **#1428** (hardcoded `/var/lock`). Both touch the Linux adapter and a maintainer already flagged doing them together. **Note:** open **PR #1504** also edits this file (Windows adapter, `AbandonedMutexException`) — see §3.
- **`SIL.Scripture/VerseRef.cs`** (+ `SIL.Scripture.Tests/VerseRefTests.cs`) → **#1474** (`TrySetVerse` skips validation) + **#1479** (`GetBBBCCCVVV` off-by-one at the limit). Two independent bugs in the same class; bundle the test changes.
- **`SIL.Windows.Forms/Clipboarding/LinuxClipboard.NativeMethods.cs`** → **#1170** (UTF-8 byte-length bug in `gtk_clipboard_set_text`) + **#1105** (implement `CopyImageToClipboard`). Same native-interop file; #1105 also edits `LinuxClipboard.cs`. Fixing #1170 may directly un-skip the tests blocked in #1170.

### Same project / related area

- **`SIL.Media`** → **#1266** (`Naudio/AudioRecorder.cs`, mic-permission error) + **#1425** (`WindowsAudioSession.cs`/`AudioFactory.cs`, irrKlang→NAudio). Same audio project; #1425 is the big one and may subsume recorder hardening. **PR #1325** renames this whole project (see §3).
- **`SIL.Windows.Forms/ImageToolbox/`** → **#1272** (`ImageAcquisitionService.cs` + `lib/x64/Interop.WIA.dll`) + **#1275** (`Cropping/ImageCropper.cs`). Different files, same toolbox feature; recent master commits already touch ImageToolbox.
- **`SIL.Windows.Forms.WritingSystems/`** → **#1337** (`WritingSystemSetupDialog.Designer.cs`/`.cs` localization) + **#1411** (one of seven obsolete-call sites is `WSSpellingControl.cs`/`WritingSystemSetupModel.cs`). Adjacent, mild overlap.

### Documentation / release / nuget housekeeping cluster

- **#1010**, **#1021**, **#1430**, **#1431** are all nuget/release documentation or nuget.org operations (README + wiki + unlisting old betas). Best handled as one "release-docs refresh" pass; #1430 and #1431 belong on the same release checklist.

### nuget umbrella sub-tasks (mostly cross-repo)

- **#993** is the umbrella; **#1001** (FieldWorks), **#1007** (LfMerge), **#1008** (FieldWorks packaging), **#1016** (autosrtests), **#1037** (pdfdroplet) are all sub-tasks. The **libpalaso-side work is done**; the remaining work lives in other repos. These are triage/close candidates, not libpalaso code changes (see notes below).

---

## 3. In-progress PRs

Both open PRs are **drafts**.

### PR #1504 — Recover from AbandonedMutexException in GlobalMutex (Windows)
- **Author:** myieye · **Draft** · `+48/-1`, 3 files · opened 2026-06-02.
- `WindowsGlobalMutexAdapter.Wait()` now catches `AbandonedMutexException`, logs a warning, and proceeds — matching the .NET `Mutex` contract (the lock is still acquired when the exception is thrown). Fixes JIRA **LT-21834** plus a shutdown-time crash via `WritingSystemManager.Save → GlobalWritingSystemRepository.TryGet → GlobalMutex.Lock()`.
- **Cross-reference:** touches the **same file** as issues **#359** and **#1428** (Windows path vs. their Linux path). Whoever picks up the Linux mutex issues should rebase on / coordinate with this PR. Small and self-contained; close to mergeable once reviewed.

### PR #1325 — Renamed SIL.Media to SIL.Windows.Forms.Media
- **Author:** josephmyers · **Draft** · `+87/-83`, **89 files** · opened 2024-06-04, last updated 2026-02-02 (**stale ~1.5 yrs**).
- Wide-reaching project/namespace rename. Low line-count but huge file fan-out, so high merge-conflict risk against anything touching `SIL.Media`.
- **Breaking change:** Yes — renaming the assembly/namespace breaks every downstream consumer of `SIL.Media`. Needs a clear migration story and semver-major bump.
- **Cross-reference:** directly collides with the `SIL.Media` work in **#1425** (irrKlang migration) and **#1266** (audio recorder). Decide the rename's fate *before* doing substantial `SIL.Media` work, or the rename will need re-doing.

---

## 4. Per-issue detail

### XS

#### #761 — Corruption in Windows language leads to a crash
- **Files:** `SIL.Windows.Forms.WritingSystems/WritingSystemFromWindowsLocaleProvider.cs` (edit)
- **Review:** Low — a single try/catch / null-guard. **Breaking:** No. **Already fixed:** No.
- Line ~120 still accesses `language.LayoutName` with no guard; the existing try/catch only covers `language.Culture`. `InputLanguage.LayoutName` can throw `NullReferenceException` from WinForms when the Windows language install is corrupt. Wrap the `yield return` in try/catch and skip the offending language. 2018 discussion noted it was WeSay-specific and may not repro on Win10/11 — low priority, but the crash point is still live.

#### #1016 — lfmerge-autosrtests: make feature/nuget the main dev branch
- **Files:** none — verification + close-out.
- **Review:** Low. **Breaking:** No. **Already fixed:** Likely (done).
- `feature/nuget` merged to master 2021-02-23 (`29e851ed`); `GitVersion.yml` has no nuget-branch overrides; GHA publishes packages automatically. Only remaining action is **closing the issue**. Stale title appears to be a copy-paste artifact.

#### #1037 — pdfdroplet should consume and publish nuget package
- **Files:** `CopyTo pdfDroplet.bat` (delete/deprecate); rest in `sillsdev/pdfdroplet` + Bloom (external).
- **Review:** Low. **Breaking:** No. **Already fixed:** Partially.
- First three checklist items done (pdfdroplet PRs #24/#25). Remaining items (Bloom consuming pdfdroplet as nuget, removing TC deps) are external. Only libpalaso-side cleanup: remove obsolete `CopyTo pdfDroplet.bat`. Close or move to pdfdroplet repo.

#### #1392 — GetDocumentationUri…ReturnsRootedPathToExistingFile fails
- **Files:** `SIL.Archiving/Generic/AccessProtocol/AccessProtocolList.cs` (edit)
- **Review:** Low — one-line logic error. **Breaking:** No. **Already fixed:** No.
- Introduced by PR #1317. Line ~208 strips the extension (`"ailca.html"` → `"ailca"`) then `GetResource("ailca")` misses the manifest name `SIL.Archiving.Resources.ailca.html`. Simplest fix: don't strip the extension. Test already written (`[SkipOnTeamCity]`).

#### #1430 — [Wiki] Release Procedure out of date
- **Files:** wiki page only (not in repo).
- **Review:** Low. **Breaking:** No. **Already fixed:** No.
- Wiki references decommissioned AppVeyor + manual release steps; GHA (`build.yml`) now publishes on tag/master. A 2026-06-16 comment confirms v17 shipped via GHA, making the update overdue. Pure wiki edit. Pairs with #1431.

#### #1431 — [nuget] Hide old betas
- **Files:** none in repo — nuget.org operation.
- **Review:** Low. **Breaking:** No. **Already fixed:** No.
- Run [SIL.NuGetCleaner](https://github.com/sillsdev/SIL.NuGetCleaner) to unlist beta versions 14.0.1–16.0.0. Operational; add to the standard release checklist (ties to #1430).

#### #1448 — Date wrong in generated PR commit message
- **Files:** `.github/workflows/update-language-data.yml` (edit)
- **Review:** Low. **Breaking:** No. **Already fixed:** No.
- Line ~235 `- Updated: $(date -u …)` is a YAML `commit-message:` value passed to `peter-evans/create-pull-request`, so the `$()` shell substitution never runs and is emitted literally. Fix: add a `run:` step writing the date to `$GITHUB_OUTPUT`, then reference `${{ steps.<id>.outputs.date }}`.

#### #1479 — VerseRef.GetBBBCCCVVV does unexpected things at the upper limit
- **Files:** `SIL.Scripture/VerseRef.cs` (edit), `SIL.Scripture.Tests/VerseRefTests.cs` (edit)
- **Review:** Low — arithmetic only. **Breaking:** **Yes** — callers relying on modulus-wrap would change. **Already fixed:** No.
- `bcvMaxValue = 999`, so `% bcvMaxValue` is `% 999`: `verseNum 999 → 0` (off-by-one) and values >999 wrap nonsensically (line ~1469). Fix by clamping (`Math.Min(value, bcvMaxValue)`) or `% (bcvMaxValue + 1)`. paranext-core has a `ToComparableBCV` workaround to remove once a fixed ParatextData ships. **Bundle with #1474** (same file).

### S

#### #359 — GlobalMutex.LinuxGlobalMutexAdapter fails to handle EINTR
- **Files:** `SIL.Core/Threading/GlobalMutex.cs` (edit)
- **Review:** Medium — Unix signal semantics. **Breaking:** No. **Already fixed:** No.
- `LinuxGlobalMutexAdapter.Wait()` (~line 293) calls `flock(LOCK_EX)` with no `EINTR` retry, so a signal surfaces as an unhandled `NativeException`. Loop and retry when `GetLastWin32Error()==4`. The `SIL_CORE_MAKE_GLOBAL_MUTEX_LOCAL_ONLY` workaround (PR #1378) exists but root cause is unfixed. @rmunn planned to fix alongside **#1428**. **Bundle with #1428; coordinate with PR #1504.**

#### #959 — GetFileDistributedWithApplication under VS Test Platform
- **Files:** `SIL.Core/Reflection/ReflectionHelper.cs`, `SIL.Core/IO/FileLocationUtilities.cs`, `SIL.Core.Tests/IO/FileLocationUtilitiesTests.cs`
- **Review:** Medium. **Breaking:** Maybe. **Already fixed:** Partially.
- `ReflectionHelper.RunningFromUnitTest` already detects MSTest's `TestFramework`, but under `microsoft.testplatform.testhost` `GetEntryAssembly()` returns the host, pointing `DirectoryOfApplicationOrSolution` at the wrong place. Detect `testhost` by name or fall back to executing-assembly path. Maintainer (@tombogle, Feb 2025) suspects it may be moot (Vessel/GlyssenEngine inactive) — possible **close/needs-repro** candidate.

#### #1010 — Document what changed with the nuget packages
- **Files:** `README.md`, `CHANGELOG.md`, wiki (external).
- **Review:** Low. **Breaking:** No. **Already fixed:** Partially.
- README already has a "Deployment on nuget.org" section and a local-package wiki link. Remaining gap is likely wiki content. Audit wiki pages, fill gaps, verify README matches current GHA workflow. Part of the **docs cluster** with #1021/#1430/#1431.

#### #1021 — Investigate/document how to always get latest nuget version
- **Files:** `dependabot.yml`/`renovate.json` (add) in `.github/` and/or `README.md`/wiki.
- **Review:** Low. **Breaking:** No. **Already fixed:** No.
- Discussion converged on floating versions (`*`) vs. an update bot (Renovate/Dependabot). No such config exists in the repo. Pick an approach, add config, document. Mostly decision-making.

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

### M

#### #597 — Additional attachments are ignored when sending email
- **Files:** `SIL.Core/Email/MailToEmailProvider.cs`, `LinuxEmailProvider.cs`, `ThunderbirdEmailProvider.cs` + their tests (edit)
- **Review:** Medium — per-client protocol quirks. **Breaking:** No. **Already fixed:** No.
- `MailToEmailProvider.GetAttachments()` comma-joins into one `&attachment=` (invalid for `mailto:`); `ThunderbirdEmailProvider` similarly uses a single param; `LinuxEmailProvider` correctly repeats `--attach`; macOS correctly iterates. Fix the format per provider (repeat the key). Note: a known upstream `xdg-email` bug (Feb 2025 comment) may still drop all but the last attachment even when formatted correctly — document the limitation.

#### #1007 — update/test LfMerge packaging
- **Files:** none in libpalaso; work is in the LfMerge repo. `debian-src/` here is vestigial (2008, nant-based).
- **Review:** Medium. **Breaking:** No. **Already fixed:** Unclear.
- Empty-body stub sub-task of #993, no comments, last touched 2020-12-08. Triage: close or move to LfMerge.

#### #1008 — update/test FieldWorks packaging
- **Files:** none in libpalaso; FieldWorks installer/packaging scripts.
- **Review:** Medium. **Breaking:** No. **Already fixed:** Unclear.
- Empty-body stub sub-task of #993. Since #1001 (FW nuget adoption) is still open, FW packaging can't be complete. Triage: close or move to FieldWorks.

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
- Done: non-WinForms libs target `netstandard2.0`; major WinForms projects target `net8.0-windows`; AppVeyor removed (`cc60efbb`); GHA matrix builds net8.0 + net8.0-windows. Remaining: add `net8.0-windows` to `SIL.Windows.Forms.Scripture` (and verify GeckoBrowserAdapter), and enable NRT solution-wide (`LangVersion=8` set but `<Nullable>` absent) — the latter is the bulk and a deliberate later phase.

#### #1411 — Calls to obsolete methods
- **Files:** `SIL.WritingSystems/Sldr.cs`, `SIL.Core/Reporting/AnalyticsEventSender.cs`, `SIL.Windows.Forms/Miscellaneous/X11.cs`, `SIL.Windows.Forms.Keyboarding/Windows/Win32.cs`, `GeckoBrowserAdapter/NativeReplacements.cs`, `WSSpellingControl.cs`, `WritingSystemSetupModel.cs`
- **Review:** Medium. **Breaking:** Maybe — removing `[Obsolete]` public members is semver-major; `WebRequest`→`HttpClient` can change exception types. **Already fixed:** No (all 7 sites still present).
- Four categories: 3× `Assembly.LoadWithPartialName` are Mono-compat guards (likely deletable now); `Uri.EscapeUriString`→`EscapeDataString`; `WebRequest.Create`→`HttpClient` (the larger async refactor, in Sldr.cs + AnalyticsEventSender.cs); the `GetSpellCheckComboBoxItems`/`CurrentSpellChecker` pair already `[Obsolete]` — inline/replace the `WSSpellingControl` callsite. **Mild overlap with #1337** (WS setup area).

#### #1428 — GlobalMutex uses hardcoded /var/lock (deprecated on modern Linux)
- **Files:** `SIL.Core/Threading/GlobalMutex.cs` (edit), `SIL.Core.Tests/Threading/GlobalMutexTests.cs` (edit)
- **Review:** Medium — fallback strategy still debated. **Breaking:** Maybe (different versions wouldn't mutually exclude during upgrade). **Already fixed:** No.
- Line ~262 still `Path.Combine("/var/lock", name)`. An XDG_RUNTIME_DIR fallback added in `3301b97a` (2021) was effectively dropped by `1bbd00bb` (Jun 2024, macOS rewrite). On modern Linux `/var/lock`→`/run/lock` is root-only and systemd plans removal in v258. Options unresolved: `/dev/shm` fallback vs `XDG_RUNTIME_DIR` (user-scoped) vs `ExplicitGlobalMutexAdapter`. **Bundle with #359; coordinate with PR #1504.**

### L

#### #1001 — Use nuget packages in FieldWorks
- **Files:** none in libpalaso; work is the Gerrit change `gerrit.lsdev.sil.org/c/FieldWorks/+/7869`.
- **Review:** High — ICU data, COM interop, 32-bit Windows. **Breaking:** Yes (FW dev workflow). **Already fixed:** No.
- Sub-task of #993, stale since Nov 2021 (3/7 items checked). Remaining blockers (COM test failures, 32-bit ICU errors) are FieldWorks-side. Nothing for libpalaso unless ICU failures trace to the libpalaso nuget artifacts.

#### #1089 — ICU collation parsing and generation problems
- **Files:** `SIL.WritingSystems/SimpleRulesParser.cs`, `LdmlCollationParser.cs`, `SimpleCollationRulesParserTests.cs`
- **Review:** High — deep ICU tailoring-syntax knowledge. **Breaking:** Maybe (changes generated ICU rules / stored LDML). **Already fixed:** No.
- `SimpleRulesParser.IcuEscape()` blindly escapes all non-alphanumeric ASCII including `/`, so `a/A` (meant as tertiary `a <<< A`) becomes semantically wrong; `LdmlCollationParser.GetIcuRulesFromCollationNode()` copies the `<cr>` CDATA verbatim without validation. Parse `/` and emit `<<<`; stop double-escaping on round-trip. Stored ICU rules aren't actually tested since the code regenerates from simple rules.

#### #1505 — Linux: keyboard switching broken since Ubuntu 24.04
- **Files:** `SIL.Windows.Forms.Keyboarding/Linux/GnomeShellIbusKeyboardSwitchingAdaptor.cs` (edit), possibly the retrieving adaptor; likely a new bundled GNOME Shell extension.
- **Review:** High — GNOME Shell dev, hard to CI-test. **Breaking:** Maybe (changes packaging/install). **Already fixed:** No.
- `SelectKeyboard()` still calls `--method org.gnome.Shell.Eval` with `imports.ui.status.keyboard…` (lines 36–38). `org.gnome.Shell.Eval` was deprecated in GNOME 41 and removed in the version shipping with Ubuntu 24.04+, so switching silently fails. Fix likely requires deploying a custom GNOME Shell extension exposing a replacement D-Bus method (or an alternative like `ibus engine`). No fallback exists today. Most complex of the two keyboard issues.

### XL

#### #993 — Create and use nuget packages in libpalaso and related projects (umbrella)
- **Files:** cross-repo meta-issue; libpalaso-side files (`build.yml`, `Directory.Build.props`/`.targets`, per-project `.csproj`) were all done when `feature/nuget` merged 2021-02-23.
- **Review:** High (spans 6+ repos). **Breaking:** Maybe (downstream build-process change). **Already fixed:** Partially — **libpalaso's own work is done**; ~16/35 checklist items remain, all in external repos.
- Long-running (2020–) dashboard. Remaining items are blockers in FieldWorks (#1001), LfMerge (#1007), Chorus (#1002), docs (#1009/#1010), autosrtests (#1016). Consider retitling/splitting; its value now is status-tracking.

#### #1425 — Prepare to migrate away from irrKlang
- **Files:** `SIL.Media/WindowsAudioSession.cs`, `SIL.Media/AudioFactory.cs`, `SIL.Media/SIL.Media.csproj` (remove irrKlang ref), remove `lib/win-x64|x86/irrKlang.NET4.dll`, possibly new NAudio recorder files.
- **Review:** High — audio APIs differ; needs hardware to test. **Breaking:** Maybe (subtle recording-behavior changes; net8 Windows already throws today). **Already fixed:** No.
- irrKlang's vendor confirmed no updates/new licenses. `AudioFactory.CreateAudioSession` already throws `PlatformNotSupportedException` on .NET8+ Windows, blocking net8 Windows consumers. NAudio (1.10.0) is already a dependency and used for playback; migrate the `IrrKlang.IAudioRecorder` recording path to NAudio `WaveInEvent`/`WaveFileWriter` and drop the NET462/48 conditional blocks. **Same project as #1266; collides with the PR #1325 rename — settle the rename first.**

---

## 5. Closed PRs with undeleted branches

After `git fetch --prune`, **49** branches remain on `origin`. Cross-referencing each against the PR list, these branches are the head of a **closed (or merged) same-repo PR** yet were never deleted. (Fork PRs don't leave branches on `origin`, so they're out of scope; merged same-repo PRs normally auto-delete — none did except the three noted below.) The two **open** PRs' branches (`fix/global-mutex-abandoned-recovery` → #1504, `sil.windows.forms.media` → #1325) are covered in §3 and excluded here.

**Recommendation key:** *Delete* = work landed elsewhere, was abandoned, or is obsolete — branch is safe to remove. *Keep* = author asked to retain, or it's a long-lived maintenance branch.

### Closed-unmerged PRs

| Branch | PR | Closed | Why it closed | Issue action | Branch |
|--------|----|:------:|---------------|--------------|:------:|
| `bug/paint-file-crash` | [#1254](https://github.com/sillsdev/libpalaso/pull/1254) | 2023-06-05 | Fix abandoned (never reviewed to merge) | **Create issue** — crash still live (see below) | Keep until triaged → then delete |
| `task/merge60` | [#1141](https://github.com/sillsdev/libpalaso/pull/1141) | 2024-08-26 | libpalaso-6.0→master merge stalled on coordination | Optional: open a tracking issue for the remaining 6.0→master merge | **Keep** — author explicitly asked to retain the branch |
| `daymonth-monthday` | [#1458](https://github.com/sillsdev/libpalaso/pull/1458) | 2025-09-05 | Punted; superseded by merged [#1461](https://github.com/sillsdev/libpalaso/pull/1461) | None — issue [#1456](https://github.com/sillsdev/libpalaso/issues/1456) already closed | Delete |
| `github-actions` | [#1329](https://github.com/sillsdev/libpalaso/pull/1329) | 2024-07-19 | "Absorbed into [#1326](https://github.com/sillsdev/libpalaso/pull/1326)" (merged) | None — GHA build now in place | Delete |
| `feat/net6.0` | [#1226](https://github.com/sillsdev/libpalaso/pull/1226) | 2022-09-21 | Couldn't build net6 on TC/AppVeyor then; superseded by merged [#1227](https://github.com/sillsdev/libpalaso/pull/1227) | None — .NET work now tracked by #1320 | Delete |
| `Use_SIL_NAudio` | [#943](https://github.com/sillsdev/libpalaso/pull/943) | 2020-06-17 | SIL NAudio fork no longer relied on (author searched, found nothing) | Optional: note the fork investigation in #1425 | Delete |
| `add-indexof-based-on-icomparer-of-string` | [#1096](https://github.com/sillsdev/libpalaso/pull/1096) | 2021-09-17 | Author concluded the `IndexOf(IComparer)` semantics are a logical contradiction | None — dead end, don't revive | Delete |
| `bugfix/master/ldata_sldr` | [#668](https://github.com/sillsdev/libpalaso/pull/668) | 2018-10-30 | Old, abandoned | None | Delete |
| `feature/LinuxSyslog` | [#369](https://github.com/sillsdev/libpalaso/pull/369) | 2016-03-24 | Old merge-PR, abandoned | None | Delete |
| `feature/LinuxSyslog-2.6` | [#366](https://github.com/sillsdev/libpalaso/pull/366), [#368](https://github.com/sillsdev/libpalaso/pull/368) | 2016-03-24 | Old merge-PRs, abandoned | None | Delete |
| `ParatextSplitter` | [#65](https://github.com/sillsdev/libpalaso/pull/65) | 2014-07-16 | Old, abandoned (12 yrs) | None | Delete |

### Merged PRs whose branch wasn't auto-deleted

| Branch | PR | Merged | Note | Branch |
|--------|----|:------:|------|:------:|
| `TrimWav` | [#363](https://github.com/sillsdev/libpalaso/pull/363) | 2016-03-07 | `AudioRecorder.TrimWavFile()` — work landed | Delete |
| `feature/master/silbuildtasks` | [#643](https://github.com/sillsdev/libpalaso/pull/643), [#649](https://github.com/sillsdev/libpalaso/pull/649) | 2018-03 | SIL.BuildTasks.dll — work landed | Delete |
| `libpalaso-4.1` | [#687](https://github.com/sillsdev/libpalaso/pull/687) | 2018-04-20 | Release branch (also the head of a "merge into master" PR) | **Keep** — release branch |

### The one branch that warrants a new issue

#### `bug/paint-file-crash` ([#1254](https://github.com/sillsdev/libpalaso/pull/1254)) — crash adding metadata to a Paint-authored JPG
- The fix was never merged and **no issue tracks it**. The crash path is still live: `MetadataCore.FileFormatSupportsMetadata()` ([MetadataCore.cs:435](SIL.Core/ClearShare/MetadataCore.cs#L435)) calls `TagLib.File.Create(path)` inside `RetryUtility.Retry` with no catch, so a file TagLib can't parse (the reporter's `star.jpg` from MS Paint on Win10) throws straight up through `Write()` ([MetadataCore.cs:469](SIL.Core/ClearShare/MetadataCore.cs#L469)). The PR's fix was a one-line `try/catch ⇒ return false`. The recent BL-16221 PalasoImage hardening (`cca708f4`) touched adjacent code but not this method.
- **Recommended:** open a small bug issue capturing the repro (the `star.jpg` from the PR thread) and the one-line guard, then delete the branch. Keep the branch only as long as it's the sole record of the repro.

### Note on the remaining ~36 undeleted branches (no associated PR — out of scope)

These were pushed directly (predating or bypassing the PR workflow), so they aren't "closed PRs." For completeness:
- **Keep (intentional maintenance/release branches):** `libpalaso-2.6`/`-3.1`/`-4.0`/`-4.2`/`-6.0`, `wesay1.2`/`1.4`/`1.5`, `Bloom3.7`/`Bloom-5.2`, `HearThis1.4`, `FW7.2`, `LIFT-0.15`, `DotNet35Profile`, `DotNet40ClientProfile`.
- **Stale experiment/feature branches, likely deletable** (each would need a quick owner check; not tied to any PR): `BL-16414-imagetoolbox-dpi`, `HandleLaunchesRight`, `IWritingSystemDefinition`, `ImproveGACookie`, `LT-16358a`, `LdmlDataMapperToDumpXmlReaderAndXmlWriter`, `QueryWork`, `TrimWav`-era siblings, `animator-int`, `bugfix/master/ldata_sldr`, `feature/master/20180319alltags`, `feature/master/ethnologueFeb18update`, `feature/master/fixldata_updatesldr`, `fix/CheckVersion`, `main-copy-for-testing`, `move-metadata`, `naudioForWeSay`.

---

*Generated by a technical review of all 34 open issues (each read with its full conversation) plus the 2 open PRs, checked against the current `issues`-branch checkout. §5 cross-references the 1,402-PR history against the 49 surviving `origin` branches. No issues, PRs, or branches were created or modified.*
