using System.IO;
using System.Xml;
using System.Xml.Xsl;

namespace Palaso.Migration
{
	///<summary>
	///</summary>
	public class XslMigrator : MigrationStrategyBase
	{
		protected string XslSource { get; private set; }

		///<summary>
		///</summary>
		///<param name="fromVersion"></param>
		///<param name="toVersion"></param>
		///<param name="xslSource">A string reference to the source of the xslt</param>
		public XslMigrator(int fromVersion, int toVersion, string xslSource) :
			base(fromVersion, toVersion)
		{
			XslSource = xslSource;
		}

		///<summary>
		///</summary>
		///<param name="source"></param>
		///<param name="destinationFilePath"></param>
		public override void Migrate(string source, string destinationFilePath)
		{
			MigrateUsingXslt(OpenXslStream(XslSource), OpenSourceStream(source), destinationFilePath);
		}

		protected virtual TextReader OpenXslStream(string xslSource)
		{
			return new StreamReader(xslSource);
		}

		protected virtual TextReader OpenSourceStream(string source)
		{
			return new StreamReader(source);
		}

		protected static void MigrateUsingXslt(TextReader xslStream, TextReader xmlStream, string destinationFilePath)
		{
			var transform = new XslCompiledTransform();
			using (xslStream)
			{
				using (xmlStream)
				{
					using (var destinationStream = new StreamWriter(destinationFilePath))
					{
						var xslReader = XmlReader.Create(xslStream);
						transform.Load(xslReader);
						xslReader.Close();
						xslStream.Close();

						var reader = XmlReader.Create(xmlStream);

						var settings = new XmlWriterSettings { Indent = true };
						var writer = XmlWriter.Create(destinationStream, settings);

						transform.Transform(reader, writer);

						var tempfiles = transform.TemporaryFiles;
						if (tempfiles != null) // tempfiles will be null when debugging is not enabled
						{
							tempfiles.Delete();
						}
						writer.Close();
						reader.Close();
						destinationStream.Close();
					}
					xmlStream.Close();
				}
			}
		}
	}
}
