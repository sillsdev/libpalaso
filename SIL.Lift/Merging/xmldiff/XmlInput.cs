/* From http://xmlunit.sourceforge.net/ Moved here because the original library is for testing, and
 * it is tied to nunit, which we don't want to ship in production
 */

using System.IO;
using System.Xml;

namespace SIL.Lift.Merging.xmldiff
{
	///<summary></summary>
	public class XmlInput {
		private delegate XmlReader XmlInputTranslator(object originalInput, string baseURI);
		private readonly string _baseURI;
		private readonly object _originalInput;
		private readonly XmlInputTranslator _translateInput;
		private static readonly string CURRENT_FOLDER = ".";

		private XmlInput(string baseURI, object someXml, XmlInputTranslator translator) {
			_baseURI = baseURI;
			 _originalInput = someXml;
			_translateInput = translator;
		}

		///<summary>
		/// Constructor.
		///</summary>
		public XmlInput(string someXml, string baseURI) :
			this(baseURI, someXml, TranslateString) {
		}

		///<summary>
		/// Constructor.
		///</summary>
		public XmlInput(string someXml) :
			this(someXml, CURRENT_FOLDER) {
		}

		private static XmlReader TranslateString(object originalInput, string baseURI) {
			return new XmlTextReader(baseURI, new StringReader((string) originalInput));
		}

		///<summary>
		/// Constructor.
		///</summary>
		public XmlInput(Stream someXml, string baseURI) :
			this(baseURI, someXml, TranslateStream) {
		}

		///<summary>
		/// Constructor.
		///</summary>
		public XmlInput(Stream someXml) :
			this(someXml, CURRENT_FOLDER) {
		}

		private static XmlReader TranslateStream(object originalInput, string baseURI) {
			return new XmlTextReader(baseURI, new StreamReader((Stream) originalInput));
		}

		///<summary>
		/// Constructor.
		///</summary>
		public XmlInput(TextReader someXml, string baseURI) :
			this(baseURI, someXml, TranslateReader) {
		}

		///<summary>
		/// Constructor.
		///</summary>
		public XmlInput(TextReader someXml) :
			this(someXml, CURRENT_FOLDER) {
		}

		private static XmlReader TranslateReader(object originalInput, string baseURI) {
			return new XmlTextReader(baseURI, (TextReader) originalInput);
		}

		///<summary>
		/// Constructor.
		///</summary>
		public XmlInput(XmlReader someXml) :
			this(null, someXml, NullTranslator) {
		}

		private static XmlReader NullTranslator(object originalInput, string baseURI) {
			return (XmlReader) originalInput;
		}

		///<summary></summary>
		public XmlReader CreateXmlReader() {
			return _translateInput(_originalInput, _baseURI);
		}

		///<summary></summary>
		public override bool Equals(object other)
		{
			if (other != null && other is XmlInput) {
				return _originalInput.Equals(((XmlInput)other)._originalInput);
			}
			return false;
		}

		///<summary></summary>
		public override int GetHashCode()
		{
			return _originalInput.GetHashCode();
		}
	}
}
