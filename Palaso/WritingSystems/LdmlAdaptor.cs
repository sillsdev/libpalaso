using System;
using System.IO;
using System.Xml;

namespace Palaso
{
	public class LdmlAdaptor
	{
		private WritingSystemDefinition _ws;
		private const string _kExtension = ".ldml";
		private string _oldFileName;
		private XmlNamespaceManager _nameSpaceManager;

		public LdmlAdaptor(WritingSystemDefinition ws)
		{
			_nameSpaceManager = MakeNameSpaceManager();
			_ws = ws;
		}

		public void Load(WritingSystemRepository repository, string identifier)
		{
			XmlDocument doc = new XmlDocument();
			string path = Path.Combine(repository.PathToWritingSystems, identifier + _kExtension);
			if (File.Exists(path))
			{
				doc.Load(path);
			}
			_ws.ISO = GetIdentityValue(doc, "language");
			_ws.Variant = GetIdentityValue(doc, "variant");
			_ws.Region = GetIdentityValue(doc, "region");
			_ws.Script = GetIdentityValue(doc, "script");

			_ws.Abbreviation = GetSpecialValue(doc, "abbreviation");
			_ws.LanguageName = GetSpecialValue(doc, "languageName");
		}

		public string FileName
		{
			get
			{
				string name;
				if (String.IsNullOrEmpty(_ws.ISO))
				{
					name = "unknown";
				}
				else
				{
					name = _ws.ISO;
				}
				if (!String.IsNullOrEmpty(_ws.Script))
				{
					name += "-" + _ws.Script;
				}
				if (!String.IsNullOrEmpty(_ws.Region))
				{
					name += "-" + _ws.Region;
				}
				if (!String.IsNullOrEmpty(_ws.Variant))
				{
					name += "-" + _ws.Variant;
				}

				return name + _kExtension;
			}
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

		public void SaveToRepository(WritingSystemRepository repository)
		{
			XmlDocument doc = new XmlDocument();
			string savePath = Path.Combine(repository.PathToWritingSystems,FileName);
			string incomingPath;
			if (!String.IsNullOrEmpty(_oldFileName))
			{
				incomingPath = Path.Combine(repository.PathToWritingSystems, _oldFileName);
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
			UpdateDOM(doc);
			doc.Save(savePath);

			RemoveOldFileIfNeeded(repository);
			//save this so that if the user makes a name-changing change and saves again, we
			//can remove or rename to this version
			_oldFileName = FileName;
		}
		public static XmlNamespaceManager MakeNameSpaceManager()
		{
			XmlNamespaceManager m = new XmlNamespaceManager(new NameTable());
			 m.AddNamespace("palaso", "urn://palaso.org/ldmlExtensions/v1");
			return m;
		}
		private void RemoveOldFileIfNeeded(WritingSystemRepository repository)
		{
			if (!String.IsNullOrEmpty(_oldFileName) && _oldFileName != FileName)
			{
				string oldGuyPath = Path.Combine(repository.PathToWritingSystems, _oldFileName);
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

		public void UpdateDOM(XmlDocument dom)
		{
			SetSubIdentityNode(dom, "language", _ws.ISO);
			SetSubIdentityNode(dom, "script", _ws.Script);
			SetSubIdentityNode(dom, "territory", _ws.Region);
			SetSubIdentityNode(dom, "variant", _ws.Variant);

			SetTopLevelSpecialNode(dom, "languageName", _ws.LanguageName);
			SetTopLevelSpecialNode(dom, "abbreviation", _ws.Abbreviation);
		}

		public void SetSubIdentityNode(XmlDocument dom, string field, string value)
		{
		   if (!String.IsNullOrEmpty(value))
			{
			XmlNode node = XmlHelpers.GetOrCreateElement(dom,"ldml/identity",field, null, _nameSpaceManager);
			Palaso.XmlHelpers.AddOrUpdateAttribute(node, "type", value);
			}
			else
			{
				XmlHelpers.RemoveElement(dom, "ldml/identity/" + field, _nameSpaceManager);
			}
		}

		public void SetTopLevelSpecialNode(XmlDocument dom, string field, string value)
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
