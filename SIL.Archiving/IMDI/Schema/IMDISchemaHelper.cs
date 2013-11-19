using System.IO;
using System.Xml.Serialization;
using Palaso.Xml;

namespace SIL.Archiving.IMDI.Schema
{
	/// <summary>Functions to simplify access to IMDI objects</summary>
	public static class IMDISchemaHelper
	{
		/// <summary>Creates a new String_Type object and sets the Value</summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static String_Type SetString(string value)
		{
			return value == null ? null : new String_Type { Value = value };
		}

		/// <summary>Creates a new Vocabulary_Type object and sets the Value</summary>
		/// <param name="value"></param>
		/// <param name="isClosedVocabulary"></param>
		/// <param name="link"></param>
		/// <returns></returns>
		public static Vocabulary_Type SetVocabulary(string value, bool isClosedVocabulary, string link)
		{
			if (value == null)
				return null;

			return new Vocabulary_Type
			{
				Value = value,
				Type = isClosedVocabulary
					? VocabularyType_Value_Type.ClosedVocabulary
					: VocabularyType_Value_Type.OpenVocabulary,
				Link = link
			};
		}

		/// <summary>Creates the IMDI xml file from an object</summary>
		/// <param name="itemType"></param>
		/// <param name="itemToWrite"></param>
		/// <param name="fileName"></param>
		public static void WriteImdiFile(Metatranscript_Value_Type itemType, object itemToWrite, string fileName)
		{
			// the IMDI file is always built from a METATRANSCRIPT_Type object
			var wrapper = new MetaTranscript
			{
				Type = itemType,
				Items = new[] {itemToWrite}
			};

			//XmlSerializationHelper.SerializeToFile(fileName, wrapper);
			XmlSerializer serializer = new XmlSerializer(typeof(MetaTranscript));
			TextWriter writer = new StreamWriter(fileName);
			serializer.Serialize(writer, wrapper);
			writer.Close();
		}

	}
}
