namespace SIL.Windows.Forms.Archiving.IMDI.Lists
{
	/// <summary>
	/// Contains the IMDI list types known at this time.
	/// </summary>
	public static class ListType
	{
// ReSharper disable CSharpWarnings::CS1591
		public const string ActorFamilySocialRole = "Actor-FamilySocialRole.xml";
		public const string ActorRole = "Actor-Role.xml";
		public const string ActorSex = "Actor-Sex.xml";
		public const string Boolean = "Boolean.xml";
		public const string ContentChannel = "Content-Channel.xml";
		public const string ContentEventStructure = "Content-EventStructure.xml";
		public const string ContentGenre = "Content-Genre.xml";
		public const string ContentInteractivity = "Content-Interactivity.xml";
		public const string ContentInvolvement = "Content-Involvement.xml";
		public const string ContentModalities = "Content-Modalities.xml";
		public const string ContentPlanningType = "Content-PlanningType.xml";
		public const string ContentSocialContext = "Content-SocialContext.xml";
		public const string ContentSubGenre = "Content-SubGenre.xml";
		public const string ContentSubGenreDiscourse = "Content-SubGenre-Discourse.xml";
		public const string ContentSubGenreDrama = "Content-SubGenre-Drama.xml";
		public const string ContentSubGenreLiterature = "Content-SubGenre-Literature.xml";
		public const string ContentSubGenrePoetry = "Content-SubGenre-Poetry.xml";
		public const string ContentSubGenreSinging = "Content-SubGenre-Singing.xml";
		public const string ContentSubGenreStimuli = "Content-SubGenre-Stimuli.xml";
		public const string ContentSubject = "Content-Subject.xml";
		public const string ContentTask = "Content-Task.xml";
		public const string Continents = "Continents.xml";
		public const string Countries = "Countries.xml";
		public const string MediaFileFormat = "MediaFile-Format.xml";
		public const string MediaFileType = "MediaFile-Type.xml";
		public const string MPILanguages = "MPI-Languages.xml";
		public const string Quality = "Quality.xml";
		public const string ValidationMethodology = "Validation-Methodology.xml";
		public const string ValidationType = "Validation-Type.xml";
		public const string WrittenResourceDerivation = "WrittenResource-Derivation.xml";
		public const string WrittenResourceFormat = "WrittenResource-Format.xml";
		public const string WrittenResourceSubType = "WrittenResource-SubType.xml";
		public const string WrittenResourceSubTypeAnnotation = "WrittenResource-SubType-Annotation.xml";
		public const string WrittenResourceSubTypeEthnography = "WrittenResource-SubType-Ethnography.xml";
		public const string WrittenResourceSubTypeLexicalAnalysis = "WrittenResource-SubType-LexicalAnalysis.xml";
		public const string WrittenResourceSubTypeOLACLS = "WrittenResource-SubType-OLAC-LS.xml";
		public const string WrittenResourceSubTypePrimaryText = "WrittenResource-SubType-PrimaryText.xml";
		public const string WrittenResourceType = "WrittenResource-Type.xml";
// ReSharper restore CSharpWarnings::CS1591

		/// <summary>Returns the value for the Link attribute</summary>
		/// <param name="listName"></param>
		/// <returns></returns>
		public static string Link(string listName)
		{
			return "http://www.mpi.nl/IMDI/Schema/" + listName;
		}
	}
}