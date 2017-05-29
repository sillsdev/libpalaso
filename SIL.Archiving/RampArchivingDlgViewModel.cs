﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Ionic.Zip;
using L10NSharp;
using SIL.Archiving.Generic;
using SIL.Archiving.Properties;
using SIL.IO;
using SIL.PlatformUtilities;
using SIL.Windows.Forms.ClearShare;
using Timer = System.Threading.Timer;

namespace SIL.Archiving
{
	/// ------------------------------------------------------------------------------------
	public class RampArchivingDlgViewModel: ArchivingDlgViewModel
	{
		[DllImport("user32.dll", EntryPoint = "SetWindowPos")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool SetWindowPosWindows(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

		private static bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx,
			int cy, uint uFlags)
		{
			// on Linux simply return true
			return !Platform.IsWindows || SetWindowPosWindows(hWnd, hWndInsertAfter, x, y, cx, cy, uFlags);
		}
		// ReSharper disable InconsistentNaming
		private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);    // brings window to top and makes it "always on top"
		private static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);  // brings window to top but not "always on top"
		private const UInt32 SWP_NOSIZE = 0x0001;
		private const UInt32 SWP_NOMOVE = 0x0002;
		private const UInt32 TOPMOST_FLAGS = SWP_NOMOVE | SWP_NOSIZE;
		// ReSharper restore InconsistentNaming

		#region RAMP and METS constants
// ReSharper disable CSharpWarnings::CS1591

		// Generic constants
		public const string kDefaultKey = " ";
		public const string kSeparator = ",";

		public const string kRampProcessName = "RAMP";
		public const string kRampFileExtension = ".ramp";

		/// <summary>Dublin Core fields. See http://dublincore.org/ for explanation‎</summary>
		public const string kPackageTitle = "dc.title";
		public const string kGeneralDescription = "dc.description";
		public const string kFileTypeModeList = "dc.type.mode";
		public const string kScholarlyWorkType = "dc.type.scholarlyWork";
		public const string kSilDomain = "dc.subject.silDomain";
		public const string kDateCreated = "dc.date.created";
		public const string kDateModified = "dc.date.modified";
		public const string kSubjectLanguage = "dc.subject.subjectLanguage";
		public const string kSoftwareOrFontRequirements = "dc.relation.requires";
		public const string kContentLanguages = "dc.language.iso";
		public const string kContentLanguageScripts = "dc.language.script";
		public const string kSchemaConformance = "dc.relation.conformsto";
		public const string kContributor = "dc.contributor";
		public const string kAbstractDescription = "dc.description.abstract";
		public const string kStageDescription = "dc.description.stage";
		public const string kTableOfContentsDescription = "dc.description.tableofcontents";
		public const string kVernacularContent = "dc.subject.vernacularContent";

		// flags
		public const string kFlagHasSubjectLanguage = "subject.subjectLanguage.has";
		public const string kFlagHasSoftwareOrFontRequirements = "relation.requires.has";
		public const string kFlagHasGeneralDescription = "description.has";
		public const string kFlagHasAbstractDescription = "description.abstract.has";
		public const string kFlagHasTableOfContentsDescription = "description.tableofcontents.has";
		public const string kFlagHasPromotionDescription = "description.promotion.has";
		public const string kTrue = "Y";

		// Mode constants
		public const string kModeSpeech = "Speech";
		public const string kModeVideo = "Video";
		public const string kModeText = "Text";
		public const string kModePhotograph = "Photograph";
		public const string kModeGraphic = "Graphic";
		public const string kModeMusicalNotation = "Musical notation";
		public const string kModeDataset = "Dataset";
		public const string kModeSoftwareOrFont = "Software application";
		public const string kModePresentation = "Presentation";

		// work-stage constants
		public const string kStageRoughDraft = "rough_draft";
		public const string kStageReviewedDraft = "reviewed_draft";
		public const string kStageReleasedDraft = "released_draft";
		public const string kStageApprovedDraft = "approved_draft";
		public const string kStagePublished = "published";
		public const string kStageUsedInCourse = "used_in_course";
		public const string kStagePrepublication = "prepublication";
		public const string kStageFinished = "finished";

		// SIL domain-related constants
		public const string kFmtDomainSubtype = "type.domainSubtype.{0}";
		public const string kAcademicTrainingAbbrev = "ATRN";
		public const string kAnthropologyAbbrev = "ANTH";
		public const string kEthnomusicologyAbbrev = "EMUS";
		public const string kLinguisticsAbbrev = "LING";
		public const string kCommunicationsAbbrev = "COMM";
		public const string kCommunityDevelopmentAbbrev = "CMDV";
		public const string kCounselingAbbrev = "COUN";
		public const string kInternationalOrGovernmentRelationsAbbrev = "INTR";
		public const string kLanguageAndCultureLearningAbbrev = "LGCL";
		public const string kLanguageAssessmentAbbrev = "LGAS";
		public const string kLanguageProgramManagementAbbrev = "LPMT";
		public const string kLanguageTechnologyAbbrev = "LTEC";
		public const string kLearningAndDevelopmentAbbrev = "LRND";
		public const string kLibraryOrMuseumOrArchivingAbbrev = "LIBR";
		public const string kLiteracyAndEducationAbbrev = "LTCY";
		public const string kManagementAbbrev = "MGMT (Management, General)";
		public const string kPublishingAbbrev = "PUBL";
		public const string kScriptureUseAbbrev = "SUSE";
		public const string kSignLanguagesAbbrev = "SIGN";
		public const string kSociolinguisticsAbbrev = "SLNG";
		public const string kTranslationAbbrev = "TRAN";
		public const string kVernacularMediaAbbrev = "VEMA";

		// other SIL METS keys
		public const string kSourceFilesForMets = "files";
		public const string kVernacularMaterialsType = "ramp.vernacularmaterialstype";
		public const string kImageExtent = "format.extent.images";
		public const string kDatasetExtent = "format.extent.dataset";
		public const string kRecordingExtent = "format.extent.recording";
		public const string kAudience = "broad_type";
		public const string kRole = "role";
		public const string kLanguageName = "lang";
		public const string kPromotionDescription = "sil.description.promotion";

		// Audience ("Broad Type") constants
		public const string kAudienceVernacular = "vernacular";
		public const string kAudienceTraining = "training";
		public const string kAudienceInternal = "internal";
		public const string kAudienceWide = "wider_audience";

		// Material and Contents Types
		public const string kVernacularMaterialScripture = "scripture";
		public const string kVernacularMaterialGeneral = "generalVernacular";

		// File-related constants
		public const string kFileDescription = "description";
		public const string kFileRelationship = "relationship";
		public const string kRelationshipSource = "Source";
		public const string kRelationshipPresentation = "Presentation";
		public const string kRelationshipSupporting = "Supporting";
// ReSharper restore CSharpWarnings::CS1591
		#endregion

		#region Data members
		private readonly List<string> _metsPairs;
		private AudienceType _metsAudienceType;
		private string _metsFilePath;
		private string _tempFolder;
		private Timer _timer;
		private bool _workerException;
		private Dictionary<string, string> _languageList;
		private readonly Func<string, string, string> _getFileDescription; // first param is filelist key, second param is filename
		private int _imageCount = -1;
		private int _audioCount = -1;
		private int _videoCount = -1;
		private HashSet<string> _modes;
		#endregion

		#region properties
		/// ------------------------------------------------------------------------------------
		internal override string ArchiveType
		{
			get { return LocalizationManager.GetString("DialogBoxes.ArchivingDlg.RAMPArchiveType", "RAMP (SIL Only)"); }
		}

		/// ------------------------------------------------------------------------------------
		public override string NameOfProgramToLaunch
		{
			get { return kRampProcessName; }
		}

		/// ------------------------------------------------------------------------------------
		public override string InformativeText
		{
			get
			{
				return string.Format(LocalizationManager.GetString("DialogBoxes.ArchivingDlg.RAMPOverviewText",
					"{0} is a utility for entering metadata and uploading submissions to SIL's internal archive, " +
					"REAP. If you have access to this archive, this tool will help you use {0} to archive your " +
					"{1} data. {2} When the {0} package has been created, you can  launch {0} and enter any " +
					"additional information before doing the actual submission.",
					"Parameter 0  is the word 'RAMP' (the first one will be turned into a hyperlink); " +
					"Parameter 1 is the name of the calling (host) program (SayMore, FLEx, etc.); " +
					"Parameter 2 is additional app-specifc information."), NameOfProgramToLaunch, AppName,
					_appSpecificArchivalProcessInfo);
			}
		}

		/// ------------------------------------------------------------------------------------
		public override string ArchiveInfoHyperlinkText
		{
			get { return NameOfProgramToLaunch; }
		}

		/// ------------------------------------------------------------------------------------
		public override string ArchiveInfoUrl
		{
			get { return Properties.Settings.Default.RampWebSite; }
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Show the count of audio/video files rather than the length
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public bool ShowRecordingCountNotLength { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Are image files to be counted as photographs or graphics
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public bool ImagesArePhotographs { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the number of image files in the list(s) of files to archive.
		/// </summary>
		/// <remarks>Public (and self-populating on-demand) to facilitate testing</remarks>
		/// ------------------------------------------------------------------------------------
		public int ImageCount
		{
			get
			{
				if (_fileLists != null && _imageCount < 0)
					ExtractInformationFromFiles();
				return _imageCount;
			}
		}

		/// ------------------------------------------------------------------------------------
		public int AudioCount
		{
			get
			{
				if (_fileLists != null && _audioCount < 0)
					ExtractInformationFromFiles();
				return _audioCount;
			}
		}

		/// ------------------------------------------------------------------------------------
		public int VideoCount
		{
			get
			{
				if (_fileLists != null && _videoCount < 0)
					ExtractInformationFromFiles();
				return _videoCount;
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets a comma-separated list of types found in the files to be archived
		/// (e.g. Text, Video, etc.).
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void ExtractInformationFromFiles()
		{
			ExtractInformationFromFiles(_fileLists.SelectMany(f => f.Value.Item1));
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets a comma-separated list of types found in the files to be archived
		/// (e.g. Text, Video, etc.).
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void ExtractInformationFromFiles(IEnumerable<string> files)
		{
			_imageCount = 0;
			_audioCount = 0;
			_videoCount = 0;
			_modes = new HashSet<string>();

			AddModesToSet(files);
		}

		/// ------------------------------------------------------------------------------------
		private void AddModesToSet(IEnumerable<string> files)
		{
			foreach (var file in files)
			{
				if (FileUtils.GetIsZipFile(file))
				{
					using (var zipFile = new ZipFile(file))
						AddModesToSet(zipFile.EntryFileNames);
					continue;
				}

				if (FileUtils.GetIsAudio(file))
				{
					_audioCount++;
					_modes.Add(kModeSpeech);
				}
				if (FileUtils.GetIsVideo(file))
				{
					_videoCount++;
					_modes.Add(kModeVideo);
				}
				if (FileUtils.GetIsText(file))
					_modes.Add(kModeText);
				if (FileUtils.GetIsImage(file))
				{
					_imageCount++;
					_modes.Add(ImagesArePhotographs ? kModePhotograph : kModeGraphic);
				}
				if (FileUtils.GetIsMusicalNotation(file))
					_modes.Add(kModeMusicalNotation);
				if (FileUtils.GetIsDataset(file))
					_modes.Add(kModeDataset);
				if (FileUtils.GetIsSoftwareOrFont(file))
					_modes.Add(kModeSoftwareOrFont);
				if (FileUtils.GetIsPresentation(file))
					_modes.Add(kModePresentation);
			}
		}
		#endregion

		#region construction and initialization
		/// ------------------------------------------------------------------------------------
		/// <summary>Constructor</summary>
		/// <param name="appName">The application name</param>
		/// <param name="title">Title of the submission</param>
		/// <param name="id">Identifier (used as filename) for the package being created</param>
		/// <param name="appSpecificArchivalProcessInfo">Application can use this to pass
		/// additional information that will be displayed to the user in the dialog to explain
		/// any application-specific details about the archival process.</param>
		/// <param name="setFilesToArchive">Delegate to request client to call methods to set
		/// which files should be archived (this is deferred to allow display of progress message)</param>
		/// <param name="getFileDescription">Callback function to get a file description based
		/// on the file-list key (param 1) and the filename (param 2)</param>
		/// ------------------------------------------------------------------------------------
		public RampArchivingDlgViewModel(string appName, string title, string id,
			string appSpecificArchivalProcessInfo, Action<ArchivingDlgViewModel> setFilesToArchive,
			Func<string, string, string> getFileDescription) : base(appName, title, id,
			appSpecificArchivalProcessInfo, setFilesToArchive)
		{
			if (getFileDescription == null)
				throw new ArgumentNullException("getFileDescription");
			_getFileDescription = getFileDescription;

			ShowRecordingCountNotLength = false;
			ImagesArePhotographs = true;

			_metsPairs = new List<string>(new [] {JSONUtils.MakeKeyValuePair(kPackageTitle, _titles[_id])});

			foreach (var orphanedRampPackage in Directory.GetFiles(Path.GetTempPath(), "*" + kRampFileExtension))
			{
				try { File.Delete(orphanedRampPackage); }
// ReSharper disable once EmptyGeneralCatchClause
				catch { }
			}
		}

		/// ------------------------------------------------------------------------------------
		override protected bool DoArchiveSpecificInitialization()
		{
			DisplayMessage(LocalizationManager.GetString("DialogBoxes.ArchivingDlg.SearchingForRampMsg",
				"Searching for the RAMP program..."), MessageType.Volatile);

			Application.DoEvents();
			PathToProgramToLaunch = GetExeFileLocation();

			if (PathToProgramToLaunch == null)
			{
				DisplayMessage(LocalizationManager.GetString("DialogBoxes.ArchivingDlg.RampNotFoundMsg",
					"The RAMP program cannot be found!"), MessageType.Error);
				return false;
			}

			return true;
		}

		/// ------------------------------------------------------------------------------------
		public override int CalculateMaxProgressBarValue()
		{
			// One for analyzing each list, one for copying each file, one for adding each file
			// to the zip file and one for the mets.xml file.
			return _fileLists.Count + 2 * _fileLists.SelectMany(kvp => kvp.Value.Item1).Count() + 1;
		}
		#endregion

		#region Methods to add app-specific METS pairs
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Sets the "Broad Type", the audience for which the resource being archived is
		/// primarily intended. This is set automatically as a side-effect of setting the
		/// stage or type.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public void SetAudience(AudienceType audienceType)
		{
			if (IsMetadataPropertySet(MetadataProperties.Audience))
			{
				if (_metsAudienceType != audienceType)
					throw new InvalidOperationException(string.Format("Audience has already been set and cannot be changed to a different value."));
				return; // Already added
			}

			MarkMetadataPropertyAsSet(MetadataProperties.Audience);
			_metsAudienceType = audienceType;

			string audience;
			switch (audienceType)
			{
				case AudienceType.Vernacular: audience = kAudienceVernacular; break;
				case AudienceType.Training: audience = kAudienceTraining; break;
				case AudienceType.Internal: audience = kAudienceInternal; break;
				case AudienceType.Wider: audience = kAudienceWide; break;
				default:
					throw new NotImplementedException("Need to add appropriate METS constant for audience type " + audienceType);
			}
			_metsPairs.Add(JSONUtils.MakeKeyValuePair(kAudience, audience));
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Sets the type of vernacular material represented by the resource being archived.
		/// Setting one of these types also indicates that the resource is intended for use by
		/// the general population in the vernacular language community.
		/// For resources containing vernacular material related to the translation task but not
		/// specifically Scripture or other material intended for use by the general population
		/// (e.g. back translation, checking questions, key terms list), call
		/// SetInternalWorkType instead.
		/// </summary>
		/// <param name="vernacularMaterialsTypes">Types of material(s) that this resource
		/// contains. To specify only the broad type of materials but not details about the
		/// contents, you can specify merely Scripture or Other (but never both); otherwise,
		/// use the more specific content types. If this resource contains multiple types of
		/// content, "OR" the types together (but don't mix "Bible" types with the other types).
		/// </param>
		/// ------------------------------------------------------------------------------------
		public void SetVernacularMaterialsAndContentType(VernacularMaterialsType vernacularMaterialsTypes)
		{
			SetAudience(AudienceType.Vernacular);

			PreventDuplicateMetadataProperty(MetadataProperties.Type);

			if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.Scripture))
			{
				if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.Other))
					throw new ArgumentException("Resource cannot ");
				_metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularMaterialsType, kVernacularMaterialScripture));
			}
			else if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.Other))
				_metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularMaterialsType, kVernacularMaterialGeneral));

			// Scripture
			if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.BibleBackground))
				_metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, ""));

			// Other - Community & Culture
			if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.CommunityAndCulture_Calendar))
				_metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Calendar"));
			if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.CommunityAndCulture_CivicEducation))
				_metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Civic education"));
			if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.CommunityAndCulture_CommunityNews))
				_metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Community news"));
			if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.CommunityAndCulture_CultureAndFolklore))
				_metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Culture and folklore"));
			if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.CommunityAndCulture_ProverbsAndMaxims))
				_metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Proverbs and maxims"));
			if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.CommunityAndCulture_SongLyrics))
				_metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Songs"));

			// Other - Community & Culture
			if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.ResourceFormats_ActivityBook))
				_metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Activity book"));
			if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.ResourceFormats_TeachersGuide))
				_metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Teacher's guide"));
			if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.ResourceFormats_Textbook))
				_metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Textbook"));
			if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.ResourceFormats_WallChart))
				_metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Wall chart"));
			if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.ResourceFormats_Workbook))
				_metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Workbook"));

			// Other - Community Development
			if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.CommunityDevelopment_AgricultureAndFoodProduction))
				_metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Agriculture and food production"));
			if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.CommunityDevelopment_BusinessAndIncomeGeneration))
				_metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Business and income generation"));
			if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.CommunityDevelopment_EnvironmentalCare))
				_metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Environmental care"));
			if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.CommunityDevelopment_InstructionalManual))
				_metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Instructional manual"));
			if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.CommunityDevelopment_VocationalEducation))
				_metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Vocational education"));

			// Other - Health
			if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.Health_AIDSAndHIV))
				_metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "AIDS and HIV"));
			if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.Health_AvianFlu))
				_metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Avian flu"));
			if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.Health_GeneralHealthAndHygiene))
				_metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Health and hygiene"));
			if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.Health_Malaria))
				_metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Malaria"));
			if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.Health_PrenatalAndInfantCare))
				_metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Prenatal and infant care"));

			// Other - Language Acquisition
			if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.LanguageAcquisition_EnglishLanguageInstruction))
				_metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "English language instruction"));
			if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.LanguageAcquisition_GrammarInstruction))
				_metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Grammar instruction"));
			if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.LanguageAcquisition_LanguageInstruction))
				_metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Language instruction"));
			if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.LanguageAcquisition_PhraseBook))
				_metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Phrase book"));

			// Other - Literacy Education
			if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.LiteracyEducation_Alphabet))
				_metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Alphabet"));
			if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.LiteracyEducation_Prereading))
				_metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Prereading"));
			if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.LiteracyEducation_Prewriting))
				_metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Prewriting"));
			if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.LiteracyEducation_Primer))
				_metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Primer"));
			if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.LiteracyEducation_Reader))
				_metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Reader"));
			if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.LiteracyEducation_Riddles))
				_metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Riddles"));
			if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.LiteracyEducation_Spelling))
				_metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Spelling"));
			if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.LiteracyEducation_TonePrimer))
				_metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Tone primer"));
			if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.LiteracyEducation_TransitionPrimer))
				_metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Transition primer"));
			if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.LiteracyEducation_Vocabulary))
				_metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Vocabulary"));
			if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.LiteracyEducation_Writing))
				_metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Writing"));

			// Other - Scripture Use
			if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.ScriptureUse_Catechism))
				_metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Catechism"));
			if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.ScriptureUse_ChristianLiving))
				_metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Christian living"));
			if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.ScriptureUse_Devotional))
				_metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Devotional"));
			if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.ScriptureUse_Evangelistic))
				_metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Evangelistic"));
			if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.ScriptureUse_Theology))
				_metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Theology"));
			if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.ScriptureUse_WorshipAndLiturgy))
				_metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Worship and liturgy"));

			// Other - Vernacular Education
			if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.VernacularEducation_Arithmetic))
				_metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Arithmetic"));
			if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.VernacularEducation_Arts))
				_metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Arts"));
			if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.VernacularEducation_Ethics))
				_metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Ethics"));
			if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.VernacularEducation_Numbers))
				_metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Numbers"));
			if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.VernacularEducation_Science))
				_metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Science"));
			if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.VernacularEducation_SocialStudies))
				_metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Social studies"));
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Sets the type of training resource represented by the resource being archived.
		/// Setting one of these types also indicates that the resource is intended for use in a
		/// classroom or instructional setting for SIL or Wycliffe role-related training
		/// (for published textbooks call SetScholarlyWorkType instead as these are generally for
		/// a wider audience).
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public void SetTrainingResourceType(TrainingResourceType trainingResourceType)
		{
			SetAudience(AudienceType.Training);

			PreventDuplicateMetadataProperty(MetadataProperties.Type);

			// TODO: This is currently not used.
			//string type;
			switch (trainingResourceType)
			{
				default:
					throw new NotImplementedException("Need to add appropriate METS constant for training resource type " + trainingResourceType);
			}

			// TODO: This is currently not reachable.
			//_metsPairs.Add(JSONUtils.MakeKeyValuePair(kScholarlyWorkType, type));
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Sets the type of training resource represented by the resource being archived.
		/// Setting one of these types also indicates that the resource is intended for use in a
		/// classroom or instructional setting for SIL or Wycliffe role-related training
		/// (for published textbooks call SetScholarlyWorkType instead as these are generally for
		/// a wider audience).
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public void SetInternalWorkType(InternalWorkType internalWorkType)
		{
			SetAudience(AudienceType.Internal);

			PreventDuplicateMetadataProperty(MetadataProperties.Type);

			string type;
			switch (internalWorkType)
			{
				case InternalWorkType.Correspondence: type = "Correspondence"; break;
				case InternalWorkType.InternalReport: type = "Internal report"; break;
				case InternalWorkType.Map: type = "Map"; break;
				case InternalWorkType.PositionPaper: type = "Position paper"; break;
				case InternalWorkType.Other: type = "Other"; break;
				default:
					throw new NotImplementedException("Need to add appropriate METS constant for internal work type " + internalWorkType);
			}
			_metsPairs.Add(JSONUtils.MakeKeyValuePair(kScholarlyWorkType, type));
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Sets the bibliographic type of the resource being archived (for a "wider audience")
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public void SetScholarlyWorkType(ScholarlyWorkType scholarlyWorkType)
		{
			SetAudience(AudienceType.Wider);

			PreventDuplicateMetadataProperty(MetadataProperties.Type);

			string type;
			switch (scholarlyWorkType)
			{
				case ScholarlyWorkType.PrimaryData: type = "Data set"; break;
				default:
					throw new NotImplementedException("Need to add appropriate METS constant for scholarly work type " + scholarlyWorkType);
			}
			_metsPairs.Add(JSONUtils.MakeKeyValuePair(kScholarlyWorkType, type));
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Sets the current stage of development of his resource.
		/// </summary>
		/// <param name="stage">Some work stage values relate to work being done for a
		/// particular type of audience. Where a stage is relevant to only a single type of
		/// audience, the audience will already be correctly inferred from the stage. For
		/// stages that can apply to more than one type of audience, either call SetAudience
		/// directly or "OR" the appropriate AudienceType with the WorkStage when calling this
		/// method. Not that some work stages can apply to a subset of the auidence types, so
		/// pay attention to the comments for each work stage to avoid pairing it with an
		/// invalid audience type.</param>
		/// ------------------------------------------------------------------------------------
		public void SetStage(WorkStage stage)
		{
			PreventDuplicateMetadataProperty(MetadataProperties.Stage);

			// Some of the work stages imply a particular audience and therefore have the appropriate audience bit set
			if (stage.HasFlag(AudienceType.Vernacular))
				SetAudience(AudienceType.Vernacular);
			if (stage.HasFlag(AudienceType.Training))
				SetAudience(AudienceType.Training);
			if (stage.HasFlag(AudienceType.Internal))
				SetAudience(AudienceType.Internal);
			if (stage.HasFlag(AudienceType.Wider))
				SetAudience(AudienceType.Wider);

			if (stage.HasFlag(WorkStage.RoughDraft))
				SetStage(kStageRoughDraft);
			if (stage.HasFlag(WorkStage.SelfReviewedDraft))
				SetStage(kStageReviewedDraft);
			if (stage.HasFlag(WorkStage.ConsultantOrEditorReleasedDraft))
			{
				PreventInvalidAudienceTypeForWorkStage(AudienceType.Training | AudienceType.Internal,
					WorkStage.ConsultantOrEditorReleasedDraft);
				SetStage(kStageReleasedDraft);
			}
			if (stage.HasFlag(WorkStage.ConsultantOrEditorApproved))
			{
				PreventInvalidAudienceTypeForWorkStage(AudienceType.Training, WorkStage.ConsultantOrEditorApproved);
				SetStage(kStageApprovedDraft);
			}
			if (stage.HasFlag(WorkStage.InPressOrPublished))
			{
				PreventInvalidAudienceTypeForWorkStage(AudienceType.Internal, WorkStage.InPressOrPublished);
				SetStage(kStagePublished);
			}
			if (stage.HasFlag(WorkStage.UsedInTrainingCourse))
				SetStage(kStageUsedInCourse);
			if (stage.HasFlag(WorkStage.ReadyForPublicationOrFormalPreprint))
			{
				PreventInvalidAudienceTypeForWorkStage(AudienceType.Vernacular | AudienceType.Internal,
					WorkStage.ReadyForPublicationOrFormalPreprint);
				SetStage(kStagePrepublication);
			}
			if (stage.HasFlag(WorkStage.FinishedInternal))
				SetStage(kStageFinished);
		}

		/// ------------------------------------------------------------------------------------
// ReSharper disable once UnusedParameter.Local
		private void PreventInvalidAudienceTypeForWorkStage(Enum invalidAudience, WorkStage stage)
		{
			if ((invalidAudience != null) && (invalidAudience.HasFlag(_metsAudienceType)))
			{
				throw new InvalidOperationException(string.Format(
					"Resources with an audience of \"{0}\" cannot have a work stage of {1}",
					_metsAudienceType, stage));
			}
		}

		/// ------------------------------------------------------------------------------------
		private void SetStage(string stage)
		{
			_metsPairs.Add(JSONUtils.MakeKeyValuePair(kStageDescription, stage));
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Sets the SIL Domains and/or sub-domains to which the resource being archived relates.
		/// </summary>
		/// <param name="domains">The domains(s) and/or sub-domains. If the resource relates to
		/// multiple domains, "OR" them together to set all the necessary flags. It is not
		/// necessary to OR the sub-domains with their corresponding domains, since the
		/// subdomains already have the correct domain bits set.</param>
		/// ------------------------------------------------------------------------------------
		public void SetDomains(SilDomain domains)
		{
			PreventDuplicateMetadataProperty(MetadataProperties.Domains);

			if (domains.HasFlag(SilDomain.AcademicTraining))
				AddDomain(kAcademicTrainingAbbrev, "Academic Training");

			if (domains.HasFlag(SilDomain.Anthropology))
			{
				AddDomain(kAnthropologyAbbrev, "Anthropology");

				// sub-domains
				if (domains.HasFlag(SilDomain.Anth_Ethnography))
					AddSubDomain(kAnthropologyAbbrev, "ethnography");
				if (domains.HasFlag(SilDomain.Anth_Interview))
					AddSubDomain(kAnthropologyAbbrev, "interview");
				if (domains.HasFlag(SilDomain.Anth_KinshipAnalysis))
					AddSubDomain(kAnthropologyAbbrev, "kinship analysis");
			}

			if (domains.HasFlag(SilDomain.ArtsandEthnomusicology))
			{
				AddDomain(kEthnomusicologyAbbrev, "Arts and Ethnomusicology");

				// sub-domains
				if (domains.HasFlag(SilDomain.Emus_ArtisticCommunicationProfile))
					AddSubDomain(kEthnomusicologyAbbrev, "artistic communication profile");
				if (domains.HasFlag(SilDomain.Emus_PerformanceCollection))
					AddSubDomain(kEthnomusicologyAbbrev, "aperformance collection");
				if (domains.HasFlag(SilDomain.Emus_SongCollection))
					AddSubDomain(kEthnomusicologyAbbrev, "song collection");
				if (domains.HasFlag(SilDomain.Emus_SummaryArtisticEventFormAnalysis))
					AddSubDomain(kEthnomusicologyAbbrev, "summary artistic event form analysis");
				if (domains.HasFlag(SilDomain.Emus_SummaryArtisticGenreAnalysis))
					AddSubDomain(kEthnomusicologyAbbrev, "summary artistic genre analysis");
			}

			if (domains.HasFlag(SilDomain.Communications))
				AddDomain(kCommunicationsAbbrev, "Communications");
			if (domains.HasFlag(SilDomain.CommunityDevelopment))
				AddDomain(kCommunityDevelopmentAbbrev, "Community Development");
			if (domains.HasFlag(SilDomain.Counseling))
				AddDomain(kCounselingAbbrev, "Counseling");
			if (domains.HasFlag(SilDomain.InternationalOrGovernmentRelations))
				AddDomain(kInternationalOrGovernmentRelationsAbbrev, "International/Government Relations");

			if (domains.HasFlag(SilDomain.LanguageAssessment))
			{
				AddDomain(kLanguageAssessmentAbbrev, "Language Assessment");

				// sub-domains
				if (domains.HasFlag(SilDomain.Lgas_SurveyArtifacts))
					AddSubDomain(kLanguageAssessmentAbbrev, "survey artifacts");
				if (domains.HasFlag(SilDomain.Lgas_SurveyInstrument))
					AddSubDomain(kLanguageAssessmentAbbrev, "survey instrument");
				if (domains.HasFlag(SilDomain.Lgas_SurveyReport))
					AddSubDomain(kLanguageAssessmentAbbrev, "survey report");
				if (domains.HasFlag(SilDomain.Lgas_Wordlist))
					AddSubDomain(kLanguageAssessmentAbbrev, "wordlist");
			}

			if (domains.HasFlag(SilDomain.LanguageProgramManagement))
				AddDomain(kLanguageProgramManagementAbbrev, "Language Program Management");

			if (domains.HasFlag(SilDomain.LanguageTechnology))
			{
				AddDomain(kLanguageTechnologyAbbrev, "Language Technology");

				// sub-domains
				if (domains.HasFlag(SilDomain.Ltec_ComputerTool))
					AddSubDomain(kLanguageTechnologyAbbrev, "computer tool");
			}

			if (domains.HasFlag(SilDomain.LanguageAndCultureLearning))
				AddDomain(kLanguageAndCultureLearningAbbrev, "Language and Culture Learning");
			if (domains.HasFlag(SilDomain.LearningAndDevelopment))
				AddDomain(kLearningAndDevelopmentAbbrev, "Learning and Development");
			if (domains.HasFlag(SilDomain.LibraryOrMuseumOrArchiving))
				AddDomain(kLibraryOrMuseumOrArchivingAbbrev, "Library/Museum/Archiving");

			if (domains.HasFlag(SilDomain.Linguistics))
			{
				AddDomain(kLinguisticsAbbrev, "Linguistics");

				// sub-domains
				if (domains.HasFlag(SilDomain.Ling_ComparativeDescription))
					AddSubDomain(kLinguisticsAbbrev, "comparative description");
				if (domains.HasFlag(SilDomain.Ling_DiscourseDescription))
					AddSubDomain(kLinguisticsAbbrev, "discourse description");
				if (domains.HasFlag(SilDomain.Ling_GrammaticalDescription))
					AddSubDomain(kLinguisticsAbbrev, "grammatical description");
				if (domains.HasFlag(SilDomain.Ling_InterlinearizedText))
					AddSubDomain(kLinguisticsAbbrev, "interlinearized text");
				if (domains.HasFlag(SilDomain.Ling_LanguageDocumentation))
					AddSubDomain(kLinguisticsAbbrev, "language documentation");
				if (domains.HasFlag(SilDomain.Ling_Lexicon))
					AddSubDomain(kLinguisticsAbbrev, "lexicon");
				if (domains.HasFlag(SilDomain.Ling_PhonologicalDescription))
					AddSubDomain(kLinguisticsAbbrev, "phonological description");
				if (domains.HasFlag(SilDomain.Ling_SemanticDescription))
					AddSubDomain(kLinguisticsAbbrev, "semantic description");
				if (domains.HasFlag(SilDomain.Ling_Text))
					AddSubDomain(kLinguisticsAbbrev, "text (primary language data)");
			}

			if (domains.HasFlag(SilDomain.LiteracyAndEducation))
			{
				AddDomain(kLiteracyAndEducationAbbrev, "Literacy and Education");

				// sub-domains
				if (domains.HasFlag(SilDomain.Ltcy_OrthographyDescription))
					AddSubDomain(kLiteracyAndEducationAbbrev, "orthography description");
			}

			if (domains.HasFlag(SilDomain.Management))
				AddDomain(kManagementAbbrev, "Management, General");
			if (domains.HasFlag(SilDomain.Publishing))
				AddDomain(kPublishingAbbrev, "Publishing");
			if (domains.HasFlag(SilDomain.ScriptureUse))
				AddDomain(kScriptureUseAbbrev, "Scripture Use");
			if (domains.HasFlag(SilDomain.SignLanguages))
				AddDomain(kSignLanguagesAbbrev, "Sign Languages");

			if (domains.HasFlag(SilDomain.Sociolinguistics))
			{
				AddDomain(kSociolinguisticsAbbrev, "Sociolinguistics");

				// sub-domains
				if (domains.HasFlag(SilDomain.Slng_SociolinguisticDescription))
					AddSubDomain(kSociolinguisticsAbbrev, "sociolinguistic description");
			}

			if (domains.HasFlag(SilDomain.Translation))
			{
				AddDomain(kTranslationAbbrev, "Translation");

				// sub-domains
				if (domains.HasFlag(SilDomain.Tran_BackTranslation))
					AddSubDomain(kTranslationAbbrev, "back translation");
				if (domains.HasFlag(SilDomain.Tran_BibleNames))
					AddSubDomain(kTranslationAbbrev, "Bible names");
				if (domains.HasFlag(SilDomain.Tran_ComprehensionCheckingQuestions))
					AddSubDomain(kTranslationAbbrev, "comprehension checking questions");
				if (domains.HasFlag(SilDomain.Tran_Exegesis))
					AddSubDomain(kTranslationAbbrev, "exegesis");
				if (domains.HasFlag(SilDomain.Tran_KeyBibleTerms))
					AddSubDomain(kTranslationAbbrev, "key Bible terms");
			}

			if (domains.HasFlag(SilDomain.VernacularMedia))
				AddDomain(kVernacularMediaAbbrev, "VernacularMedia");
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Adds a METS pair for the given domain
		/// </summary>
		/// <param name="domainAbbrev">Four-letter (ALL CAPS) abbreviation for the SIL domain</param>
		/// <param name="domainName">Domain name (Title Case)</param>
		/// ------------------------------------------------------------------------------------
		private void AddDomain(string domainAbbrev, string domainName)
		{
			// if a mets pair already exists for domains, add this domain to the existing list.
			var existingValue = _metsPairs.Find(s => s.Contains(kSilDomain));
			if (!string.IsNullOrEmpty(existingValue))
			{
				int pos = existingValue.IndexOf(']');
				string newValue = existingValue.Insert(pos, string.Format(",\"{0}:{1}\"", domainAbbrev, domainName));
				_metsPairs[_metsPairs.IndexOf(existingValue)] = newValue;
			}
			else
			{
				_metsPairs.Add(JSONUtils.MakeKeyValuePair(kSilDomain, string.Format("{0}:{1}", domainAbbrev, domainName), true));
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Adds a METS pair for the given sub-domain
		/// </summary>
		/// <param name="domainAbbrev">Four-letter (ALL CAPS) abbreviation for the SIL domain</param>
		/// <param name="subDomain">sub-domain name (all lowercase)</param>
		/// ------------------------------------------------------------------------------------
		private void AddSubDomain(string domainAbbrev, string subDomain)
		{
			// if a mets pair already exists for this domain, add this subdomain to the existing list.
			var key = string.Format(kFmtDomainSubtype, domainAbbrev);
			var existingValue = _metsPairs.Find(s => s.Contains(key));
			if (!string.IsNullOrEmpty(existingValue))
			{
				int pos = existingValue.IndexOf(']');
				string newValue = existingValue.Insert(pos, string.Format(",\"{0} ({1})\"", subDomain, domainAbbrev));
				_metsPairs[_metsPairs.IndexOf(existingValue)] = newValue;
			}
			else
			{
				_metsPairs.Add(JSONUtils.MakeKeyValuePair(key,
					string.Format("{0} ({1})", subDomain, domainAbbrev), true));
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Adds a METS pair for the Date this resource was initially created
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public void SetCreationDate(DateTime date)
		{
			SetCreationDate(date.ToString("yyyy-MM-dd"));
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Adds a METS pair for the range of years (YYYY-YYYY) during which data
		/// was collected or work was being done on this resource
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public void SetCreationDate(int startYear, int endYear)
		{
			if (endYear < startYear)
				throw new ArgumentException("startYear must be before endYear");

			string startYr = Math.Abs(startYear).ToString("D4");
			if (startYear < 0 && endYear > 0)
				startYr += " BCE";
			string endYr = Math.Abs(endYear).ToString("D4");
			if (startYear < 0)
				endYr += (endYear < 0) ? " BCE" : " CE";

			SetCreationDate(string.Format("{0}-{1}", startYr, endYr));
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Adds a METS pair for the range of years (YYYY-YYYY) during which data
		/// was collected or work was being done on this resource
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public void SetCreationDate(string date)
		{
			if (string.IsNullOrEmpty(date))
				throw new ArgumentNullException("date");

			PreventDuplicateMetadataProperty(MetadataProperties.CreationDate);

			_metsPairs.Add(JSONUtils.MakeKeyValuePair(kDateCreated, date));
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Adds a METS pair for the Date this resource was last updated
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public void SetModifiedDate(DateTime date)
		{
			PreventDuplicateMetadataProperty(MetadataProperties.ModifiedDate);

			_metsPairs.Add(JSONUtils.MakeKeyValuePair(kDateModified, date.ToString("yyyy-MM-dd")));
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Sets the subject language for a resource that is concerned with a particular
		/// language or language community. ENHANCE: Deal with dialect options.
		/// </summary>
		/// <param name="iso3Code">The 3-letter ISO 639-2 code for the language</param>
		/// <param name="languageName">The English name of the language</param>
		/// ------------------------------------------------------------------------------------
		public void SetSubjectLanguage(string iso3Code, string languageName)
		{
			PreventDuplicateMetadataProperty(MetadataProperties.SubjectLanguage);

			SetFlag(kFlagHasSubjectLanguage);
			_metsPairs.Add(JSONUtils.MakeArrayFromValues(kSubjectLanguage,
				new[] { JSONUtils.MakeKeyValuePair(kDefaultKey, iso3Code + ":" + GetLanguageName(iso3Code)) }));
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Sets the given METS flag (typically appears as a checkbox in RAMP) to true/yes ("Y")
		/// </summary>
		/// <param name="flagKey">One of the kFlag... contants</param>
		/// ------------------------------------------------------------------------------------
		private void SetFlag(string flagKey)
		{
			_metsPairs.Add(JSONUtils.MakeKeyValuePair(flagKey, kTrue));
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Sets specific requirements of software, font, or data schema support needed to use
		/// or interpret this resource.
		/// </summary>
		/// <param name="requirements">Be as specific a possible, indicating version number(s)
		/// where appropriate</param>
		/// ------------------------------------------------------------------------------------
		public void SetSoftwareRequirements(params string[] requirements)
		{
			SetSoftwareRequirements((IEnumerable<string>)requirements);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Sets specific requirements of software, font, or data schema support needed to use
		/// or interpret this resource.
		/// </summary>
		/// <param name="requirements">Be as specific a possible, indicating version number(s)
		/// where appropriate</param>
		/// ------------------------------------------------------------------------------------
		public void SetSoftwareRequirements(IEnumerable<string> requirements)
		{
			requirements = requirements.Where(r => !string.IsNullOrEmpty(r));

			HashSet<string> softwareKeyValuePairs = new HashSet<string>();
// ReSharper disable PossibleMultipleEnumeration
			if (requirements.Any())
			{
				PreventDuplicateMetadataProperty(MetadataProperties.SoftwareRequirements);

				SetFlag(kFlagHasSoftwareOrFontRequirements);

				foreach (var softwareOrFontName in requirements)
					softwareKeyValuePairs.Add(JSONUtils.MakeKeyValuePair(kDefaultKey, softwareOrFontName));

				_metsPairs.Add(JSONUtils.MakeArrayFromValues(kSoftwareOrFontRequirements, softwareKeyValuePairs));
			}
// ReSharper restore PossibleMultipleEnumeration
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Sets the language in which this resource is written. If this resource is a diglot or
		/// triglot work, specify all languages in which there is significant content in the
		/// work. (For language documentation, dictionaries, and the like, one of the languages
		/// specified should be the same as a subject language specified using
		/// SetSubjectLanguage.) ENHANCE: Deal with dialect options.
		/// </summary>
		/// <param name="iso3Codes">The 3-letter ISO 639-2 codes for the languages</param>
		/// ------------------------------------------------------------------------------------
		public void SetContentLanguages(params string[] iso3Codes)
		{
			var languages = new List<ArchivingLanguage>();

			foreach (var iso3Code in iso3Codes)
			{
				if (!string.IsNullOrEmpty(iso3Code))
					languages.Add(new ArchivingLanguage(iso3Code));
			}

			SetContentLanguages(languages);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Sets the language in which this resource is written. If this resource is a diglot or
		/// triglot work, specify all languages in which there is significant content in the
		/// work. (For language documentation, dictionaries, and the like, one of the languages
		/// specified should be the same as a subject language specified using
		/// SetSubjectLanguage.) ENHANCE: Deal with dialect options.
		/// </summary>
		/// <param name="languages">The 3-letter ISO 639-2 codes for the languages, as well as
		/// the English name, dialect and writing system script</param>
		/// ------------------------------------------------------------------------------------
		public void SetContentLanguages(IEnumerable<ArchivingLanguage> languages)
		{
			var languageValues = new HashSet<string>();
			var scripts = new HashSet<string>();

			foreach (var lang in languages.Where(r => !string.IsNullOrEmpty(r.Iso3Code)))
			{
				var langPair = JSONUtils.MakeKeyValuePair(kDefaultKey, string.Format("{0}:{1}", lang.Iso3Code, GetLanguageName(lang.Iso3Code)));
				if (!string.IsNullOrEmpty(lang.Dialect))
					langPair += "," + JSONUtils.MakeKeyValuePair("dialect", lang.Dialect);

				languageValues.Add(langPair);

				if (!string.IsNullOrEmpty(lang.Script))
					scripts.Add(JSONUtils.MakeKeyValuePair(kDefaultKey, lang.Script));
			}

			if (languageValues.Any())
			{
				PreventDuplicateMetadataProperty(MetadataProperties.ContentLanguages);

				_metsPairs.Add(JSONUtils.MakeArrayFromValues(kContentLanguages, languageValues));

				if (scripts.Any())
					_metsPairs.Add(JSONUtils.MakeArrayFromValues(kContentLanguageScripts, scripts));
			}


		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Sets the schema(s) to which this resource -- or the file(s) it contains -- conforms.
		/// </summary>
		/// <param name="schemaDescriptor">Known schema name (typically, but not nescessarily,
		/// for XML files). RAMP doesn't say what to do if the resource contains files conforming to
		/// multiple standards, but I would say just make a comma-separated list.</param>
		/// ------------------------------------------------------------------------------------
		public void SetSchemaConformance(string schemaDescriptor)
		{
			PreventDuplicateMetadataProperty(MetadataProperties.SchemaConformance);

			_metsPairs.Add(JSONUtils.MakeKeyValuePair(kSchemaConformance, schemaDescriptor));
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Sets the number of records/entries in this resource.
		/// </summary>
		/// <param name="extent">For example, "2505 entries"</param>
		/// ------------------------------------------------------------------------------------
		public void SetDatasetExtent(string extent)
		{
			if (string.IsNullOrEmpty(extent))
				throw new ArgumentNullException("extent");

			PreventDuplicateMetadataProperty(MetadataProperties.DatasetExtent);

			_metsPairs.Add(JSONUtils.MakeKeyValuePair(kDatasetExtent, extent));
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Sets the total duration of all audio and/or video recording in this resource.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public void SetAudioVideoExtent(TimeSpan totalDuration)
		{
			SetAudioVideoExtent(totalDuration.ToString());
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Sets the total duration of all audio and/or video recording in this resource.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public void SetAudioVideoExtent(string totalDuration)
		{
			PreventDuplicateMetadataProperty(MetadataProperties.RecordingExtent);

			_metsPairs.Add(JSONUtils.MakeKeyValuePair(kRecordingExtent, totalDuration));
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Sets the collection of contributors (and their roles) to this resource.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public void SetContributors(ContributionCollection contributions)
		{
			if (contributions == null)
				throw new ArgumentNullException("contributions");

			if (contributions.Count == 0)
				return;

			PreventDuplicateMetadataProperty(MetadataProperties.Contributors);

			_metsPairs.Add(JSONUtils.MakeArrayFromValues(kContributor,
				contributions.Select(GetContributorsMetsPairs)));
		}

		/// ------------------------------------------------------------------------------------
		private string GetContributorsMetsPairs(Contribution contribution)
		{
			var roleCode = (contribution.Role != null &&
				Properties.Settings.Default.RampContributorRoles.Contains(contribution.Role.Code) ?
				contribution.Role.Code : string.Empty);

			return JSONUtils.MakeKeyValuePair(kDefaultKey, contribution.ContributorName) +
				kSeparator + JSONUtils.MakeKeyValuePair(kRole, roleCode);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Sets the general description for this resource in a single language
		/// </summary>
		/// <param name="description">The description</param>
		/// <param name="language">ISO 639-2 3-letter language code (RAMP only supports about
		/// 20 major LWCs. Not sure what happens if an unrecognized code gets passed to this.
		/// Feel free to try it and find out.</param>
		/// ------------------------------------------------------------------------------------
		public void SetDescription(string description, string language)
		{
			SetGeneralDescription(new[] { GetKvpsForLanguageSpecificString(language, description) });
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Sets descriptions for this resource in (potentially) multiple languages
		/// </summary>
		/// <param name="descriptions">Dictionary of language->description, where the keys are
		/// ISO 639-2 3-letter language code (RAMP only supports about 20 major LWCs. Not sure
		/// what happens if an unrecognized code gets passed to this. Feel free to try it and
		/// find out.</param>
		/// ------------------------------------------------------------------------------------
		public void SetDescription(IDictionary<string, string> descriptions)
		{
			if (descriptions == null)
				throw new ArgumentNullException("descriptions");

			if (descriptions.Count == 0)
				return;

			List<string> abs = new List<string>();
			foreach (var desc in descriptions)
			{
				if (desc.Key.Length != 3)
					throw new ArgumentException();
				abs.Add(GetKvpsForLanguageSpecificString(desc.Key, desc.Value));
			}

			SetGeneralDescription(abs);
		}

		/// ------------------------------------------------------------------------------------
		private void SetGeneralDescription(IEnumerable<string> abs)
		{
			PreventDuplicateMetadataProperty(MetadataProperties.GeneralDescription);

			SetFlag(kFlagHasGeneralDescription);

			_metsPairs.Add(JSONUtils.MakeArrayFromValues(kGeneralDescription, abs));
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Sets abstracts for this resource in (potentially) multiple languages
		/// </summary>
		/// <param name="descriptions">Dictionary of language->abstract, where the keys are ISO
		/// 639-2 3-letter language code (RAMP only supports about 20 major LWCs. Not sure what
		/// happens if an unrecognized code gets passed to this. Feel free to try it and find
		/// out.</param>
		/// ------------------------------------------------------------------------------------
		protected override void SetAbstract_Impl(IDictionary<string, string> descriptions)
		{
			SetFlag(kFlagHasAbstractDescription);

			IEnumerable<string> metsDescriptions;

			if (descriptions.Count == 1 && string.IsNullOrEmpty(descriptions.Keys.ElementAt(0)))
				metsDescriptions = new [] {JSONUtils.MakeKeyValuePair(kDefaultKey, descriptions.Values.ElementAt(0))};
			else
				metsDescriptions = descriptions.Select(desc => GetKvpsForLanguageSpecificString(desc.Key, desc.Value));

			_metsPairs.Add(JSONUtils.MakeArrayFromValues(kAbstractDescription, metsDescriptions));
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Sets the promotion text for this resource in a single language
		/// </summary>
		/// <param name="text">The promotion text</param>
		/// <param name="language">ISO 639-2 3-letter language code (RAMP only supports about
		/// 20 major LWCs. Not sure what happens if an unrecognized code gets passed to this.
		/// Feel free to try it and find out.</param>
		/// ------------------------------------------------------------------------------------
		public void SetPromotion(string text, string language)
		{
			SetPromotion(new[] { GetKvpsForLanguageSpecificString(language, text) });
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Sets the promotion text for this resource in (potentially) multiple languages
		/// </summary>
		/// <param name="descriptions">Dictionary of language->promotion, where the keys are ISO
		/// 639-2 3-letter language code (RAMP only supports about 20 major LWCs. Not sure what
		/// happens if an unrecognized code gets passed to this. Feel free to try it and find
		/// out.</param>
		/// ------------------------------------------------------------------------------------
		public void SetPromotion(IDictionary<string, string> descriptions)
		{
			if (descriptions == null)
				throw new ArgumentNullException("descriptions");

			if (descriptions.Count == 0)
				return;

			List<string> abs = new List<string>();
			foreach (var desc in descriptions)
			{
				if (desc.Key.Length != 3)
					throw new ArgumentException();
				abs.Add(GetKvpsForLanguageSpecificString(desc.Key, desc.Value));
			}

			SetPromotion(abs);
		}

		/// ------------------------------------------------------------------------------------
		private void SetPromotion(IEnumerable<string> abs)
		{
			PreventDuplicateMetadataProperty(MetadataProperties.Promotion);

			SetFlag(kFlagHasPromotionDescription);

			_metsPairs.Add(JSONUtils.MakeArrayFromValues(kPromotionDescription, abs));
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets a pair of JSON key-value pairs for a language-specific string (used for the
		/// various language-specific descriptions)
		/// </summary>
		/// <param name="lang"></param>
		/// <param name="s"></param>
		/// ------------------------------------------------------------------------------------
		private string GetKvpsForLanguageSpecificString(string lang, string s)
		{
			if (lang.Length != 3)
				throw new ArgumentException("Language must be specified as a valid 3-letter code as specified in ISO-639-2.");

			return JSONUtils.MakeKeyValuePair(kDefaultKey, s) + kSeparator + JSONUtils.MakeKeyValuePair(kLanguageName, lang);
		}
		#endregion

		#region RAMP calling methods
		/// ------------------------------------------------------------------------------------
		internal override void LaunchArchivingProgram()
		{
			if (!File.Exists(PackagePath))
			{
				ReportError(null, string.Format("RAMP package prematurely removed: {0}", PackagePath));
				return;
			}

			LaunchArchivingProgram(EnsureRampHasFocusAndWaitForPackageToUnlock);
		}

		/// ------------------------------------------------------------------------------------
		private void EnsureRampHasFocusAndWaitForPackageToUnlock()
		{
			if (IsMono)
			{
				BringToFrontMono();
			}
			else
			{
				BringToFrontWindows();
			}

			// Every 4 seconds we'll check to see if the RAMP package is locked. When
			// it gets unlocked by RAMP, then we'll delete it.
			_timer = new Timer(CheckIfPackageFileIsLocked, PackagePath, 2000, 4000);
		}

		private static void BringToFrontWindows()
		{
			var processes = Process.GetProcessesByName(kRampProcessName);
			if (processes.Length < 1) return;

			// First, make the window topmost: this puts it in front of all other windows
			// and sets it as "always on top."
			SetWindowPos(processes[0].MainWindowHandle, HWND_TOPMOST, 0, 0, 0, 0, TOPMOST_FLAGS);

			// Second, make the window notopmost: this removes the "always on top" behavior
			// and positions the window on top of all other "not always on top" windows.
			SetWindowPos(processes[0].MainWindowHandle, HWND_NOTOPMOST, 0, 0, 0, 0, TOPMOST_FLAGS);
		}

		private static void BringToFrontMono()
		{
			// On mono this requires xdotool or wmctrl
			string args = null;
			if (!string.IsNullOrEmpty(FileLocator.LocateInProgramFiles("xdotool", true)))      /* try to find xdotool first */
				args = "-c \"for pid in `xdotool search --name RAMP`; do xdotool windowactivate $pid; done\"";
			else if (!string.IsNullOrEmpty(FileLocator.LocateInProgramFiles("wmctrl", true)))  /* if xdotool is not installed, look for wmctrl */
				args = "-c \"wmctrl -a RAMP\"";

			if (string.IsNullOrEmpty(args)) return;

			var prs = new Process
			{
				StartInfo =
				{
					FileName = "bash",
					Arguments = args,
					UseShellExecute = false,
					RedirectStandardError = true
				}
			};

			prs.Start();
		}

		/// ------------------------------------------------------------------------------------
		private void CheckIfPackageFileIsLocked(Object packageFile)
		{
			if (!FileUtils.IsFileLocked(packageFile as string))
				CleanUpTempRampPackage();
		}

		#endregion

		/// ------------------------------------------------------------------------------------
		/// <remarks>Public to facilitate testing</remarks>
		/// ------------------------------------------------------------------------------------
		public override bool CreatePackage()
		{
			IsBusy = true;

			var	success = CreateMetsFile() != null;

			if (success)
				success = CreateRampPackage();

			CleanUp();

			if (success)
			{
				DisplayMessage(LocalizationManager.GetString("DialogBoxes.ArchivingDlg.ReadyToCallRampMsg",
					"Ready to hand the package to RAMP"), MessageType.Success);
			}

			IsBusy = false;
			return success;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Get the path and file name of the RAMP executable file
		/// </summary>
		/// <returns>The full name of the RAMP executable file</returns>
		/// ------------------------------------------------------------------------------------
		public static string GetExeFileLocation()
		{
			return ArchivingPrograms.GetRampExeFileLocation();
		}

		#region Methods for creating mets file.
		/// ------------------------------------------------------------------------------------
		public override string GetMetadata()
		{
			var bldr = new StringBuilder();

			SetMetsPairsForFiles();

			foreach (var value in _metsPairs)
				bldr.AppendFormat("{0},", value);

			return string.Format("{{{0}}}", bldr.ToString().TrimEnd(','));
		}

		/// ------------------------------------------------------------------------------------
		public string CreateMetsFile()
		{
			try
			{
				var metsData = Resources.EmptyMets.Replace("<binData>", "<binData>" + JSONUtils.EncodeData(GetMetadata()));
				_tempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
				Directory.CreateDirectory(_tempFolder);
				_metsFilePath = Path.Combine(_tempFolder, "mets.xml");
				File.WriteAllText(_metsFilePath, metsData);
			}
			catch (Exception e)
			{
				if ((e is IOException) || (e is UnauthorizedAccessException) || (e is SecurityException))
				{
					ReportError(e, LocalizationManager.GetString("DialogBoxes.ArchivingDlg.CreatingInternalReapMetsFileErrorMsg",
						"There was an error attempting to create the RAMP/REAP mets file."));
					return null;
				}
				throw;
			}

			if (IncrementProgressBarAction != null)
				IncrementProgressBarAction();

			return _metsFilePath;
		}

		 /// ------------------------------------------------------------------------------------
		private void SetMetsPairsForFiles()
		{
			if (_fileLists.Any())
			{
				string value = GetMode();
				if (value != null)
					_metsPairs.Add(value);

				if (!IsMetadataPropertySet(MetadataProperties.Files))
				{
					// Return JSON array of files with their descriptions.
					_metsPairs.Add(JSONUtils.MakeArrayFromValues(kSourceFilesForMets,
						GetSourceFilesForMetsData(_fileLists)));
					MarkMetadataPropertyAsSet(MetadataProperties.Files);
				}

				if (ImageCount > 0)
					_metsPairs.Add(JSONUtils.MakeKeyValuePair(kImageExtent, string.Format("{0} image{1}.",
						_imageCount.ToString(CultureInfo.InvariantCulture),
						(_imageCount == 1) ? "" : "s")));

				var avExtent = new StringBuilder();
				const string delimiter = "; ";

				if (ShowRecordingCountNotLength)
				{
					if (_audioCount > 0)
						avExtent.AppendLineFormat("{0} audio recording file{1}", new object[] { _audioCount, (_audioCount == 1) ? "" : "s" }, delimiter);

					if (_videoCount > 0)
						avExtent.AppendLineFormat("{0} video recording file{1}", new object[] { _videoCount, (_videoCount == 1) ? "" : "s" }, delimiter);

					SetAudioVideoExtent(avExtent + ".");
				}
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets a comma-separated list of types found in the files to be archived
		/// (e.g. Text, Video, etc.).
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private string GetMode()
		{
			if (_modes == null)
				ExtractInformationFromFiles();

			if ((_modes == null) ||
				(IsMetadataPropertySet(MetadataProperties.DatasetExtent) && !_modes.Contains(kModeDataset)))
				throw new InvalidOperationException("Cannot set dataset extent for a resource which does not contain any \"dataset\" files.");

			return JSONUtils.MakeBracketedListFromValues(kFileTypeModeList, _modes);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets a comma-separated list of types found in the files to be archived
		/// (e.g. Text, Video, etc.).
		/// </summary>
		/// <remarks>This version with parameter is public to facilitate testing.</remarks>
		/// ------------------------------------------------------------------------------------
		public string GetMode(IEnumerable<string> files)
		{
			if (files == null)
				return null;

			ExtractInformationFromFiles(files);

			return GetMode();
		}

		/// ------------------------------------------------------------------------------------
		public IEnumerable<string> GetSourceFilesForMetsData(IDictionary<string, Tuple<IEnumerable<string>, string>> fileLists)
		{
// ReSharper disable once LoopCanBeConvertedToQuery
			foreach (var kvp in fileLists)
			{
				foreach (var file in kvp.Value.Item1)
				{
					var description = _getFileDescription(kvp.Key, file);

					var fileName = NormalizeFilename(kvp.Key, Path.GetFileName(file));

					yield return JSONUtils.MakeKeyValuePair(kDefaultKey, fileName) + kSeparator +
						JSONUtils.MakeKeyValuePair(kFileDescription, description) + kSeparator +
						JSONUtils.MakeKeyValuePair(kFileRelationship, kRelationshipSource);
				}
			}
		}

		/// ------------------------------------------------------------------------------------
		protected override StringBuilder  DoArchiveSpecificFilenameNormalization(string key, string fileName)
		{
			var bldr = new StringBuilder(fileName);
			for (int i = 0; i < bldr.Length; i++)
			{
				if (bldr[i] == ' ')
					bldr[i] = '-';
			}
			return bldr;
		}
		#endregion

		#region Creating RAMP package (zip file) in background thread.
		/// ------------------------------------------------------------------------------------
		public bool CreateRampPackage()
		{
			try
			{
				PackagePath = Path.Combine(Path.GetTempPath(), _id + kRampFileExtension);

				using (_worker = new BackgroundWorker())
				{
					_cancelProcess = false;
					_workerException = false;
					_worker.ProgressChanged += HandleBackgroundWorkerProgressChanged;
					_worker.WorkerReportsProgress = true;
					_worker.WorkerSupportsCancellation = true;
					_worker.DoWork += CreateZipFileInWorkerThread;
					_worker.RunWorkerAsync();

					while (_worker.IsBusy)
						Application.DoEvents();
				}
			}
			catch (Exception e)
			{
				ReportError(e, LocalizationManager.GetString(
					"DialogBoxes.ArchivingDlg.CreatingZipFileErrorMsg",
					"There was a problem starting process to create zip file."));

				return false;
			}
			finally
			{
				_worker = null;
			}

			if (!File.Exists(PackagePath))
			{
				ReportError(null, string.Format("Failed to make the RAMP package: {0}", PackagePath));
				return false;
			}

			return !_cancelProcess && !_workerException;
		}

		/// ------------------------------------------------------------------------------------
		private void CreateZipFileInWorkerThread(object sender, DoWorkEventArgs e)
		{
			try
			{
				if (Thread.CurrentThread.Name == null)
					Thread.CurrentThread.Name = "CreateZipFileInWorkerThread";

				// Before adding the files to the RAMP (zip) file, we need to copy all the
				// files to a temp folder, flattening out the directory structure and renaming
				// the files as needed to comply with REAP guidelines.
				// REVIEW: Are multiple periods and/or non-Roman script really a problem?

				_worker.ReportProgress(0, LocalizationManager.GetString("DialogBoxes.ArchivingDlg.PreparingFilesMsg",
					"Analyzing component files"));

				var filesToCopyAndZip = new Dictionary<string, string>();
				foreach (var list in _fileLists)
				{
					_worker.ReportProgress(1 /* actual value ignored, progress just increments */,
						string.IsNullOrEmpty(list.Key) ? _id: list.Key);
					foreach (var file in list.Value.Item1)
					{
						string newFileName = Path.GetFileName(file);
						newFileName = NormalizeFilename(list.Key, newFileName);
						filesToCopyAndZip[file] = Path.Combine(_tempFolder, newFileName);
					}
					if (_cancelProcess)
						return;
				}

				_worker.ReportProgress(0, LocalizationManager.GetString("DialogBoxes.ArchivingDlg.CopyingFilesMsg",
					"Copying files"));

				foreach (var fileToCopy in filesToCopyAndZip)
				{
					if (_cancelProcess)
						return;
					_worker.ReportProgress(1 /* actual value ignored, progress just increments */,
						Path.GetFileName(fileToCopy.Key));
					if (FileCopyOverride != null)
					{
						try
						{
							if (FileCopyOverride(this, fileToCopy.Key, fileToCopy.Value))
							{
								if (!File.Exists(fileToCopy.Value))
									throw new FileNotFoundException("Calling application claimed to copy file but didn't", fileToCopy.Value);
								continue;
							}
						}
						catch (Exception error)
						{
							var msg = string.Format(LocalizationManager.GetString("DialogBoxes.ArchivingDlg.FileExcludedFromPackage",
								"File excluded from {0} package: ", "Parameter is the type of archive (e.g., RAMP/IMDI)"), NameOfProgramToLaunch) +
								fileToCopy.Value;
							ReportError(error, msg);
						}
					}
					// Don't use File.Copy because it's asynchronous.
					CopyFile(fileToCopy.Key, fileToCopy.Value);
				}

				_worker.ReportProgress(0, string.Format(LocalizationManager.GetString("DialogBoxes.ArchivingDlg.SavingFilesInPackageMsg",
					"Saving files in {0} package", "Parameter is the type of archive (e.g., RAMP/IMDI)"), NameOfProgramToLaunch));

				using (var zip = new ZipFile())
				{
					// RAMP packages must not be compressed or RAMP can't read them.
					zip.CompressionLevel = Ionic.Zlib.CompressionLevel.None;
					zip.AddFiles(filesToCopyAndZip.Values, @"\");
					zip.AddFile(_metsFilePath, string.Empty);
					zip.SaveProgress += HandleZipSaveProgress;
					zip.Save(PackagePath);

					if (!_cancelProcess && IncrementProgressBarAction != null)
						Thread.Sleep(800);
				}
			}
			catch (Exception exception)
			{
				_worker.ReportProgress(0, new KeyValuePair<Exception, string>(exception,
					LocalizationManager.GetString("DialogBoxes.ArchivingDlg.CreatingArchiveErrorMsg",
						"There was an error attempting to create the RAMP file.")));

				_workerException = true;
			}
		}


		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// This is called by the Save method on the ZipFile class as the zip file is being
		/// saved to the disk.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void HandleZipSaveProgress(object s, SaveProgressEventArgs e)
		{
			if (_cancelProcess || e.EventType != ZipProgressEventType.Saving_BeforeWriteEntry)
				return;

			string msg;
			if (_progressMessages.TryGetValue(e.CurrentEntry.FileName, out msg))
				DisplayMessage(msg, MessageType.Progress);

			_worker.ReportProgress(e.EntriesSaved + 1, Path.GetFileName(e.CurrentEntry.FileName));
		}

		/// ------------------------------------------------------------------------------------
		void HandleBackgroundWorkerProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			if (e.UserState == null || _cancelProcess)
				return;

			if (e.UserState is KeyValuePair<Exception, string>)
			{
				var kvp = (KeyValuePair<Exception, string>)e.UserState;
				ReportError(kvp.Key, kvp.Value);
				return;
			}

			if (!string.IsNullOrEmpty(e.UserState as string))
			{
				if (e.ProgressPercentage == 0)
				{
					DisplayMessage(e.UserState.ToString(), MessageType.Success);
					return;
				}

				DisplayMessage(e.UserState.ToString(), MessageType.Detail);
			}

			if (IncrementProgressBarAction != null)
				IncrementProgressBarAction();
		}

		#endregion

		#region Language functions
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Get the path and file name of the RAMP Languages file
		/// </summary>
		/// <returns>The full name of the RAMP languages file</returns>
		/// ------------------------------------------------------------------------------------
		private static string GetLanguageFileLocation()
		{
			var exeFile = GetExeFileLocation();
			if (exeFile == null)
				throw new DirectoryNotFoundException("The RAMP directory was not found.");

			var dir = Path.GetDirectoryName(exeFile);
			if (dir == null)
				throw new DirectoryNotFoundException("The RAMP directory was not found.");

			// on Linux the exe and data directory are not in the same directory
			if (!Directory.Exists(Path.Combine(dir, "data")))
			{
				dir = Directory.GetParent(dir).FullName;
				if (Directory.Exists(Path.Combine(dir, "share")))
					dir = Path.Combine(dir, "share");
			}

			// get the data directory
			dir = Path.Combine(dir, "data");
			if (!Directory.Exists(dir))
				throw new DirectoryNotFoundException(string.Format("The path {0} is not valid.", dir));

			// get the options directory
			dir = Path.Combine(dir, "options");
			if (!Directory.Exists(dir))
				throw new DirectoryNotFoundException(string.Format("The path {0} is not valid.", dir));

			// get the languages.yaml file
			var langFile = Path.Combine(dir, "languages.yaml");
			if (!File.Exists(langFile))
				throw new FileNotFoundException(string.Format("The file {0} was not found.", langFile));

			return langFile;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets a Dictionary of the languages supported by RAMP.  The key is the ISO3 code,
		/// and the entry value is the language name.
		/// </summary>
		/// <returns></returns>
		/// ------------------------------------------------------------------------------------
		private Dictionary<string, string> GetLanguageList()
		{
			if (_languageList == null)
			{
				_languageList = new Dictionary<string, string>();
				var langFile = GetLanguageFileLocation();

				foreach (var fileLine in File.ReadLines(langFile).Where(l => l.StartsWith("  code: \"")))
				{
					const int start = 9;
					var end = fileLine.IndexOf('"', start);
					if (end > start)
					{
						var parts = fileLine.Substring(start, end - start).Split(':');
						if (parts.Length == 2)
							_languageList[parts[0]] = parts[1];
					}
				}
			}

			return _languageList;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Returns the official RAMP name of the language associated with the is03Code.
		/// </summary>
		/// <param name="iso3Code"></param>
		/// <returns></returns>
		/// ------------------------------------------------------------------------------------
		public string GetLanguageName(string iso3Code)
		{
			var langs = GetLanguageList();

			if (langs == null)
				throw new Exception("The language list for RAMP was not retrieved.");

			return langs.ContainsKey(iso3Code) ? langs[iso3Code] : null;
		}
		#endregion

		/// ------------------------------------------------------------------------------------
		public override void Cancel()
		{
			base.Cancel();

			CleanUp();
			CleanUpTempRampPackage();
		}

		/// ------------------------------------------------------------------------------------
		public override IArchivingSession AddSession(string sessionId)
		{
			throw new NotImplementedException();
		}

		public override IArchivingPackage ArchivingPackage
		{
			get { throw new NotImplementedException(); }
		}

		#region Clean-up methods
		/// ------------------------------------------------------------------------------------
		public void CleanUp()
		{
			try { Directory.Delete(_tempFolder, true); }
// ReSharper disable once EmptyGeneralCatchClause
			catch { }
		}

		/// ------------------------------------------------------------------------------------
		public void CleanUpTempRampPackage()
		{
			// Comment out as a test !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
			//try { File.Delete(RampPackagePath); }
			//catch { }

			if (_timer != null)
			{
				_timer.Dispose();
				_timer = null;
			}
		}

		#endregion
	}
}
