using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using SIL.IO;
using SIL.WritingSystems;
using SIL.Xml;

namespace SIL.Lift
{
	public class WritingSystemsInLiftFileHelper
	{
		private readonly string _liftFilePath;
		private readonly IWritingSystemRepository _writingSystemRepository;

		public WritingSystemsInLiftFileHelper(IWritingSystemRepository writingSystemRepository, string liftFilePath)
		{
			_writingSystemRepository = writingSystemRepository;
			_liftFilePath = liftFilePath;
		}

		public IEnumerable<string> WritingSystemsInUse
		{
			get
			{
				var uniqueIds = new List<string>();
				using (var reader = XmlReader.Create(_liftFilePath))
				{
					while (reader.Read())
					{
						if (reader.MoveToAttribute("lang"))
						{
							if (!uniqueIds.Contains(reader.Value))
							{
								uniqueIds.Add(reader.Value);
							}
						}
					}
				}
				return uniqueIds;
			}
		}

		public void DeleteWritingSystemId(string id)
		{
			var fileToBeWrittenTo = new TempFile();
			var reader = XmlReader.Create(_liftFilePath, CanonicalXmlSettings.CreateXmlReaderSettings());
			var writer = XmlWriter.Create(fileToBeWrittenTo.Path, CanonicalXmlSettings.CreateXmlWriterSettings());
			//System.Diagnostics.Process.Start(fileToBeWrittenTo.Path);
			try
			{
				bool readerMovedByXmlDocument = false;
				while (readerMovedByXmlDocument || reader.Read())
				{
					readerMovedByXmlDocument = false;
					var xmldoc = new XmlDocument();
					if (reader.NodeType == XmlNodeType.Element && reader.Name == "entry")
					{
						var entryFragment = xmldoc.ReadNode(reader);
						readerMovedByXmlDocument = true;
						var nodesWithLangId = entryFragment.SelectNodes(String.Format("//*[@lang='{0}']", id));
						if (nodesWithLangId != null)
						{
							foreach (XmlNode node in nodesWithLangId)
							{
								var parent = node.SelectSingleNode("parent::*");
								if (node.Name == "gloss")
								{
									parent.RemoveChild(node);
								}
								else
								{
									var siblingNodes =
										node.SelectNodes("following-sibling::form | preceding-sibling::form");
									if (siblingNodes.Count == 0)
									{
										var grandParent = parent.SelectSingleNode("parent::*");
										grandParent.RemoveChild(parent);
									}
									else
									{
										parent.RemoveChild(node);
									}
								}
							}
						}
						entryFragment.WriteTo(writer);
					}
					else
					{
						writer.WriteNodeShallow(reader);
					}
					//writer.Flush();
				}
			}
			finally
			{
				reader.Close();
				writer.Close();
			}
			File.Delete(_liftFilePath);
			fileToBeWrittenTo.MoveTo(_liftFilePath);
		}

		public void ReplaceWritingSystemId(string oldId, string newId)
		{
			var fileToBeWrittenTo = new TempFile();
			var reader = XmlReader.Create(_liftFilePath, CanonicalXmlSettings.CreateXmlReaderSettings());
			var writer = XmlWriter.Create(fileToBeWrittenTo.Path, CanonicalXmlSettings.CreateXmlWriterSettings());
			//System.Diagnostics.Process.Start(fileToBeWrittenTo.Path);
			try
			{
				bool readerMovedByXmlDocument = false;
				while (readerMovedByXmlDocument || reader.Read())
				{
					readerMovedByXmlDocument = false;
					var xmldoc = new XmlDocument();
					//We load up the header and entry nodes individually as XmlDocuments and replace the writing systems
					//This is not as fast as pure reader writers, but as this is not a frequent operation, that is ok and
					//it is MUCH easier to code than a statemachine using readers and writers only.
					if (reader.NodeType == XmlNodeType.Element && reader.Name == "header")
					{
						var headerFragment = xmldoc.ReadNode(reader);
						readerMovedByXmlDocument = true;
						GetNodeWithWritingSystemIdsReplaced(oldId, newId, headerFragment);
						headerFragment.WriteTo(writer);
					}
					if (reader.NodeType == XmlNodeType.Element && reader.Name == "entry")
					{
						var entryFragment = xmldoc.ReadNode(reader);
						readerMovedByXmlDocument = true;
						GetNodeWithWritingSystemIdsReplaced(oldId, newId, entryFragment);
						entryFragment.WriteTo(writer);
					}
					else
					{
						writer.WriteNodeShallow(reader);
					}
					//writer.Flush();
				}
			}
			finally
			{
				reader.Close();
				writer.Close();
			}
			File.Delete(_liftFilePath);
			fileToBeWrittenTo.MoveTo(_liftFilePath);
		}

		private static void GetNodeWithWritingSystemIdsReplaced(string oldId, string newId, XmlNode entryFragment)
		{
			var nodesWithLangId = entryFragment.SelectNodes(String.Format("//*[@lang='{0}']", oldId));
			if (nodesWithLangId != null)
			{
				foreach (XmlNode node in nodesWithLangId)
				{
					node.Attributes["lang"].Value = newId;
					var textOfCurrentNode = node.SelectSingleNode("./text/text()") == null
												? ""
												: node.SelectSingleNode("./text/text()").Value;
					var xPathForSiblingsWithIdenticalLang =
						String.Format(
							"following-sibling::{0}[@lang='{1}'] | preceding-sibling::{0}[@lang='{1}']",
							node.Name, node.Attributes["lang"].Value);
					var siblingNodesWithNewId = node.SelectNodes(xPathForSiblingsWithIdenticalLang).Cast<XmlNode>();
					foreach (var identicalNode in siblingNodesWithNewId)
					{
						var textNode = identicalNode.SelectSingleNode("./text/text()");
						var textOfNodeWithSameLang = textNode == null ? "" : textNode.Value;
						if (textOfCurrentNode == textOfNodeWithSameLang)
						{
							var parent = identicalNode.SelectSingleNode("parent::*");
							parent.RemoveChild(identicalNode);
						}
					}
				}
			}
		}

		public void CreateNonExistentWritingSystemsFoundInFile()
		{
			WritingSystemOrphanFinder.FindOrphans(WritingSystemsInUse, ReplaceWritingSystemId, _writingSystemRepository);
		}
	}
}
