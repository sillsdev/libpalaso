using SIL.Archiving.Generic;
using SIL.Archiving.IMDI.Lists;

namespace SIL.Archiving.IMDI.Schema
{
	/// <summary>Extension methods to simplify access to IMDI objects</summary>
	public static class IMDIExtensions
	{
		/// <summary>Set the value of a Vocabulary_Type variable</summary>
		/// <param name="vocabularyType"></param>
		/// <param name="value"></param>
		/// <param name="isClosedVocabulary"></param>
		/// <param name="link"></param>
		public static void SetValue(this VocabularyType vocabularyType, string value, bool isClosedVocabulary, string link)
		{
			if (value == null) return;

			vocabularyType ??= new VocabularyType();

			vocabularyType.Value = value;
			vocabularyType.Type = isClosedVocabulary
				? VocabularyTypeValueType.ClosedVocabulary
				: VocabularyTypeValueType.OpenVocabulary;

			vocabularyType.Link = link;
		}

		/// <summary>Copy information from ArchivingLocation object to Location_Type object</summary>
		/// <param name="archivingLocation"></param>
		/// <returns></returns>
		public static LocationType ToIMDILocationType(this ArchivingLocation archivingLocation)
		{
			var returnVal = new LocationType
			{
				Address = archivingLocation.Address
			};
			returnVal.SetContinent(archivingLocation.Continent);
			returnVal.SetCountry(archivingLocation.Country);

			// region is an array
			if (!string.IsNullOrEmpty(archivingLocation.Region))
				returnVal.Region.Add(archivingLocation.Region);

			return returnVal;
		}

		/// <summary>Converts a LanguageString into a Description_Type</summary>
		/// <param name="langString"></param>
		/// <returns></returns>
		public static DescriptionType ToIMDIDescriptionType(this LanguageString langString)
		{
			var desc = new DescriptionType { Value = langString.Value };
			if (!string.IsNullOrEmpty(langString.Iso3LanguageId))
				desc.LanguageId = LanguageList.FindByISO3Code(langString.Iso3LanguageId).Id;

			return desc;
		}

		/// <summary>Converts a string to a Vocabulary_Type</summary>
		/// <param name="stringValue"></param>
		/// <param name="isClosedVocabulary"></param>
		/// <param name="link"></param>
		/// <returns></returns>
		public static VocabularyType ToVocabularyType(this string stringValue, bool isClosedVocabulary, string link)
		{
			VocabularyType returnVal = new VocabularyType();
			returnVal.SetValue(stringValue, isClosedVocabulary, link);
			return returnVal;
		}

		///// <summary>Add an Actor_Type to the collection</summary>
		///// <param name="archivingActor"></param>
		//public static Actor_Type ToIMDIActorType(this ArchivingActor archivingActor)
		//{
		//    var newActor = new Actor_Type
		//    {
		//        Name = new[] { archivingActor.GetName() },
		//        FullName = archivingActor.GetFullName()
		//    };

		//    if (!string.IsNullOrEmpty(archivingActor.Age))
		//        newActor.Age = archivingActor.Age;


		//    // languages
		//    ClosedIMDIItemList boolList = ListConstructor.GetClosedList(ListType.Boolean);
		//    foreach (var langIso3 in archivingActor.Iso3LanguageIds)
		//    {
		//        var langType = LanguageList.FindByISO3Code(langIso3).ToLanguageType();
		//        if (langType == null) continue;
		//        langType.PrimaryLanguage = boolList.FindByValue((archivingActor.PrimaryLanguageIso3Code == langIso3) ? "true" : "false").ToBooleanType();
		//        langType.MotherTongue = boolList.FindByValue((archivingActor.MotherTongueLanguageIso3Code == langIso3) ? "true" : "false").ToBooleanType();
		//        newActor.Languages.Language.Add(langType);
		//    }

		//    // BirthDate (year)
		//    var birthDate = archivingActor.GetBirthDate();
		//    if (!string.IsNullOrEmpty(birthDate))
		//        newActor.SetBirthDate(birthDate);

		//    // Sex
		//    ClosedIMDIItemList genderList = ListConstructor.GetClosedList(ListType.ActorSex);
		//    newActor.SetSex(archivingActor.Gender);

		//    // Education
		//    if (!string.IsNullOrEmpty(archivingActor.Education))
		//        newActor.Education = archivingActor.Education;

		//    // Occupation
		//    if (!string.IsNullOrEmpty(archivingActor.Occupation))
		//        newActor.FamilySocialRole = archivingActor.Occupation.ToVocabularyType(false, ListType.Link(ListType.ActorFamilySocialRole));

		//    return newActor;
		//}
	}
}
