using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using L10NSharp;

namespace SIL.Archiving
{
	/// ------------------------------------------------------------------------------------
	public abstract class ArchivingDlgViewModel
	{
		#region Data members
		protected readonly string _title;
		protected readonly string _id;
		protected readonly Func<string, string, string> _getFileDescription; // first param is filelist key, second param is filename
		protected bool _cancelProcess;
		protected readonly Dictionary<string, string> _progressMessages = new Dictionary<string, string>();
		protected IDictionary<string, Tuple<IEnumerable<string>, string>> _fileLists;
		protected BackgroundWorker _worker;
		protected int _imageCount = -1;
		protected int _audioCount = -1;
		protected int _videoCount = -1;
		#endregion

		#region Delegates and Events
		/// ------------------------------------------------------------------------------------
		public enum MessageType
		{
			/// <summary>Normal (bold) text</summary>
			Normal,
			/// <summary>Red text, followed by new line</summary>
			Error,
			/// <summary>Non-bold, indented with tab</summary>
			Detail,
			/// <summary>New line</summary>
			Progress,
			/// <summary>Non-bold, indented 8 spaces, with bullet character (U+00B7)</summary>
			Bullet,
			/// <summary>New line, Dark Green text</summary>
			Success,
			/// <summary>New line, indented 4 spaces</summary>
			Indented,
			/// <summary>Normal text, which will cause display to be cleared when the next message is to be displayed</summary>
			Volatile,
		}

		/// <summary>Delegate for OnDisplayMessage event</summary>
		/// <param name="msg">Message to display</param>
		/// <param name="type">Type of message (which handler can use to determine appropriate color, style, indentation, etc.</param>
		public delegate void DisplayMessageEventHandler(string msg, MessageType type);

		/// <summary>
		/// Notifiers subscribers of a message to display.
		/// </summary>
		public event DisplayMessageEventHandler OnDisplayMessage;

		/// <summary>Delegate for DisplayError event</summary>
		/// <param name="msg">Message to display</param>
		/// <param name="packageTitle">Title of package being created</param>
		/// <param name="e">Exception (can be null)</param>
		public delegate void DisplayErrorEventHandler(string msg, string packageTitle, Exception e);

		/// <summary>
		/// Notifiers subscribers of an error message to report.
		/// </summary>
		public event DisplayErrorEventHandler OnDisplayError;

		/// <summary>Action raised when progress happens</summary>
		public Action IncrementProgressBarAction { protected get; set; }
		#endregion

		#region properties
		/// ------------------------------------------------------------------------------------
		public string AppName { get; private set; }
		/// ------------------------------------------------------------------------------------
		public bool IsBusy { get; protected set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Are image files to be counted as photographs or graphics
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public bool ImagesArePhotographs { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Show the count of audio/video files rather than the length
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public bool ShowRecordingCountNotLength { get; set; }
		#endregion

		#region callbacks
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Callback function to allow the application to modify the contents of a file rather
		/// than merely copying it. If application performs the "copy" for the given file,
		/// it should return true; otherwise, false.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public Func<ArchivingDlgViewModel, string, string, bool> FileCopyOverride { protected get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Callback to do application-specific normalization of filenames to be added to
		/// archive based on the file-list key (param 1) and the filename (param 2).
		/// The StringBuilder (param 3) has the normalized name which the app can alter as needed.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public Action<string, string, StringBuilder> AppSpecificFilenameNormalization { private get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Callback to allow application to do special handling of exceptions or other error
		/// conditions. The exception parameter can be null.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public Action<Exception, string> HandleNonFatalError { private get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Callback to allow application to handly display of initial summary in log box. If
		/// the application implements this, then the default summary display will be suppressed.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public Action<IDictionary<string, Tuple<IEnumerable<string>, string>>> OverrideDisplayInitialSummary { private get; set; }
		#endregion

		#region construction and initialization
		/// ------------------------------------------------------------------------------------
		/// <summary>Constructor</summary>
		/// <param name="appName">The application name</param>
		/// <param name="title">Title of the submission</param>
		/// <param name="id">Identifier (used as filename) for the package being created</param>
		/// <param name="getFileDescription">Callback function to get a file description based
		/// on the file-list key (param 1) and the filename (param 2)</param>
		/// ------------------------------------------------------------------------------------
		public ArchivingDlgViewModel(string appName, string title, string id,
			Func<string, string, string> getFileDescription)
		{
			ShowRecordingCountNotLength = false;
			ImagesArePhotographs = true;
			if (appName == null)
				throw new ArgumentNullException("appName");
			AppName = appName;
			if (title == null)
				throw new ArgumentNullException("title");
			_title = title;
			if (id == null)
				throw new ArgumentNullException("id");
			_id = id;

			if (getFileDescription == null)
				throw new ArgumentNullException("getFileDescription");
			_getFileDescription = getFileDescription;
		}

		/// ------------------------------------------------------------------------------------
		/// <param name="getFilesToArchive">delegate to retrieve the lists of files of files to
		/// archive, keyed and grouped according to whatever logical grouping makes sense in the
		/// calling application. The key for each group will be supplied back to the calling app
		/// for use in "normalizing" file names. For each group, in addition to the enumerated
		/// files to include (in Item1 of the Tuple), the calling app can provide a progress
		/// message (in Item2 of the Tuple) to be displayed when that group of files is being
		/// zipped and added to the RAMP file.</param>
		/// <param name="maxProgBarValue">Value calculated as the max value for the progress
		/// bar so the dialog can set that correctly</param>

		/// ------------------------------------------------------------------------------------
		public bool Initialize(Func<IDictionary<string, Tuple<IEnumerable<string>, string>>> getFilesToArchive,
			out int maxProgBarValue)
		{
			IsBusy = true;

			try
			{
				if (!DoArchiveSpecificInitialization())
				{
					maxProgBarValue = 0;
					return false;
				}

				_fileLists = getFilesToArchive();
				foreach (var fileList in _fileLists.Where(fileList => fileList.Value.Item1.Any()))
				{
					string normalizedName = NormalizeFilename(fileList.Key, Path.GetFileName(fileList.Value.Item1.First()));
					_progressMessages[normalizedName] = fileList.Value.Item2;
				}
				DisplayInitialSummary();

				// One for analyzing each list, one for copying each file, one for saving each file in the zip file
				// and one for the mets.xml file.
				maxProgBarValue = _fileLists.Count + 2 * _fileLists.SelectMany(kvp => kvp.Value.Item1).Count() + 1;

				return true;
			}
			finally
			{
				IsBusy = false;
			}
		}

		/// ------------------------------------------------------------------------------------
		protected abstract bool DoArchiveSpecificInitialization();

		/// ------------------------------------------------------------------------------------
		private void DisplayInitialSummary()
		{
			if (OverrideDisplayInitialSummary != null)
				OverrideDisplayInitialSummary(_fileLists);
			else if (OnDisplayMessage != null)
			{
				OnDisplayMessage(LocalizationManager.GetString("DialogBoxes.ArchivingDlg.PrearchivingStatusMsg",
						"The following files will be added to the archive:"), MessageType.Normal);

				foreach (var kvp in _fileLists)
				{
					if (kvp.Key != string.Empty)
						OnDisplayMessage(kvp.Key, MessageType.Indented);

					foreach (var file in kvp.Value.Item1)
						OnDisplayMessage(Path.GetFileName(file), MessageType.Bullet);
				}
			}
		}
		#endregion

		/// ------------------------------------------------------------------------------------
		protected void DisplayMessage(string msg, MessageType type)
		{
			if (OnDisplayMessage != null)
				OnDisplayMessage(msg, type);
		}

		//#region Methods to add app-specific METS pairs
		///// ------------------------------------------------------------------------------------
		///// <summary>
		///// Sets the "Broad Type", the audience for which the resource being archived is
		///// primarily intended. This is set automatically as a side-effect of setting the
		///// stage or type.
		///// </summary>
		///// ------------------------------------------------------------------------------------
		//public void SetAudience(AudienceType audienceType)
		//{
		//    if (_metsPropertiesSet.HasFlag(MetsProperties.Audience))
		//    {
		//        if (_metsAudienceType != audienceType)
		//            throw new InvalidOperationException(string.Format("Audience has already been set and cannot be changed to a different value."));
		//        return; // Already added
		//    }

		//    _metsPropertiesSet |= MetsProperties.Audience;
		//    _metsAudienceType = audienceType;

		//    string audience;
		//    switch (audienceType)
		//    {
		//        case AudienceType.Vernacular: audience = kAudienceVernacular; break;
		//        case AudienceType.Training: audience = kAudienceTraining; break;
		//        case AudienceType.Internal: audience = kAudienceInternal; break;
		//        case AudienceType.Wider: audience = kAudienceWide; break;
		//        default:
		//            throw new NotImplementedException("Need to add appropriate METS constant for audience type " + audienceType);
		//    }
		//    _metsPairs.Add(JSONUtils.MakeKeyValuePair(kAudience, audience));
		//}

		///// ------------------------------------------------------------------------------------
		///// <summary>
		///// Sets the type of vernacular material represented by the resource being archived.
		///// Setting one of these types also indicates that the resource is intended for use by
		///// the general population in the vernacular language community.
		///// For resources containing vernacular material related to the translation task but not
		///// specifically Scripture or other material intended for use by the general population
		///// (e.g. back translation, checking questions, key terms list), call
		///// SetInternalWorkType instead.
		///// </summary>
		///// <param name="vernacularMaterialsTypes">Types of material(s) that this resource
		///// contains. To specify only the broad type of materials but not details about the
		///// contents, you can specify merely Scripture or Other (but never both); otherwise,
		///// use the more specific content types. If this resource contains multiple types of
		///// content, "OR" the types together (but don't mix "Bible" types with the other types).
		///// </param>
		///// ------------------------------------------------------------------------------------
		//public void SetVernacularMaterialsAndContentType(VernacularMaterialsType vernacularMaterialsTypes)
		//{
		//    SetAudience(AudienceType.Vernacular);

		//    PreventDuplicateMetsKey(MetsProperties.Type);

		//    if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.Scripture))
		//    {
		//        if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.Other))
		//            throw new ArgumentException("Resource cannot ");
		//        _metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularMaterialsType, kVernacularMaterialScripture));
		//    }
		//    else if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.Other))
		//        _metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularMaterialsType, kVernacularMaterialGeneral));

		//    // Scripture
		//    if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.BibleBackground))
		//        _metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, ""));

		//    // Other - Community & Culture
		//    if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.CommunityAndCulture_Calendar))
		//        _metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Calendar"));
		//    if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.CommunityAndCulture_CivicEducation))
		//        _metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Civic education"));
		//    if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.CommunityAndCulture_CommunityNews))
		//        _metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Community news"));
		//    if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.CommunityAndCulture_CultureAndFolklore))
		//        _metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Culture and folklore"));
		//    if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.CommunityAndCulture_ProverbsAndMaxims))
		//        _metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Proverbs and maxims"));
		//    if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.CommunityAndCulture_SongLyrics))
		//        _metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Songs"));

		//    // Other - Community & Culture
		//    if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.ResourceFormats_ActivityBook))
		//        _metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Activity book"));
		//    if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.ResourceFormats_TeachersGuide))
		//        _metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Teacher's guide"));
		//    if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.ResourceFormats_Textbook))
		//        _metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Textbook"));
		//    if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.ResourceFormats_WallChart))
		//        _metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Wall chart"));
		//    if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.ResourceFormats_Workbook))
		//        _metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Workbook"));

		//    // Other - Community Development
		//    if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.CommunityDevelopment_AgricultureAndFoodProduction))
		//        _metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Agriculture and food production"));
		//    if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.CommunityDevelopment_BusinessAndIncomeGeneration))
		//        _metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Business and income generation"));
		//    if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.CommunityDevelopment_EnvironmentalCare))
		//        _metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Environmental care"));
		//    if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.CommunityDevelopment_InstructionalManual))
		//        _metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Instructional manual"));
		//    if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.CommunityDevelopment_VocationalEducation))
		//        _metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Vocational education"));

		//    // Other - Health
		//    if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.Health_AIDSAndHIV))
		//        _metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "AIDS and HIV"));
		//    if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.Health_AvianFlu))
		//        _metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Avian flu"));
		//    if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.Health_GeneralHealthAndHygiene))
		//        _metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Health and hygiene"));
		//    if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.Health_Malaria))
		//        _metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Malaria"));
		//    if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.Health_PrenatalAndInfantCare))
		//        _metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Prenatal and infant care"));

		//    // Other - Language Acquisition
		//    if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.LanguageAcquisition_EnglishLanguageInstruction))
		//        _metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "English language instruction"));
		//    if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.LanguageAcquisition_GrammarInstruction))
		//        _metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Grammar instruction"));
		//    if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.LanguageAcquisition_LanguageInstruction))
		//        _metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Language instruction"));
		//    if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.LanguageAcquisition_PhraseBook))
		//        _metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Phrase book"));

		//    // Other - Literacy Education
		//    if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.LiteracyEducation_Alphabet))
		//        _metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Alphabet"));
		//    if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.LiteracyEducation_Prereading))
		//        _metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Prereading"));
		//    if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.LiteracyEducation_Prewriting))
		//        _metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Prewriting"));
		//    if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.LiteracyEducation_Primer))
		//        _metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Primer"));
		//    if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.LiteracyEducation_Reader))
		//        _metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Reader"));
		//    if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.LiteracyEducation_Riddles))
		//        _metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Riddles"));
		//    if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.LiteracyEducation_Spelling))
		//        _metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Spelling"));
		//    if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.LiteracyEducation_TonePrimer))
		//        _metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Tone primer"));
		//    if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.LiteracyEducation_TransitionPrimer))
		//        _metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Transition primer"));
		//    if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.LiteracyEducation_Vocabulary))
		//        _metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Vocabulary"));
		//    if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.LiteracyEducation_Writing))
		//        _metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Writing"));

		//    // Other - Scripture Use
		//    if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.ScriptureUse_Catechism))
		//        _metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Catechism"));
		//    if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.ScriptureUse_ChristianLiving))
		//        _metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Christian living"));
		//    if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.ScriptureUse_Devotional))
		//        _metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Devotional"));
		//    if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.ScriptureUse_Evangelistic))
		//        _metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Evangelistic"));
		//    if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.ScriptureUse_Theology))
		//        _metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Theology"));
		//    if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.ScriptureUse_WorshipAndLiturgy))
		//        _metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Worship and liturgy"));

		//    // Other - Vernacular Education
		//    if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.VernacularEducation_Arithmetic))
		//        _metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Arithmetic"));
		//    if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.VernacularEducation_Arts))
		//        _metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Arts"));
		//    if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.VernacularEducation_Ethics))
		//        _metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Ethics"));
		//    if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.VernacularEducation_Numbers))
		//        _metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Numbers"));
		//    if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.VernacularEducation_Science))
		//        _metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Science"));
		//    if (vernacularMaterialsTypes.HasFlag(VernacularMaterialsType.VernacularEducation_SocialStudies))
		//        _metsPairs.Add(JSONUtils.MakeKeyValuePair(kVernacularContent, "Social studies"));
		//}

		///// ------------------------------------------------------------------------------------
		///// <summary>
		///// Sets the type of training resource represented by the resource being archived.
		///// Setting one of these types also indicates that the resource is intended for use in a
		///// classroom or instructional setting for SIL or Wycliffe role-related training
		///// (for published textbooks call SetScholarlyWorkType instead as these are generally for
		///// a wider audience).
		///// </summary>
		///// ------------------------------------------------------------------------------------
		//public void SetTrainingResourceType(TrainingResourceType trainingResourceType)
		//{
		//    SetAudience(AudienceType.Training);

		//    PreventDuplicateMetsKey(MetsProperties.Type);

		//    string type;
		//    switch (trainingResourceType)
		//    {
		//        default:
		//            throw new NotImplementedException("Need to add appropriate METS constant for training resource type " + trainingResourceType);
		//    }
		//    _metsPairs.Add(JSONUtils.MakeKeyValuePair(kScholarlyWorkType, type));
		//}

		///// ------------------------------------------------------------------------------------
		///// <summary>
		///// Sets the type of training resource represented by the resource being archived.
		///// Setting one of these types also indicates that the resource is intended for use in a
		///// classroom or instructional setting for SIL or Wycliffe role-related training
		///// (for published textbooks call SetScholarlyWorkType instead as these are generally for
		///// a wider audience).
		///// </summary>
		///// ------------------------------------------------------------------------------------
		//public void SetInternalWorkType(InternalWorkType internalWorkType)
		//{
		//    SetAudience(AudienceType.Internal);

		//    PreventDuplicateMetsKey(MetsProperties.Type);

		//    string type;
		//    switch (internalWorkType)
		//    {
		//        case InternalWorkType.Correspondence: type = "Correspondence"; break;
		//        case InternalWorkType.InternalReport: type = "Internal report"; break;
		//        case InternalWorkType.Map: type = "Map"; break;
		//        case InternalWorkType.PositionPaper: type = "Position paper"; break;
		//        case InternalWorkType.Other: type = "Other"; break;
		//        default:
		//            throw new NotImplementedException("Need to add appropriate METS constant for internal work type " + internalWorkType);
		//    }
		//    _metsPairs.Add(JSONUtils.MakeKeyValuePair(kScholarlyWorkType, type));
		//}

		///// ------------------------------------------------------------------------------------
		///// <summary>
		///// Sets the bibliographic type of the resource being archived (for a "wider audience")
		///// </summary>
		///// ------------------------------------------------------------------------------------
		//public void SetScholarlyWorkType(ScholarlyWorkType scholarlyWorkType)
		//{
		//    SetAudience(AudienceType.Wider);

		//    PreventDuplicateMetsKey(MetsProperties.Type);

		//    string type;
		//    switch (scholarlyWorkType)
		//    {
		//        case ScholarlyWorkType.PrimaryData: type = "Data set"; break;
		//        default:
		//            throw new NotImplementedException("Need to add appropriate METS constant for scholarly work type " + scholarlyWorkType);
		//    }
		//    _metsPairs.Add(JSONUtils.MakeKeyValuePair(kScholarlyWorkType, type));
		//}

		///// ------------------------------------------------------------------------------------
		///// <summary>
		///// Sets the current stage of development of his resource.
		///// </summary>
		///// <param name="stage">Some work stage values relate to work being done for a
		///// particular type of audience. Where a stage is relevant to only a single type of
		///// audience, the audience will already be correctly inferred from the stage. For
		///// stages that can apply to more than one type of audience, either call SetAudience
		///// directly or "OR" the appropriate AudienceType with the WorkStage when calling this
		///// method. Not that some work stages can apply to a subset of the auidence types, so
		///// pay attention to the comments for each work stage to avoid pairing it with an
		///// invalid audience type.</param>
		///// ------------------------------------------------------------------------------------
		//public void SetStage(WorkStage stage)
		//{
		//    // Some of the work stages imply a particular audience and therefore have the appropriate audience bit set
		//    if (stage.HasFlag(AudienceType.Vernacular))
		//        SetAudience(AudienceType.Vernacular);
		//    if (stage.HasFlag(AudienceType.Training))
		//        SetAudience(AudienceType.Training);
		//    if (stage.HasFlag(AudienceType.Internal))
		//        SetAudience(AudienceType.Internal);
		//    if (stage.HasFlag(AudienceType.Wider))
		//        SetAudience(AudienceType.Wider);

		//    if (stage.HasFlag(WorkStage.RoughDraft))
		//        SetStage(kStageRoughDraft);
		//    if (stage.HasFlag(WorkStage.SelfReviewedDraft))
		//        SetStage(kStageReviewedDraft);
		//    if (stage.HasFlag(WorkStage.ConsultantOrEditorReleasedDraft))
		//    {
		//        PreventInvalidAudienceTypeForWorkStage(AudienceType.Training | AudienceType.Internal,
		//            WorkStage.ConsultantOrEditorReleasedDraft);
		//        SetStage(kStageReleasedDraft);
		//    }
		//    if (stage.HasFlag(WorkStage.ConsultantOrEditorApproved))
		//    {
		//        PreventInvalidAudienceTypeForWorkStage(AudienceType.Training, WorkStage.ConsultantOrEditorApproved);
		//        SetStage(kStageApprovedDraft);
		//    }
		//    if (stage.HasFlag(WorkStage.InPressOrPublished))
		//    {
		//        PreventInvalidAudienceTypeForWorkStage(AudienceType.Internal, WorkStage.InPressOrPublished);
		//        SetStage(kStagePublished);
		//    }
		//    if (stage.HasFlag(WorkStage.UsedInTrainingCourse))
		//        SetStage(kStageUsedInCourse);
		//    if (stage.HasFlag(WorkStage.ReadyForPublicationOrFormalPreprint))
		//                {
		//        PreventInvalidAudienceTypeForWorkStage(AudienceType.Vernacular | AudienceType.Internal,
		//            WorkStage.ReadyForPublicationOrFormalPreprint);
		//        SetStage(kStagePrepublication);
		//    }
		//    if (stage.HasFlag(WorkStage.FinishedInternal))
		//        SetStage(kStageFinished);
		//}

		///// ------------------------------------------------------------------------------------
		//private void PreventInvalidAudienceTypeForWorkStage(Enum invalidAudience, WorkStage stage)
		//{
		//    if (invalidAudience.HasFlag(_metsAudienceType))
		//    {
		//        throw new InvalidOperationException(string.Format(
		//            "Resources with an audience of \"{0}\" cannot have a work stage of {1}",
		//            _metsAudienceType, stage));
		//    }
		//}

		///// ------------------------------------------------------------------------------------
		//private void SetStage(string stage)
		//{
		//    _metsPairs.Add(JSONUtils.MakeKeyValuePair(kStageDescription, stage));
		//}

		///// ------------------------------------------------------------------------------------
		///// <summary>
		///// Sets the SIL Domains and/or sub-domains to which the resource being archived relates.
		///// </summary>
		///// <param name="domains">The domains(s) and/or sub-domains. If the resource relates to
		///// multiple domains, "OR" them together to set all the necessary flags. It is not
		///// necessary to OR the sub-domains with their corresponding domains, since the
		///// subdomains already have the correct domain bits set.</param>
		///// ------------------------------------------------------------------------------------
		//public void SetDomains(SilDomain domains)
		//{
		//    PreventDuplicateMetsKey(MetsProperties.Domains);

		//    if (domains.HasFlag(SilDomain.AcademicTraining))
		//        AddDomain(kAcademicTrainingAbbrev, "Academic Training");

		//    if (domains.HasFlag(SilDomain.Anthropology))
		//    {
		//        AddDomain(kAnthropologyAbbrev, "Anthropology");

		//        // sub-domains
		//        if (domains.HasFlag(SilDomain.Anth_Ethnography))
		//            AddSubDomain(kAnthropologyAbbrev, "ethnography");
		//        if (domains.HasFlag(SilDomain.Anth_Interview))
		//            AddSubDomain(kAnthropologyAbbrev, "interview");
		//        if (domains.HasFlag(SilDomain.Anth_KinshipAnalysis))
		//            AddSubDomain(kAnthropologyAbbrev, "kinship analysis");
		//    }

		//    if (domains.HasFlag(SilDomain.ArtsandEthnomusicology))
		//    {
		//        AddDomain(kEthnomusicologyAbbrev, "Arts and Ethnomusicology");

		//        // sub-domains
		//        if (domains.HasFlag(SilDomain.Emus_ArtisticCommunicationProfile))
		//            AddSubDomain(kEthnomusicologyAbbrev, "artistic communication profile");
		//        if (domains.HasFlag(SilDomain.Emus_PerformanceCollection))
		//            AddSubDomain(kEthnomusicologyAbbrev, "aperformance collection");
		//        if (domains.HasFlag(SilDomain.Emus_SongCollection))
		//            AddSubDomain(kEthnomusicologyAbbrev, "song collection");
		//        if (domains.HasFlag(SilDomain.Emus_SummaryArtisticEventFormAnalysis))
		//            AddSubDomain(kEthnomusicologyAbbrev, "summary artistic event form analysis");
		//        if (domains.HasFlag(SilDomain.Emus_SummaryArtisticGenreAnalysis))
		//            AddSubDomain(kEthnomusicologyAbbrev, "summary artistic genre analysis");
		//    }

		//    if (domains.HasFlag(SilDomain.Communications))
		//        AddDomain(kCommunicationsAbbrev, "Communications");
		//    if (domains.HasFlag(SilDomain.CommunityDevelopment))
		//        AddDomain(kCommunityDevelopmentAbbrev, "Community Development");
		//    if (domains.HasFlag(SilDomain.Counseling))
		//        AddDomain(kCounselingAbbrev, "Counseling");
		//    if (domains.HasFlag(SilDomain.InternationalOrGovernmentRelations))
		//        AddDomain(kInternationalOrGovernmentRelationsAbbrev, "International/Government Relations");

		//    if (domains.HasFlag(SilDomain.LanguageAssessment))
		//    {
		//        AddDomain(kLanguageAssessmentAbbrev, "Language Assessment");

		//        // sub-domains
		//        if (domains.HasFlag(SilDomain.Lgas_SurveyArtifacts))
		//            AddSubDomain(kLanguageAssessmentAbbrev, "survey artifacts");
		//        if (domains.HasFlag(SilDomain.Lgas_SurveyInstrument))
		//            AddSubDomain(kLanguageAssessmentAbbrev, "survey instrument");
		//        if (domains.HasFlag(SilDomain.Lgas_SurveyReport))
		//            AddSubDomain(kLanguageAssessmentAbbrev, "survey report");
		//        if (domains.HasFlag(SilDomain.Lgas_Wordlist))
		//            AddSubDomain(kLanguageAssessmentAbbrev, "wordlist");
		//    }

		//    if (domains.HasFlag(SilDomain.LanguageProgramManagement))
		//        AddDomain(kLanguageProgramManagementAbbrev, "Language Program Management");

		//    if (domains.HasFlag(SilDomain.LanguageTechnology))
		//    {
		//        AddDomain(kLanguageTechnologyAbbrev, "Language Technology");

		//        // sub-domains
		//        if (domains.HasFlag(SilDomain.Ltec_ComputerTool))
		//            AddSubDomain(kLanguageTechnologyAbbrev, "computer tool");
		//    }

		//    if (domains.HasFlag(SilDomain.LanguageAndCultureLearning))
		//        AddDomain(kLanguageAndCultureLearningAbbrev, "Language and Culture Learning");
		//    if (domains.HasFlag(SilDomain.LearningAndDevelopment))
		//        AddDomain(kLearningAndDevelopmentAbbrev, "Learning and Development");
		//    if (domains.HasFlag(SilDomain.LibraryOrMuseumOrArchiving))
		//        AddDomain(kLibraryOrMuseumOrArchivingAbbrev, "Library/Museum/Archiving");

		//    if (domains.HasFlag(SilDomain.Linguistics))
		//    {
		//        AddDomain(kLinguisticsAbbrev, "Linguistics");

		//        // sub-domains
		//        if (domains.HasFlag(SilDomain.Ling_ComparativeDescription))
		//            AddSubDomain(kLinguisticsAbbrev, "comparative description");
		//        if (domains.HasFlag(SilDomain.Ling_DiscourseDescription))
		//            AddSubDomain(kLinguisticsAbbrev, "discourse description");
		//        if (domains.HasFlag(SilDomain.Ling_GrammaticalDescription))
		//            AddSubDomain(kLinguisticsAbbrev, "grammatical description");
		//        if (domains.HasFlag(SilDomain.Ling_InterlinearizedText))
		//            AddSubDomain(kLinguisticsAbbrev, "interlinearized text");
		//        if (domains.HasFlag(SilDomain.Ling_LanguageDocumentation))
		//            AddSubDomain(kLinguisticsAbbrev, "language documentation");
		//        if (domains.HasFlag(SilDomain.Ling_Lexicon))
		//            AddSubDomain(kLinguisticsAbbrev, "lexicon");
		//        if (domains.HasFlag(SilDomain.Ling_PhonologicalDescription))
		//            AddSubDomain(kLinguisticsAbbrev, "phonological description");
		//        if (domains.HasFlag(SilDomain.Ling_SemanticDescription))
		//            AddSubDomain(kLinguisticsAbbrev, "semantic description");
		//        if (domains.HasFlag(SilDomain.Ling_Text))
		//            AddSubDomain(kLinguisticsAbbrev, "text (primary language data)");
		//    }

		//    if (domains.HasFlag(SilDomain.LiteracyAndEducation))
		//    {
		//        AddDomain(kLiteracyAndEducationAbbrev, "Literacy and Education");

		//        // sub-domains
		//        if (domains.HasFlag(SilDomain.Ltcy_OrthographyDescription))
		//            AddSubDomain(kLiteracyAndEducationAbbrev, "orthography description");
		//    }

		//    if (domains.HasFlag(SilDomain.Management))
		//        AddDomain(kManagementAbbrev, "Management, General");
		//    if (domains.HasFlag(SilDomain.Publishing))
		//        AddDomain(kPublishingAbbrev, "Publishing");
		//    if (domains.HasFlag(SilDomain.ScriptureUse))
		//        AddDomain(kScriptureUseAbbrev, "Scripture Use");
		//    if (domains.HasFlag(SilDomain.SignLanguages))
		//        AddDomain(kSignLanguagesAbbrev, "Sign Languages");

		//    if (domains.HasFlag(SilDomain.Sociolinguistics))
		//    {
		//        AddDomain(kSociolinguisticsAbbrev, "Sociolinguistics");

		//        // sub-domains
		//        if (domains.HasFlag(SilDomain.Slng_SociolinguisticDescription))
		//            AddSubDomain(kSociolinguisticsAbbrev, "sociolinguistic description");
		//    }

		//    if (domains.HasFlag(SilDomain.Translation))
		//    {
		//        AddDomain(kTranslationAbbrev, "Translation");

		//        // sub-domains
		//        if (domains.HasFlag(SilDomain.Tran_BackTranslation))
		//            AddSubDomain(kTranslationAbbrev, "back translation");
		//        if (domains.HasFlag(SilDomain.Tran_BibleNames))
		//            AddSubDomain(kTranslationAbbrev, "Bible names");
		//        if (domains.HasFlag(SilDomain.Tran_ComprehensionCheckingQuestions))
		//            AddSubDomain(kTranslationAbbrev, "comprehension checking questions");
		//        if (domains.HasFlag(SilDomain.Tran_Exegesis))
		//            AddSubDomain(kTranslationAbbrev, "exegesis");
		//        if (domains.HasFlag(SilDomain.Tran_KeyBibleTerms))
		//            AddSubDomain(kTranslationAbbrev, "key Bible terms");
		//    }

		//    if (domains.HasFlag(SilDomain.VernacularMedia))
		//        AddDomain(kVernacularMediaAbbrev, "VernacularMedia");
		//}

		///// ------------------------------------------------------------------------------------
		///// <summary>
		///// Adds a METS pair for the given domain
		///// </summary>
		///// <param name="domainAbbrev">Four-letter (ALL CAPS) abbreviation for the SIL domain</param>
		///// <param name="domainName">Domain name (Title Case)</param>
		///// ------------------------------------------------------------------------------------
		//private void AddDomain(string domainAbbrev, string domainName)
		//{
		//    // if a mets pair already exists for domains, add this domain to the existing list.
		//    var existingValue = _metsPairs.Find(s => s.Contains(kSilDomain));
		//    if (!string.IsNullOrEmpty(existingValue))
		//    {
		//        int pos = existingValue.IndexOf(']');
		//        string newValue = existingValue.Insert(pos, string.Format(",\"{0}:{1}\"", domainAbbrev, domainName));
		//        _metsPairs[_metsPairs.IndexOf(existingValue)] = newValue;
		//    }
		//    else
		//    {
		//        _metsPairs.Add(JSONUtils.MakeKeyValuePair(kSilDomain, string.Format("{0}:{1}", domainAbbrev, domainName), true));
		//    }
		//}

		///// ------------------------------------------------------------------------------------
		///// <summary>
		///// Adds a METS pair for the given sub-domain
		///// </summary>
		///// <param name="domainAbbrev">Four-letter (ALL CAPS) abbreviation for the SIL domain</param>
		///// <param name="subDomain">sub-domain name (all lowercase)</param>
		///// ------------------------------------------------------------------------------------
		//private void AddSubDomain(string domainAbbrev, string subDomain)
		//{
		//    // if a mets pair already exists for this domain, add this subdomain to the existing list.
		//    var key = string.Format(kFmtDomainSubtype, domainAbbrev);
		//    var existingValue = _metsPairs.Find(s => s.Contains(key));
		//    if (!string.IsNullOrEmpty(existingValue))
		//    {
		//        int pos = existingValue.IndexOf(']');
		//        string newValue = existingValue.Insert(pos, string.Format(",\"{0} ({1})\"", subDomain, domainAbbrev));
		//        _metsPairs[_metsPairs.IndexOf(existingValue)] = newValue;
		//    }
		//    else
		//    {
		//        _metsPairs.Add(JSONUtils.MakeKeyValuePair(key,
		//            string.Format("{0} ({1})", subDomain, domainAbbrev), true));
		//    }
		//}

		///// ------------------------------------------------------------------------------------
		///// <summary>
		///// Adds a METS pair for the Date this resource was initially created
		///// </summary>
		///// ------------------------------------------------------------------------------------
		//public void SetCreationDate(DateTime date)
		//{
		//    SetCreationDate(date.ToString("yyyy-MM-dd"));
		//}

		///// ------------------------------------------------------------------------------------
		///// <summary>
		///// Adds a METS pair for the range of years (YYYY-YYYY) during which data
		///// was collected or work was being done on this resource
		///// </summary>
		///// ------------------------------------------------------------------------------------
		//public void SetCreationDate(int startYear, int endYear)
		//{
		//    if (endYear < startYear)
		//        throw new ArgumentException("startYear must be before endYear");

		//    string startYr = Math.Abs(startYear).ToString("D4");
		//    if (startYear < 0 && endYear > 0)
		//        startYr += " BCE";
		//    string endYr = Math.Abs(endYear).ToString("D4");
		//    if (startYear < 0)
		//        endYr += (endYear < 0) ? " BCE" : " CE";

		//    SetCreationDate(string.Format("{0}-{1}", startYr, endYr));
		//}

		///// ------------------------------------------------------------------------------------
		///// <summary>
		///// Adds a METS pair for the range of years (YYYY-YYYY) during which data
		///// was collected or work was being done on this resource
		///// </summary>
		///// ------------------------------------------------------------------------------------
		//public void SetCreationDate(string date)
		//{
		//    if (string.IsNullOrEmpty(date))
		//        throw new ArgumentNullException("date");

		//    PreventDuplicateMetsKey(MetsProperties.CreationDate);

		//    _metsPairs.Add(JSONUtils.MakeKeyValuePair(kDateCreated, date));
		//}

		///// ------------------------------------------------------------------------------------
		///// <summary>
		///// Adds a METS pair for the Date this resource was last updated
		///// </summary>
		///// ------------------------------------------------------------------------------------
		//public void SetModifiedDate(DateTime date)
		//{
		//    PreventDuplicateMetsKey(MetsProperties.ModifiedDate);

		//    _metsPairs.Add(JSONUtils.MakeKeyValuePair(kDateModified, date.ToString("yyyy-MM-dd")));
		//}

		///// ------------------------------------------------------------------------------------
		///// <summary>
		///// Sets the subject language for a resource that is concerned with a particular
		///// language or language community. ENHANCE: Deal with dialect options.
		///// </summary>
		///// <param name="iso3Code">The 3-letter ISO 639-2 code for the language</param>
		///// <param name="languageName">The English name of the language</param>
		///// ------------------------------------------------------------------------------------
		//public void SetSubjectLanguage(string iso3Code, string languageName)
		//{
		//    PreventDuplicateMetsKey(MetsProperties.SubjectLanguage);

		//    SetFlag(kFlagHasSubjectLanguage);
		//    _metsPairs.Add(JSONUtils.MakeArrayFromValues(kSubjectLanguage,
		//        new[] { JSONUtils.MakeKeyValuePair(kDefaultKey, iso3Code + ":" + languageName) }));
		//}

		///// ------------------------------------------------------------------------------------
		///// <summary>
		///// Sets the given METS flag (typically appears as a checkbox in RAMP) to true/yes ("Y")
		///// </summary>
		///// <param name="flagKey">One of the kFlag... contants</param>
		///// ------------------------------------------------------------------------------------
		//private void SetFlag(string flagKey)
		//{
		//    _metsPairs.Add(JSONUtils.MakeKeyValuePair(flagKey, kTrue));
		//}

		///// ------------------------------------------------------------------------------------
		///// <summary>
		///// Sets specific requirements of software, font, or data schema support needed to use
		///// or interpret this resource.
		///// </summary>
		///// <param name="requirements">Be as specific a possible, indicating version number(s)
		///// where appropriate</param>
		///// ------------------------------------------------------------------------------------
		//public void SetSoftwareRequirements(params string[] requirements)
		//{
		//    SetSoftwareRequirements((IEnumerable<string>)requirements);
		//}

		///// ------------------------------------------------------------------------------------
		///// <summary>
		///// Sets specific requirements of software, font, or data schema support needed to use
		///// or interpret this resource.
		///// </summary>
		///// <param name="requirements">Be as specific a possible, indicating version number(s)
		///// where appropriate</param>
		///// ------------------------------------------------------------------------------------
		//public void SetSoftwareRequirements(IEnumerable<string> requirements)
		//{
		//    requirements = requirements.Where(r => !string.IsNullOrEmpty(r));

		//    HashSet<string> softwareKeyValuePairs = new HashSet<string>();
		//    if (requirements.Any())
		//    {
		//        PreventDuplicateMetsKey(MetsProperties.SoftwareRequirements);

		//        SetFlag(kFlagHasSoftwareOrFontRequirements);

		//        foreach (var softwareOrFontName in requirements)
		//            softwareKeyValuePairs.Add(JSONUtils.MakeKeyValuePair(kDefaultKey, softwareOrFontName));

		//        _metsPairs.Add(JSONUtils.MakeArrayFromValues(kSoftwareOrFontRequirements, softwareKeyValuePairs));
		//    }
		//}

		///// ------------------------------------------------------------------------------------
		///// <summary>
		///// Sets the language in which this resource is written. If this resource is a diglot or
		///// triglot work, specify all languages in which there is significant content in the
		///// work. (For language documentation, dictionaries, and the like, one of the languages
		///// specified should be the same as a subject language specified using
		///// SetSubjectLanguage.) ENHANCE: Deal with dialect options.
		///// </summary>
		///// <param name="iso3Codes">The 3-letter ISO 639-2 codes for the languages</param>
		///// ------------------------------------------------------------------------------------
		//public void SetContentLanguages(params string[] iso3Codes)
		//{
		//    SetContentLanguages((IEnumerable<string>)iso3Codes);
		//}

		///// ------------------------------------------------------------------------------------
		///// <summary>
		///// Sets the language in which this resource is written. If this resource is a diglot or
		///// triglot work, specify all languages in which there is significant content in the
		///// work. (For language documentation, dictionaries, and the like, one of the languages
		///// specified should be the same as a subject language specified using
		///// SetSubjectLanguage.) ENHANCE: Deal with dialect options.
		///// </summary>
		///// <param name="iso3Codes">The 3-letter ISO 639-2 codes for the languages</param>
		///// ------------------------------------------------------------------------------------
		//public void SetContentLanguages(IEnumerable<string> iso3Codes)
		//{
		//    HashSet<string> languageKeyValuePairs = new HashSet<string>();
		//    foreach (var iso3Code in iso3Codes.Where(r => !string.IsNullOrEmpty(r)))
		//        languageKeyValuePairs.Add(JSONUtils.MakeKeyValuePair(kDefaultKey, iso3Code));

		//    if (languageKeyValuePairs.Any())
		//    {
		//        PreventDuplicateMetsKey(MetsProperties.ContentLanguages);

		//        _metsPairs.Add(JSONUtils.MakeArrayFromValues(kContentLanguages, languageKeyValuePairs));
		//    }
		//}

		///// ------------------------------------------------------------------------------------
		///// <summary>
		///// Sets the schema(s) to which this resource -- or the file(s) it contains -- conforms.
		///// </summary>
		///// <param name="schemaDescriptor">Known schema name (typically, but not nescessarily,
		///// for XML files). RAMP doesn't say what to do if the resource contains files conforming to
		///// multiple standards, but I would say just make a comma-separated list.</param>
		///// ------------------------------------------------------------------------------------
		//public void SetSchemaConformance(string schemaDescriptor)
		//{
		//    PreventDuplicateMetsKey(MetsProperties.SchemaConformance);

		//    _metsPairs.Add(JSONUtils.MakeKeyValuePair(kSchemaConformance, schemaDescriptor));
		//}

		///// ------------------------------------------------------------------------------------
		///// <summary>
		///// Sets the number of records/entries in this resource.
		///// </summary>
		///// <param name="extent">For example, "2505 entries"</param>
		///// ------------------------------------------------------------------------------------
		//public void SetDatasetExtent(string extent)
		//{
		//    if (string.IsNullOrEmpty(extent))
		//        throw new ArgumentNullException("extent");

		//    PreventDuplicateMetsKey(MetsProperties.DatasetExtent);

		//    _metsPairs.Add(JSONUtils.MakeKeyValuePair(kDatasetExtent, extent));
		//}

		///// ------------------------------------------------------------------------------------
		///// <summary>
		///// Sets the total duration of all audio and/or video recording in this resource.
		///// </summary>
		///// ------------------------------------------------------------------------------------
		//public void SetAudioVideoExtent(TimeSpan totalDuration)
		//{
		//    SetAudioVideoExtent(totalDuration.ToString());
		//}

		///// ------------------------------------------------------------------------------------
		///// <summary>
		///// Sets the total duration of all audio and/or video recording in this resource.
		///// </summary>
		///// ------------------------------------------------------------------------------------
		//public void SetAudioVideoExtent(string totalDuration)
		//{
		//    PreventDuplicateMetsKey(MetsProperties.RecordingExtent);

		//    _metsPairs.Add(JSONUtils.MakeKeyValuePair(kRecordingExtent, totalDuration));
		//}

		///// ------------------------------------------------------------------------------------
		///// <summary>
		///// Sets the collection of contributors (and their roles) to this resource.
		///// </summary>
		///// ------------------------------------------------------------------------------------
		//public void SetContributors(ContributionCollection contributions)
		//{
		//    if (contributions == null)
		//        throw new ArgumentNullException("contributions");

		//    if (contributions.Count == 0)
		//        return;

		//    PreventDuplicateMetsKey(MetsProperties.Contributors);

		//    _metsPairs.Add(JSONUtils.MakeArrayFromValues(kContributor,
		//        contributions.Select(GetContributorsMetsPairs)));
		//}

		///// ------------------------------------------------------------------------------------
		//private string GetContributorsMetsPairs(Contribution contribution)
		//{
		//    var roleCode = (contribution.Role != null &&
		//        Settings.Default.RampContributorRoles.Contains(contribution.Role.Code) ?
		//        contribution.Role.Code : string.Empty);

		//    return JSONUtils.MakeKeyValuePair(kDefaultKey, contribution.ContributorName) +
		//        kSeparator + JSONUtils.MakeKeyValuePair(kRole, roleCode);
		//}

		///// ------------------------------------------------------------------------------------
		///// <summary>
		///// Sets the general description for this resource in a single language
		///// </summary>
		///// <param name="description">The description</param>
		///// <param name="language">ISO 639-2 3-letter language code (RAMP only supports about
		///// 20 major LWCs. Not sure what happens if an unrecognized code gets passed to this.
		///// Feel free to try it and find out.</param>
		///// ------------------------------------------------------------------------------------
		//public void SetDescription(string description, string language)
		//{
		//    SetGeneralDescription(new[] { GetKvpsForLanguageSpecificString(language, description) });
		//}

		///// ------------------------------------------------------------------------------------
		///// <summary>
		///// Sets descriptions for this resource in (potentially) multiple languages
		///// </summary>
		///// <param name="descriptions">Dictionary of language->description, where the keys are
		///// ISO 639-2 3-letter language code (RAMP only supports about 20 major LWCs. Not sure
		///// what happens if an unrecognized code gets passed to this. Feel free to try it and
		///// find out.</param>
		///// ------------------------------------------------------------------------------------
		//public void SetDescription(IDictionary<string, string> descriptions)
		//{
		//    if (descriptions == null)
		//        throw new ArgumentNullException("descriptions");

		//    if (descriptions.Count == 0)
		//        return;

		//    List<string> abs = new List<string>();
		//    foreach (var desc in descriptions)
		//    {
		//        if (desc.Key.Length != 3)
		//            throw new ArgumentException();
		//        abs.Add(GetKvpsForLanguageSpecificString(desc.Key, desc.Value));
		//    }

		//    SetGeneralDescription(abs);
		//}

		///// ------------------------------------------------------------------------------------
		//private void SetGeneralDescription(IEnumerable<string> abs)
		//{
		//    PreventDuplicateMetsKey(MetsProperties.GeneralDescription);

		//    SetFlag(kFlagHasGeneralDescription);

		//    _metsPairs.Add(JSONUtils.MakeArrayFromValues(kGeneralDescription, abs));
		//}

		///// ------------------------------------------------------------------------------------
		///// <summary>
		///// Sets an abstract for this resource in a single language
		///// </summary>
		///// <param name="description">The abstract description</param>
		///// <param name="language">ISO 639-2 3-letter language code (RAMP only supports about
		///// 20 major LWCs. Not sure what happens if an unrecognized code gets passed to this.
		///// Feel free to try it and find out.</param>
		///// ------------------------------------------------------------------------------------
		//public void SetAbstract(string description, string language)
		//{
		//    SetAbstractDescription(new[] { GetKvpsForLanguageSpecificString(language, description) });
		//}

		///// ------------------------------------------------------------------------------------
		///// <summary>
		///// Sets abstracts for this resource in (potentially) multiple languages
		///// </summary>
		///// <param name="descriptions">Dictionary of language->abstract, where the keys are ISO
		///// 639-2 3-letter language code (RAMP only supports about 20 major LWCs. Not sure what
		///// happens if an unrecognized code gets passed to this. Feel free to try it and find
		///// out.</param>
		///// ------------------------------------------------------------------------------------
		//public void SetAbstract(IDictionary<string, string> descriptions)
		//{
		//    if (descriptions == null)
		//        throw new ArgumentNullException("descriptions");

		//    if (descriptions.Count == 0)
		//        return;

		//    List<string> abs = new List<string>();
		//    foreach (var desc in descriptions)
		//    {
		//        if (desc.Key.Length != 3)
		//            throw new ArgumentException();
		//        abs.Add(GetKvpsForLanguageSpecificString(desc.Key, desc.Value));
		//    }

		//    SetAbstractDescription(abs);
		//}

		///// ------------------------------------------------------------------------------------
		//private void SetAbstractDescription(IEnumerable<string> abs)
		//{
		//    PreventDuplicateMetsKey(MetsProperties.AbstractDescription);

		//    SetFlag(kFlagHasAbstractDescription);

		//    _metsPairs.Add(JSONUtils.MakeArrayFromValues(kAbstractDescription, abs));
		//}

		///// ------------------------------------------------------------------------------------
		///// <summary>
		///// Sets the promotion text for this resource in a single language
		///// </summary>
		///// <param name="text">The promotion text</param>
		///// <param name="language">ISO 639-2 3-letter language code (RAMP only supports about
		///// 20 major LWCs. Not sure what happens if an unrecognized code gets passed to this.
		///// Feel free to try it and find out.</param>
		///// ------------------------------------------------------------------------------------
		//public void SetPromotion(string text, string language)
		//{
		//    SetPromotion(new[] { GetKvpsForLanguageSpecificString(language, text) });
		//}

		///// ------------------------------------------------------------------------------------
		///// <summary>
		///// Sets the promotion text for this resource in (potentially) multiple languages
		///// </summary>
		///// <param name="descriptions">Dictionary of language->promotion, where the keys are ISO
		///// 639-2 3-letter language code (RAMP only supports about 20 major LWCs. Not sure what
		///// happens if an unrecognized code gets passed to this. Feel free to try it and find
		///// out.</param>
		///// ------------------------------------------------------------------------------------
		//public void SetPromotion(IDictionary<string, string> descriptions)
		//{
		//    if (descriptions == null)
		//        throw new ArgumentNullException("descriptions");

		//    if (descriptions.Count == 0)
		//        return;

		//    List<string> abs = new List<string>();
		//    foreach (var desc in descriptions)
		//    {
		//        if (desc.Key.Length != 3)
		//            throw new ArgumentException();
		//        abs.Add(GetKvpsForLanguageSpecificString(desc.Key, desc.Value));
		//    }

		//    SetPromotion(abs);
		//}

		///// ------------------------------------------------------------------------------------
		//private void SetPromotion(IEnumerable<string> abs)
		//{
		//    PreventDuplicateMetsKey(MetsProperties.Promotion);

		//    SetFlag(kFlagHasPromotionDescription);

		//    _metsPairs.Add(JSONUtils.MakeArrayFromValues(kPromotionDescription, abs));
		//}

		///// ------------------------------------------------------------------------------------
		///// <summary>
		///// Gets a pair of JSON key-value pairs for a language-specific string (used for the
		///// various language-specific descriptions)
		///// </summary>
		///// <param name="lang"></param>
		///// <param name="s"></param>
		///// ------------------------------------------------------------------------------------
		//private string GetKvpsForLanguageSpecificString(string lang, string s)
		//{
		//    if (lang.Length != 3)
		//        throw new ArgumentException("Language must be specified as a valid 3-letter code as specified in ISO-639-2.");

		//    return JSONUtils.MakeKeyValuePair(kDefaultKey, s) + kSeparator + JSONUtils.MakeKeyValuePair(kAbstractLanguageName, lang);
		//}
		//#endregion

		/// ------------------------------------------------------------------------------------
		public abstract bool LaunchArchivingProgram();
		/// ------------------------------------------------------------------------------------
		public abstract bool CreatePackage();

		//#region Methods for creating mets file.
		///// ------------------------------------------------------------------------------------
		//public string GetUnencodedMetsData()
		//{
		//    var bldr = new StringBuilder();

		//    SetMetsPairsForFiles();

		//    foreach (var value in _metsPairs)
		//        bldr.AppendFormat("{0},", value);

		//    return string.Format("{{{0}}}", bldr.ToString().TrimEnd(','));
		//}

		///// ------------------------------------------------------------------------------------
		//public string CreateMetsFile()
		//{
		//    try
		//    {
		//        var metsData = Resources.EmptyMets.Replace("<binData>", "<binData>" + JSONUtils.EncodeData(GetUnencodedMetsData()));
		//        _tempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
		//        Directory.CreateDirectory(_tempFolder);
		//        _metsFilePath = Path.Combine(_tempFolder, "mets.xml");
		//        File.WriteAllText(_metsFilePath, metsData);
		//    }
		//    catch (Exception e)
		//    {
		//        if ((e is IOException) || (e is UnauthorizedAccessException) || (e is SecurityException))
		//        {
		//            ReportError(e, LocalizationManager.GetString("DialogBoxes.ArchivingDlg.CreatingInternalReapMetsFileErrorMsg",
		//                "There was an error attempting to create the RAMP/REAP mets file."));
		//            return null;
		//        }
		//        throw;
		//    }

		//    if (_incrementProgressBarAction != null)
		//        _incrementProgressBarAction();

		//    return _metsFilePath;
		//}

		// /// ------------------------------------------------------------------------------------
		//private void SetMetsPairsForFiles()
		//{
		//    if (_fileLists != null)
		//    {
		//        string value = GetMode();
		//        if (value != null)
		//            _metsPairs.Add(value);

		//        // Return JSON array of files with their descriptions.
		//        _metsPairs.Add(JSONUtils.MakeArrayFromValues(kSourceFilesForMets,
		//            GetSourceFilesForMetsData(_fileLists)));

		//        if (ImageCount > 0)
		//            _metsPairs.Add(JSONUtils.MakeKeyValuePair(kImageExtent, ImageCount.ToString(CultureInfo.InvariantCulture)));
		//if (ShowRecordingCountNotLength && _audioVideoCount > 0)
		//    SetAudioVideoExtent(string.Format("{0} recording files.", _audioVideoCount));
		//    }
		//}

		///// ------------------------------------------------------------------------------------
		///// <summary>
		///// Gets the number of image files in the list(s) of files to archive.
		///// </summary>
		///// <remarks>Public (and self-populating on-demand) to facilitate testing</remarks>
		///// ------------------------------------------------------------------------------------
		//public int ImageCount
		//{
		//    get
		//    {
		//        if (_fileLists != null && _imageCount < 0)
		//            GetMode();
		//        return _imageCount;
		//    }
		//}

		///// ------------------------------------------------------------------------------------
		//public int AudioVideoCount
		//{
		//    get
		//    {
		//        if (_fileLists != null && _audioVideoCount < 0)
		//            GetMode();
		//        return _audioVideoCount;
		//    }
		//}

		///// ------------------------------------------------------------------------------------
		///// <summary>
		///// Gets a comma-separated list of types found in the files to be archived
		///// (e.g. Text, Video, etc.).
		///// </summary>
		///// ------------------------------------------------------------------------------------
		//private string GetMode()
		//{
		//    _imageCount = 0;
		//    _audioVideoCount = 0;
		//    return GetMode(_fileLists.SelectMany(f => f.Value.Item1));
		//}

		///// ------------------------------------------------------------------------------------
		///// <summary>
		///// Gets a comma-separated list of types found in the files to be archived
		///// (e.g. Text, Video, etc.).
		///// </summary>
		///// ------------------------------------------------------------------------------------
		//public string GetMode(IEnumerable<string> files)
		//{
		//    if (files == null)
		//        return null;

		//    var list = new HashSet<string>();

		//    AddModesToSet(list, files);

		//    if (_metsPropertiesSet.HasFlag(MetsProperties.DatasetExtent) && !list.Contains(kModeDataset))
		//        throw new InvalidOperationException("Cannot set dataset extent for a resource which does not contain any \"dataset\" files.");

		//    return JSONUtils.MakeBracketedListFromValues(kFileTypeModeList, list);
		//}

		///// ------------------------------------------------------------------------------------
		//private void AddModesToSet(HashSet<string> list, IEnumerable<string> files)
		//{
		//    foreach (var file in files)
		//    {
		//        if (FileUtils.GetIsZipFile(file))
		//        {
		//            using (var zipFile = new ZipFile(file))
		//                AddModesToSet(list, zipFile.EntryFileNames);
		//            continue;
		//        }

		//        if (FileUtils.GetIsAudio(file))
		//        {
		//            _audioCount++;
		//            list.Add(kModeSpeech);
		//        }
		//        if (FileUtils.GetIsVideo(file))
		//        {
		//            _videoCount++;
		//            list.Add(kModeVideo);
		//        }
		//        if (FileUtils.GetIsText(file))
		//            list.Add(kModeText);
		//        if (FileUtils.GetIsImage(file))
		//            list.Add(ImagesArePhotographs ? kModePhotograph : kModeGraphic);
		//        if (FileUtils.GetIsMusicalNotation(file))
		//            list.Add(kModeMusicalNotation);
		//        if (FileUtils.GetIsDataset(file))
		//            list.Add(kModeDataset);
		//        if (FileUtils.GetIsSoftwareOrFont(file))
		//            list.Add(kModeSoftwareOrFont);
		//        if (FileUtils.GetIsPresentation(file))
		//            list.Add(kModePresentation);
		//    }
		//}

		///// ------------------------------------------------------------------------------------
		//public IEnumerable<string> GetSourceFilesForMetsData(IDictionary<string, Tuple<IEnumerable<string>, string>> fileLists)
		//{
		//    foreach (var kvp in fileLists)
		//    {
		//        foreach (var file in kvp.Value.Item1)
		//        {
		//            var description = _getFileDescription(kvp.Key, file);

		//            var fileName = NormalizeFilenameForRAMP(kvp.Key, Path.GetFileName(file));

		//            yield return JSONUtils.MakeKeyValuePair(kDefaultKey, fileName) + kSeparator +
		//                JSONUtils.MakeKeyValuePair(kFileDescription, description) + kSeparator +
		//                JSONUtils.MakeKeyValuePair(kFileRelationship, kRelationshipSource);
		//        }
		//    }
		//}

		/// ------------------------------------------------------------------------------------
		protected virtual StringBuilder DoArchiveSpecificFilenameNormalization(string key, string fileName)
		{
			return AppSpecificFilenameNormalization != null ? new StringBuilder(fileName) : null;
		}

		/// ------------------------------------------------------------------------------------
		public virtual string NormalizeFilename(string key, string fileName)
		{
			StringBuilder bldr = DoArchiveSpecificFilenameNormalization(key, fileName);
			if (AppSpecificFilenameNormalization != null)
				AppSpecificFilenameNormalization(key, fileName, bldr);
			return bldr.ToString();
		}

		/// ------------------------------------------------------------------------------------
		const int CopyBufferSize = 64 * 1024;
		/// ------------------------------------------------------------------------------------
		protected static void CopyFile(string src, string dest)
		{
			using (var outputFile = File.OpenWrite(dest))
			{
				using (var inputFile = File.OpenRead(src))
				{
					var buffer = new byte[CopyBufferSize];
					int bytesRead;
					while ((bytesRead = inputFile.Read(buffer, 0, CopyBufferSize)) != 0)
					{
						outputFile.Write(buffer, 0, bytesRead);
					}
				}
			}
		}

		/// ------------------------------------------------------------------------------------
		public virtual void Cancel()
		{
			if (_cancelProcess)
				return;

			_cancelProcess = true;

			if (_worker != null)
			{
				DisplayMessage(Environment.NewLine + LocalizationManager.GetString(
						"DialogBoxes.ArchivingDlg.CancellingMsg", "Canceling..."), MessageType.Error);

				_worker.CancelAsync();
				while (_worker.IsBusy)
					Application.DoEvents();
			}
		}

		/// ------------------------------------------------------------------------------------
		protected void ReportError(Exception e, string msg)
		{
			if (OnDisplayError != null)
				OnDisplayError(msg, _title, e);
			else if (e != null)
				throw e;

			if (HandleNonFatalError != null)
				HandleNonFatalError(e, msg);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// The file locations are different on Linux than on Windows
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static bool IsMono
		{
			get { return (Type.GetType("Mono.Runtime") != null); }
		}
	}
}
