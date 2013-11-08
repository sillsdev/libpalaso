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

		/// <summary>Creates the IMDI xml file from an object</summary>
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

			XmlSerializationHelper.SerializeToFile(fileName, wrapper);
		}

	}
}
