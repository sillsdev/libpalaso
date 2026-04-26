# LibPalaso Copilot Instructions

You are an expert C# developer assisting with the `sillsdev/libpalaso` repository. Follow these critical guidelines for all code generation.

## 1. Code Standards & Quality
- **Modern C#:** Prefer modern C# syntax (e.g., pattern matching, switch expressions, file-scoped namespaces) unless maintaining legacy consistency.
- **Null Safety:** Strictly adhere to Nullable Reference Types. Explicitly handle potential nulls; do not suppress warnings with `!` unless absolutely necessary.
- **Cross-Platform:** Cross-Platform: Some libraries are intended to be cross-platform. In those cases, avoid Windows-specific APIs (like `Registry`) and Windows-specific assumptions (such as hardcoded path separators) unless properly guarded or abstracted.

Projects under `SIL.Windows.Forms` are primarily Windows desktop UI libraries (WinForms). Historically, many of these projects have also been used under Mono, but Mono support is no longer actively maintained and should not be assumed.

Windows-specific APIs may be used in these projects where appropriate. These projects are not required to be cross-platform compatible, and Mono support is not a design goal.

However, existing platform checks (such as `Platform.IsMono` or `#if __MonoCS__`) may still exist in the codebase and should not be removed or broken without a clear understanding of their purpose.

## 2. Testing
- **Framework:** Use **NUnit** for all unit tests.
- **Mocking:** Use **Moq** for mocking interfaces when necessary.
- **Structure:** When writing tests, follow the `Given_When_Then` or `State_Action_Result` naming convention for clarity.

## 3. Documentation & Process (Critical)
- **Update the Changelog:** If the suggested code changes functionality, fixes a bug, or adds a feature, you **must** generate an update for `CHANGELOG.md`.
    - Look for the `## [Unreleased]` section at the top of the changelog.
    - Insert a bullet point under the appropriate subsection based on the type of change:
        - `### Added` - for new features, methods, classes, or capabilities
        - `### Fixed` - for bug fixes and corrections
        - `### Changed` - for changes in existing functionality, including BREAKING CHANGES
        - `### Deprecated` - for soon-to-be removed features (mark with `[Obsolete]` attribute)
        - `### Removed` - for features that have been completely removed
        - `### Security` - for security-related fixes or improvements
    - If the subsections do not exist under `[Unreleased]`, create them as needed.
    - Format: `- **[Scope]** Description of the change.` (where Scope is the affected library/namespace)
    - For breaking changes, prefix with `BREAKING CHANGE:` in the description.

- **Commit Message Format:** Follow semantic versioning guidelines in commit messages:
  - Include semantic version tags to indicate the impact of changes:
    - `+semver:major` - for breaking changes that require a major version bump
    - `+semver:minor` - for new features, added functionality, or deprecations that require a minor version bump
    - If the change doesn't require a major or minor version bump (i.e., it's a patch-level change), the `+semver` tag is omitted
  - Format: `Brief description of change +semver:level` (or just the description for patch-level changes)
  - Examples:
    - `Add new IWritingSystemRepository interface +semver:minor`
    - `BREAKING CHANGE: Remove deprecated FileLocator methods +semver:major`
    - `Fix null reference exception in BetterLabel` (no tag needed for patch)
    - `Refactor internal caching mechanism` (no tag needed for patch)
  - **Note:** BREAKING CHANGE can appear in any changelog category but always requires `+semver:major`