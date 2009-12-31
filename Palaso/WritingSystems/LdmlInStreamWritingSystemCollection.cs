using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.XPath;

using Palaso.WritingSystems;

namespace Palaso.WritingSystems
{
	public class LdmlInXmlWritingSystemStore : WritingSystemStoreBase
	{
		/// <summary>
		/// Use the default repository
		/// </summary>
		public LdmlInXmlWritingSystemStore()
		{
		}

		public void SaveAllDefinitions(XmlWriter xmlWriter)
		{
			xmlWriter.WriteStartElement("writingsystems");
			foreach (WritingSystemDefinition ws in WritingSystemDefinitions)
			{
				LdmlAdaptor adaptor = new LdmlAdaptor();
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
			LdmlAdaptor adaptor = new LdmlAdaptor();
			foreach (XPathNavigator nav in nodes)
			{
				WritingSystemDefinition ws = new WritingSystemDefinition();
				XmlReader xmlReader = nav.ReadSubtree();
				adaptor.Read(xmlReader, ws);
				ws.StoreID = ws.RFC5646;
				Set(ws);
			}
		}

		public void LoadAllDefinitions(XmlReader xmlReader)
		{
			LdmlAdaptor adaptor = new LdmlAdaptor();
			// Check the current node, it should be 'writingsystems'
			if ("writingsystems" == xmlReader.Name)
			{
				while (xmlReader.ReadToFollowing("ldml"))
				{
					WritingSystemDefinition ws = new WritingSystemDefinition();
					adaptor.Read(xmlReader.ReadSubtree(), ws);
					ws.StoreID = ws.RFC5646;
					Set(ws);
				}

			}
		}


	}
}