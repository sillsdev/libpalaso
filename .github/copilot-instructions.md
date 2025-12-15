# LibPalaso Copilot Instructions

You are an expert C# developer assisting with the `sillsdev/libpalaso` repository. Follow these critical guidelines for all code generation.

## 1. Code Standards & Quality
- **Modern C#:** Prefer modern C# syntax (e.g., pattern matching, switch expressions, file-scoped namespaces) unless maintaining legacy consistency.
- **Null Safety:** Strictly adhere to Nullable Reference Types. Explicitly handle potential nulls; do not suppress warnings with `!` unless absolutely necessary.
- **Cross-Platform:** Remember that LibPalaso runs on Windows and Linux (Mono/.NET). Avoid Windows-specific APIs (like `Registry` or hardcoded `\` paths) unless wrapped in OS checks.

## 2. Testing
- **Framework:** Use **NUnit** for all unit tests.
- **Mocking:** Use **Moq** for mocking interfaces when necessary.
- **Structure:** When writing tests, follow the `Given_When_Then` or `State_Action_Result` naming convention for clarity.

## 3. Documentation & Process (Critical)
- **Update the Changelog:** If the suggested code changes functionality, fixes a bug, or adds a feature, you **must** generate an update for `CHANGELOG.md`.
    - Look for the `## [Unreleased]` section at the top of the changelog.
    - Insert a bullet point under the appropriate subsection: `### Added`, `### Changed`, or `### Fixed`.
    - If the subsections do not exist under `[Unreleased]`, create them.
    - Format: `- **Scope:** Description of the change.`
