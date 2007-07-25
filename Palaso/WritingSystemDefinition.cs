using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Schema;

namespace Palaso
{
	public class WritingSystemDefinition : System.Xml.Serialization.IXmlSerializable
	{
		private const string _kExtension = ".ldml";
		private string _iso;
		private string _region;
		private string _variant;

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
			if (!String.IsNullOrEmpty(_oldFileName))
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

		#region IXmlSerializable Members

		///<summary>
		///This property is reserved, apply the <see cref="T:System.Xml.Serialization.XmlSchemaProviderAttribute"></see> to the class instead.
		///</summary>
		///
		///<returns>
		///An <see cref="T:System.Xml.Schema.XmlSchema"></see> that describes the XML representation of the object that is produced by the <see cref="M:System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter)"></see> method and consumed by the <see cref="M:System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader)"></see> method.
		///</returns>
		///
		public XmlSchema GetSchema()
		{
			throw new NotImplementedException();
		}

		///<summary>
		///Generates an object from its XML representation.
		///</summary>
		///
		///<param name="reader">The <see cref="T:System.Xml.XmlReader"></see> stream from which the object is deserialized. </param>
		public void ReadXml(XmlReader reader)
		{

		}
		#endregion
		///<summary>
		///Converts an object into its XML representation.
		///</summary>
		///
		///<param name="writer">The <see cref="T:System.Xml.XmlWriter"></see> stream to which the object is serialized. </param>
		public void WriteXml(XmlWriter writer)
		{
			writer.WriteStartElement("language");
			writer.WriteAttributeString("type", _iso);
			writer.WriteAttributeString("script", _script);
			writer.WriteAttributeString("territory", _region);
			writer.WriteAttributeString("variant", _variant);

			writer.WriteEndElement();
		}

		public void UpdateDOM(XmlDocument dom)
		{
			SetSubIdentityNode(dom, "language", _iso);
			SetSubIdentityNode(dom, "script", _script);
			SetSubIdentityNode(dom, "territory", _region);
			SetSubIdentityNode(dom, "variant", _variant);

			SetTopLevelSpecialNode(dom, "abbreviation", _abbreviation);
		}

		public void SetSubIdentityNode(XmlDocument dom, string field, string value)
		{
			XmlNode node = XmlHelpers.GetOrCreateElement(dom,"ldml/identity",field, null, _nameSpaceManager);
			Palaso.XmlHelpers.AddOrUpdateAttribute(node, "type", value);
		}

		public void SetTopLevelSpecialNode(XmlDocument dom, string field, string value)
		{
			XmlNode node = XmlHelpers.GetOrCreateElement(dom, "ldml", "special", null, _nameSpaceManager);
			node = XmlHelpers.GetOrCreateElement(dom, "ldml/special", field, "palaso", _nameSpaceManager);
			Palaso.XmlHelpers.AddOrUpdateAttribute(node, "value", value);
		}
	}
}
