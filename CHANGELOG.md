# Change Log

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/)
and this project adheres to [Semantic Versioning](http://semver.org/).

<!-- Available types of changes:
### Added
### Changed
### Fixed
### Deprecated
### Removed
### Security
-->

## [Unreleased]

### Added
- [SIL.DblBundle] DblMetadata.Load overload to allow deserialization from a TextReader.
- [SIL.Scripture] Versification.Table.Load overload to allow deserialization from a TextReader.
- [SIL.DblBundle] TextBundle<TM, TL>.GetVersification (to replace deprecated CopyVersificationFile)
- [SIL.DblBundle] TextBundle<TM, TL>.GetFonts (to replace deprecated CopyFontFiles)
- [SIL.DblBundle] TextBundle<TM, TL>.GetLdml (to replace deprecated CopyLdmlFile)

### Changed

- Add build number to AssemblyFileVersion
- [SIL.Core] and [SIL.Core.Desktop] Move several classes back to SIL.Core from SIL.Core.Desktop to make
  them available to .NET Standard clients.
  - IO/PathUtilities
  - IO/TempFileForSafeWriting
  - Reporting/AnalyticsEventSender
  - Reporting/ConfigurationException
  - Reporting/ConsoleErrorReporter
  - Reporting/ConsoleExceptionHandler
  - Reporting/ErrorReport
  - Reporting/ExceptionHandler
  - Reporting/ExceptionHelper
  - Reporting/Logger
  - Reporting/ReportingSettings
  - Reporting/UsageReporter
- [SIL.Windows.Forms] Remove unnecessary dependency on NAudio
- [SIL.Core] Deprecate `ExceptionHandler.Init()` method in favor of more explicit version
  `ExceptionHandler.Init(ExceptionHandler)`, e.g. `ExceptionHandler.Init(new WinFormsExceptionHandler())`
- [SIL.Core] Move `HandleUnhandledException()` method from derived classes to base class
- [SIL.DblBundle.Tests] Create nuget package
- Improve nuget symbol packages
- Changed to use the new Registered trademark logo (in About Box). Removed 132x148 logo.
- [SIL.DblBundle] Deprecated CopyVersificationFile, CopyFontFiles and CopyLdmlFile in favor of Get...

### Fixed

- [SIL.Windows.Forms.Keyboarding] Merge missing `Keyman*Interop.dll` into the assembly (#865)
- [SIL.Windows.Forms] Use signed version of `ibusdotnet.dll` (#865)
- [SIL.Windows.Forms] Merge `Interop.WIA.dll`, `DialogAdapters.dll`, and `MarkdownDeep.dll` into the assembly
- [SIL.Media] Fix missing `irrKlang.NET4.dll` exception by copying it to lib folder in output

## [7.0.0] - 2019-08-29

### Changed

- Create nuget packages
- [SIL.Media] `IAudioRecorder.SelectedDevice` now returns a `IRecordingDevice` which both the
  NAudio and AlsaAudio `RecordingDevice` implement. This allows to use the same assembly
  on both Windows and Linux (although the limitations what works and what doesn't work remain the
  same)
- [SIL.Media] cleanup of `AudioSession` API: rename `AudioIrrKlangSession` to `WindowsAudioSession`.
  `AudioAlsaSession` and `WindowsAudioSession` are now  internal (they were never intended to
  be used directly)
- [SIL.Media] move some interfaces around so that they live in `SIL.Media` instead of
  `SIL.Media.Naudio`: `IAudioRecorder`, `RecordingState`, `IAudioPlayer`

### Added

- [SIL.Core] new properties for processor architecture: `Platform.ProcessArchitecture` and
  `Platform.IsRunning64Bit`
- [SIL.NUnit3Compatibility] new project/package that allows to use NUnit3 syntax with NUnit2
  projects
