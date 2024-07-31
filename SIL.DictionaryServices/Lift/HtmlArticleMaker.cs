#if NO
This is here as a refugee from the purge of services from WeSay, but hasn't been
checked for use in Palaso.  We will revive it someday when there is call for it.

using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Xsl;

namespace Palaso.DictionaryServices.Lift
{
	/// <summary>
	/// Warning: This is a temporary thing (hopefully), which will be replaced by a multi-backend architecture some day
	/// </summary>
	public class HtmlArticleMaker
	{
		private readonly XslCompiledTransform _transformer;
		private readonly XsltArgumentList _transformArguments;

		public HtmlArticleMaker(string pathToWritingSystemPrefs, string pathToPartsOfSpeech)
		{
			_transformer = new XslCompiledTransform();

			using (Stream stream = GetXsltStream())
			{
				using (XmlReader xsltReader = XmlReader.Create(stream))
				{
					XsltSettings settings = new XsltSettings(true, true);
					_transformer.Load(xsltReader, settings, new XmlUrlResolver());
					xsltReader.Close();
				}
				stream.Close();
			}

			_transformArguments = new XsltArgumentList();
			_transformArguments.AddParam("writing-system-info-file",
										string.Empty,
										pathToWritingSystemPrefs);
			_transformArguments.AddParam("grammatical-info-optionslist-file",
										string.Empty,
										pathToPartsOfSpeech);
		}

		private static Stream GetXsltStream()
		{
			return
					Assembly.GetExecutingAssembly().GetManifestResourceStream(
							"WeSay.App.plift2html.xsl");
		}

		public string GetHtmlFragment(string entryXml)
		{
			using (StringReader entryXmlReader = new StringReader(entryXml))
			{
				using (XmlReader reader = XmlReader.Create(entryXmlReader))
				{
					StringBuilder builder = new StringBuilder();
					using (XmlWriter writer = XmlWriter.Create(builder)) // Don't forget to use CanonicalXmlSettings CP 2011-01
					{
						_transformer.Transform(reader, _transformArguments, writer);
						return builder.ToString();
					}
				}
			}
		}
	}
}
#endif