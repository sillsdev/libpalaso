using System;
using System.IO;
using System.Xml;
using Palaso.WritingSystems;

namespace Palaso.WritingSystems
{
	public class LdmlAdaptor
	{
		private const string _kExtension = ".ldml";
		private XmlNamespaceManager _nameSpaceManager;

		internal LdmlAdaptor()
		{
			_nameSpaceManager = MakeNameSpaceManager();
		}

		internal void Load(LdmlInFolderWritingSystemRepository repository, string identifier, WritingSystemDefinition ws)
		{
			XmlDocument doc = new XmlDocument();
			string path = Path.Combine(repository.PathToWritingSystems, identifier + _kExtension);
			if (File.Exists(path))
			{
				doc.Load(path);
			}
			ws.ISO = GetIdentityValue(doc, "language");
			ws.Variant = GetIdentityValue(doc, "variant");
			ws.Region = GetIdentityValue(doc, "territory");
			ws.Script = GetIdentityValue(doc, "script");

			ws.Abbreviation = GetSpecialValue(doc, "abbreviation");
			ws.LanguageName = GetSpecialValue(doc, "languageName");
			ws.DefaultFontName = GetSpecialValue(doc, "defaultFontFamily");
			ws.PreviousRepositoryIdentifier = identifier;
			ws.Modified = false;
		}

		internal string GetFileName(WritingSystemDefinition ws)
		{
			return ws.RFC4646 + _kExtension;
		}

		private string GetSpecialValue(XmlDocument doc, string field)
		{
			XmlNode node = doc.SelectSingleNode("ldml/special/palaso:"+field, _nameSpaceManager);
			return XmlHelpers.GetOptionalAttributeValue(node, "value", string.Empty);
		}

		private string GetIdentityValue(XmlDocument doc, string field)
		{
			XmlNode node = doc.SelectSingleNode("ldml/identity/"+field);
			return XmlHelpers.GetOptionalAttributeValue(node, "type", string.Empty);
		}

		internal void SaveToRepository(LdmlInFolderWritingSystemRepository repository, WritingSystemDefinition ws)
		{
			XmlDocument doc = new XmlDocument();
			string savePath = Path.Combine(repository.PathToWritingSystems,GetFileName(ws));
			string incomingPath;
			if (!ws.Modified && File.Exists(savePath))
			{
				return; // no need to save (better to preserve the modified date)
			}
			if (!String.IsNullOrEmpty(ws.PreviousRepositoryIdentifier))
			{
				incomingPath = Path.Combine(repository.PathToWritingSystems, ws.PreviousRepositoryIdentifier);
			}
			else
			{
				incomingPath = savePath;
			}
			if (File.Exists(incomingPath))
			{
				doc.Load(incomingPath);
			}
			else
			{
				XmlHelpers.GetOrCreateElement(doc, ".", "ldml", null, _nameSpaceManager);
				XmlHelpers.GetOrCreateElement(doc, "ldml", "identity", null, _nameSpaceManager);
			}
			UpdateDOM(doc, ws);
			doc.Save(savePath);
			ws.Modified = false;

			RemoveOldFileIfNeeded(repository, ws);
			//save this so that if the user makes a name-changing change and saves again, we
			//can remove or rename to this version
			ws.PreviousRepositoryIdentifier = GetFileName(ws);
		}
		public static XmlNamespaceManager MakeNameSpaceManager()
		{
			XmlNamespaceManager m = new XmlNamespaceManager(new NameTable());
			m.AddNamespace("palaso", "urn://palaso.org/ldmlExtensions/v1");
			return m;
		}
		private void RemoveOldFileIfNeeded(LdmlInFolderWritingSystemRepository repository, WritingSystemDefinition ws)
		{
			if (!String.IsNullOrEmpty(ws.PreviousRepositoryIdentifier) && ws.PreviousRepositoryIdentifier != ws.RFC4646)
			{
				string oldGuyPath = Path.Combine(repository.PathToWritingSystems, ws.PreviousRepositoryIdentifier+_kExtension);
				if (File.Exists(oldGuyPath))
				{
					try
					{
						File.Delete(oldGuyPath);
					}
					catch (Exception )
					{
						//swallow. It's ok, we're just trying to clean up.
					}

				}
			}
		}

		private void UpdateDOM(XmlDocument dom, WritingSystemDefinition ws)
		{
			SetSubIdentityNode(dom, "language", ws.ISO);
			SetSubIdentityNode(dom, "script", ws.Script);
			SetSubIdentityNode(dom, "territory", ws.Region);
			SetSubIdentityNode(dom, "variant", ws.Variant);

			SetTopLevelSpecialNode(dom, "languageName", ws.LanguageName);
			SetTopLevelSpecialNode(dom, "abbreviation", ws.Abbreviation);
			SetTopLevelSpecialNode(dom, "defaultFontFamily", ws.DefaultFontName);
		}

		private void SetSubIdentityNode(XmlDocument dom, string field, string value)
		{
			if (!String.IsNullOrEmpty(value))
			{
				XmlNode node = XmlHelpers.GetOrCreateElement(dom, "ldml/identity", field, null, _nameSpaceManager);
				Palaso.XmlHelpers.AddOrUpdateAttribute(node, "type", value);
			}
			else
			{
				XmlHelpers.RemoveElement(dom, "ldml/identity/" + field, _nameSpaceManager);
			}
		}

		private void SetTopLevelSpecialNode(XmlDocument dom, string field, string value)
		{
			if (!String.IsNullOrEmpty(value))
			{
				XmlHelpers.GetOrCreateElement(dom, "ldml", "special", null, _nameSpaceManager);
				XmlNode node = XmlHelpers.GetOrCreateElement(dom, "ldml/special", field, "palaso", _nameSpaceManager);
				Palaso.XmlHelpers.AddOrUpdateAttribute(node, "value", value);
			}
			else
			{
				XmlHelpers.RemoveElement(dom, "ldml/special/palaso:" + field, _nameSpaceManager);
			}
		}
	}
}