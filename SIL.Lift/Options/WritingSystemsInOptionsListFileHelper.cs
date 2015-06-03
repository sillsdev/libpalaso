using System.Collections.Generic;
using System.Linq;
using System.Xml;
using SIL.WritingSystems;
using SIL.Xml;

namespace SIL.Lift.Options
{
	public class WritingSystemsInOptionsListFileHelper
	{
		private XmlDocument _xmlDoc;
		private readonly string _optionListFilePath;
		private readonly IWritingSystemRepository _writingSystemRepository;

		public WritingSystemsInOptionsListFileHelper(IWritingSystemRepository writingSystemRepository, string optionsListFilePath)
		{
			_writingSystemRepository = writingSystemRepository;
			_optionListFilePath = optionsListFilePath;
		}

		private XmlDocument XmlDoc
		{
			get
			{
				if(_xmlDoc == null)
				{
					_xmlDoc = new XmlDocument();
					var canonicalReader = XmlReader.Create(_optionListFilePath, CanonicalXmlSettings.CreateXmlReaderSettings());
					try
					{
						//_xmlDoc.PreserveWhitespace = true;
						_xmlDoc.Load(canonicalReader);
					}
					catch (XmlException)
					{
						//Do nothing... guess it wasn't an optionlist file
					}
					canonicalReader.Close();
				}
				return _xmlDoc;
			}
		}

		public IEnumerable<string> WritingSystemsInUse
		{
			get
			{
				var nodes = XmlDoc.SelectNodes("//form");
				if (nodes != null)
				{
					return
						nodes.Cast<XmlNode>().Where(node => node.Attributes != null && node.Attributes["lang"] != null).
							Select(node => node.Attributes["lang"].Value).Distinct();
				}
				return new List<string>();  //return an empty enumerable
			}
		}

		public void DeleteWritingSystemId(string id)
		{
			if (XmlDoc.SelectSingleNode("/optionsList") != null)
			{
				var formCollections =
					XmlDoc.SelectNodes("//option/name").Cast<XmlNode>().Concat(
						XmlDoc.SelectNodes("//option/abbreviation").Cast<XmlNode>()).Concat(
							XmlDoc.SelectNodes("//option/description").Cast<XmlNode>());
				foreach (XmlNode nameNode in formCollections)
				{
					var formNodes = nameNode.SelectNodes(".//form").Cast<XmlNode>();
					var formWithOldId = formNodes.SingleOrDefault(node => node.Attributes["lang"].Value == id);
					if (formWithOldId != null)
					{
						nameNode.RemoveChild(formWithOldId);
					}
				}
				var canonicalWriter = XmlWriter.Create(_optionListFilePath,
														CanonicalXmlSettings.CreateXmlWriterSettings());
				XmlDoc.Save(canonicalWriter);
				canonicalWriter.Close();
			}
		}

		public void ReplaceWritingSystemId(string oldId, string newId)
		{
			if (XmlDoc.SelectSingleNode("/optionsList") != null)
			{
				var formCollections =
					XmlDoc.SelectNodes("//option/name").Cast<XmlNode>().Concat(
						XmlDoc.SelectNodes("//option/abbreviation").Cast<XmlNode>()).Concat(
							XmlDoc.SelectNodes("//option/description").Cast<XmlNode>());
				foreach (XmlNode nameNode in formCollections)
				{
					var formNodes = nameNode.SelectNodes(".//form").Cast<XmlNode>();
					var formWithOldId = formNodes.SingleOrDefault(node => node.Attributes["lang"].Value == oldId);
					if(formWithOldId != null)
					{
						if(formNodes.Any(node=>node.Attributes["lang"].Value == newId))
						{
							nameNode.RemoveChild(formWithOldId);
						}
						else
						{
							formWithOldId.Attributes["lang"].Value = newId;
						}
					}
				}
				var canonicalWriter = XmlWriter.Create(_optionListFilePath,
														CanonicalXmlSettings.CreateXmlWriterSettings());
				XmlDoc.Save(canonicalWriter);
				canonicalWriter.Close();
			}
		}

		public void CreateNonExistentWritingSystemsFoundInFile()
		{
			WritingSystemOrphanFinder.FindOrphans(WritingSystemsInUse, ReplaceWritingSystemId, _writingSystemRepository);
		}
	}
}
