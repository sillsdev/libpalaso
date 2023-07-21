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

- [SIL.Windows.Forms] `ImageToolboxControl.ImageChanged` (selected or cropped) and `ImageToolboxControl.MetadataChanged` events
- [SIL.Windows.Forms] Interop.WIA.dll for MSIL (doesn't seem to work with 32-bit apps, so the existing dll remains unchanged)
- [SIL.Scripture] Made static methods TryGetVerseNum, ParseVerseNumberRange, and ParseVerseNumber public
- [SIL.Core] `CanWriteToDirectories` and `CanWriteToDirectory`
- [SIL.Windows.Forms] `CanWriteToDirectories`, `CanWriteToDirectory` and `ReportDefenderProblem`
- [SIL.Core] `StrLengthComparer`, IEnumerable<T>.ToString extension methods, IList<T>.ContainsSequence<T> extension method
- [SIL.Windows.Forms] `ConfirmFileOverwriteDlg`
- [SIL.Windows.Forms] several constructors and `Restore` method to `WaitCursor`

### Changed

- [SIL.Media] Changed the FrameRate reported in VideoInfo from FrameRate to AvgFrameRate.
- [SIL.Windows.Forms] Fixed spelling error in ImageGalleryControl, renaming SetIntialSearchTerm to SetInitialSearchTerm.
- [SIL.Windows.Forms] Made `WaitCursor` class (which used to contain only static methods) implement IDisposable

### Fixed

- [SIL.Core] Make RetryUtility retry for exceptions that are subclasses of the ones listed to try. For example, by default (IOException) it will now retry for FileNotFoundException.

### Removed
- [SIL.Windows.Forms] ImageGalleryControl.InSomeoneElesesDesignMode (seemingly unused and misspelled)

## [12.0.1] - 2023-05-26

### Fixed

- [SIL.Windows.Forms] Make `PalasoImage.FromFile(Robustly)` methods more robust
- [SIL.Windows.Forms] Update dll to `libdl.so.2` to make compatible with Ubuntu 22.x.  Affects multiple projects.
- [SIL.Core] Fixed `BulkObservableList.MoveRange` method when moving a single item forward.

## [12.0.0] - 2023-02-14

### Added

- [SIL.Core.Desktop] Added aiff, m4a, voc, and amr formats to AudioFileExtensions
- [SIL.Core.Desktop] Added webm and mkv formats to VideoFileExtensions
- [SIL.Media] MediaInfo.AnalysisData property
- [SIL.Media] MediaInfo.FFprobeFolder

### Changed

- [SIL.Core.Desktop] Fixed typo in list of AudioFileExtensions: "acc" changed to "aac"
- [SIL.Media] FFmpegRunner will now also look for and use a version of FFmpeg installed using chocolatey
- [SIL.Media] MediaInfo now used FFprobe instead of FFmpeg to get media information. Depends on FFMpegCore library.

### Removed

- [SIL.Media] MediaInfo.RawData property (replaced by AnalysisData)

## [11.0.1] - 2023-01-27

### Fixed

- [SIL.Windows.Forms] Prevent changing row in ContributorsListControl if the row is dirty and is not in a valid state to commit edit (SP-2297)

## [11.0.0] - 2023-01-19

### Added

- [SIL.Core] Added `SIL.Reporting.FontAnalytics` class.
- [SIL.Core] Added `ObjectModel.ObservableISet` as a parent class to the existing `ObservableHashSet`
- [SIL.Core] Added `ObjectModel.ObservableSortedSet` (child class of `ObservableISet`)
- [SIL.DblBundle] Added const strings to UsxNode for the various USX element names.
- [SIL.DblBundle] Added protected method GetAttribute to UsxNode.
- [SIL.DblBundle] Added sealed subclasses of UsxNode: UsxPara and UsxChar.
- [SIL.DblBundle] Added property IsChapterStart to UsxChapter.
- [SIL.Reporting] Added TypeOfExistingHandler property to ExceptionHandler.

### Fixed

- [SIL.DblBundle] Attempting to construct a UsxNode based on an invalid XmlNode now throws an exception in the constructor in most cases rather than later when properties are accessed.
- [SIL.DblBundle] Accessing UsxChapter.ChapterNumber on a chapter end node returns the chapter number (from the eid attribute) instead of throwing an exception.
- [SIL.WritingSystems] Prevent (and clean up) duplicate URLs in LDML files for Fonts, Keyboards, and Spell Check Dictionaries.
- [SIL.Archiving] Set UseZip64WhenSaving to Zip64Option.AsNecessary to prevent crash with large archives

### Changed

- [SIL.Archiving] Changed REAP access protocol label from "Insite users" to "REAP users"
- [SIL.Archiving] Fixed typo in name of ArchiveAccessProtocol.GetDocumentationUri methods
- [SIL.Archiving] Changed ArchiveAccessProtocol.GetDocumentationUri methods
- [SIL.Archiving] Changed ArchiveAccessProtocol.SetChoicesFromCsv to thow ArgumentNullException instead of NullReferenceException. Also made it discard duplicate choices if the list contains duplicates.
- [SIL.Core] `FileLocationUtilities.GetDirectoryDistributedWithApplication` checks not only in
  `DistFiles`, `common`, and `src` subdirectories, but also directly in the application or solution directory.
- [SIL.Core] Store URLs in Sets instead of Lists in `IKeyboardDefinition` (to prevent duplicates)
- [SIL.DblBundle.Tests] Made GetChaptersAndParasForMarkOneContaining2Verses private.
- [SIL.DblBundle] Made UsxNode abstract.
- [SIL.DblBundle] Made UsxNode.StyleTag virtual. Calling UsxChapter.StyleTag on a chapter end node returns null instead of throwing an exception.
- [SIL.DblBundle] Made UsxChapter sealed.
- [SIL.Core] Store URLs in Sets instead of Lists in `FontDefinition` and `SpellCheckDictionaryDefinition` (to prevent duplicates)
- [SIL.Windows.Forms] Upgraded to L10nSharp 6.0.0
- [SIL.Windows.Forms.DblBundle] Upgraded to L10nSharp 6.0.0
- [SIL.Windows.Forms.WritingSystems] Upgraded to L10nSharp 6.0.0

### Removed

- [SIL.Core.Desktop] Removed deprecated properties and methods from `FileLocator`:
  `DirectoryOfApplicationOrSolution`, `DirectoryOfTheApplicationExecutable`, `LocateExecutable`,
  `GetFileDistributedWithApplication`, `GetDirectoryDistributedWithApplication`,
  and `LocateInProgramFiles`.
- [SIL.Core.Desktop] Removed deprecated methods from `DirectoryUtilities`:
  `CopyDirectoryWithException`, `AreDirectoriesEquivalent`, `MoveDirectorySafely`,
  `GetSafeDirectories`, `DeleteDirectoryRobust`, `GetUniqueFolderPath`. and `DirectoryIsEmpty`.
- [SIL.Core.Desktop] Removed deprecated methods from `FileUtils`: `IsFileLocked`,
  `GrepFile`, `CheckValidPathname`, `ReplaceByCopyDelete`, `MakePathSafeFromEncodingProblems`,
  `NormalizePath`, and `StripFilePrefix`.
- [SIL.Core] Removed deprecated class `CoreSetup`.
- [SIL.Core] Removed deprecated method `CreateResultsWithNoDuplicates` from `ResultSet`.
- [SIL.Core] Removed deprecated extension method `IEnumerable<T>.Concat<T>(string)`.
- [SIL.Core] Removed deprecated methods from `PathUtilities`: `GetDeviceNumber`,
  `PathsAreOnSameVolume`, `PathContainsDirectory`.
- [SIL.Core] Removed deprecated class `HttpUtilityFromMono`.
- [SIL.Core] Removed deprecated parameterless `Init` method from `ExceptionHandler`.
- [SIL.Core] Removed deprecated `Init` method from `UsageReporter` (the one without
  the `reportAsDeveloper` parameter).
- [SIL.Core] Removed deprecated methods from `XmlUtils`: `GetAttributeValue`,
  `GetManditoryAttributeValue`, and `AppendAttribute`.
- [SIL.DblBundle] Removed deprecated methods from `TextBundle`: `CopyVersificationFile`,
  `CopyFontFiles`, and `CopyLdmlFile`.
- [SIL.DictionaryServices.Tests] Removed deprecated `AssertEqualsCanonicalString`
  method from `LiftWriterTests`.
- [SIL.Media] Removed deprecated `AudioSession` method from `AudioFactory`.
- [SIL.TestUtilities] Removed deprecated c'tor, properties and methods from
  `TemporaryFolder`: `TemporaryFolder()`, `FolderPath`, `Delete`, and `GetTemporaryFile`.
- [SIL.Windows.Forms] Removed deprecated `GetSummaryParagraph(string)` method from
  `MetaData`.
- [SIL.Windows.Forms] Removed deprecated `UseComboButtonStyle` from PushButtonColumn`.

## [10.1.0] - 2022-08-26

### Added

- [SIL.Core] Added SIL.PlatformUtilities.Platform.IsFlatpak property.
- [SIL.Core.Desktop] Added Testing channel to UpdateSettings.

### Fixed
- [SIL.Archiving] Fixed formatting of DateTimes
- [SIL.Core] Fixed SIL.IO.PathUtilities.DeleteToRecycleBin and .GetDefaultFileManager to work in a flatpak environment.
- [SIL.Lexicon] Fixed crash caused by incorrect processing of keyboard data
- [SIL.Scripture] Fixed SIL.Scripture.MultilingScrBooks.VerseRefRegex to make punctuation more specific
- [SIL.Windows.Forms] Fixed ImageToolbox.ImageGallery.ImageCollectionManager.FromStandardLocations to work in a flatpak environment.
- [SIL.WritingSystems] Fixed SLDR initialization for users with European number formats.

## [10.0.0] - 2022-08-04

### Added
- [SIL.Windows.Forms] Added extension method InitializeWithAvailableUILocales.
- [SIL.Windows.Forms] Added LocalizationIncompleteDlg and LocalizationIncompleteViewModel classes
- [SIL.Windows.Forms] Added property SummaryDisplayMember to CheckedComboBox.
- [SIL.Core] ErrorReport now has a GetErrorReporter() getter function.
- [SIL.Core] ErrorReport exposes a NotifyUserOfProblemWrapper() protected function, which is designed to make it easier for subclasses to add additional NotifyUserOfProblem options
- [SIL.Core] Added AltImplGetUnicodeCategory function to StringExtensions
- [SIL.WritingSystems] Added static class IcuUCharCategoryExtensions with a method ToUnicodeCategory to translate from Character.UCharCategory to System.Globalization.UnicodeCategory.
- [SIL.Windows.Forms.Scripture] Added event InvalidReferencePasted to VerseControl, which is fired whenever an attempt to paste an invalid scripture reference is made.

### Changed
- [SIL.Windows.Forms.WritingSystems] Moved (internal) extension method InitializeWithAvailableUILocales to SIL.Windows.Forms.
- [SIL.Windows.Forms.WritingSystems] Added additional optional localizationIncompleteViewModel parameter to ToolStripExtensions.InitializeWithAvailableUILocales.
- [SIL.Core] If NotifyUserOfProblem is called with a null exception, it will no longer call UsageReporter.ReportException
- Replace deprecated `Mono.Posix` dependency with `Mono.Unix` (#1186)

### Removed
- Removed the "new" DisplayMember property from CheckedComboBox (which overrode the base class member). I don't believe this is a breaking change.

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

[Unreleased]: https://github.com/sillsdev/libpalaso/compare/v12.0.1...master

[12.0.1]: https://github.com/sillsdev/libpalaso/compare/v12.0.0...v12.0.1
[12.0.0]: https://github.com/sillsdev/libpalaso/compare/v11.0.1...v12.0.0
[11.0.1]: https://github.com/sillsdev/libpalaso/compare/v11.0.0...v11.0.1
[11.0.0]: https://github.com/sillsdev/libpalaso/compare/v10.1.0...v11.0.0
[10.1.0]: https://github.com/sillsdev/libpalaso/compare/v10.0.0...v10.1.0
[10.0.0]: https://github.com/sillsdev/libpalaso/compare/v9.0.0...v10.0.0
[9.0.0]: https://github.com/sillsdev/libpalaso/compare/v8.0.0...v9.0.0
[8.0.0]: https://github.com/sillsdev/libpalaso/compare/v7.0.0...v8.0.0
[7.0.0]: https://github.com/sillsdev/libpalaso/compare/v5.0...v7.0.0
