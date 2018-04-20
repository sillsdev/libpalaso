using System.IO;
using System.Xml;
using System.Xml.Xsl;

namespace SIL.Migration
{
	///<summary>
	///</summary>
	public class XslMigrationStrategy : MigrationStrategyBase
	{
		protected string XslSource { get; }

		///<summary>
		///</summary>
		///<param name="fromVersion"></param>
		///<param name="toVersion"></param>
		///<param name="xslSource">A string reference to the source of the xslt</param>
		public XslMigrationStrategy(int fromVersion, int toVersion, string xslSource) :
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
			using (TextReader xslStream = OpenXslStream(XslSource))
			using (TextReader xmlStream = OpenSourceStream(source))
			{
				MigrateUsingXslt(xslStream, xmlStream, destinationFilePath);
			}
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
			using (var xslReader = XmlReader.Create(xslStream))
			using (var reader = XmlReader.Create(xmlStream))
			using (var writer = XmlWriter.Create(destinationFilePath, new XmlWriterSettings { Indent = true }))
			{
				var transform = new XslCompiledTransform();
				transform.Load(xslReader);
				transform.Transform(reader, writer);
#if NET461
				transform.TemporaryFiles?.Delete();
#endif
			}
		}
	}
}
