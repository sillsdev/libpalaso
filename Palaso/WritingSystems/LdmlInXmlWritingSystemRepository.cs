using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Palaso.Code;
using Palaso.WritingSystems;

namespace Palaso.WritingSystems
{
	public class LdmlInXmlWritingSystemRepository : WritingSystemRepositoryBase
	{
		/// <summary>
		/// Use the default repository
		/// </summary>
		public LdmlInXmlWritingSystemRepository() :
			base(WritingSystemCompatibility.Strict)
		{
		}

		public void SaveAllDefinitions(XmlWriter xmlWriter)
		{
			xmlWriter.WriteStartElement("writingsystems");
			foreach (WritingSystemDefinition ws in AllWritingSystems)
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
			Guard.AgainstNullOrEmptyString(filePath, "filePath");

			LoadAllDefinitions(XDocument.Load(filePath).Root.Element("writingsystems"));
		}

		public void LoadAllDefinitions(XElement writingsystemsElement)
		{
			// Check the current node, it should be 'writingsystems'
			if (writingsystemsElement.Name != "writingsystems")
				return;

			var adaptor = CreateLdmlAdaptor();
			foreach (var ldmlElement in writingsystemsElement.Elements("ldml"))
			{
				var ws = (WritingSystemDefinition)CreateNew();
				adaptor.Read(ldmlElement, ws);
				ws.StoreID = ws.Bcp47Tag;
				Set(ws);
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