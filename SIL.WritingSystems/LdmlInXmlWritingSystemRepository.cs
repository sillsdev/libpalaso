using System;
using System.IO;
using System.Xml;
using System.Xml.XPath;

namespace SIL.WritingSystems
{
	/// <summary>
	/// A writing system repository where all LDML defintions are stored in a single XML file.
	/// </summary>
	public class LdmlInXmlWritingSystemRepository : WritingSystemRepositoryBase
	{
		/// <summary>
		/// Saves all writing system definitions.
		/// </summary>
		public void SaveAllDefinitions(XmlWriter xmlWriter)
		{
			xmlWriter.WriteStartElement("writingsystems");
			foreach (WritingSystemDefinition ws in AllWritingSystems)
			{
				ws.DateModified = DateTime.UtcNow;
				var ldmlDataMapper = new LdmlDataMapper();
				ldmlDataMapper.Write(xmlWriter, ws, null);
			}
			xmlWriter.WriteEndElement();
			//delete anything we're going to delete first, to prevent loosing
			//a WS we want by having it deleted by an old WS we don't want
			//(but which has the same identifier)
		}

		/// <summary>
		/// Loads all writing system definitions.
		/// </summary>
		public void LoadAllDefinitions(string filePath)
		{
			var xpDoc = new XPathDocument(new StreamReader(filePath));
			XPathNavigator xpNav = xpDoc.CreateNavigator();
			XPathNodeIterator nodes = xpNav.Select("//writingsystems/ldml");
			var ldmlDataMapper = new LdmlDataMapper();
			foreach (XPathNavigator nav in nodes)
			{
				WritingSystemDefinition ws = CreateNew();
				XmlReader xmlReader = nav.ReadSubtree();
				ldmlDataMapper.Read(xmlReader, ws);
				ws.StoreID = ws.ID;
				Set(ws);
			}
		}

		/// <summary>
		/// Loads all writing system definitions.
		/// </summary>
		public void LoadAllDefinitions(XmlReader xmlReader)
		{
			var ldmlDataMapper = new LdmlDataMapper();
			// Check the current node, it should be 'writingsystems'
			if ("writingsystems" == xmlReader.Name)
			{
				while (xmlReader.ReadToFollowing("ldml"))
				{
					WritingSystemDefinition ws = CreateNew();
					ldmlDataMapper.Read(xmlReader.ReadSubtree(), ws);
					ws.StoreID = ws.ID;
					Set(ws);
				}

			}
		}

		public override bool WritingSystemIDHasChanged(string id)
		{
			throw new NotImplementedException();
		}

		public override string WritingSystemIDHasChangedTo(string id)
		{
			throw new NotImplementedException();
		}
	}
}