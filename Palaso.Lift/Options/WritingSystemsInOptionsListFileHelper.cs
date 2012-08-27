using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Palaso.WritingSystems;
using Palaso.Xml;

namespace Palaso.Lift.Options
{
	public class WritingSystemsInOptionsListFileHelper
	{
		private readonly XmlDocument _xmlDoc = new XmlDocument();
		private readonly string _optionListFilePath;
		private readonly IWritingSystemRepository _writingSystemRepository;

		public WritingSystemsInOptionsListFileHelper(IWritingSystemRepository writingSystemRepository, string optionsListFilePath)
		{
			_writingSystemRepository = writingSystemRepository;
			_optionListFilePath = optionsListFilePath;
			var canonicalReader = XmlReader.Create(optionsListFilePath, CanonicalXmlSettings.CreateXmlReaderSettings());
			try
			{
				//_xmlDoc.PreserveWhitespace = true;
				_xmlDoc.Load(canonicalReader);
			}
			catch(Exception e)
			{
				//Do nothing... guess it wasn't an optionlist file
			}
			canonicalReader.Close();
		}

		public IEnumerable<string> WritingSystemsInUse
		{
			get
			{
				var nodes = _xmlDoc.SelectNodes("//form");
				return
					nodes.Cast<XmlNode>().Where(node => node.Attributes != null && node.Attributes["lang"] != null).
						Select(node => node.Attributes["lang"].Value).Distinct();
			}
		}

		public void ReplaceWritingSystemId(string oldId, string newId)
		{

			try
			{
				if (_xmlDoc.SelectSingleNode("/optionsList") != null)
				{
					var formCollections =
						_xmlDoc.SelectNodes("//option/name").Cast<XmlNode>().Concat(
							_xmlDoc.SelectNodes("//option/abbreviation").Cast<XmlNode>()).Concat(
								_xmlDoc.SelectNodes("//option/description").Cast<XmlNode>());
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
					_xmlDoc.Save(canonicalWriter);
					canonicalWriter.Close();
				}
			}
			catch (Exception e)
			{
				//Do nothing. If the load failed then it's not an optionlist.
			}
		}

		public void CreateNonExistentWritingSystemsFoundInFile()
		{
			WritingSystemOrphanFinder.FindOrphans(WritingSystemsInUse, ReplaceWritingSystemId, _writingSystemRepository);
		}
	}
}
