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

## [9.0.0] - 2022-06-03

### Added
- [SIL.Core] NamePrefix setting and CleanupTempFolder method added to TempFile
- [SIL.Core] Utility methods to remove XML namespaces
- [SIL.Core.Desktop] Serializable class `UpdateSettings` (settings for getting updates)
- [SIL.Windows.Forms] `CssLinkHref` property to `ShowReleaseNotesDialog` to allow linking to CSS
  file for displaying Markdown output.
- [SIL.Scripture] `IScrVerseRef` interface (largely extracted from `VerseRef`)
- [SIL.Windows.Forms] `ParentFormBase` to allow showing a child form that is modal with respect to
  the parent but not application modal
- [SIL.Windows.Forms] `GraphicsManager` class that allows to select desired GTK version.
  Default: GTK2
- [SIL.Windows.Forms] Options for `FlexibleMessageBox` to show in the taskbar and to show on top of other windows
- [SIL.Windows.Forms.DblBundle] virtual method `SelectProjectDlgBase.CreateFileDialog()` to allow
  customization in derived class (#797)
- [SIL.Windows.Forms.SettingProtection] overload of SetSettingsProtection method that takes a ToolStripItem
- [SIL.WritingSystems] Allow specifying an alias to another Writing System for changing between upper- and lowercase
- [SIL.Core] Extension method to get longest useful substring
- [SIL.Core] Extension method IsLikelyWordForming to include letters, format characters, PUA and marks (diacritics, etc.)
- [SIL.Core.Desktop, SIL.Lift, SIL.Linux.Logging] Added .NET Standard 2.0 target.
- [SIL.Core.Desktop] USBDrive API is only supported in .NET Framework.
- [SIL.Windows.Forms] Caller can override the default save image metadata action from the image toolbox
- [SIL.Core, SIL.Windows.Forms] `IErrorReporter` interface added a simpler overload of NotifyUserOfProblem method, which must be implemented by IErrorReporters.
  (It is acceptable for implementers to just fill some parameters then call the original method)
  `ConsoleErrorReporter` and `WinFormsErrorReporter` implement `IErrorReporter`'s new interface method
- [SIL.Core] Added override of SerializeToFileWithWriteThrough to simplify error handling.
- [SIL.Windows.Forms] Added a CheckedComboBox control
- [SIL.WritingSystems] Added several methods to IetfLanguageTag class to support getting language names.
- [SIL.Windows.Forms.WritingSystems] Added extension method InitializeWithAvailableUILocales
- [SIL.WritingSystems] Added WellKnownSubtag zh-TW.

### Changed

- [SIL.WritingSystems] Update `langtags.json` to the latest
- [SIL.Scripture] Made `VerseRef` class implement new `IScrVerseRef` interface
- [SIL.Forms.Scripture] Changed VerseControl to use `IScrVerseRef` and not depend directly on
  `ScrVerse`
- [SIL.Windows.Forms] Removed dependency on gtk-sharp/gdk-sharp; unmanaged libgtk/libgdk libraries
  get loaded dynamically at runtime
- [SIL.Windows.Forms] `PortableClipboard` uses unmanaged libgtk/libgdk methods instead of using
  gtk-sharp
- [SIL.Windows.Forms.SettingProtection] Deprecated ManageComponent method
- [SIL.Scripture] VerseRef.TrySetVerseUnicode: Improve handling of non-decimal numerals and surrogate pair numerals (#1000)
- [SIL.Windows.Forms.WritingSystems] Ignore deprecated region subtags in `ScriptRegionVariantView`(#763)
- [SIL.Windows.Forms.DblBundle] Upgraded to version 5.0 (beta) of `L10NSharp.dll`
- [SIL.Windows.Forms.Keyboarding] Upgraded to version 5.0 (beta) of `L10NSharp.dll`
- [SIL.Windows.Forms.WritingSystems] Upgraded to version 5.0 (beta) of `L10NSharp.dll`
- [SIL.Windows.Forms] Upgraded to version 5.0 (beta) of `L10NSharp.dll`
- [SIL.Core] Corrected logic in extension method GetLongestUsefulCommonSubstring
- [SIL.Windows.Forms.ClearShare.WinFormsUI] Default to CC-BY for new CC licenses
- [SIL.Media] Allow RecordingDeviceIndicator to find new sound input device when there was no selected device (state == NotYetStarted)
- [SIL.Windows.Forms] Internationalized the ExceptionReportingDialog.
- [SIL.Windows.Forms] Corrected typo in the name of AcquireImageControl.SetInitialSearchString
- [SIL.Core] ConsoleErrorReporter logs exception if available
- [SIL.Core, SIL.Windows.Forms] If WinFormsErrorReporter is set as the ErrorReporter, and ErrorReporter.NotifyUserOfProblem(IRepeatNoticePolicy, Exception, String, params object[]) is passed null for the exception, the "Details" button will no longer appear, making this consistent with the no-Exception overload of this method
- [SIL.WritingSystems] Changed behavior of IetfLanguageTag to better handle zh-TW.

### Fixed

- [SIL.Windows.Forms] Fix bug where changing `ImageCollection` search language too soon could crash.
- [SIL.Windows.Forms] Fix bug where image license could not be changed from Creative Commons.
- [SIL.Windows.Forms] Fix bug where `PalasoImage` disposes of its `Image` prematurely
- [SIL.Windows.Forms] Save non-CC licenses properly in images
- [SIL.Windows.Forms.Keyboarding] Avoid crashes in cases where Ibus connection dropped
- [SIL.Windows.Forms.Keyboarding] Copy `SIL.Windows.Forms.Keyboarding.dll.config` to output directory
- [SIL.WritingSystems] Fix case mismatch with `needsCompiling` attribute
- [SIL.Windows.Forms.ClearShare.WinFormsUI] Restore default version (4.0) for CC licenses after CC0 was used
- [SIL.Windows.Forms] Layout issues in the ExceptionReportingDialog to prevent overlapping text.

## [8.0.0] - 2021-03-04

### Added

- [SIL.DblBundle] `DblMetadata.Load` overload to allow deserialization from a `TextReader`.
- [SIL.Scripture] `Versification.Table.Load` overload to allow deserialization from a `TextReader`.
- [SIL.DblBundle] `TextBundle<TM, TL>.GetVersification` (to replace deprecated `CopyVersificationFile`)
- [SIL.DblBundle] `TextBundle<TM, TL>.GetFonts` (to replace deprecated `CopyFontFiles`)
- [SIL.DblBundle] `TextBundle<TM, TL>.GetLdml` (to replace deprecated `CopyLdmlFile`)
- [SIL.Scripture] `ScrVers.Save` overload to allow serialization to a `TextWriter`.
- [SIL.Scripture] `VerseRef.TrySetVerseUnicode` to set 'verse' and 'verseRef' variables with non-Roman numerals.
- [SIL.Core] `XmlSerializationHelper.Serialize<T>` to allow serialization to a `TextWriter`.
- [SIL.Core] `XmlSerializationHelper.Deserialize<T>` to allow deserialization from a `TextReader`.
- [SIL.Core] `Platform.IsGnomeShell` to detect if executing in a Gnome Shell
- [SIL.Core] `XmlSerializationHelper.SerializeToString<T>` overload to allow caller to specify encoding.
- [SIL.Core] Additional parameter to `ProcessExtensions.RunProcess` to allow redirecting stderr.

### Changed

- Add build number to `AssemblyFileVersion`
- Improve nuget symbol packages
- Use NUnit 3 for unit tests
- [SIL.Core, SIL.Core.Desktop] Move several classes back to `SIL.Core` from `SIL.Core.Desktop` to
  make them available to .NET Standard clients:
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
- [SIL.Windows.Forms] Remove unnecessary dependency on `NAudio`
- [SIL.Core] Move `HandleUnhandledException()` method from derived classes to base class
- [SIL.Core] `ConsoleExceptionHandler` class is now public
- [SIL.DblBundle.Tests] Create nuget package
- [SIL.Windows.Forms] Use the new Registered trademark logo (in About Box). Remove 132x148 logo.
- [SIL.Windows.Forms] Use [Markdig](https://github.com/lunet-io/markdig) instead of
  [MarkdownDeep.NET](https://www.toptensoftware.com/markdowndeep/)

### Fixed

- [SIL.Windows.Forms.Keyboarding] Use signed version of `Keyman*Interop.dll` (#865)
- [SIL.Windows.Forms.Keyboarding] Fixed keyboard switching for Ubuntu 18.04 (#887)
- [SIL.Windows.Forms] Use signed versions of `ibusdotnet.dll`, `Interop.WIA.dll`,
  `DialogAdapters.dll`, and `MarkdownDeep.dll` (#865)
- [SIL.Media] Fix missing `irrKlang.NET4.dll` exception by copying it to `lib` folder in output

### Deprecated

- [SIL.Core] Deprecate `ExceptionHandler.Init()` method in favor of more explicit version
  `ExceptionHandler.Init(ExceptionHandler)`, e.g. `ExceptionHandler.Init(new WinFormsExceptionHandler())`
- [SIL.Core] Deprecate `HttpUtilityFromMono` class. Use `System.Web.HttpUtility` instead.
- [SIL.DblBundle] Deprecate `TextBundle.CopyVersificationFile`, `CopyFontFiles` and
  `CopyLdmlFile` in favor of `GetVersificationFile`, `GetFontFiles`, and `GetLdmlFile`.

### Removed

- [SIL.NUnit3Compatibility] Remove this project because we're using NUnit 3 now.

## [7.0.0] - 2019-08-29

### Changed

- Create nuget packages
- [SIL.Media] `IAudioRecorder.SelectedDevice` now returns a `IRecordingDevice` which both the
  NAudio and AlsaAudio `RecordingDevice` implement. This allows to use the same assembly
  on both Windows and Linux (although the limitations what works and what doesn't work remain the
  same)
- [SIL.Media] cleanup of `AudioSession` API: rename `AudioIrrKlangSession` to `WindowsAudioSession`.
  `AudioAlsaSession` and `WindowsAudioSession` are now internal (they were never intended to
  be used directly)
- [SIL.Media] move some interfaces around so that they live in `SIL.Media` instead of
  `SIL.Media.Naudio`: `IAudioRecorder`, `RecordingState`, `IAudioPlayer`

### Added

- [SIL.Core] new properties for processor architecture: `Platform.ProcessArchitecture` and
  `Platform.IsRunning64Bit`
- [SIL.NUnit3Compatibility] new project/package that allows to use NUnit3 syntax with NUnit2
  projects

[Unreleased]: https://github.com/sillsdev/libpalaso/compare/v9.0.0...master

[9.0.0]: https://github.com/sillsdev/libpalaso/compare/v8.0.0...v9.0.0
[8.0.0]: https://github.com/sillsdev/libpalaso/compare/v7.0.0...v8.0.0
[7.0.0]: https://github.com/sillsdev/libpalaso/compare/v5.0...v7.0.0
