using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.XPath;

using Palaso.WritingSystems;

namespace Palaso.WritingSystems
{
	public class LdmlInXmlWritingSystemRepository : WritingSystemRepositoryBase
	{
		/// <summary>
		/// Use the default repository
		/// </summary>
		public LdmlInXmlWritingSystemRepository()
		{
		}

		public void SaveAllDefinitions(XmlWriter xmlWriter)
		{
			xmlWriter.WriteStartElement("writingsystems");
			foreach (WritingSystemDefinition ws in WritingSystemDefinitions)
			{
				LdmlDataMapper adaptor = CreateLdmlAdaptor();
				adaptor.Write(xmlWriter, ws, null);
			}
			xmlWriter.WriteEndElement();
			//delete anything we're going to delete first, to prevent loosing
			//a WS we want by having it deleted by an old WS we don't want
			//(but which has the same identifier)
		}

		public void LoadAllDefinitions(string filePath)
		{
			XPathDocument xpDoc = new XPathDocument(new StreamReader(filePath));
			XPathNavigator xpNav = xpDoc.CreateNavigator();
			XPathNodeIterator nodes = xpNav.Select("//writingsystems/ldml");
			LdmlDataMapper adaptor = CreateLdmlAdaptor();
			foreach (XPathNavigator nav in nodes)
			{
				WritingSystemDefinition ws = CreateNew();
				XmlReader xmlReader = nav.ReadSubtree();
				adaptor.Read(xmlReader, ws);
				ws.StoreID = ws.Bcp47Tag;
				Set(ws);
			}
		}

		public void LoadAllDefinitions(XmlReader xmlReader)
		{
			LdmlDataMapper adaptor = CreateLdmlAdaptor();
			// Check the current node, it should be 'writingsystems'
			if ("writingsystems" == xmlReader.Name)
			{
				while (xmlReader.ReadToFollowing("ldml"))
				{
					WritingSystemDefinition ws = CreateNew();
					adaptor.Read(xmlReader.ReadSubtree(), ws);
					ws.StoreID = ws.Bcp47Tag;
					Set(ws);
				}

			}
		}


		public override bool WritingSystemIdHasChanged(string id)
		{
			throw new NotImplementedException();
		}

		public override string WritingSystemIdHasChangedTo(string id)
		{
			throw new NotImplementedException();
		}
	}
}