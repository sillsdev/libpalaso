using System;

// ReSharper disable CSharpWarnings::CS1591
// ReSharper disable InconsistentNaming

namespace SIL.Archiving
{
	/// <summary>
	/// For use with SetAudience method. Note: Both 'Internal audience' and 'Wider audience' works are expected to be in a
	/// language of wider communication, or in both a vernacular language and a language of wider communication. An Internal
	/// Audience work is of a nature that is always intended only for internal use, while a Wider Audience work is eventually
	/// intended for public use, when it is sufficiently developed to be shared. Later in the description you will be asked
	/// to describe the 'work stage' and also to recommend the 'sensitivity' category (who can see and use the work). The
	/// work stage can be a consideration in determining sensitivity.
	/// </summary>
	[Flags]
	public enum AudienceType
	{
		/// <summary>
		/// for use by a specific local language community, e.g., Scripture, Multilingual education, Health, Scripture use, etc.
		/// </summary>
		Vernacular = 1,
		/// <summary>
		/// for use in a classroom or instructional setting for SIL or Wycliffe role related training (published textbooks
		/// generally belong in Wider Audience).
		/// </summary>
		Training = 2,
		/// <summary>
		/// for an audience of SIL and WO staff, e.g., a position paper, internal report, or document generated and used to
		/// facilitate the production of other resources, such as a back translation, translation checking questions, or a
		/// survey instrument.
		/// </summary>
		Internal = 3,
		/// <summary>
		/// for an external audience, communicating in a major or national language, e.g., a conference paper, article, book,
		/// data set, or language documentation set. Use this also for any kind of work intended for both the language
		/// community and also speakers of the language of wider communication used in the work (e.g., a bilingual or
		/// trilingual dictionary).
		/// </summary>
		Wider = 4,
	}

	/// <summary>
	/// Types of materials for use by a specific local language community. This does not include materials related
	/// to the translation task that are not specifically Scripture or other material intended for use by the general
	/// population (e.g. back translation, checking questions, key terms list)
	/// The Scripture Type values (the ones prefixed with "Bible") work along with (and should not be confused with)
	/// the Scripture Part (specific testaments/books included).
	/// </summary>
	[Flags]
	public enum VernacularMaterialsType : long
	{
		// First two bits determine the overall vernacular materials type. Only one of these may be set. Remaining
		// bits indicate specific content.

		/// <summary>
		/// Scripture in any format (e.g. translated text, selected verses, abridgement, story format, Bible study, concordance, etc.)
		/// </summary>
		Scripture = 1L << 0,
		/// <summary>
		/// For example, health, literacy instruction, or Scripture application (devotional, evangelistic, life application, etc.)
		/// </summary>
		Other =  1L << 1,

		/// <summary>
		/// Does not imply that the resource is a complete Bible, merely that whatever books present represent the complete text, as
		/// opposed to summaries or excerpts. It can apply whether the resource is text, sound, or video recording.
		/// </summary>
		BibleCompleteText = Scripture | 1L << 2,

		BibleTextAbridgement = Scripture | 1L << 3,
		/// <summary>Excerpts</summary>
		BibleSelectedText = Scripture | 1L << 4,
		/// <summary>Verses not marked</summary>
		BibleStory = Scripture | 1L << 5,
		BibleComicBookStyle = Scripture | 1L << 6,
		BibleBackground = Scripture | 1L << 7,
		BibleCommentary = Scripture | 1L << 8,
		BibleConcordance = Scripture | 1L << 9,
		BibleDictionary = Scripture | 1L << 10,
		BibleLectionary = Scripture | 1L << 11,
		BibleStudyMaterial = Scripture | 1L << 12,
		BibleTeachingMaterial = Scripture | 1L << 13,


		CommunityAndCulture_Calendar = Other | 1L << 2,
		CommunityAndCulture_CivicEducation = Other | 1L << 3,
		CommunityAndCulture_CommunityNews = Other | 1L << 4,
		CommunityAndCulture_CultureAndFolklore = Other | 1L << 5,
		CommunityAndCulture_ProverbsAndMaxims = Other | 1L << 6,
		CommunityAndCulture_SongLyrics = Other | 1L << 7,

		CommunityDevelopment_AgricultureAndFoodProduction = Other | 1L << 8,
		CommunityDevelopment_BusinessAndIncomeGeneration = Other | 1L << 9,
		CommunityDevelopment_EnvironmentalCare = Other | 1L << 10,
		CommunityDevelopment_InstructionalManual = Other | 1L << 11,
		CommunityDevelopment_VocationalEducation = Other | 1L << 12,

		Health_AIDSAndHIV = Other | 1L << 13,
		Health_AvianFlu = Other | 1L << 14,
		Health_GeneralHealthAndHygiene = Other | 1L << 15,
		Health_Malaria = Other | 1L << 16,
		Health_PrenatalAndInfantCare = Other | 1L << 17,

		LanguageAcquisition_EnglishLanguageInstruction = Other | 1L << 18,
		LanguageAcquisition_GrammarInstruction = Other | 1L << 19,
		LanguageAcquisition_LanguageInstruction = Other | 1L << 20,
		LanguageAcquisition_PhraseBook = Other | 1L << 21,

		LiteracyEducation_Alphabet = Other | 1L << 22,
		LiteracyEducation_Prereading = Other | 1L << 23,
		LiteracyEducation_Prewriting = Other | 1L << 24,
		LiteracyEducation_Primer = Other | 1L << 25,
		LiteracyEducation_Reader = Other | 1L << 26,
		LiteracyEducation_Riddles = Other | 1L << 27,
		LiteracyEducation_Spelling = Other | 1L << 28,
		LiteracyEducation_TonePrimer = Other | 1L << 29,
		LiteracyEducation_TransitionPrimer = Other | 1L << 30,
		LiteracyEducation_Vocabulary = Other | 1L << 31,
		LiteracyEducation_Writing = Other | 1L << 32,

		ScriptureUse_Catechism = Other | 1L << 33,
		ScriptureUse_ChristianLiving = Other | 1L << 34,
		ScriptureUse_Devotional = Other | 1L << 35,
		ScriptureUse_Evangelistic = Other | 1L << 36,
		ScriptureUse_Theology = Other | 1L << 37,
		ScriptureUse_WorshipAndLiturgy = Other | 1L << 38,

		VernacularEducation_Arithmetic = Other | 1L << 39,
		VernacularEducation_Arts = Other | 1L << 40,
		VernacularEducation_Ethics = Other | 1L << 41,
		VernacularEducation_Numbers = Other | 1L << 42,
		VernacularEducation_Science = Other | 1L << 43,
		VernacularEducation_SocialStudies = Other | 1L << 44,

		ResourceFormats_ActivityBook = Other | 1L << 45,
		ResourceFormats_TeachersGuide = Other | 1L << 46,
		ResourceFormats_Textbook = Other | 1L << 47,
		ResourceFormats_WallChart = Other | 1L << 48,
		ResourceFormats_Workbook = Other | 1L << 49,
	}

	/// <summary>
	/// Types of resources for use in a classroom or instructional setting for SIL or Wycliffe role-related training
	/// </summary>
	public enum TrainingResourceType
	{
		/// <summary>
		/// Whole workshop, workshop module, or component
		/// </summary>
		Workshop,
		/// <summary>
		/// Whole course, course module or component
		/// </summary>
		Course,
		/// <summary>
		/// Published work/textbook closely associated with a specific course.
		/// </summary>
		PublishedWork,
	}

	/// <summary>
	/// Types of internal resources. This does not include work in progress that is eventually intended to be
	/// a publicly shared work (e.g., an article or book)
	/// </summary>
	public enum InternalWorkType
	{
		/// <summary>
		/// Works such as formal or personal communication supporting a point of research or scholarship, or in
		/// preparation of a survey (personal communication should only be submitted with the permission of the
		/// author).
		/// </summary>
		Correspondence,
		InternalReport,
		/// <summary>
		/// Map not included in a larger work
		/// </summary>
		Map,
		PositionPaper,
		/// <summary>
		/// For example, health, literacy instruction, or Scripture application (devotional, evangelistic, life application, etc.)
		/// </summary>
		Other,
	}

	/// <summary>
	/// Bibliographic type.
	/// </summary>
	public enum ScholarlyWorkType
	{
		ArticleOrChapter,
		/// <summary>
		/// 50 pages or less
		/// </summary>
		ShortBook,
		BookReview,
		/// <summary>
		/// can describe an online volume as well as a physical book
		/// </summary>
		CollectiveWork,
		ConferencePaper,
		ConferencePoster,
		DoctoralWork,
		EncyclopediaArticle,
		JournalArticle,
		/// <summary>
		/// accepted for publication but not the published version
		/// </summary>
		JournalArticlePreprint,
		AcademicManuscript,
		MastersWork,
		/// <summary>
		/// can describe an online resource as well as a physical book
		/// </summary>
		Monograph,
		NewsItem,
		Presentation,
		PrimaryData,
		Report,
		WorkingPaper,
		OtherResource,
	}

	/// <summary>
	/// Domains to which a resource relates http://purl.org/net/sword-types/SIL/metadata/dc/subject/silDomain
	/// </summary>
	[Flags]
	public enum SilDomain : long
	{
		// Academic domains

		/// <summary>ATRN</summary>
		AcademicTraining = 1L << 0,
		/// <summary>ANTH</summary>
		Anthropology = 1L << 1,
		/// <summary>EMUS</summary>
		ArtsandEthnomusicology = 1L << 2,
		/// <summary>COMM</summary>
		Communications = 1L << 3,
		/// <summary>CMDV</summary>
		CommunityDevelopment = 1L << 4,
		/// <summary>COUN</summary>
		Counseling = 1L << 5,
		/// <summary>INTR</summary>
		InternationalOrGovernmentRelations = 1L << 6,
		/// <summary>LGCL</summary>
		LanguageAndCultureLearning = 1L << 7,
		/// <summary>LGAS</summary>
		LanguageAssessment = 1L << 8,
		/// <summary>LPMT</summary>
		LanguageProgramManagement = 1L << 9,
		/// <summary>LTEC</summary>
		LanguageTechnology = 1L << 10,
		/// <summary>LRND</summary>
		LearningAndDevelopment = 1L << 11,
		/// <summary>LIBR</summary>
		LibraryOrMuseumOrArchiving = 1L << 12,
		/// <summary>LING</summary>
		Linguistics = 1L << 13,
		/// <summary>LTCY</summary>
		LiteracyAndEducation = 1L << 14,
		/// <summary>MGMT (Management, General)</summary>
		Management = 1L << 15,
		/// <summary>PUBL</summary>
		Publishing = 1L << 16,
		/// <summary>SUSE</summary>
		ScriptureUse = 1L << 17,
		/// <summary>SIGN</summary>
		SignLanguages = 1L << 18,
		/// <summary>SLNG</summary>
		Sociolinguistics = 1L << 19,
		/// <summary>TRAN</summary>
		Translation = 1L << 20,
		/// <summary>VEMA</summary>
		VernacularMedia = 1L << 21,

		// Sub-domains

		Anth_Ethnography = Anthropology | 1L << 22,
		Anth_Interview  = Anthropology | 1L << 23,
		Anth_KinshipAnalysis = Anthropology | 1L << 24,

		Emus_ArtisticCommunicationProfile = ArtsandEthnomusicology | 1L << 25,
		Emus_SummaryArtisticGenreAnalysis = ArtsandEthnomusicology | 1L << 26,
		Emus_SummaryArtisticEventFormAnalysis = ArtsandEthnomusicology | 1L << 27,
		Emus_PerformanceCollection = ArtsandEthnomusicology | 1L << 28,
		Emus_SongCollection = ArtsandEthnomusicology | 1L << 29,

		Lgas_SurveyArtifacts = LanguageAssessment | 1L << 30,
		Lgas_SurveyInstrument = LanguageAssessment | 1L << 31,
		Lgas_SurveyReport = LanguageAssessment | 1L << 32,
		Lgas_Wordlist = LanguageAssessment | 1L << 33,

		/// <summary>Tool for a specific language</summary>
		Ltec_ComputerTool = LanguageTechnology | 1L << 34,

		Ling_ComparativeDescription = Linguistics | 1L << 35,
		Ling_DiscourseDescription = Linguistics | 1L << 36,
		Ling_GrammaticalDescription = Linguistics | 1L << 37,
		Ling_LanguageDocumentation = Linguistics | 1L << 38,
		Ling_PhonologicalDescription = Linguistics | 1L << 39,
		Ling_SemanticDescription = Linguistics | 1L << 40,
		Ling_Lexicon = Linguistics | 1L << 41,
		Ling_InterlinearizedText = Linguistics | 1L << 42,
		/// <summary>primary language data of discourse length, any mode</summary>
		Ling_Text = Linguistics | 1L << 43,

		Ltcy_OrthographyDescription = LiteracyAndEducation | 1L << 44,

		Slng_SociolinguisticDescription = Sociolinguistics | 1L << 45,

		Tran_BackTranslation = Translation | 1L << 46,
		Tran_BibleNames = Translation | 1L << 47,
		Tran_ComprehensionCheckingQuestions = Translation | 1L << 48,
		Tran_Exegesis = Translation | 1L << 49,
		Tran_KeyBibleTerms = Translation | 1L << 50,
	}

	/// <summary>
	/// Stage of development.
	/// </summary>
	[Flags]
	public enum WorkStage
	{
		// First 4 bits are reserved for AudienceType

		/// <summary>Work has not been reviewed, even by its creator(s)</summary>
		RoughDraft = 1 << 4,
		/// <summary>The creators have reviewed the work and made corrections that they felt were needed.</summary>
		SelfReviewedDraft = 1 << 5,
		/// <summary>
		/// Consultant or Editor has reviewed the work and has agreed that it may be publicly released,
		/// though not all issues have been addressed. Use with Vernacular or Wider audience types.
		/// Not compatible with Training or Internal.
		/// </summary>
		ConsultantOrEditorReleasedDraft = 1 << 6,
		/// <summary>
		/// A consultant or editor has reviewed the work and made recommendations, and these have been applied.
		/// Use with Vernacular, Internal, or Wider audience types. Not compatible with Training.
		/// </summary>
		ConsultantOrEditorApproved = 1 << 7,
		/// <summary>
		/// Final editorial changes have been made.
		/// Use with Vernacular, Training, or Wider audience types. Not compatible with Internal.
		/// </summary>
		InPressOrPublished = 1 << 8,
		/// <summary>
		/// Materials have been used in a training course.
		/// </summary>
		UsedInTrainingCourse = AudienceType.Training | 1 << 9,
		/// <summary>
		/// The work has been accepted for publication but this version is prior to final editing and layout.
		/// Use with Training or Wider audience types. Not compatible with Vernacular or Internal.
		/// </summary>
		ReadyForPublicationOrFormalPreprint = 1 << 10,
		/// <summary>
		/// Finished internal work not intended for formal publication
		/// </summary>
		FinishedInternal = AudienceType.Internal | 1 << 11,
	}
}
// ReSharper restore CSharpWarnings::CS1591
// ReSharper restore InconsistentNaming