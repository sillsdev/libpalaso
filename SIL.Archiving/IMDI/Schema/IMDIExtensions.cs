using SIL.Archiving.Generic;
using SIL.Archiving.IMDI.Lists;

namespace SIL.Archiving.IMDI.Schema
{
	/// <summary>Extension methods to simplify access to IMDI objects</summary>
	public static class IMDIExtensions
	{
		/// <summary>Set the value of a String_Type variable</summary>
		/// <param name="stringType"></param>
		/// <param name="value"></param>
		public static void SetValue(this String_Type stringType, string value)
		{
			if (value == null) return;

			if (stringType == null)
				stringType = new String_Type();

			stringType.Value = value;
		}

		/// <summary>Set the value of a Vocabulary_Type variable</summary>
		/// <param name="vocabularyType"></param>
		/// <param name="value"></param>
		/// <param name="isClosedVocabulary"></param>
		public static void SetValue(this Vocabulary_Type vocabularyType, string value, bool isClosedVocabulary)
		{
			if (value == null) return;

			if (vocabularyType == null)
				vocabularyType = new Vocabulary_Type();

			vocabularyType.Value = value;
			vocabularyType.Type = isClosedVocabulary
				? VocabularyType_Value_Type.ClosedVocabulary
				: VocabularyType_Value_Type.OpenVocabulary;
		}

		/// <summary>Copy information from ArchivingLocation object to Location_Type object</summary>
		/// <param name="archivingLocation"></param>
		/// <returns></returns>
		public static Location_Type ToIMDILocationType(this ArchivingLocation archivingLocation)
		{
			var returnVal = new Location_Type
			{
				Continent = IMDISchemaHelper.SetVocabulary(archivingLocation.Continent, true),
				Country = IMDISchemaHelper.SetVocabulary(archivingLocation.Country, false),
				Address = IMDISchemaHelper.SetString(archivingLocation.Address)
			};

			// region is an array
			if (!string.IsNullOrEmpty(archivingLocation.Region))
			{
				var region = new[] { new String_Type { Value = archivingLocation.Region } };
				returnVal.Region = region;
			}

			return returnVal;
		}

		/// <summary>Converts a LanguageString into a Description_Type</summary>
		/// <param name="langString"></param>
		/// <returns></returns>
		public static Description_Type ToIMDIDescriptionType(this LanguageString langString)
		{
			var lang = LanguageList.FindByISO3Code(langString.Iso3LanguageId);
			return new Description_Type
			{
				LanguageId = lang.Id,
				Value = langString.Value
			};
		}

		/// <summary>Add an Actor_Type to the collection</summary>
		/// <param name="archivingActor"></param>
		public static Actor_Type ToIMDIActorType(this ArchivingActor archivingActor)
		{
			var newActor = new Actor_Type
			{
				Name = new[] { new String_Type { Value = archivingActor.GetName() } }
			};
			newActor.FullName.SetValue(archivingActor.GetFullName());

			// languages
			ClosedIMDIItemList boolList = ListConstructor.GetClosedList(ListType.Boolean);
			foreach (var langIso3 in archivingActor.Iso3LanguageIds)
			{
				var langType = LanguageList.FindByISO3Code(langIso3).ToLanguageType();
				if (langType == null) continue;
				langType.PrimaryLanguage = boolList.FindByValue((archivingActor.PrimaryLanguageIso3Code == langIso3) ? "true" : "false").ToBooleanType();
				langType.MotherTongue = boolList.FindByValue((archivingActor.MotherTongueLanguageIso3Code == langIso3) ? "true" : "false").ToBooleanType();
				newActor.Languages.Language.Add(langType);
			}

			// BirthDate (year)
			var birthDate = archivingActor.GetBirthDate();
			if (!string.IsNullOrEmpty(birthDate))
				newActor.BirthDate = new Date_Type { Value = birthDate };

			// Sex
			ClosedIMDIItemList genderList = ListConstructor.GetClosedList(ListType.ActorSex);
			newActor.Sex = genderList.FindByValue(archivingActor.Gender).ToVocabularyType(VocabularyType_Value_Type.ClosedVocabulary);

			// Education
			if (!string.IsNullOrEmpty(archivingActor.Education))
				newActor.Education.SetValue(archivingActor.Education);

			return newActor;
		}
	}
}
