using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using SIL.Archiving.IMDI.Lists;

namespace SIL.Archiving.IMDI.Schema
{
	/// <summary>
	/// Functions to simplify access to IMDI objects
	/// </summary>
	public static class SchemaHelper
	{
		/// <summary>
		/// Creates a new String_Type object and sets the Value
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static String_Type SetString(string value)
		{
			return new String_Type { Value = value };
		}

		/// <summary>
		/// Set the value of a String_Type variable
		/// </summary>
		/// <param name="stringType"></param>
		/// <param name="value"></param>
		public static void SetValue(this String_Type stringType, string value)
		{
			if (stringType == null)
				stringType = new String_Type();

			stringType.Value = value;
		}

		/// <summary>
		/// Creates the IMDI xml file from an object
		/// </summary>
		/// <param name="itemType"></param>
		/// <param name="itemToWrite"></param>
		/// <param name="fileName"></param>
		public static void WriteImdiFile(Metatranscript_Value_Type itemType, object itemToWrite, string fileName)
		{
			// the IMDI file is always built from a METATRANSCRIPT_Type object
			var wrapper = new METATRANSCRIPT_Type
			{
				Type = itemType,
				Items = new[] {itemToWrite}
			};

			XmlSerializer serializer = new XmlSerializer(typeof(METATRANSCRIPT_Type));
			TextWriter writer = new StreamWriter(fileName);
			serializer.Serialize(writer, wrapper);
			writer.Close();
		}

		/// <summary>
		/// Add a description of the object
		/// </summary>
		/// <param name="descriptionArray"></param>
		/// <param name="languageName"></param>
		/// <param name="descriptionText"></param>
		public static void AddDescription(this List<Description_Type> descriptionArray, string languageName, string descriptionText)
		{
			descriptionArray.AddDescription(languageName, descriptionText, false);
		}

		/// <summary>
		/// Add a description of the object
		/// </summary>
		/// <param name="descriptionArray"></param>
		/// <param name="languageName"></param>
		/// <param name="descriptionText"></param>
		/// <param name="languageNameIsISO3Code"></param>
		public static void AddDescription(this List<Description_Type> descriptionArray, string languageName, string descriptionText, bool languageNameIsISO3Code)
		{
			var lang = languageNameIsISO3Code
				? new LanguageList().FindByISO3Code(languageName)
				: new LanguageList().FindByEnglishName(languageName);

			descriptionArray.AddDescription(lang, descriptionText);
		}

		/// <summary>
		/// Add a description of the object
		/// </summary>
		/// <param name="descriptionArray"></param>
		/// <param name="language"></param>
		/// <param name="descriptionText"></param>
		public static void AddDescription(this List<Description_Type> descriptionArray, LanguageItem language, string descriptionText)
		{
			// description
			if (descriptionArray == null)
				descriptionArray = new List<Description_Type>();

			var description = descriptionArray.FirstOrDefault(d => d.LanguageId == language.Id);

			// if not found, add a new description
			if (description == null)
			{
				description = new Description_Type { LanguageId = language.Id };
				descriptionArray.Add(description);
			}

			description.Value = descriptionText;
		}
	}
}
