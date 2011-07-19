using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Palaso.WritingSystems;

namespace Palaso.Lift.Options
{
	public class WritingSystemsInOptionsListFileHelper
	{
		private readonly XmlDocument _xmlDoc = new XmlDocument();
		private readonly string _optionListFilePath;
		private readonly string _writingSystemFolderPath;

		public WritingSystemsInOptionsListFileHelper(string writingSystemFolderPath, string optionsListFilePath)
		{
			_writingSystemFolderPath = writingSystemFolderPath;
			_optionListFilePath = optionsListFilePath;
			try
			{
				_xmlDoc.Load(_optionListFilePath);
			}
			catch(Exception e)
			{
				//Do nothing... guess it wasn't an optionlist file
			}
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
					foreach (XmlNode node in _xmlDoc.SelectNodes("//form"))
					{
						if (node.Attributes["lang"].Value == oldId)
						{
							node.Attributes["lang"].Value = newId;
						}
					}
				}
				_xmlDoc.Save(_optionListFilePath);
			}
			catch (Exception e)
			{
				//Do nothing. If the load failed then it's not an optionlist.
			}
		}

		public void CreateNonExistentWritingSystemsFoundInFile()
		{
			var writingSystemRepository =
				new LdmlInFolderWritingSystemRepository(_writingSystemFolderPath);
			WritingSystemOrphanFinder.FindOrphans(WritingSystemsInUse, ReplaceWritingSystemId, writingSystemRepository);
		}
	}
}
