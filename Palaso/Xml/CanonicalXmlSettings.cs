using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Palaso.Xml
{
	///<summary>
	/// Canonical xml settings suitable for use in Chorus applications.
	///</summary>
	public class CanonicalXmlSettings
	{
		///<summary>
		/// Return an XmlWriterSettings suitable for use in Chorus applications.
		///</summary>
		/// <remarks>
		/// This formats with new line on attributes, indents with tab, and encoded in UTF8 with no BOM.
		/// </remarks>
		/// <param name="conformanceLevel">Document|Fragment</param>
		///<returns>XmlWriterSettings</returns>
		public static XmlWriterSettings CreateXmlWriterSettings(ConformanceLevel conformanceLevel)
		{
			var settings = new XmlWriterSettings
							   {
								   NewLineOnAttributes = true,              // New line for each attribute, saves space on a typical Chorus changeset.
								   Indent = true,                           // Indent entities
								   IndentChars = "\t",                      // Tabs for the indent
								   CheckCharacters = false,
								   Encoding = new UTF8Encoding(false),      // UTF8 without a BOM.
								   CloseOutput = true,                      // Close the underlying stream on Close.  This is not the default.
								   ConformanceLevel = conformanceLevel,
								   NewLineChars = "\r\n",                   // Use /r/n for our end of lines
								   NewLineHandling = NewLineHandling.None,  // Assume that the input is as written /r/n
								   OmitXmlDeclaration = false               // The default, an xml declaration is written
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
