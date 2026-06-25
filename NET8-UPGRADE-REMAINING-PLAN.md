# .NET 8 Upgrade — Remaining Work Plan

**Tracks:** [#1320 "Upgrade to .NET8"](https://github.com/sillsdev/libpalaso/issues/1320) · [PR #1325 "Renamed SIL.Media to SIL.Windows.Forms.Media"](https://github.com/sillsdev/libpalaso/pull/1325)
**Branch:** `issues` · **Assessed against checkout:** 2026-06-25 (≈master)
**Author of original issue/PR (josephmyers) has changed roles** and stated the work now falls outside his purview; PR #1325 has languished with merge conflicts. This plan re-scopes what is left.

---

## 1. Where things actually stand

A lot of #1320 has already shipped since the 2024 discussion. The issue checkboxes are **stale**. Reconciling the checklist against the current tree:

| Original checklist item | Issue box | Reality on `issues`/`master` |
| --- | :---: | --- |
| Research L10nSharp refs in non-WinForms projects | ☑ | Done — only WinForms projects use L10nSharp. |
| Long-term branching strategy & roadmap + nuget publishing | ☑ | Done — roadmap posted in issue thread. |
| Replace TeamCity with GHA builds | ☑ | Done — `.github/workflows/build.yml`. |
| **Replace AppVeyor with GHA builds** | ☐ | **Done** — `appveyor.yml` removed (commit `cc60efbb`); nuget now built/pushed via GHA. *Checkbox is stale.* |
| Upgrade all projects to Framework 4.8 | ☑ | Done — `Directory.Build.props` defaults to `net462;net48`. |
| Add .NET 8 tests for cross-platform libraries | ☑ | Done — `net8.0` added to cross-platform + test projects; CI matrix runs `net8.0`. |
| **Upgrade WinForms projects to .NET8-Windows** | ☐ | **Mostly done** — most `SIL.Windows.Forms.*` libs + `SIL.Media` multi-target `net8.0-windows`; CI matrix includes `net8.0-windows` and packs nuget from it. **Still missing: `SIL.Windows.Forms.Scripture` and `SIL.Windows.Forms.GeckoBrowserAdapter`** (both inherit only `net462;net48`). Plus the Framework-only app/util projects (see §2.1). |
| **Add support for NRT (nullable reference types)** | ☐ | **Not started** — `LangVersion` is `8` globally; no `<Nullable>` anywhere except `TestApps/ArchivingTestApp`. This is the real open item. |

**Bonus already done (not on the original list):**
- `SIL.DictionaryServices` now has `netstandard2.0` (the explicit ask in the 2024 thread).
- `MetadataCore` and `LicenseInfo` were moved out of WinForms so they can be used independently (#1478) — directly relevant to the SIL.Media decision below.
- L10NSharp bumped and targeting net8 (#1500).

**Net:** Of the original 8 items, the only substantive remaining engineering work is **NRT adoption**. Everything else is either done or is a stale checkbox to tick. The other live decision is the **fate of PR #1325 (SIL.Media)**.

---

## 2. Remaining work items

### 2.1 Finish framework coverage (small)

Two **WinForms libraries** were skipped by the net8 sweep and still inherit only `net462;net48`:

- `SIL.Windows.Forms.Scripture` → add `net8.0-windows` (straightforward; it only references `SIL.Core`, `SIL.Scripture`, `SIL.Windows.Forms`, all of which already target it).
- `SIL.Windows.Forms.GeckoBrowserAdapter` → **verify before adding** — it wraps Geckofx (`Geckofx-Core`/`Geckofx-Winforms`), which is Framework-only, so this one may have to stay `net462;net48` until/unless the Gecko dependency is replaced. Treat as a decision, not a mechanical add.

Separately, two **app/util projects** target Framework only and were never in scope for net8:

- `AddSortKey/AddSortKey.csproj` → `net462` only
- `SIL.Installer/SIL.Installer.csproj` → `net48` only (and `LangVersion latest`)

**Action:** Add `net8.0-windows` to `SIL.Windows.Forms.Scripture`; investigate Gecko for `GeckoBrowserAdapter`; confirm `AddSortKey`/`SIL.Installer` are *intentionally* Framework-only and document that (see Decision D).

### 2.2 NRT (nullable reference types) adoption — the main remaining engineering item

This is the one genuinely open, breaking-ish, multi-day item. It is large and should be staged. See Decision B for *how*, and §4 for sequencing. Key sub-tasks:

- Decide bump of `LangVersion` (currently `8`) — see Decision C.
- Choose rollout strategy (big-bang vs. per-project) — Decision B.
- Per project: add `<Nullable>enable</Nullable>`, annotate public API, resolve `CS86xx` warnings.
- Decide whether NRT warnings are errors (interaction with existing `WarningsAsErrors=NU1605;CS8002`).

### 2.3 Resolve PR #1325 / SIL.Media (decision-gated)

PR #1325 (pure rename `SIL.Media` → `SIL.Windows.Forms.Media`) is stale, conflicted, and unowned. `SIL.Media` still exists under the old name and now multi-targets `net8.0-windows`. The original author suggested it "may be easier to start from scratch." This needs a decision (Decision A) before any code is touched, then a fresh PR off current `master`.

---

## 3. Decisions that need to be made (with pros/cons)

### Decision A — What to do with `SIL.Media`

**Recommendation:** If the cross-platform audio need is real and near-term, do **A2** (and skip A1 entirely to avoid two breaking renames). If it is not near-term, do **A3** now and revisit when a non-WinForms consumer actually materializes. **A1 is the weakest** — it pays the full breaking-change cost (semver major + nuget ID change) without buying either cleanliness or capability. Whatever is chosen, **open a fresh PR off current `master`** rather than reviving #1325, and close #1325 with a pointer.

<details>
<summary>Context & options (A1 rename / A2 split / A3 leave)</summary>

This is the crux of PR #1325. Three options surfaced in the thread.

> Context: `SIL.Media` contains a small handful of WinForms-specific things (UI controls like `PeakMeterCtrl`, `RecordingDeviceIndicator`, `SoundFieldControl`) plus a larger body of non-UI audio code (NAudio/ALSA recording, `FFmpegRunner`, `MediaInfo`). It references native `irrKlang` for `net462/net48` only. Tom noted HearThis/Bloom will eventually want the non-UI parts without a WinForms dependency. #1478 already set a precedent by extracting `MetadataCore`/`LicenseInfo`.

**Option A1 — Pure rename → `SIL.Windows.Forms.Media`** (what PR #1325 does)

| Pros | Cons |
| --- | --- |
| Smallest, mostly-mechanical change (file/namespace/csproj rename). | Misleading: keeps non-WinForms audio code bundled under a "Windows.Forms" name. |
| Removes the current misnomer (`SIL.Media` today *is* WinForms-dependent). | Breaking change for consumers (`+semver:major`, nuget ID change) for no functional gain. |
| Aligns the name with the rest of the `SIL.Windows.Forms.*` family. | Doesn't advance the cross-platform goal; a future split would be a *second* breaking rename. |

**Option A2 — Split into `SIL.Media` (cross-platform) + `SIL.Windows.Forms.Media` (UI)**

| Pros | Cons |
| --- | --- |
| Lets HearThis/Bloom/non-WinForms apps consume audio without WinForms (the stated long-term goal). | Most work; must untangle which types are truly UI-free and target `netstandard2.0`/`net8.0`. |
| Consistent with the `Core`/`Windows.Forms` split pattern already used elsewhere (and #1478). | Two breaking changes for consumers if done after a rename — so do it *instead of* A1, not after. |
| `MetadataCore`/`LicenseInfo` precedent shows the team accepts this shape. | Native `irrKlang` / NAudio dependencies need careful framework conditioning. |

**Option A3 — Leave as `SIL.Media`, do nothing**

| Pros | Cons |
| --- | --- |
| Zero churn, zero breaking change; consumers untouched. | Name stays misleading. |
| Frees effort for NRT (the higher-value item). | Punts the cross-platform split that's eventually needed anyway. |

</details>

### Decision B — NRT rollout strategy

**Recommendation:** **B2** (incremental, per-project), in dependency order: `SIL.Core` → `SIL.Core.Desktop` → `SIL.WritingSystems`/`SIL.Scripture`/`SIL.Lift`/`SIL.DblBundle` → `SIL.Windows.Forms.*`. Set `<Nullable>enable</Nullable>` per-project as each is cleaned, rather than globally in `Directory.Build.props`, until the last one is done.

<details>
<summary>Context & options (B1 big-bang / B2 per-project / B3 new-code-only)</summary>

**Option B1 — Big-bang (enable `<Nullable>enable</Nullable>` repo-wide in `Directory.Build.props`, fix everything)**

| Pros | Cons |
| --- | --- |
| One consistent state; no "half-annotated" limbo. | Hundreds–thousands of warnings at once across ~20 projects; huge single PR, hard to review. |
| Public API annotations all land together (cleaner for consumers). | High risk of churn/merge conflicts with everything else in flight. |

**Option B2 — Incremental, per-project (recommended)**

| Pros | Cons |
| --- | --- |
| Each PR is reviewable; warnings bounded per project. | Repo is in a mixed state for a while. |
| Can start with leaf libraries (`SIL.Core`) and work up the dependency graph. | Annotating a low-level lib re-surfaces warnings in dependents — order matters. |
| Easy to pause/resume around other work. | Slower to "fully done." |

**Option B3 — Annotate-only-new-code (`<Nullable>annotations</Nullable>` or `#nullable enable` per file)**

| Pros | Cons |
| --- | --- |
| Near-zero immediate work; no warning flood. | Inconsistent; most of the codebase never gets the benefit. |
| Captures intent on new APIs. | Public API surface stays unannotated for consumers. |

</details>

### Decision C — Bump `LangVersion` from `8`?

**Recommendation:** NRT does **not** require this bump — keep it out of the critical path. Treat a `LangVersion` bump as an optional, separate, well-tested change *after* NRT lands, validated against the `net462`/`net48` matrix legs.

<details>
<summary>Context & options (keep 8 / bump to modern)</summary>

`LangVersion` is currently pinned to `8` in `Directory.Build.props` (one project, `SIL.Installer`, overrides to `latest`).

| Option | Pros | Cons |
| --- | --- | --- |
| **Keep `8`** | NRT works fine at C# 8; conservative; predictable behavior across `net462`/`net48`. | Misses later C# conveniences; the override in `SIL.Installer` stays inconsistent. |
| **Bump to a fixed modern version (e.g. `latest` or `12`)** | Better ergonomics while annotating; consolidates the `SIL.Installer` override. | Some newer C# features need polyfills/`IsExternalInit` on `net462`/`net48`; must verify nothing regresses on Framework targets. |

</details>

### Decision D — App/util projects on Framework (`AddSortKey`, `SIL.Installer`)

**Recommendation:** Leave them, and **note the exception in the issue** so the box can be checked honestly.

<details>
<summary>Context & options (leave / move to net8)</summary>

| Option | Pros | Cons |
| --- | --- | --- |
| **Leave Framework-only (recommended)** | They're an internal sort tool and an installer — no cross-platform need; zero risk. | Issue item "upgrade WinForms projects to .NET8-Windows" not literally 100%. |
| **Move to net8.0-windows too** | Literal completeness. | Effort/risk with no consumer benefit (installer tooling is Framework-oriented). |

</details>

### Decision E — Timeline for dropping Framework `net462`

**Recommendation:** Out of scope for #1320's *remaining* work; track separately. Gate on confirming FieldWorks/LfMerge no longer need 4.6.2.

<details>
<summary>Context & options (keep net462 / drop it)</summary>

The 2024 roadmap had Framework 4.6.2 dropping in "Phase 3." Not urgent, but worth a decision so it doesn't drift.

| Option | Pros | Cons |
| --- | --- | --- |
| **Keep `net462` for now** | Existing consumers (FieldWorks/LfMerge era) may still need it. | Every target multiplies CI time and conditional code. |
| **Drop `net462`, keep `net48`** | Simplifies multi-targeting and conditionals; faster CI. | Breaking for any consumer still on 4.6.2 — needs consumer audit (#1007, #1008). |

</details>

---

## 4. Recommended sequence

1. **Housekeeping (XS):** Update issue #1320 — check the AppVeyor box (it's done), and add notes for the remaining framework gaps so the WinForms box reflects reality. Reconciles the issue with the tree.
2. **Finish framework coverage (S):** Add `net8.0-windows` to `SIL.Windows.Forms.Scripture`; investigate Gecko for `GeckoBrowserAdapter`; document the `AddSortKey`/`SIL.Installer` Framework-only decision (§2.1, Decision D). Only then is the WinForms→net8 box honestly checkable.
3. **Resolve #1325 (decision-gated):** Make Decision A. Close #1325; if A1/A2, open a fresh PR off `master` with `+semver:major` in the commit message.
4. **NRT, incremental (L, the main work):** Decision B2 in dependency order. One PR per project (or small cluster), starting at `SIL.Core`.
5. **Optional follow-ups:** `LangVersion` bump (Decision C); `net462` drop (Decision E) once consumers are confirmed.

## 5. Open questions for the team

- **A:** Is there a near-term non-WinForms consumer of `SIL.Media`'s audio code (HearThis/Bloom)? That answer decides A2 vs. A3.
- **NRT:** Should nullable warnings be promoted to errors (extend `WarningsAsErrors`) once a project is clean, to prevent regressions?
- **E:** Can we confirm FieldWorks/LfMerge no longer require `net462` (ties into #1007/#1008)?
