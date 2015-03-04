using System;
using System.IO;
using System.Xml;
using System.Xml.XPath;

namespace SIL.WritingSystems
{
	/// <summary>
	/// A writing system repository where all LDML defintions are stored in a single XML file.
	/// </summary>
	public class LdmlInXmlWritingSystemRepository : LdmlInXmlWritingSystemRepository<WritingSystemDefinition>
	{
		protected override WritingSystemDefinition ConstructDefinition()
		{
			return new WritingSystemDefinition();
		}

		protected override WritingSystemDefinition ConstructDefinition(string ietfLanguageTag)
		{
			return new WritingSystemDefinition(ietfLanguageTag);
		}

		protected override WritingSystemDefinition CloneDefinition(WritingSystemDefinition ws)
		{
			return ws.Clone();
		}
	}

	/// <summary>
	/// A writing system repository where all LDML defintions are stored in a single XML file.
	/// </summary>
	public abstract class LdmlInXmlWritingSystemRepository<T> : WritingSystemRepositoryBase<T> where T : WritingSystemDefinition
	{
		/// <summary>
		/// Saves all writing system definitions.
		/// </summary>
		public void SaveAllDefinitions(XmlWriter xmlWriter)
		{
			xmlWriter.WriteStartElement("writingsystems");
			foreach (T ws in AllWritingSystems)
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
				T ws = ConstructDefinition();
				XmlReader xmlReader = nav.ReadSubtree();
				ldmlDataMapper.Read(xmlReader, ws);
				ws.Id = ws.IetfLanguageTag;
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
					T ws = ConstructDefinition();
					ldmlDataMapper.Read(xmlReader.ReadSubtree(), ws);
					ws.Id = ws.IetfLanguageTag;
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