# .NET 8 Upgrade — Plan

**Tracks:** [#1320 "Upgrade to .NET8"](https://github.com/sillsdev/libpalaso/issues/1320) · [PR #1325 "Renamed SIL.Media to SIL.Windows.Forms.Media"](https://github.com/sillsdev/libpalaso/pull/1325)

Goal: multi-target the libraries for Framework (`net462`/`net48`), `netstandard2.0`, and `net8.0` / `net8.0-windows`, publish nuget from CI, and adopt nullable reference types.

---

## 1. Status

**Build & packaging**

- [x] GHA builds (replacing TeamCity) — `.github/workflows/build.yml`.
- [x] GHA nuget publishing (replacing AppVeyor) — `appveyor.yml` removed; nuget packed/pushed from the `net8.0-windows` CI leg.
- [x] CI matrix builds `net462`, `net48`, `net8.0`, `net8.0-windows`.

**Frameworks**

- [x] All libraries target Framework 4.8 — `Directory.Build.props` defaults to `net462;net48`.
- [x] Cross-platform libraries multi-target `netstandard2.0` (incl. `SIL.DictionaryServices`).
- [x] `net8.0` added to cross-platform + test projects.
- [x] `net8.0-windows` added to most WinForms libraries (`SIL.Windows.Forms`, `.Archiving`, `.DblBundle`, `.Keyboarding`, `.WritingSystems`) and `SIL.Media`.
- [ ] `net8.0-windows` for `SIL.Windows.Forms.Scripture` — see §2.1.
- [ ] `net8.0-windows` for `SIL.Windows.Forms.GeckoBrowserAdapter` — blocked on a Framework-only dependency; see §2.1.

**Code**

- [x] `MetadataCore` / `LicenseInfo` moved out of WinForms so they're usable independently (#1478).
- [x] L10NSharp bumped and targeting net8 (#1500).
- [ ] Nullable reference types (NRT) — not started; `LangVersion` is `8` and `<Nullable>` is unset. This is the main remaining engineering work. See §2.2.
- [ ] `SIL.Media` naming/structure — open decision tied to PR #1325. See §2.3.

---

## 2. Remaining work

### 2.1 Finish framework coverage

Two WinForms libraries still inherit only `net462;net48`:

- **`SIL.Windows.Forms.Scripture`** → add `net8.0-windows`. Straightforward; it only references `SIL.Core`, `SIL.Scripture`, and `SIL.Windows.Forms`, all of which already target it.
- **`SIL.Windows.Forms.GeckoBrowserAdapter`** → verify before adding. It wraps Geckofx (`Geckofx-Core`/`Geckofx-Winforms`), which is Framework-only, so it may have to stay on `net462;net48` until the Gecko dependency is replaced. Treat as a decision, not a mechanical add.

Two app/util projects are intentionally Framework-only (no cross-platform need): `AddSortKey` (`net462`) and `SIL.Installer` (`net48`). Confirm and document this rather than upgrading them (see Decision D).

### 2.2 Nullable reference types (NRT)

The one large, breaking-ish, multi-day item. Per project: add `<Nullable>enable</Nullable>`, annotate the public API, resolve the `CS86xx` warnings. Decide the rollout strategy (Decision B), whether to bump `LangVersion` (Decision C), and whether to promote nullable warnings to errors (Decision F).

### 2.3 SIL.Media / PR #1325

PR #1325 (pure rename `SIL.Media` → `SIL.Windows.Forms.Media`) is stale and conflicted. `SIL.Media` still exists under the old name and multi-targets `net8.0-windows`. Decide the structure (Decision A) before touching code, then land it as a fresh PR off `master` rather than reviving #1325.

---

## 3. Decisions

### Decision A — What to do with `SIL.Media`

**Recommendation:** If the cross-platform audio need is real and near-term, do **A2** (and skip A1 entirely to avoid two breaking renames). If it is not near-term, do **A3** now and revisit when a non-WinForms consumer actually materializes. **A1 is the weakest** — it pays the full breaking-change cost (semver major + nuget ID change) without buying either cleanliness or capability. Whatever is chosen, land it as a fresh PR off `master`.

<details>
<summary>Context & options (A1 rename / A2 split / A3 leave)</summary>

> Context: `SIL.Media` contains a small handful of WinForms-specific things (UI controls like `PeakMeterCtrl`, `RecordingDeviceIndicator`, `SoundFieldControl`) plus a larger body of non-UI audio code (NAudio/ALSA recording, `FFmpegRunner`, `MediaInfo`). It references native `irrKlang` for `net462/net48` only. HearThis/Bloom will eventually want the non-UI parts without a WinForms dependency. #1478 already set a precedent by extracting `MetadataCore`/`LicenseInfo`.

**Option A1 — Pure rename → `SIL.Windows.Forms.Media`** (what PR #1325 does)

| Pros                                                                      | Cons                                                                                         |
| ------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------- |
| Smallest, mostly-mechanical change (file/namespace/csproj rename).        | Misleading: keeps non-WinForms audio code bundled under a "Windows.Forms" name.              |
| Removes the current misnomer (`SIL.Media` today _is_ WinForms-dependent). | Breaking change for consumers (`+semver:major`, nuget ID change) for no functional gain.     |
| Aligns the name with the rest of the `SIL.Windows.Forms.*` family.        | Doesn't advance the cross-platform goal; a future split would be a _second_ breaking rename. |

**Option A2 — Split into `SIL.Media` (cross-platform) + `SIL.Windows.Forms.Media` (UI)**

| Pros                                                                                              | Cons                                                                                             |
| ------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------ |
| Lets HearThis/Bloom/non-WinForms apps consume audio without WinForms (the stated long-term goal). | Most work; must untangle which types are truly UI-free and target `netstandard2.0`/`net8.0`.     |
| Consistent with the `Core`/`Windows.Forms` split pattern already used elsewhere (and #1478).      | Two breaking changes for consumers if done after a rename — so do it _instead of_ A1, not after. |
| `MetadataCore`/`LicenseInfo` precedent shows the team accepts this shape.                         | Native `irrKlang` / NAudio dependencies need careful framework conditioning.                     |

**Option A3 — Leave as `SIL.Media`, do nothing**

| Pros                                                   | Cons                                                            |
| ------------------------------------------------------ | --------------------------------------------------------------- |
| Zero churn, zero breaking change; consumers untouched. | Name stays misleading.                                          |
| Frees effort for NRT (the higher-value item).          | Punts the cross-platform split that's eventually needed anyway. |

</details>

### Decision B — NRT rollout strategy

**Recommendation:** **B2** (incremental, per-project), in dependency order: `SIL.Core` → `SIL.Core.Desktop` → `SIL.WritingSystems`/`SIL.Scripture`/`SIL.Lift`/`SIL.DblBundle` → `SIL.Windows.Forms.*`. Set `<Nullable>enable</Nullable>` per-project as each is cleaned, rather than globally in `Directory.Build.props`, until the last one is done.

<details>
<summary>Context & options (B1 big-bang / B2 per-project / B3 new-code-only)</summary>

**Option B1 — Big-bang (enable `<Nullable>enable</Nullable>` repo-wide in `Directory.Build.props`, fix everything)**

| Pros                                                              | Cons                                                                                        |
| ----------------------------------------------------------------- | ------------------------------------------------------------------------------------------- |
| One consistent state; no "half-annotated" limbo.                  | Hundreds–thousands of warnings at once across ~20 projects; huge single PR, hard to review. |
| Public API annotations all land together (cleaner for consumers). | High risk of churn/merge conflicts with everything else in flight.                          |

**Option B2 — Incremental, per-project (recommended)**

| Pros                                                                         | Cons                                                                           |
| ---------------------------------------------------------------------------- | ------------------------------------------------------------------------------ |
| Each PR is reviewable; warnings bounded per project.                         | Repo is in a mixed state for a while.                                          |
| Can start with leaf libraries (`SIL.Core`) and work up the dependency graph. | Annotating a low-level lib re-surfaces warnings in dependents — order matters. |
| Easy to pause/resume around other work.                                      | Slower to "fully done."                                                        |

**Option B3 — Annotate-only-new-code (`<Nullable>annotations</Nullable>` or `#nullable enable` per file)**

| Pros                                        | Cons                                                       |
| ------------------------------------------- | ---------------------------------------------------------- |
| Near-zero immediate work; no warning flood. | Inconsistent; most of the codebase never gets the benefit. |
| Captures intent on new APIs.                | Public API surface stays unannotated for consumers.        |

</details>

### Decision C — Bump `LangVersion` from `8`?

**Recommendation:** NRT does **not** require this bump — keep it out of the critical path. Treat a `LangVersion` bump as an optional, separate, well-tested change _after_ NRT lands, validated against the `net462`/`net48` matrix legs.

<details>
<summary>Context & options (keep 8 / bump to modern)</summary>

`LangVersion` is currently pinned to `8` in `Directory.Build.props` (one project, `SIL.Installer`, overrides to `latest`).

| Option                                                     | Pros                                                                                | Cons                                                                                                                            |
| ---------------------------------------------------------- | ----------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------- |
| **Keep `8`**                                               | NRT works fine at C# 8; conservative; predictable behavior across `net462`/`net48`. | Misses later C# conveniences; the override in `SIL.Installer` stays inconsistent.                                               |
| **Bump to a fixed modern version (e.g. `latest` or `12`)** | Better ergonomics while annotating; consolidates the `SIL.Installer` override.      | Some newer C# features need polyfills/`IsExternalInit` on `net462`/`net48`; must verify nothing regresses on Framework targets. |

</details>

### Decision D — App/util projects on Framework (`AddSortKey`, `SIL.Installer`)

**Recommendation:** Leave them Framework-only, and note the exception in the issue so the WinForms→net8 item can be closed honestly.

<details>
<summary>Context & options (leave / move to net8)</summary>

| Option                                 | Pros                                                                        | Cons                                                                            |
| -------------------------------------- | --------------------------------------------------------------------------- | ------------------------------------------------------------------------------- |
| **Leave Framework-only (recommended)** | An internal sort tool and an installer — no cross-platform need; zero risk. | "Upgrade WinForms projects to .NET8-Windows" not literally 100%.                |
| **Move to net8.0-windows too**         | Literal completeness.                                                       | Effort/risk with no consumer benefit (installer tooling is Framework-oriented). |

</details>

### Decision E — Timeline for dropping Framework `net462`

**Recommendation:** Out of scope for the current work; track separately. Gate on confirming FieldWorks/LfMerge no longer need 4.6.2.

<details>
<summary>Context & options (keep net462 / drop it)</summary>

The roadmap envisioned Framework 4.6.2 dropping in a later phase. Not urgent, but worth a decision so it doesn't drift.

| Option                          | Pros                                                           | Cons                                                                            |
| ------------------------------- | -------------------------------------------------------------- | ------------------------------------------------------------------------------- |
| **Keep `net462` for now**       | Existing consumers (FieldWorks/LfMerge era) may still need it. | Every target multiplies CI time and conditional code.                           |
| **Drop `net462`, keep `net48`** | Simplifies multi-targeting and conditionals; faster CI.        | Breaking for any consumer still on 4.6.2 — needs consumer audit (#1007, #1008). |

</details>

### Decision F — Promote nullable warnings to errors?

**Recommendation:** Not while annotating — leave NRT findings as warnings so a project can compile mid-migration. Once a project is fully annotated and clean, add its nullable warnings (`CS86xx`) to `WarningsAsErrors` (currently `NU1605;CS8002`) so regressions can't creep back in.

<details>
<summary>Context & options (warnings-only / errors once clean / errors immediately)</summary>

NRT issues surface as `CS86xx` warnings. Whether they fail the build is independent of enabling `<Nullable>`.

| Option | Pros | Cons |
| --- | --- | --- |
| **Keep as warnings** | Simplest; never blocks a build. | Easy to ignore; annotations silently rot as new nullable holes appear. |
| **Errors once a project is clean (recommended)** | Locks in each project's annotations against regression; scoped per project. | Mixed repo state — some projects enforce, some don't, until migration finishes. |
| **Errors immediately, repo-wide** | Strongest guarantee from day one. | Blocks every build until the whole codebase is annotated; effectively forces the big-bang rollout (conflicts with Decision B2). |

</details>

---

## 4. Recommended sequence

1. **Finish framework coverage (S):** Add `net8.0-windows` to `SIL.Windows.Forms.Scripture`; investigate Gecko for `GeckoBrowserAdapter`; document the `AddSortKey`/`SIL.Installer` Framework-only decision (Decision D).
2. **Resolve #1325 (decision-gated):** Make Decision A. Close #1325; if A1/A2, open a fresh PR off `master` with `+semver:major` in the commit message.
3. **NRT, incremental (L, the main work):** Decision B2 in dependency order, one PR per project (or small cluster), starting at `SIL.Core`.
4. **Optional follow-ups:** `LangVersion` bump (Decision C); `net462` drop (Decision E) once consumers are confirmed.

## 5. Open questions

- **A:** Is there a near-term non-WinForms consumer of `SIL.Media`'s audio code (HearThis/Bloom)? Decides A2 vs. A3.
- **E:** Can we confirm FieldWorks/LfMerge no longer require `net462` (ties into #1007/#1008)?
