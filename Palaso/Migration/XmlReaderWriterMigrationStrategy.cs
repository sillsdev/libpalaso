using System.IO;
using System.Xml;

namespace Palaso.Migration
{
	///<summary>
	/// Copies an xml file, giving an opportunity to inspect each node in the CopyNode method.
	///</summary>
	public abstract class XmlReaderWriterMigrationStrategy : MigrationStrategyBase
	{
		protected XmlReaderWriterMigrationStrategy(int fromVersion, int toVersion) :
			base(fromVersion, toVersion)
		{
		}

		///<summary>
		/// Copies sourceFilePath to destinationFilePath calling CopyNode for each node read.
		///</summary>
		///<param name="source"></param>
		///<param name="destinationFilePath"></param>
		public override void Migrate(string source, string destinationFilePath)
		{
			using (var sourceStream = OpenSourceStream(source))
			{
				using (var destinationStream = new StreamWriter(destinationFilePath))
				{
					var reader = XmlReader.Create(sourceStream);
					var writer = XmlWriter.Create(destinationStream);
					while (reader.NodeType != XmlNodeType.Element)
					{
						reader.Read();
						reader.Read();
						reader.Read();
					}
					writer.WriteStartDocument();
					while (!reader.EOF)
					{
						CopyNode(reader, writer);
					}
					writer.WriteEndDocument();
					writer.Close();
					reader.Close();
				}
			}
		}

		protected StreamReader OpenSourceStream(string source)
		{
			return new StreamReader(source);
		}

		protected abstract void CopyNode(XmlReader reader, XmlWriter writer);
	}
}
