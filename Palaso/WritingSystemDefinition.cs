using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Schema;

namespace Palaso
{
	public class WritingSystemDefinition
	{
		private const string _kExtension = ".ldml";
		private string _iso;
		private string _region;
		private string _variant;
		private string _languageName;

		/// <summary>
		/// The file names we should try to delete when next we are saved,
		/// caused by a change in properties used to construct the name.
		/// </summary>
		//private List<string> _oldFileNames = new List<string>();

		private string _abbreviation;
		private string _script;
		private string _oldFileName;
		private XmlNamespaceManager _nameSpaceManager;

		public WritingSystemDefinition()
		{
			_nameSpaceManager = MakeNameSpaceManager();

		}

		public WritingSystemDefinition(WritingSystemRepository repository, string identifier):this()
		{
			XmlDocument doc = new XmlDocument();
			string path = Path.Combine(repository.PathToWritingSystems, identifier + _kExtension);
			if(File.Exists(path))
			{
				doc.Load(path);
			}
			_iso = GetIdentityValue(doc, "language");
			_variant = GetIdentityValue(doc, "variant");
			_region = GetIdentityValue(doc, "region");
			_script = GetIdentityValue(doc, "script");

			_abbreviation = GetSpecialValue(doc, "abbreviation");
			_languageName = GetSpecialValue(doc, "languageName");
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

		public static XmlNamespaceManager MakeNameSpaceManager()
		{
			XmlNamespaceManager m = new XmlNamespaceManager(new NameTable());
			 m.AddNamespace("palaso", "urn://palaso.org/ldmlExtensions/v1");
			return m;
		}

		public string Variant
		{
			get
			{
				return _variant;
			}
			set
			{
				if(_variant == value)
					return;
				RecordOldName();
				_variant = value;
			}
		}

		private void RecordOldName()
		{
			//_oldFileNames.Add(FileName);
		}

		public string Region
		{
			get
			{
				return _region;
			}
			set
			{
				if (_region == value)
					return;
				RecordOldName();
				_region = value;
			}
		}

		public string ISO
		{
			get
			{
				return _iso;
			}
			set
			{
				if (_iso == value)
					return;
				RecordOldName();
				_iso = value;
			}
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

		public string FileName
		{
			get
			{
				string name = "";
				if (String.IsNullOrEmpty(_iso))
				{
					name = "unknown";
				}
				else
				{
					name = _iso;
				}
				if (!String.IsNullOrEmpty(_script))
				{
					name += "-" + _script;
				}
				if (!String.IsNullOrEmpty(_region))
				{
					name += "-" + _region;
				}
				if (!String.IsNullOrEmpty(_variant))
				{
					name += "-" + _variant;
				}

				return name + _kExtension;
			}
		}

		public string Abbreviation
		{
			get
			{
				return _abbreviation;
			}
			set
			{
				if (_abbreviation == value)
					return;
				//no, abbreviation is not part of the name: RecordOldName();
				_abbreviation = value;
			}
		}

		public string Script
		{
			get
			{
				return _script;
			}
			set
			{
				if (_script == value)
					return;
				RecordOldName();
				_script = value;
			}
		}

		public void UpdateDOM(XmlDocument dom)
		{
			SetSubIdentityNode(dom, "language", _iso);
			SetSubIdentityNode(dom, "script", _script);
			SetSubIdentityNode(dom, "territory", _region);
			SetSubIdentityNode(dom, "variant", _variant);

			SetTopLevelSpecialNode(dom, "languageName", _languageName);
			SetTopLevelSpecialNode(dom, "abbreviation", _abbreviation);
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
