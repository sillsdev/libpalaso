using System.IO;
using System.Xml.Serialization;
using Palaso.Xml;

namespace SIL.Archiving.IMDI.Schema
{
	/// <summary>Functions to simplify access to IMDI objects</summary>
	public static class IMDISchemaHelper
	{
		/// <summary>Creates a new Vocabulary_Type object and sets the Value</summary>
		/// <param name="value"></param>
		/// <param name="isClosedVocabulary"></param>
		/// <param name="link"></param>
		/// <returns></returns>
		public static VocabularyType SetVocabulary(string value, bool isClosedVocabulary, string link)
		{
			if (value == null)
				return null;

			return new VocabularyType
			{
				Value = value,
				Type = isClosedVocabulary
					? VocabularyTypeValueType.ClosedVocabulary
					: VocabularyTypeValueType.OpenVocabulary,
				Link = link
			};
		}

		/// <summary>Creates the IMDI xml file from an object</summary>
		/// <param name="itemType"></param>
		/// <param name="itemToWrite"></param>
		/// <param name="fileName"></param>
		public static void WriteImdiFile(MetatranscriptValueType itemType, object itemToWrite, string fileName)
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
