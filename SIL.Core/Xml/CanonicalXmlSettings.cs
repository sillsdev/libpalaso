using System.Text;
using System.Xml;

namespace SIL.Xml
{
	///<summary>
	/// Canonical xml settings suitable for use in Chorus applications.
	///</summary>
	public class CanonicalXmlSettings
	{
		///<summary>
		/// Return an XmlReaderSettings suitable for use in Chorus applications.
		///</summary>
		/// <remarks>
		/// This formats with:
		///		CheckCharacters as 'false',
		///		ProhibitDtd as 'true',
		///		ValidationType as ValidationType.None,
		///		CloseInput as 'true',
		///		IgnoreWhitespace as 'true', and
		///		ConformanceLevel as 'conformanceLevel' parameter.
		/// </remarks>
		/// <param name="conformanceLevel">Document|Fragment</param>
		///<returns>XmlReaderSettings</returns>
		public static XmlReaderSettings CreateXmlReaderSettings(ConformanceLevel conformanceLevel)
		{
			var settings = new XmlReaderSettings
			{
				CheckCharacters = false,
				ConformanceLevel = conformanceLevel,
				DtdProcessing = DtdProcessing.Parse,
				ValidationType = ValidationType.None,
				CloseInput = true,
				IgnoreWhitespace = true
			};
			return settings;
		}

		///<summary>
		/// Return an XmlReaderSettings suitable for use in Chorus applications.
		///</summary>
		/// <remarks>
		/// This formats with:
		///		CheckCharacters as 'false',
		///		ProhibitDtd as 'true',
		///		ValidationType as ValidationType.None,
		///		CloseInput as 'true',
		///		IgnoreWhitespace as 'true', and
		///		ConformanceLevel as ConformanceLevel.Document.
		/// </remarks>
		///<returns>XmlReaderSettings</returns>
		public static XmlReaderSettings CreateXmlReaderSettings()
		{
			return CreateXmlReaderSettings(ConformanceLevel.Document);
		}

		///<summary>
		/// Return an XmlWriterSettings suitable for use in Chorus applications.
		///</summary>
		/// <remarks>
		/// This formats with new line on attributes, indents with tab, and encoded in UTF8 with no BOM.
		/// </remarks>
		/// <param name="conformanceLevel">Document|Fragment</param>
		/// <param name = "newLineHandling">Indicates how to normalize line breaks in the output.</param>
		///<returns>XmlWriterSettings</returns>
		public static XmlWriterSettings CreateXmlWriterSettings(ConformanceLevel conformanceLevel,
			NewLineHandling newLineHandling = NewLineHandling.Replace)
		{
			var settings = new XmlWriterSettings
			{
				NewLineOnAttributes = true,			// New line for each attribute, saves space on a typical Chorus changeset.
				Indent = true,						// Indent entities
				IndentChars = "\t",					// Tabs for the indent
				CheckCharacters = false,
				Encoding = new UTF8Encoding(false),	// UTF8 without a BOM.
				CloseOutput = true,					// Close the underlying stream on Close.  This is not the default.
				ConformanceLevel = conformanceLevel,
				NewLineChars = "\r\n",				// Use /r/n for our end of lines
				// NOTE: .NET and Mono (3.4) behave differently with NewLineHandling.Replace when calling XmlWriter.WriteRaw():
				// .NET will still replace line endings whereas Mono will write the data as-is.
				NewLineHandling = newLineHandling,	// Normalize line endings
				OmitXmlDeclaration = false			// The default, an xml declaration is written
			};
			return settings;
		}

		///<summary>
		///
		///</summary>
		///<returns></returns>
		public static XmlWriterSettings CreateXmlWriterSettings()
		{
			return CreateXmlWriterSettings(ConformanceLevel.Document);
		}
	}
}
