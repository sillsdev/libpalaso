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

- [SIL.WritingSystems] Added public DownloadLanguageTags method for updating the cached langtags.json from the SLDR repository.
- [SIL.Core] Add environment variable to disable `GlobalMutex` across processes. Helpful for snap packages in Linux.


### Changed

- [SIL.TestUtilities] Made FluentAssertXml classes use "Assert.That" so they can work in clients that use NUnit 4.
- [SIL.Windows.Forms] Removed protected members from FadingMessageWindow: MsgThread, MsgForm, Text, MsgPoint, and ShowForm. (The underlying implementation needed to be changed, and the existing implementation was such that these members would be unlikely to have been meaningful or helpful in a derived class anyway.)
- [SIL.Windows.Forms] Added the stated requirement that FadingMessageWindow.Show be called on the UI thread.
- [SIL.WritingSystems] Changed optional parameter of SLDR's Initialize from offlineMode to offlineTestMode (technically a breaking change).
- [SIL.WritingSystems] Added optional parameter to InitializeLanguageTags: bool downloadLanguageTags (default true).
- [SIL.WritingSystems] Download of langtags.json handled with ETag and If-None-Match headers instead of If-Modified-Since.
- [SIL.WritingSystems] Added version number to the UserAgent string used for LDML and langtags.json requests.
- [SIL.DictionaryServices] In class LiftLexEntryRepository, renamed private method GetTrimmedElementsSeperatedBySemiColon to GetTrimmedElementsSeparatedBySemiColon and private method CheckIfTokenHasAlreadyBeenReturnedForThisSemanticDomain parameter fieldsandValuesForRecordTokens to fieldsAndValuesForRecordTokens.
- [SIL.DictionaryServices.Tests] Renamed test class LiftLexEntryRepositoryStateUnitializedTests to LiftLexEntryRepositoryStateUninitializedTests.
- [SIL.DictionaryServices.Tests.Lift] Renamed test class LiftRepositoryStateUnitializedTests to LiftRepositoryStateUninitializedTests.
- [SIL.Tests.Data] Renamed test interface IRepositoryStateUnitializedTests to IRepositoryStateUninitializedTests and test class MemoryRepositoryStateUnitializedTests to MemoryRepositoryStateUninitializedTests.
- [SIL.Tests.Spelling] Renamed test class SpellingwordTokenizerTests to SpellingWordTokenizerTests.
- [SIL.Tests.Text] In test class MultiTextBaseTests, renamed method AnnotationOfMisssingAlternative to AnnotationOfMissingAlternative.
- [SIL.Windows.Forms.Keyboarding.Tests] In test class XkbKeyboardAdapterTests, renamed method Errorkeyboards to ErrorKeyboards.
- [SIL.Windows.Forms.Keyboarding.Windows] In internal interface ITfInputProcessorProfileMgr, renamed method RegisterProfile parameter hklsubstitute to hklSubstitute.
- [SIL.Windows.Forms.Reporting] In class ProblemNotificationDialog, renamed internal property _reoccurenceMessage to _reoccurrenceMessage.
- [SIL.WritingSystems] In class LanguageLookup changed private method AddLanguage parameter threelettercode to threeLetterCode.
- [SIL.Xml] (Breaking change!) In class XmlUtils, renamed method GetIndendentedXml to GetIndentedXml.
- [SIL.UsbDrive.Linux] (Breaking change!) In interface IUDiskDevice renamed method DriveAtaSmartInitiateSelftest to DriveAtaSmartInitiateSelfTest and property DevicePresentationNopolicy to DevicePresentationNoPolicy.
- [SIL.DictionaryServices.Model] (Breaking change!) In class LexEntry, renamed WellKnownProperties.FlagSkipBaseform to WellKnownProperties.FlagSkipBaseForm and GetSomeMeaningToUseInAbsenseOfHeadWord to GetSomeMeaningToUseInAbsenceOfHeadWord.
- [SIL.DictionaryServices.Model] (Breaking change!) In classes LexEntry and LexSense, renamed method CleanUpAfterEditting to CleanUpAfterEditing.
- [SIL.Lift] (Breaking change!) In abstract class PalasoDataObject, renamed method CleanUpAfterEditting to CleanUpAfterEditing.
- [SIL.Reporting] (Breaking change!) In classes ErrorReport and ExceptionHelper, renamed methed GetHiearchicalExceptionInfo to GetHierarchicalExceptionInfo.
- [SIL.Reporting] (Breaking change!) In interface IRepeatNoticePolicy and classes ShowAlwaysPolicy and ShowOncePerSessionBasedOnExactMessagePolicy, renamed property ReoccurenceMessage to ReoccurrenceMessage.
- [SIL.Reporting] (Breaking change!) In class FontAnalytics, renamed property MinumumInterval to MinimumInterval.
- [SIL.Scripture] (Breaking change!) In class BCVRef method MakeReferenceString, renamed parameter supressChapterForIntroMatter to suppressChapterForIntroMatter.
- [SIL.Windows.Forms.ImageToolbox] (Breaking change!) Changed class PalsoImageNotDisposed to PalasoImageNotDisposed.
- [SIL.Windows.Forms.Reporting] (Breaking change!) In class ProblemNotificationDialog, renamed method ReoccurenceMessage to ReoccurrenceMessage.

### Fixed

- [SIL.Windows.Forms] Changed build date in SILAboutBox to be computed using the last write time instead of creation time.
- [SIL.Windows.Forms] Made FadingMessageWindow implement all UI logic on the main UI thread in a thread-safe way. Fixes crashes like SP-2340.

## [15.0.0] - 2025-01-06

### Added

- [SIL.Core] Added optional parameter, preserveNamespaces, to XmlUtils.WriteNode
- [SIL.Core] Added optional parameter, includeSystemLibraries, to AcknowledgementsProvider.CollectAcknowledgements
- [SIL.Windows.Forms] Added ability to select which SIL logo(s) to use in SILAboutBox.
- [SIL.Windows.Forms] Added public enum Widgets.SilLogoVariant
- [SIL.Windows.Forms] Added to Widgets.SilResources: AllLogoVariants, GetLogo, and SilLogoRandom (to replace SilLogo101x113)
- [SIL.Core] Added macOS support for `GlobalMutex`
- [SIL.Archiving] Added ArchivingDlgViewModel.Standard and ArchivingDlgViewModel.StringId enumerations.
- [SIL.Archiving] Added public delegate ArchivingDlgViewModel.ExceptionHandler and event ArchivingDlgViewModel.OnExceptionDuringLaunch.
- [SIL.Archiving] Added IArchivingProgressDisplay interface.
- [SIL.Archiving] Added overload of ArchivingDlgViewModel.DisplayMessage to take format parameters.
- [SIL.Archiving] Added public overload of ArchivingDlgViewModel.LaunchArchivingProgram.
- [SIL.Archiving] Added protected methods to ArchivingDlgViewModel: ReportMajorProgressPoint, ReportProgress, CleanUp
- [SIL.Windows.Forms.Archiving] Added protected virtual properties ArchiveTypeForTitleBar and InformativeText to ArchivingDlg.
- [SIL.Windows.Forms.Archiving] Added public virtual method GetMessage to ArchivingDlg.
- [SIL.Windows.Forms.Archiving] Added public virtual property ArchiveTypeName to ArchivingDlg.
- [SIL.Windows.Forms.Archiving] Added protected methods DisplayMessage and Initialize (async) to ArchivingDlg.
- [SIL.Windows.Forms.Archiving] Added protected virtual method PackageCreationComplete to ArchivingDlg.
- [SIL.Windows.Forms.Archiving] Added (public) override of property ArchiveTypeName to IMDIArchivingDlg.
- [SIL.Windows.Forms.Archiving] Added (protected) override of property InformativeText to IMDIArchivingDlg.
- [SIL.Windows.Forms.Archiving] Added (protected) override of method PackageCreationComplete to IMDIArchivingDlg.
- [SIL.Windows.Forms.Archiving] Added (public) override of method GetMessage to IMDIArchivingDlg.
- [SIL.Windows.Forms.Archiving] Added public extensions class LinkLabelExtensions with some methods that were formerly in Extensions class (now in SIL.Archiving).
- [SIL.Archiving] Added public property isValid to IMDIPackage.
- [SIL.Archiving] Added public event InitializationFailed to IMDIArchivingDlgViewModel.
- [SIL.Archiving] Added the following properties to ArchivingDlgViewModel as an alternative way to customize the initial summary displayed: GetOverriddenPreArchivingMessages, InitialFileGroupDisplayMessageType, OverrideGetFileGroupDisplayMessage
- [SIL.Media] Added FFmpegRunner.MinimumVersion property (also used by MediaInfo for FFprobe).
- [SIL.Windows.Forms.WritingSystems] Added Caption property to LanguageLookupDialog.

### Changed

- BREAKING CHANGE: Replaced dependency on DotNetZip with System.IO.Compression.ZipFile (Client installers will need to be changed.)
- BREAKING CHANGE: Changed to target .Net Framework 4.6.2 instead of 4.6.1
- [SIL.Windows.Forms] Look for PNG data on clipboard before checking for plain image in WindowsClipboard.GetImageFromClipboard() in order to preserve transparency in copied images.
- [SIL.Windows.Forms] Changed layout of SILAboutBox to accommodate wider SIL logo.
- [SIL.Windows.Forms.Archiving] Split SIL.Archiving, moving Winforms portions (including dependency on L10nSharp) to SIL.Windows.Forms.Archiving.
- [SIL.Archiving] Changed IMDIArchivingDlgViewModel.ArchivingPackage to return an IMDIPackage (instead of an IArchivingPackage).
- [SIL.Archiving] Required ArchivingDlgViewModel implementations to implement IDisposable.
- [SIL.Archiving] Made protected members in ArchivingDlgViewModel private, adding protected accessors as needed.
- [SIL.Archiving] In ArchivingDlgViewModel, renamed DisplayMessageEventHandler to MessageEventHandler, OnDisplayMessage to OnReportMessage, DisplayErrorEventHandler to ErrorEventHandler, and OnDisplayError to OnError.
- [SIL.Archiving] Changed signature of ArchivingDlgViewModel.OverrideDisplayInitialSummary to include a CancellationToken.
- [SIL.Archiving] Made ArchivingDlgViewModel.ArchiveType property public and changed it from a string to Standard (new enum).
- [SIL.Archiving] Changed signature of setFilesToArchive delegate in ArchivingDlgViewModel's protected constructor.
- [SIL.Archiving] Changed return type of ArchivingDlgViewModel.Initialize (to make it async) and added two parameters.
- [SIL.Archiving] Changed the signature of protected methods in ArchivingDlgViewModel: LaunchArchivingProgram, GetFileExcludedMsg.
- [SIL.Archiving] Changed the signature of the public method ArchivingDlgViewModel.CreatePackage.
- [SIL.Archiving] Changed underlying type of public enums VernacularMaterialsType and SilDomain from ulong to long.
- [SIL.Archiving] Replaced protected \_keys field (now private) in abstract class ArchivingPackage with protected accessor property Keys.
- [SIL.Archiving] IMDIArchivingDlgViewModel (subclass of ArchivingDlgViewModel) affected by many of the changes to the base class.
- [SIL.Archiving] IMDIArchivingDlgViewModel and RampArchivingDlgViewModel (subclasses of ArchivingDlgViewModel) affected by many of the changes to the base class.
- [SIL.Archiving] IMDIArchivingDlgViewModel constructor signature changed.
- [SIL.Archiving] RampArchivingDlgViewModel constructor signature changed.
- [SIL.Windows.Forms.Archiving] Made ArchivingDlg implement IArchivingProgressDisplay.
- [SIL.Windows.Forms.Archiving] ArchivingDlg constructor signature changed: removed localizationManagerId; added optional archiveInfoHyperlinkText; made some other parameters optional.
- [SIL.Windows.Forms.Archiving] IMDIArchivingDlg constructor signature changed: added appSpecificArchivalProcessInfo.
- [SIL.Windows.Forms] Split ClearShare code, moving non-Winforms portions to SIL.Core (SIL.Core.ClearShare namespace)
- [SIL.Core] Added optional parameter to OlacSystem.GetRoles to allow caller to provide its own XML with role definitions.
- [SIL.Windows.Forms] Split License into a base class called License and a derived LicenseWithLogo, so that License could be in SIL.Core.
- [SIL.Archiving] Changed IArchivingSession.Files (and Session.Files) into an IReadonlyList.
- [SIL.Archiving] Made IMDIPackage.CreateIMDIPackage asynchronous, changing its signature to take a CancellationToken parameter and return Task<bool>.
- [SIL.Archiving] Made MetaTranscript.WriteCorpusImdiFile asynchronous, changing its signature to return Task<bool>.
- [SIL.Archiving] Changed the name of the third parameter in ArchivingDlgViewModel.AddFileGroup from progressMessage to addingToArchiveProgressMessage.
- [SIL.Windows.Forms.Archiving] Changed Cancel Button to say Close instead in IMDIArchivingDlg.
- [SIL.Core.Desktop] Renamed GetFromRegistryProgramThatOpensFileType to GetDefaultProgramForFileType.
- [SIL.Media] Made FFmpegRunner able to use version of FFmpeg found on the path.
- [SIL.Media] Upgraded irrKlang to v. 1.6.
- [SIL.Media] In FFmpegRunner, changed ExtractMp3Audio, ExtractOggAudio, ExtractAudio, and ChangeNumberOfAudioChannels to use LocateAndRememberFFmpeg instead of LocateFFmpeg. This is potentially a breaking change but only in the edge case where an app does not install FFmpeg and the user installs it while running the app.
- [SIL.Media] Made the Windows implementation of ISimpleAudioSession more robust in that it will attempt to create an irrKlang-based recorder even if there is no audio output device enabled.
- [SIL.Media] Made FFmpegRunner explicitly a static class (technically a breaking change, though all methods were already static).
- [SIL.Media] Made FFmpegRunner look for the exe on the path before trying to find a version installed for Audacity (which is unlikely to succeed anyway).
- [SIL.Media] Made MediaInfo look for the FFprobe exe in the same location as FFmpeg when the application has specified the location for it or when it was previously located in one of the expected locations. Also made it more robust by making it more likely to find FFprobe (when it is on the system path).

### Fixed

- [SIL.Archiving] Fixed typo in RampArchivingDlgViewModel for Ethnomusicology performance collection.
- [SIL.Archiving] Changed URLs that used http: to https: in resource EmptyMets.xml.
- [SIL.Core.Desktop] Implemented GetDefaultProgramForFileType (as trenamed) in a way that works on Windows 11, Mono (probably) and MacOS (untested).
- [SIL.Media] MediaInfo.HaveNecessaryComponents properly returns true if FFprobe is on the system path.
- [SIL.Media] Made MediaInfo.FFprobeFolder look for and return the folder when first accessed, even if no prior call to the setter or other action had caused it t be found.
- [SIL.Core] Made GetSafeDirectories not crash and simply not return any subdirectory the user does not have permission to access.
- [SIL.Core] In GetDirectoryDistributedWithApplication, prevented a failure in accessing one of the specified subfolders from allowing it to try the others.

### Removed

- Support for .Net Framework 4.6.1
- [SIL.Windows.Forms] Removed SilLogo101x113 from Widgets.SilResources. Use SilLogoRandom or specify desired variant instead.
- [SIL.Windows.Forms] Removed previously deprecated CreativeCommonsLicense.IntergovernmentalOriganizationQualifier
- [SIL.Archiving] Removed abstract properties from ArchivingDlgViewModel: InformativeText and ArchiveInfoHyperlinkText.
- [SIL.Archiving] Removed public method ArchivingDlgViewModel.Cancel. (Now handled via cancellation tokens.)
- [SIL.Archiving] Removed protected methods from ArchivingDlgViewModel: PreparingFilesMsg, GetSavingFilesMsg
- [SIL.Archiving] Removed protected fields (renamed and made private) from ArchivingLanguage: \_iso3Code, \_englishName
- [SIL.Archiving] Removed protected fields (made private) from ArchivingFile: \_fullName, \_fileName, \_fileSize, \_mimeType, \_descriptions, \_accessProtocol
- [SIL.Archiving] Removed public methods CreateMetsFile and CreateRampPackage from RampArchivingDlgViewModel (made internal).
- [SIL.Archiving] Removed ArchivingPackage and AddSession from ArchivingDlgViewModel and RampArchivingDlgViewModel (where they threw NotImplementedExceptions)

### Fixed

- [SIL.Window.Forms] When choosing a file in the ImageToolbox.AcquireImageControl, a FileOk handler is simulated that verifies the selected file passes the given filter. Users can defeat the filter mechanism by pasting or typing the file name. While the returned filename does not pass the filter, the dialog is reopened until the user either chooses a proper filename or cancels the dialog. The native FileOk handler can prevent the dialog from closing: we can't achieve that. (See BL-13552.)

## [14.1.1] - 2024-05-23

### Fixed

- [SIL.Windows.Forms.DblBundle] Fixed bug in ProjectsListBase that made it impossible to select a project after double-clicking a column header. (See HT-475)

## [14.1.0] - 2024-05-13

### Added

- [SIL.Windows.Forms] Added static SilResources class with property SilLogo101x113.

### Fixed

- [SIL.Windows.Forms] Fixed backwards logic for LocalizationIncompleteViewModel.ShouldShowDialog (Technically this is a breaking contractual change, since effectively the behavior is the opposite of the original implementation, but the name so clearly indicates the desired behavior that it seems unlikely any subclass implementation would have implemented the logic according to the previously expected backwards behavior.)

## [14.0.0] - 2024-04-09

### Changed

- [SIL.Archiving] Upgraded to L10nSharp 7.0.0
- [SIL.Windows.Forms] Upgraded to L10nSharp 7.0.0
- [SIL.Windows.Forms.DblBundle] Upgraded to L10nSharp 7.0.0
- [SIL.Windows.Forms.Keyboarding] Upgraded to L10nSharp 7.0.0
- [SIL.Windows.Forms.WritingSystems] Upgraded to L10nSharp 7.0.0
- [SIL.Core] `RaiseExceptionIfFailed` no longer throws an exception if user cancelled

## [13.0.1] - 2024-01-09

### Fixed

- [SIL.Core] Fixed bug in extension method GetLongestUsefulCommonSubstring when string ends with an Object replacement character
- [SIL.Core] LogBox: Checked for disposed log box or caller-requested cancel in SafeInvoke so we don't try to write messages or scroll.

## [13.0.0] - 2023-12-07

### Added

- [SIL.Core] `RobustFile.Open`, `RobustFile.AppendAllText`, `RobustFile.WriteAllLines`, `RobustFile.GetAccessControl`, `RobustIO.EnumerateFilesInDirectory`, `RobustIO.EnumerateDirectoriesInDirectory`, `RobustIO.EnumerateEntriesInDirectory`, `RobustIO.RequireThatDirectoryExists`, `RobustIO.GetFileStream`, `RobustIO.ReadAllTextFromFileWhichMightGetWrittenTo`, and `RobustIO.IsFileLocked` methods
- [SIL.Core.Desktop] `RobustImageIO.GetImageFromFile` method
- [SIL.Windows.Forms] `ImageToolboxControl.ImageChanged` (selected or cropped) and `ImageToolboxControl.MetadataChanged` events
- [SIL.Windows.Forms] Text box to edit `AttributionUrl` in `MetadataEditorControl`
- [SIL.Windows.Forms] Interop.WIA.dll for MSIL (doesn't seem to work with 32-bit apps, so the existing dll remains unchanged)
- [SIL.Scripture] Made static methods TryGetVerseNum, ParseVerseNumberRange, and ParseVerseNumber public
- [SIL.Core] `CanWriteToDirectories` and `CanWriteToDirectory`
- [SIL.Windows.Forms] `CanWriteToDirectories`, `CanWriteToDirectory` and `ReportDefenderProblem`
- [SIL.Core] `StrLengthComparer`, IEnumerable<T>.ToString extension methods, IList<T>.ContainsSequence<T> extension method
- [SIL.Windows.Forms] `ConfirmFileOverwriteDlg`
- [SIL.Windows.Forms] several constructors and `Restore` method to `WaitCursor`
- [SIL.Media.NAudio] added an overload to `BeginMonitoring` with `catchAndReportExceptions` parameter

### Changed

- [SIL.DictionaryServices] Renamed parameter of LiftWriter.WriteHeader from headerConentsNotIncludingHeaderElement to headerContentsNotIncludingHeaderElement
- [SIL.WritingSystems] Updated langtags.json and ianaSubtagRegistry.txt
- [SIL.Core] Enhanced ErrorReport.GetOperatingSystemLabel method to report Windows 11+ and list the version as well.
- [SIL.Core] Enhanced RetryUtility.Retry methods to optionally improve debugging messages, and fixed existing RobustFile and RobustIO methods to use the new optional debugging parameter
- [SIL.Media] Changed the FrameRate reported in VideoInfo from FrameRate to AvgFrameRate.
- [SIL.Windows.Forms] Fixed spelling error in ImageGalleryControl, renaming SetIntialSearchTerm to SetInitialSearchTerm.
- [SIL.Windows.Forms] Made `WaitCursor` class (which used to contain only static methods) implement IDisposable

### Fixed

- [SIL.Windows.Forms.ClearShare] Fixed Metadata.LoadProperties to catch the ArgumentOutOfRangeException thrown by TagLib.File.Create when unknown data is found in the IPTC profile segment. The rest of the metadata (Exif / XMP) is likely to be okay, but won't be available until TagLib is fixed to allow this. Not having the metadata available shouldn't prevent using the image. Note that clients can now read the exception caught while loading if so desired.
- [SIL.Windows.Forms.WritingSystem.WSIdentifiers] Changed ComboBox controls in WSIdentifierView and ScriptRegionVariantView to DropDownList style to prevent accidental editing that shouldn't happen
- [SIL.Windows.Forms.ClearShare] Make Metadata.Write (and a few other methods) more robust
- [SIL.Core.Desktop] Make FileUtils.ReplaceFileWithUserInteractionIfNeeded robust
- [SIL.Core] Make RobustFile.ReplaceByCopyDelete truly robust
- [SIL.Core] Make RetryUtility retry for exceptions that are subclasses of the ones listed to try. For example, by default (IOException) it will now retry for FileNotFoundException.
- [SIL.Windows.Forms] Spelling of `CreativeCommonsLicense.IntergovernmentalOrganizationQualifier`
- [SIL.Windows.Forms] Fixed internationalization problem: SettingsProtection.LauncherButtonLabel was used as ID for two different strings.
- [SIL.Windows.Forms] Fix 4 img metadata methods that could fail due to cloud or scanning interference
- [SIL.Windows.Forms] Fixed error in BetterGrid.OnCellContentClick to make it so the delete button works correctly if there is no "new row."

### Removed

- [SIL.Windows.Forms] ImageGalleryControl.InSomeoneElesesDesignMode (seemingly unused and misspelled)
- [SIL.Windows.Forms] Checkbox for `IntergovernmentalOrganizationQualifier` from `MetadataEditorControl`

## [12.0.1] - 2023-05-26

### Fixed

- [SIL.Windows.Forms] Make `PalasoImage.FromFile(Robustly)` methods more robust
- [SIL.Windows.Forms] Update dll to `libdl.so.2` to make compatible with Ubuntu 22.x. Affects multiple projects.
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
- [SIL.Archiving] Changed ArchiveAccessProtocol.SetChoicesFromCsv to throw ArgumentNullException instead of NullReferenceException. Also made it discard duplicate choices if the list contains duplicates.
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
- [SIL.Core] Added overload of SerializeToFileWithWriteThrough to simplify error handling.
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

[Unreleased]: https://github.com/sillsdev/libpalaso/compare/v14.1.1...master
[15.0.0]: https://github.com/sillsdev/libpalaso/compare/v14.1.1...v15.0.0
[14.1.1]: https://github.com/sillsdev/libpalaso/compare/v14.1.0...v14.1.1
[14.1.0]: https://github.com/sillsdev/libpalaso/compare/v14.0.0...v14.1.0
[14.0.0]: https://github.com/sillsdev/libpalaso/compare/v13.0.1...v14.0.0
[13.0.1]: https://github.com/sillsdev/libpalaso/compare/v13.0.0...v13.0.1
[13.0.0]: https://github.com/sillsdev/libpalaso/compare/v12.0.1...v13.0.0
[12.0.1]: https://github.com/sillsdev/libpalaso/compare/v12.0.0...v12.0.1
[12.0.0]: https://github.com/sillsdev/libpalaso/compare/v11.0.1...v12.0.0
[11.0.1]: https://github.com/sillsdev/libpalaso/compare/v11.0.0...v11.0.1
[11.0.0]: https://github.com/sillsdev/libpalaso/compare/v10.1.0...v11.0.0
[10.1.0]: https://github.com/sillsdev/libpalaso/compare/v10.0.0...v10.1.0
[10.0.0]: https://github.com/sillsdev/libpalaso/compare/v9.0.0...v10.0.0
[9.0.0]: https://github.com/sillsdev/libpalaso/compare/v8.0.0...v9.0.0
[8.0.0]: https://github.com/sillsdev/libpalaso/compare/v7.0.0...v8.0.0
[7.0.0]: https://github.com/sillsdev/libpalaso/compare/v5.0...v7.0.0
