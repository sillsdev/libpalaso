using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Diagnostics;
using SIL.Archiving.Generic;
using SIL.Archiving.Generic.AccessProtocol;
using SIL.Archiving.IMDI.Lists;
using SIL.Extensions;

namespace SIL.Archiving.IMDI.Schema
{
	/// <summary>Shared properties and methods</summary>
	public interface IIMDIMajorObject
	{
		/// <summary>Name of object</summary>
		string Name { get; set; }

		/// <summary>Title of object</summary>
		string Title { get; set; }

		/// <remarks/>
		DescriptionTypeCollection Description { get; set; }
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlType(Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	[XmlRootAttribute("METATRANSCRIPT", Namespace="http://www.mpi.nl/IMDI/Schema/IMDI", IsNullable=false)]
	public class MetaTranscript
	{
		/// <remarks/>
		public MetaTranscript()
		{
			SchemaLocation = "http://www.mpi.nl/IMDI/Schema/IMDI http://www.mpi.nl/IMDI/Schema/IMDI_3.0.xsd";
			Date = DateTime.Today;
			Originator = "Palaso IMDI 0.0.1";
			Version = "0";
			FormatId = "IMDI 3.03";
		}

		/// <remarks/>
		public MetaTranscript(MetatranscriptValueType type)
		{
			SchemaLocation = "http://www.mpi.nl/IMDI/Schema/IMDI http://www.mpi.nl/IMDI/Schema/IMDI_3.0.xsd";
			Date = DateTime.Today;
			Originator = "Palaso IMDI 0.0.1";
			Version = "0";
			FormatId = "IMDI 3.03";
			Type = type;
			switch (type)
			{
				case MetatranscriptValueType.CORPUS:
					Items = new object[] { new Corpus() };
					break;
				case MetatranscriptValueType.SESSION:
					Items = new object[] { new Session() };
					break;
				case MetatranscriptValueType.CATALOGUE:
					Items = new object[] { new Catalogue() };
					break;
			}
		}

		/// <remarks/>
		[XmlAttribute("schemaLocation", Namespace = XmlSchema.InstanceNamespace)]
		public string SchemaLocation { get; set; }

		/// <remarks/>
		[XmlElementAttribute("Catalogue", typeof(Catalogue))]
		[XmlElementAttribute("Corpus", typeof(Corpus))]
		[XmlElementAttribute("Session", typeof(Session))]
		public object[] Items { get; set; }

		/// <remarks/>
		[XmlAttribute(DataType="date")]
		public DateTime Date { get; set; }

		/// <remarks/>
		[XmlAttribute]
		public string Originator { get; set; }

		/// <remarks/>
		[XmlAttribute]
		public string Version { get; set; }

		/// <remarks/>
		[XmlAttribute]
		public string FormatId { get; set; }

		/// <remarks/>
		[XmlAttributeAttribute]
		public MetatranscriptValueType Type { get; set; }

		/// <summary>
		/// Gets the XML representation of this object
		/// </summary>
		public override string ToString()
		{
			using (var memStream = new MemoryStream())
			{
				using (var strWriter = new StreamWriter(memStream, new UTF8Encoding()))
				{
					var settings = new XmlWriterSettings
					{
						Indent = true,
						IndentChars = "\t",
						CheckCharacters = true,
						Encoding = Encoding.UTF8
					};

					using (var xmlWriter = XmlWriter.Create(strWriter, settings))
					{
						var serializer = new XmlSerializer(GetType());
						serializer.Serialize(xmlWriter, this);

						//return strWriter.ToString();
						return Encoding.UTF8.GetString(memStream.ToArray());
					}
				}
			}

		}

		/// <summary>Writes the IMDI file for this object</summary>
		/// <param name="outputDirectoryName"></param>
		/// <param name="corpusName"></param>
		/// <returns>IMDI file name</returns>
		public string WriteImdiFile(string outputDirectoryName, string corpusName)
		{
			string corpusDirectoryName = IMDIArchivingDlgViewModel.NormalizeDirectoryName(corpusName);

			switch (Type)
			{
				case MetatranscriptValueType.CORPUS:
					return WriteCorpusImdiFile(outputDirectoryName, corpusDirectoryName);

				case MetatranscriptValueType.CATALOGUE:
					return WriteCatalogueImdiFile(outputDirectoryName, corpusDirectoryName);

				case MetatranscriptValueType.SESSION:
					return WriteSessionImdiFile(outputDirectoryName, corpusDirectoryName);
			}

			return null;
		}

		private string WriteCorpusImdiFile(string outputDirectoryName, string corpusDirectoryName)
		{
			// create the corpus directory
			Directory.CreateDirectory(Path.Combine(outputDirectoryName, corpusDirectoryName));

			// check required fields
			ArbilCheckCorpus((Corpus)Items[0]);

			// file name
			var imdiFile = Path.Combine(outputDirectoryName, corpusDirectoryName + ".imdi");

			TextWriter writer = new StreamWriter(imdiFile);
			writer.Write(ToString());
			writer.Close();

			return imdiFile;
		}

		private string WriteCatalogueImdiFile(string outputDirectoryName, string corpusDirectoryName)
		{
			// create the corpus directory
			Directory.CreateDirectory(Path.Combine(outputDirectoryName, corpusDirectoryName));

			// check required fields
			ArbilCheckCatalogue((Catalogue)Items[0]);

			// file name
			var imdiFile = Path.Combine(outputDirectoryName, corpusDirectoryName, corpusDirectoryName + "_Catalogue.imdi");

			TextWriter writer = new StreamWriter(imdiFile);
			writer.Write(ToString());
			writer.Close();

			return Path.Combine(corpusDirectoryName, corpusDirectoryName + "_Catalogue.imdi");
		}

		private string WriteSessionImdiFile(string outputDirectoryName, string corpusDirectoryName)
		{
			// session object
			Session s = (Session)Items[0];

			// check required fields
			ArbilCheckSession(s);

			// set access codes
			SetFileAccessCode(s);

			// normalize session name
			var sessionDirectoryName = IMDIArchivingDlgViewModel.NormalizeDirectoryName(s.Name);

			// create the session directory
			Directory.CreateDirectory(Path.Combine(outputDirectoryName, corpusDirectoryName, sessionDirectoryName));

			var imdiFile = Path.Combine(outputDirectoryName, corpusDirectoryName, sessionDirectoryName + ".imdi");

			TextWriter writer = new StreamWriter(imdiFile);
			writer.Write(ToString());
			writer.Close();

			return Path.Combine(corpusDirectoryName, sessionDirectoryName + ".imdi");
		}

		private void ArbilCheckCorpus(Corpus corpus)
		{
			if (corpus.Description.Count == 0)
				corpus.Description.Add(new LanguageString());
		}

		private void ArbilCheckCatalogue(Catalogue catalogue)
		{
			if (catalogue.Id.Count == 0)
				catalogue.Id.Add("");

			if (catalogue.Description.Count == 0)
				catalogue.Description.Add(new LanguageString());

			if (catalogue.Location.Count == 0)
				catalogue.Location.Add(new LocationType());

			if (catalogue.ContentType.Count == 0)
				catalogue.ContentType.Add(string.Empty);

			if (catalogue.Project.Count == 0)
				catalogue.Project.Add(new Project());

			if (catalogue.Publisher.Count == 0)
				catalogue.Publisher.Add(string.Empty);

			if (catalogue.Author.Count == 0)
				catalogue.Author.Add(new CommaSeparatedStringType());
		}

		private void ArbilCheckSession(Session session)
		{
			if (session.Date == null)
				session.SetDate(DateTime.Today);

			if (session.MDGroup.Project.Count == 0)
				session.MDGroup.Project.Add(new Project());

			session.MDGroup.Content.CheckRequiredFields();

			foreach (var actor in session.MDGroup.Actors.Actor)
			{
				if (actor.Role == null)
					actor.Role = string.Empty.ToVocabularyType(false, ListType.Link(ListType.ActorRole));

				if (actor.FamilySocialRole == null)
					actor.FamilySocialRole = string.Empty.ToVocabularyType(false, ListType.Link(ListType.ActorFamilySocialRole));

				if (actor.Anonymized == null)
					actor.Anonymized = new BooleanType { Link = ListType.Link(ListType.Boolean) };

				if (actor.Sex == null)
					actor.Sex = new VocabularyType { Type = VocabularyTypeValueType.ClosedVocabulary, Link = ListType.Link(ListType.ActorSex) };
			}

			foreach (var file in session.Resources.WrittenResource)
			{
				if (file.SubType == null)
					file.SubType = new VocabularyType { Link = ListType.Link(ListType.WrittenResourceSubType) };

				if (file.Validation == null)
					file.Validation = new ValidationType
					{
						Type = new VocabularyType{ Type = VocabularyTypeValueType.ClosedVocabulary, Link = ListType.Link(ListType.ValidationType) },
						Methodology = new VocabularyType { Type = VocabularyTypeValueType.ClosedVocabulary, Link = ListType.Link(ListType.ValidationMethodology) },
						Level = new IntegerType { Value = "Unspecified" },
						Description = new DescriptionTypeCollection { new LanguageString() }
					};

				if (file.Derivation == null)
					file.Derivation = new VocabularyType { Type = VocabularyTypeValueType.ClosedVocabulary, Link = ListType.Link(ListType.WrittenResourceDerivation) };

				if (file.Anonymized == null)
					file.Anonymized = new BooleanType { Link = ListType.Link(ListType.Boolean) };

				if (file.Access == null)
					file.Access = new AccessType();
			}

			foreach (var file in session.Resources.MediaFile)
			{
				if (file.Access == null)
					file.Access = new AccessType();
			}
		}

		/// <summary>Set the access code on session files if not set already.</summary>
		private void SetFileAccessCode(Session session)
		{
			if (string.IsNullOrEmpty(session.AccessCode)) return;

			foreach (var file in session.Resources.MediaFile)
			{
				if (file.Access == null)
					file.Access = new AccessType();

				if (string.IsNullOrEmpty(file.Access.Availability))
					file.Access.Availability = session.AccessCode;
			}

			foreach (var file in session.Resources.WrittenResource)
			{
				if (file.Access == null)
					file.Access = new AccessType();

				if (string.IsNullOrEmpty(file.Access.Availability))
					file.Access.Availability = session.AccessCode;
			}
		}
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class CommaSeparatedStringType
	{
		/// <remarks/>
		[XmlText]
		public string Value { get; set; }
	}

	/// <remarks/>
	[XmlIncludeAttribute(typeof(SubjectLanguageType))]
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class SimpleLanguageType
	{
		/// <remarks/>
		public string Id { get; set; }

		/// <remarks/>
		public LanguageNameType Name { get; set; }
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class LanguageNameType : VocabularyType
	{
	}

	/// <remarks/>
	[XmlIncludeAttribute(typeof(LanguageNameType))]
	[XmlIncludeAttribute(typeof(KeyType))]
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class VocabularyType
	{
		/// <remarks/>
		public VocabularyType() {
			Type = VocabularyTypeValueType.OpenVocabulary;
		}

		/// <remarks/>
		[XmlAttribute]
		public VocabularyTypeValueType Type { get; set; }

		/// <remarks/>
		[XmlAttribute(DataType="anyURI")]
		public string DefaultLink { get; set; }

		/// <remarks/>
		[XmlAttribute(DataType="anyURI")]
		public string Link { get; set; }

		/// <remarks/>
		[XmlText]
		public string Value { get; set; }
	}

	/// <remarks/>
	[SerializableAttribute]
	[XmlTypeAttribute(Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public enum VocabularyTypeValueType
	{
		/// <remarks/>
		ClosedVocabulary,

		/// <remarks/>
		ClosedVocabularyList,

		/// <remarks/>
		OpenVocabulary,

		/// <remarks/>
		OpenVocabularyList,
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class KeyType
	{
		/// <remarks/>
		[XmlAttribute]
		public string Name { get; set; }

		/// <remarks/>
		[XmlText]
		public string Value { get; set; }
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class SubjectLanguageType : SimpleLanguageType
	{
		private DescriptionTypeCollection _descriptionField;

		/// <remarks/>
		public BooleanType Dominant { get; set; }

		/// <remarks/>
		public BooleanType SourceLanguage { get; set; }

		/// <remarks/>
		public BooleanType TargetLanguage { get; set; }

		/// <remarks/>
		[XmlElementAttribute("Description")]
		public DescriptionTypeCollection Description
		{
			get { return _descriptionField ?? (_descriptionField = new DescriptionTypeCollection()); }
			set { _descriptionField = value; }
		}
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class BooleanType
	{
		/// <remarks/>
		public BooleanType() {
			Type = VocabularyTypeValueType.ClosedVocabulary;
		}

		/// <remarks/>
		[XmlAttribute]
		public VocabularyTypeValueType Type { get; set; }

		/// <remarks/>
		[XmlAttribute(DataType="anyURI")]
		public string DefaultLink { get; set; }

		/// <remarks/>
		[XmlAttribute(DataType="anyURI")]
		public string Link { get; set; }

		/// <remarks/>
		[XmlText]
		public BooleanEnum Value { get; set; }
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public partial class DescriptionType
	{
		/// <remarks/>
		[XmlAttribute(DataType="token")]
		public string LanguageId { get; set; }

		/// <remarks/>
		[XmlAttribute]
		public string Name { get; set; }

		/// <remarks/>
		[XmlAttribute]
		public string ArchiveHandle { get; set; }

		/// <remarks/>
		[XmlAttribute(DataType="anyURI")]
		public string Link { get; set; }

		/// <remarks/>
		[XmlText]
		public string Value { get; set; }
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class Catalogue : IIMDIMajorObject
	{
		/// <remarks/>
		public Catalogue()
		{
			// initialization for Arbil
			DocumentLanguages = new CatalogueDocumentLanguages();
			SubjectLanguages = new CatalogueSubjectLanguages();
			Id = new List<string>();
			Description = new DescriptionTypeCollection();
			Location = new List<LocationType>();
			ContentType = new List<string>();
			SmallestAnnotationUnit = string.Empty;
			Applications = string.Empty;
			Format = new CatalogueFormat();
			Quality = new CatalogueQuality();
			Project = new List<Project>();
			Publisher = new List<string>();
			Author = new List<CommaSeparatedStringType>();
			Size = string.Empty;
			DistributionForm = string.Empty;
			Access = new AccessType();
			Pricing = string.Empty;
			Keys = new KeysType();
		}

		/// <summary>Name of object</summary>
		[XmlElement("Name")]
		public string Name { get; set; }

		/// <summary>Title of object</summary>
		[XmlElement("Title")]
		public string Title { get; set; }

		/// <remarks/>
		[XmlElement("Id")]
		public List<string> Id { get; set; }

		/// <remarks/>
		[XmlElement("Description")]
		public DescriptionTypeCollection Description { get; set; }

		/// <remarks/>
		public CatalogueDocumentLanguages DocumentLanguages { get; set; }

		/// <remarks/>
		public CatalogueSubjectLanguages SubjectLanguages { get; set; }

		/// <remarks/>
		[XmlElement("Location")]
		public List<LocationType> Location { get; set; }

		/// <remarks/>
		[XmlElement("ContentType")]
		public List<string> ContentType { get; set; }

		/// <remarks/>
		public CatalogueFormat Format { get; set; }

		/// <remarks/>
		public CatalogueQuality Quality { get; set; }

		/// <remarks/>
		public string SmallestAnnotationUnit { get; set; }

		/// <remarks/>
		public string Applications { get; set; }

		/// <remarks/>
		public string Date { get; set; }

		/// <remarks/>
		[XmlElement("Project")]
		public List<Project> Project { get; set; }

		/// <remarks/>
		[XmlElement("Publisher")]
		public List<string> Publisher { get; set; }

		/// <remarks/>
		[XmlElement("Author")]
		public List<CommaSeparatedStringType> Author { get; set; }

		/// <remarks/>
		public string Size { get; set; }

		/// <remarks/>
		public String DistributionForm { get; set; }

		/// <remarks/>
		public AccessType Access { get; set; }

		/// <remarks/>
		public string Pricing { get; set; }

		/// <remarks/>
		public string ContactPerson { get; set; }

		/// <remarks/>
		public string ReferenceLink { get; set; }

		/// <remarks/>
		public string MetadataLink { get; set; }

		/// <remarks/>
		public string Publications { get; set; }

		/// <remarks/>
		public KeysType Keys { get; set; }
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(AnonymousType=true, Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class CatalogueDocumentLanguages : IMDIDescription
	{
		/// <remarks/>
		[XmlElement("Language")]
		public List<SimpleLanguageType> Language { get; set; }

		/// <remarks/>
		public CatalogueDocumentLanguages()
		{
			Language = new List<SimpleLanguageType>();
		}
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(AnonymousType=true, Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class CatalogueSubjectLanguages : IMDIDescription
	{
		/// <remarks/>
		[XmlElement("Language")]
		public List<SubjectLanguageType> Language { get; set; }

		/// <remarks/>
		public CatalogueSubjectLanguages()
		{
			Language = new List<SubjectLanguageType>();
		}
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class LocationType
	{
		/// <remarks/>
		public LocationType()
		{
			Continent = new VocabularyType { Type = VocabularyTypeValueType.ClosedVocabulary, Link = ListType.Link(ListType.Continents) };
			Country = new VocabularyType { Link = ListType.Link(ListType.Countries) };
			Region = new List<string>();
		}

		/// <remarks/>
		public LocationType(ArchivingLocation location) : this()
		{
			if (!string.IsNullOrEmpty(location.Address))
				Address = location.Address;

			if (!string.IsNullOrEmpty(location.Region))
				Region.Add(location.Region);

			if (!string.IsNullOrEmpty(location.Country))
				Country.Value = location.Country;

			if (!string.IsNullOrEmpty(location.Continent))
				Continent.Value = location.Continent;
		}

		/// <remarks/>
		public VocabularyType Continent { get; set; }

		/// <remarks>Closed vocabulary</remarks>
		public void SetContinent(string continent)
		{
			var continentList = ListConstructor.GetClosedList(ListType.Continents, false, ListConstructor.RemoveUnknown.RemoveNone);
			Continent = continentList.FindByValue(continent).ToVocabularyType(VocabularyTypeValueType.ClosedVocabulary, ListType.Link(ListType.Continents));
		}

		/// <remarks>Open vocabulary</remarks>
		public VocabularyType Country { get; set; }

		/// <remarks/>
		public void SetCountry(string country)
		{
			Country = IMDISchemaHelper.SetVocabulary(country, false, ListType.Link(ListType.Countries));
		}

		/// <remarks/>
		[XmlElementAttribute("Region")]
		public List<string> Region { get; set; }

		/// <remarks/>
		public string Address { get; set; }

		/// <remarks/>
		public ArchivingLocation ToArchivingLocation()
		{
			ArchivingLocation loc = new ArchivingLocation();
			if (Continent != null)
				loc.Continent = Continent.Value;

			if (Country != null)
				loc.Country = Country.Value;

			if (Region.Count > 0)
				loc.Region = Region[0];

			loc.Address = Address;

			return loc;
		}
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(AnonymousType=true, Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class CatalogueFormat
	{
		/// <remarks/>
		public VocabularyType Text { get; set; }

		/// <remarks/>
		public VocabularyType Audio { get; set; }

		/// <remarks/>
		public VocabularyType Video { get; set; }

		/// <remarks/>
		public VocabularyType Image { get; set; }
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(AnonymousType=true, Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class CatalogueQuality
	{
		/// <remarks/>
		public string Text { get; set; }

		/// <remarks/>
		public string Audio { get; set; }

		/// <remarks/>
		public string Video { get; set; }

		/// <remarks/>
		public string Image { get; set; }
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class Project : IIMDIMajorObject
	{
		/// <remarks/>
		public Project()
		{
			Name = string.Empty;
			Title = string.Empty;
			Id = new List<string> {string.Empty};
			Description = new DescriptionTypeCollection();
			Contact = new ContactType();
		}

		/// <remarks/>
		public Project(ArchivingProject project)
		{
			Name = project.Name;
			Title = project.Title;
			Id = new List<string> { project.Name };
			Description = new DescriptionTypeCollection();
			Contact = new ContactType();
			if (!string.IsNullOrEmpty(project.Author))
				Contact.Name = project.Author;
		}

		/// <summary>Name of object</summary>
		[XmlElement("Name")]
		public string Name { get; set; }

		/// <summary>Title of object</summary>
		[XmlElement("Title")]
		public string Title { get; set; }

		/// <remarks/>
		[XmlElement("Id")]
		public List<string> Id { get; set; }

		/// <remarks/>
		[XmlElement("Description")]
		public DescriptionTypeCollection Description { get; set; }

		/// <remarks/>
		public ContactType Contact { get; set; }
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class ContactType
	{
		/// <remarks/>
		public string Name { get; set; }

		/// <remarks/>
		public string Address { get; set; }

		/// <remarks/>
		public string Email { get; set; }

		/// <remarks/>
		public string Organisation { get; set; }
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class AccessType : IMDIDescription
	{
		/// <remarks/>
		public AccessType()
		{
			// initialize for Arbil
			Availability = string.Empty;
			Date = string.Empty;
			Owner = string.Empty;
			Publisher = string.Empty;
			Contact = new ContactType();
		}

		/// <remarks/>
		public string Availability { get; set; }

		/// <remarks/>
		public string Date { get; set; }

		/// <remarks/>
		public string Owner { get; set; }

		/// <remarks/>
		public string Publisher { get; set; }

		/// <remarks/>
		public ContactType Contact { get; set; }
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class KeysType
	{
		private List<KeyType> _keyField;

		/// <remarks/>
		[XmlElement("Key")]
		public List<KeyType> Key
		{
			get { return _keyField ?? (_keyField = new List<KeyType>()); }
			set { _keyField = value; }
		}
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class Corpus : IIMDIMajorObject
	{
		/// <remarks/>
		public Corpus()
		{
			Description = new DescriptionTypeCollection();
			CorpusLink = new List<CorpusLinkType>();
		}

		/// <summary>Name of object</summary>
		[XmlElement("Name")]
		public string Name { get; set; }

		/// <summary>Title of object</summary>
		[XmlElement("Title")]
		public string Title { get; set; }

		/// <remarks/>
		[XmlElement("Description")]
		public DescriptionTypeCollection Description { get; set; }

		/// <remarks/>
		[XmlElement("CorpusLink")]
		public List<CorpusLinkType> CorpusLink { get; set; }

		/// <remarks/>
		[XmlAttribute(DataType="anyURI")]
		public string SearchService { get; set; }

		/// <remarks/>
		[XmlAttribute(DataType="anyURI")]
		public string CorpusStructureService { get; set; }

		/// <remarks/>
		[XmlAttribute(DataType="anyURI")]
		public string CatalogueLink { get; set; }

		/// <remarks/>
		[XmlAttribute]
		public string CatalogueHandle { get; set; }
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class MDGroupType
	{
		/// <remarks/>
		public MDGroupType()
		{
			Location = new LocationType();
			Project = new List<Project>();
			Keys = new KeysType();
			Content = new ContentType();
			Actors = new ActorsType();
		}

		/// <remarks/>
		public LocationType Location { get; set; }

		/// <remarks/>
		[XmlElement("Project")]
		public List<Project> Project { get; set; }

		/// <remarks/>
		public KeysType Keys { get; set; }

		/// <remarks/>
		public ContentType Content { get; set; }

		/// <remarks/>
		public ActorsType Actors { get; set; }
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class ContentType
	{
		/// <remarks/>
		public ContentType()
		{
			CommunicationContext = new ContentTypeCommunicationContext();
			Languages = new LanguagesType();
			Keys = new KeysType();
			Description = new DescriptionTypeCollection();
		}

		/// <remarks>Open vocabulary, Content-Genre.xml</remarks>
		public VocabularyType Genre { get; set; }

		/// <remarks>Open vocabulary, Content-SubGenre.xml</remarks>
		public VocabularyType SubGenre { get; set; }

		/// <remarks>Open vocabulary, Content-Task.xml</remarks>
		public VocabularyType Task { get; set; }

		/// <remarks>Open vocabulary, Content-Modalities.xml</remarks>
		public VocabularyType Modalities { get; set; }

		/// <remarks>Open vocabulary, Content-Subject.xml</remarks>
		public ContentTypeSubject Subject { get; set; }

		/// <remarks/>
		public ContentTypeCommunicationContext CommunicationContext { get; set; }

		/// <remarks/>
		[XmlElement("Languages")]
		public LanguagesType Languages { get; set; }

		/// <remarks/>
		public KeysType Keys { get; set; }

		/// <remarks/>
		[XmlElement("Description")]
		public DescriptionTypeCollection Description { get; set; }

		/// <summary>Adds a language, setting some attributes also</summary>
		public void AddLanguage(ArchivingLanguage language, BooleanEnum dominantLanguage, BooleanEnum sourceLanguage, BooleanEnum targetLanguage)
		{
			var imdiLanguage = LanguageList.Find(language).ToLanguageType();
			if (imdiLanguage == null) return;

			imdiLanguage.Dominant = new BooleanType { Value = dominantLanguage };
			imdiLanguage.SourceLanguage = new BooleanType { Value = sourceLanguage };
			imdiLanguage.TargetLanguage = new BooleanType { Value = targetLanguage };
			Languages.Language.Add(imdiLanguage);
		}

		/// <remarks/>
		public void SetInteractivity(string interactivity)
		{
			IMDIItemList list = ListConstructor.GetClosedList(ListType.ContentInteractivity);
			CommunicationContext.Interactivity = list.FindByValue(interactivity).ToVocabularyType(VocabularyTypeValueType.ClosedVocabulary, ListType.Link(ListType.ContentInteractivity));
		}

		/// <remarks/>
		public void SetPlanningType(string planningType)
		{
			IMDIItemList list = ListConstructor.GetClosedList(ListType.ContentPlanningType);
			CommunicationContext.PlanningType = list.FindByValue(planningType).ToVocabularyType(VocabularyTypeValueType.ClosedVocabulary, ListType.Link(ListType.ContentPlanningType));
		}

		/// <remarks/>
		public void SetInvolvement(string involvement)
		{
			IMDIItemList list = ListConstructor.GetClosedList(ListType.ContentInvolvement);
			CommunicationContext.Involvement = list.FindByValue(involvement).ToVocabularyType(VocabularyTypeValueType.ClosedVocabulary, ListType.Link(ListType.ContentInvolvement));
		}

		/// <remarks/>
		public void SetSocialContext(string socialContext)
		{
			IMDIItemList list = ListConstructor.GetClosedList(ListType.ContentSocialContext);
			CommunicationContext.SocialContext = list.FindByValue(socialContext).ToVocabularyType(VocabularyTypeValueType.ClosedVocabulary, ListType.Link(ListType.ContentSocialContext));
		}

		/// <remarks/>
		public void SetEventStructure(string eventStructure)
		{
			IMDIItemList list = ListConstructor.GetClosedList(ListType.ContentEventStructure);
			CommunicationContext.EventStructure = list.FindByValue(eventStructure).ToVocabularyType(VocabularyTypeValueType.ClosedVocabulary, ListType.Link(ListType.ContentEventStructure));
		}

		/// <remarks/>
		public void SetChannel(string channel)
		{
			IMDIItemList list = ListConstructor.GetClosedList(ListType.ContentChannel);
			CommunicationContext.Channel = list.FindByValue(channel).ToVocabularyType(VocabularyTypeValueType.ClosedVocabulary, ListType.Link(ListType.ContentChannel));
		}

		/// <remarks/>
		public void CheckRequiredFields()
		{
			if (Genre == null)
				Genre = new VocabularyType { Link = ListType.Link(ListType.ContentGenre) };

			if (SubGenre == null)
				SubGenre = new VocabularyType { Link = ListType.Link(ListType.ContentSubGenre) };

			if (Task == null)
				Task = new VocabularyType { Link = ListType.Link(ListType.ContentTask) };

			if (Modalities == null)
				Modalities = new VocabularyType { Link = ListType.Link(ListType.ContentModalities) };

			if (Subject == null)
				Subject = new ContentTypeSubject { Link = ListType.Link(ListType.ContentSubject) };

			if (CommunicationContext.Interactivity == null)
				CommunicationContext.Interactivity = new VocabularyType { Type = VocabularyTypeValueType.ClosedVocabulary, Link = ListType.Link(ListType.ContentInteractivity) };

			if (CommunicationContext.PlanningType == null)
				CommunicationContext.PlanningType = new VocabularyType { Type = VocabularyTypeValueType.ClosedVocabulary, Link = ListType.Link(ListType.ContentPlanningType) };

			if (CommunicationContext.Involvement == null)
				CommunicationContext.Involvement = new VocabularyType { Type = VocabularyTypeValueType.ClosedVocabulary, Link = ListType.Link(ListType.ContentInvolvement) };

			if (CommunicationContext.SocialContext == null)
				CommunicationContext.SocialContext = new VocabularyType { Type = VocabularyTypeValueType.ClosedVocabulary, Link = ListType.Link(ListType.ContentSocialContext) };

			if (CommunicationContext.EventStructure == null)
				CommunicationContext.EventStructure = new VocabularyType { Type = VocabularyTypeValueType.ClosedVocabulary, Link = ListType.Link(ListType.ContentEventStructure) };

			if (CommunicationContext.Channel == null)
				CommunicationContext.Channel = new VocabularyType { Type = VocabularyTypeValueType.ClosedVocabulary, Link = ListType.Link(ListType.ContentChannel) };
		}
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class ContentTypeCommunicationContext
	{
		/// <remarks>Closed vocabulary, Content-Interactivity.xml</remarks>
		public VocabularyType Interactivity { get; set; }

		/// <remarks>Closed vocabulary, Content-PlanningType.xml</remarks>
		public VocabularyType PlanningType { get; set; }

		/// <remarks>Closed vocabulary, Content-Involvement.xml</remarks>
		public VocabularyType Involvement { get; set; }

		/// <remarks>Closed vocabulary, Content-SocialContext.xml</remarks>
		public VocabularyType SocialContext { get; set; }

		/// <remarks>Closed vocabulary, Content-EventStructure.xml</remarks>
		public VocabularyType EventStructure { get; set; }

		/// <remarks>Closed vocabulary, Content-Channel.xml</remarks>
		public VocabularyType Channel { get; set; }
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(AnonymousType=true, Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class ContentTypeSubject : VocabularyType
	{
		/// <remarks/>
		[XmlAttribute]
		public string Encoding { get; set; }
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class LanguagesType : IMDIDescription
	{
		private List<LanguageType> _languageField;

		/// <remarks/>
		[XmlElementAttribute("Language")]
		public List<LanguageType> Language {
			get { return _languageField ?? (_languageField = new List<LanguageType>()); }
			set { _languageField = value; }
		}

		/// <summary></summary>
		/// <param name="iso3CodeOrEnglishName"></param>
		public static LanguageType GetLanguage(string iso3CodeOrEnglishName)
		{
			return LanguageList.Find(new ArchivingLanguage(iso3CodeOrEnglishName, iso3CodeOrEnglishName)).ToLanguageType();
		}
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class LanguageType : IMDIDescription
	{
		/// <remarks/>
		public string Id { get; set; }

		/// <remarks/>
		[XmlElement("Name")]
		public LanguageNameType[] Name { get; set; }

		/// <remarks/>
		public BooleanType MotherTongue { get; set; }

		/// <remarks/>
		public BooleanType PrimaryLanguage { get; set; }

		/// <remarks/>
		public BooleanType Dominant { get; set; }

		/// <remarks/>
		public BooleanType SourceLanguage { get; set; }

		/// <remarks/>
		public BooleanType TargetLanguage { get; set; }

		/// <remarks/>
		[XmlAttribute]
		public string ResourceRef { get; set; }
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class ActorsType : IMDIDescription
	{
		private List<ActorType> _actorField;

		/// <remarks/>
		[XmlElementAttribute("Actor")]
		public List<ActorType> Actor {
			get { return _actorField ?? (_actorField = new List<ActorType>()); }
			set { _actorField = value; }
		}
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class ActorType
	{
		private LanguagesType _languagesField;

		/// <summary>Default constructor</summary>
		public ActorType()
		{
			Keys = new KeysType();
			Code = string.Empty;
			EthnicGroup = string.Empty;
			Age = string.Empty;
			BirthDate = "Unspecified";
			Description = new DescriptionTypeCollection();
			Education = string.Empty;
		}

		/// <summary></summary>
		public ActorType(ArchivingActor actor) : this()
		{
			Name = new[] {actor.GetName()};
			FullName = actor.GetFullName();

			if (!string.IsNullOrEmpty(actor.Age))
				Age = actor.Age;

			// languages
			bool hasPrimary = (actor.PrimaryLanguage != null);
			bool hasMother = (actor.MotherTongueLanguage != null);

			foreach (var lang in actor.Iso3Languages)
			{
				BooleanEnum isPrimary = (hasPrimary)
					? (actor.PrimaryLanguage == lang) ? BooleanEnum.@true : BooleanEnum.@false
					: BooleanEnum.Unspecified;

				BooleanEnum isMother = (hasMother)
					? (actor.MotherTongueLanguage == lang) ? BooleanEnum.@true : BooleanEnum.@false
					: BooleanEnum.Unspecified;

				AddLanguage(lang, isPrimary, isMother);
			}

			// keys
			foreach (var kvp in actor.Keys)
				Keys.Key.Add(new KeyType { Name = kvp.Key, Value = kvp.Value });

			// BirthDate (can be just year)
			var birthDate = actor.GetBirthDate();
			if (!string.IsNullOrEmpty(birthDate))
				SetBirthDate(birthDate);

			// Sex
			if (!string.IsNullOrEmpty(actor.Gender))
				SetSex(actor.Gender);

			// Education
			if (!string.IsNullOrEmpty(actor.Education))
				Education = actor.Education;

			// Role
			if (!string.IsNullOrEmpty(actor.Role))
				Role = actor.Role.ToVocabularyType(false, ListType.Link(ListType.ActorRole)); ;

			// Occupation
			if (!string.IsNullOrEmpty(actor.Occupation))
				Keys.Key.Add(new KeyType { Name = "Occupation", Value = actor.Occupation });

			// Ethnic Group
			if (!string.IsNullOrEmpty(actor.EthnicGroup))
				EthnicGroup = actor.EthnicGroup;

			// Anonymize
			if (actor.Anonymize)
				Anonymized = new BooleanType { Value = BooleanEnum.@true };
		}

		/// <remarks/>
		public VocabularyType Role { get; set; }

		/// <remarks/>
		[XmlElementAttribute("Name")]
		public string[] Name { get; set; }

		/// <remarks/>
		public string FullName { get; set; }

		/// <remarks/>
		public string Code { get; set; }

		/// <remarks/>
		public VocabularyType FamilySocialRole { get; set; }

		/// <remarks/>
		[XmlElement("Languages")]
		public LanguagesType Languages
		{
			get { return _languagesField ?? (_languagesField = new LanguagesType()); }
			set { _languagesField = value; }
		}

		/// <remarks/>
		public string EthnicGroup { get; set; }

		/// <remarks/>
		public string Age { get; set; }

		/// <remarks/>
		public string BirthDate { get; set; }

		/// <remarks/>
		public void SetBirthDate(DateTime birthDate)
		{
			BirthDate = birthDate.ToISO8601TimeFormatDateOnlyString();
		}

		/// <remarks/>
		public void SetBirthDate(string birthDate)
		{
			BirthDate = birthDate;
		}

		/// <remarks/>
		public VocabularyType Sex { get; set; }

		/// <remarks/>
		public void SetSex(string gender)
		{
			IMDIItemList genderList = ListConstructor.GetClosedList(ListType.ActorSex);
			Sex = genderList.FindByValue(gender).ToVocabularyType(VocabularyTypeValueType.ClosedVocabulary, ListType.Link(ListType.ActorSex));
		}

		/// <remarks/>
		public string Education { get; set; }


		/// <summary>Adds a language, setting some attributes also</summary>
		public void AddLanguage(ArchivingLanguage language, BooleanEnum primaryLanguage, BooleanEnum motherTongue)
		{
			var imdiLanguage = LanguageList.Find(language).ToLanguageType();
			if (imdiLanguage == null) return;

			imdiLanguage.PrimaryLanguage = new BooleanType { Value = primaryLanguage, Link = ListType.Link(ListType.Boolean) };
			imdiLanguage.MotherTongue = new BooleanType { Value = motherTongue, Link = ListType.Link(ListType.Boolean) };
			Languages.Language.Add(imdiLanguage);
		}

		/// <remarks/>
		public BooleanType Anonymized { get; set; }

		/// <remarks/>
		public ContactType Contact { get; set; }

		/// <remarks/>
		public KeysType Keys { get; set; }

		/// <remarks/>
		[XmlElement("Description")]
		public DescriptionTypeCollection Description { get; set; }

		/// <remarks/>
		[XmlAttributeAttribute]
		public string ResourceRef { get; set; }

		// TODO: Do we need this method?
		/// <remarks/>
		public ArchivingActor ToArchivingActor()
		{
			ArchivingActor actr = new ArchivingActor
			{
				Age = Age,
				Education = Education,
				FullName = FullName
			};

			if (Sex != null)
				actr.Gender = Sex.Value;

			if (Name.Length > 0)
				actr.Name = Name[0];

			if (!string.IsNullOrEmpty(Role.Value))
				actr.Role = Role.Value;

			if (!string.IsNullOrEmpty(BirthDate))
				actr.BirthDate = BirthDate;

			foreach (LanguageType lang in Languages.Language)
			{
				var iso3 = lang.Id.Substring(lang.Id.Length - 3);
				var archLanguage = new ArchivingLanguage(iso3, lang.Name[0].Value);
				actr.Iso3Languages.Add(archLanguage);

				if (lang.PrimaryLanguage.Value == BooleanEnum.@true)
					actr.PrimaryLanguage = archLanguage;

				if (lang.MotherTongue.Value == BooleanEnum.@true)
					actr.MotherTongueLanguage = archLanguage;
			}

			return actr;
		}
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class CorpusLinkType : ResourceLinkType
	{
		/// <remarks/>
		[XmlAttribute]
		public string Name { get; set; }
	}

	/// <remarks/>
	[XmlIncludeAttribute(typeof(CorpusLinkType))]
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class ResourceLinkType
	{
		/// <remarks/>
		[XmlAttribute]
		public string ArchiveHandle { get; set; }

		/// <remarks/>
		[XmlText(DataType="anyURI")]
		public string Value { get; set; }
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class AnonymsType
	{
		/// <remarks/>
		public ResourceLinkType ResourceLink { get; set; }

		/// <remarks/>
		public AccessType Access { get; set; }
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class CounterPositionType
	{
		/// <remarks/>
		public IntegerType Start { get; set; }

		/// <remarks/>
		public IntegerType End { get; set; }
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class IntegerType
	{
		/// <remarks/>
		[XmlText]
		public string Value { get; set; }
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class SourceType : IMDIDescription
	{
		/// <remarks/>
		public string Id { get; set; }

		/// <remarks/>
		public VocabularyType Format { get; set; }

		/// <remarks/>
		public QualityType Quality { get; set; }

		/// <remarks/>
		public CounterPositionType CounterPosition { get; set; }

		/// <remarks/>
		public AccessType Access { get; set; }

		/// <remarks/>
		public KeysType Keys { get; set; }

		/// <remarks/>
		[XmlAttribute]
		public string ResourceRefs { get; set; }
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class QualityType
	{
		/// <remarks/>
		public QualityType() {
			Type = VocabularyTypeValueType.ClosedVocabulary;
		}

		/// <remarks/>
		[XmlAttribute(DataType="anyURI")]
		public string Link { get; set; }

		/// <remarks/>
		[XmlAttribute]
		public VocabularyTypeValueType Type { get; set; }

		/// <remarks/>
		[XmlText]
		public string Value { get; set; }
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class ValidationType
	{
		/// <remarks/>
		public ValidationType()
		{
			Description = new DescriptionTypeCollection();
		}

		/// <remarks/>
		public VocabularyType Type { get; set; }

		/// <remarks/>
		public VocabularyType Methodology { get; set; }

		/// <remarks/>
		public IntegerType Level { get; set; }

		/// <remarks/>
		[XmlElement("Description")]
		public DescriptionTypeCollection Description { get; set; }
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class WrittenResourceType : IIMDISessionFile
	{
		/// <remarks/>
		public WrittenResourceType()
		{
			Description = new DescriptionTypeCollection();
			Keys = new KeysType();
			Date = "Unspecified";
			CharacterEncoding = string.Empty;
			ContentEncoding = String.Empty;
			LanguageId = string.Empty;
		}

		/// <remarks/>
		public ResourceLinkType ResourceLink { get; set; }

		/// <remarks/>
		public ResourceLinkType MediaResourceLink { get; set; }

		/// <remarks/>
		public string Date { get; set; }

		/// <remarks/>
		public VocabularyType Type { get; set; }

		/// <remarks/>
		public VocabularyType SubType { get; set; }

		/// <remarks/>
		public VocabularyType Format { get; set; }

		/// <remarks/>
		public string Size { get; set; }

		/// <remarks/>
		public ValidationType Validation { get; set; }

		/// <remarks/>
		public VocabularyType Derivation { get; set; }

		/// <remarks/>
		public string CharacterEncoding { get; set; }

		/// <remarks/>
		public string ContentEncoding { get; set; }

		/// <remarks/>
		public string LanguageId { get; set; }

		/// <remarks/>
		public BooleanType Anonymized { get; set; }

		/// <remarks/>
		public AccessType Access { get; set; }

		/// <remarks/>
		[XmlElement("Description")]
		public DescriptionTypeCollection Description { get; set; }

		/// <remarks/>
		public KeysType Keys { get; set; }

		/// <remarks/>
		[XmlAttribute]
		public string ResourceId { get; set; }

		/// <remarks/>
		public void AddDescription(LanguageString description)
		{
			Description.Add(description);
		}

		/// <remarks/>
		[XmlIgnore]
		public string FullPathAndFileName { get; set; }

		/// <remarks/>
		[XmlIgnore]
		public string OutputDirectory { get; set; }
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class MediaFileType : IIMDISessionFile
	{
		/// <remarks/>
		public MediaFileType()
		{
			RecordingConditions = string.Empty;
			TimePosition = new TimePositionRangeType();
			Description = new DescriptionTypeCollection();
			Keys = new KeysType();
		}

		/// <remarks/>
		public ResourceLinkType ResourceLink { get; set; }

		/// <remarks/>
		public VocabularyType Type { get; set; }

		/// <remarks/>
		public VocabularyType Format { get; set; }

		/// <remarks/>
		public string Size { get; set; }

		/// <remarks/>
		public QualityType Quality { get; set; }

		/// <remarks/>
		public string RecordingConditions { get; set; }

		/// <remarks/>
		public TimePositionRangeType TimePosition { get; set; }

		/// <remarks/>
		public AccessType Access { get; set; }

		/// <remarks/>
		[XmlElement("Description")]
		public DescriptionTypeCollection Description { get; set; }

		/// <remarks/>
		public KeysType Keys { get; set; }

		/// <remarks/>
		[XmlAttribute]
		public string ResourceId { get; set; }

		/// <remarks/>
		public void AddDescription(LanguageString description)
		{
			Description.Add(description);
		}

		/// <remarks/>
		[XmlIgnore]
		public string FullPathAndFileName { get; set; }

		/// <remarks/>
		[XmlIgnore]
		public string OutputDirectory { get; set; }
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class Session : IIMDIMajorObject, IArchivingSession
	{
		private static int _anonymizedCounter = 1;

		/// <remarks/>
		public Session()
		{
			Description = new DescriptionTypeCollection();
			MDGroup = new MDGroupType();
			Resources = new SessionResources();
			References = new SessionTypeReferences();
		}

		/// <summary>Name of object</summary>
		[XmlElement("Name")]
		public string Name { get; set; }

		/// <summary>Title of object</summary>
		[XmlElement("Title")]
		public string Title { get; set; }

		/// <remarks/>
		[XmlElement("Date")]
		public string Date { get; set; }

		/// <remarks/>
		[XmlElement("Description")]
		public DescriptionTypeCollection Description { get; set; }

		/// <summary>Set session date with DateTime object</summary>
		public void SetDate(DateTime date)
		{
			Date = date.ToISO8601TimeFormatDateOnlyString();
		}

		/// <summary>Set session date with a string. Can be a date range</summary>
		public void SetDate(string date)
		{
			Date = date;
		}

		/// <summary>Set session date with just the year</summary>
		public void SetDate(int year)
		{
			Date = year.ToString(CultureInfo.InvariantCulture);
		}

		/// <remarks/>
		public MDGroupType MDGroup { get; set; }

		/// <remarks/>
		public SessionResources Resources { get; set; }

		/// <remarks/>
		public SessionTypeReferences References { get; set; }

		/// <remarks/>
		[XmlIgnore]
		public ArchivingLocation Location
		{
			get { return MDGroup.Location.ToArchivingLocation(); }
			set
			{
				var loc = MDGroup.Location;

				if (!string.IsNullOrEmpty(value.Continent))
					loc.SetContinent(value.Continent);

				if (!string.IsNullOrEmpty(value.Country))
					loc.SetCountry(value.Country);

				if (!string.IsNullOrEmpty(value.Region))
					loc.Region.Add(value.Region);

				loc.Address = value.Address;
			}
		}

		/// <remarks/>
		public void AddDescription(LanguageString description)
		{
			Description.Add(description);
		}

		/// <remarks/>
		public void AddActor(ArchivingActor actor)
		{
			if (actor.Anonymize)
			{
				var nameToUse = string.IsNullOrEmpty(actor.Gender) ? "U" : actor.Gender.Substring(0, 1).ToUpper();
				var suffix = "000" + _anonymizedCounter;
				suffix = suffix.Substring(suffix.Length - 3);
				nameToUse += suffix;
				actor.Name = nameToUse;
				actor.FullName = nameToUse;
				_anonymizedCounter++;
			}

			MDGroup.Actors.Actor.Add(new ActorType(actor));

			// actor files, only if not anonymized
			if (!actor.Anonymize)
			{
				foreach (var file in actor.Files)
					AddFile(new IMDIFile(file), "Contributors");
			}
		}

		/// <remarks>Not used yet</remarks>
		[XmlIgnore]
		public ArchiveAccessProtocol AccessProtocol { get; set; }

		/// <remarks>The access level code for this object, applied to resource files and actors</remarks>
		[XmlIgnore]
		public string AccessCode { get; set; }

		/// <remarks/>
		public void AddKeyValuePair(string key, string value)
		{
			MDGroup.Keys.Key.Add(new KeyType { Name = key, Value = value });
		}

		/// <remarks/>
		public void AddFile(ArchivingFile file)
		{
			AddFile(new IMDIFile(file), IMDIArchivingDlgViewModel.NormalizeDirectoryName(Name));
		}

		/// <summary></summary>
		/// <param name="imdiFile"></param>
		/// <param name="directoryName">The sub-directory name in the imdi package directory</param>
		private void AddFile(IMDIFile imdiFile, string directoryName)
		{
			if (imdiFile.IsMediaFile)
				Resources.MediaFile.Add(imdiFile.ToMediaFileType(directoryName));
			else
				Resources.WrittenResource.Add(imdiFile.ToWrittenResourceType(directoryName));
		}

		/// <remarks/>
		[XmlIgnore]
		public List<string> Files
		{
			get
			{
				var files = Resources.MediaFile.Select(file => file.FullPathAndFileName).ToList();

				files.AddRange(Resources.WrittenResource.Select(file => file.FullPathAndFileName));

				return files;
			}
		}

		/// <remarks/>
		[XmlIgnore]
		public string Genre
		{
			get
			{
				return MDGroup.Content.Genre == null ? null : MDGroup.Content.Genre.Value;
			}
			set
			{
				MDGroup.Content.Genre = value.Replace("<","").Replace(">","").ToVocabularyType(false, ListType.Link(ListType.ContentGenre));
			}
		}

		/// <remarks/>
		[XmlIgnore]
		public string SubGenre
		{
			get
			{
				return MDGroup.Content.SubGenre == null ? null : MDGroup.Content.SubGenre.Value;
			}
			set
			{
				MDGroup.Content.SubGenre = value.ToVocabularyType(false, ListType.Link(ListType.ContentSubGenre));
			}
		}

		/// <remarks/>
		[XmlIgnore]
		public string Interactivity
		{
			get
			{
				return MDGroup.Content.CommunicationContext.Interactivity == null ? null : MDGroup.Content.CommunicationContext.Interactivity.Value;
			}
			set
			{
				MDGroup.Content.SetInteractivity(value);
			}
		}

		/// <remarks/>
		[XmlIgnore]
		public string Involvement
		{
			get
			{
				return MDGroup.Content.CommunicationContext.Involvement == null ? null : MDGroup.Content.CommunicationContext.Involvement.Value;
			}
			set
			{
				MDGroup.Content.SetInvolvement(value);
			}
		}

		/// <remarks/>
		[XmlIgnore]
		public string PlanningType
		{
			get
			{
				return MDGroup.Content.CommunicationContext.PlanningType == null ? null : MDGroup.Content.CommunicationContext.PlanningType.Value;
			}
			set
			{
				MDGroup.Content.SetPlanningType(value);
			}
		}

		/// <remarks/>
		[XmlIgnore]
		public string SocialContext
		{
			get
			{
				return MDGroup.Content.CommunicationContext.SocialContext == null ? null : MDGroup.Content.CommunicationContext.SocialContext.Value;
			}
			set
			{
				MDGroup.Content.SetSocialContext(value);
			}
		}

		/// <remarks/>
		[XmlIgnore]
		public string Task
		{
			get
			{
				return MDGroup.Content.Task == null ? null : MDGroup.Content.Task.Value;
			}
			set
			{
				MDGroup.Content.SubGenre = value.ToVocabularyType(false, ListType.Link(ListType.ContentSubGenre));
			}
		}
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(AnonymousType=true, Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class SessionResources
	{
		private List<MediaFileType> _mediaFileField;
		private List<WrittenResourceType> _writtenResourceField;

		/// <remarks/>
		[XmlElementAttribute("MediaFile")]
		public List<MediaFileType> MediaFile {
			get { return _mediaFileField ?? (_mediaFileField = new List<MediaFileType>()); }
			set { _mediaFileField = value; }
		}

		/// <remarks/>
		[XmlElementAttribute("WrittenResource")]
		public List<WrittenResourceType> WrittenResource {
			get { return _writtenResourceField ?? (_writtenResourceField = new List<WrittenResourceType>()); }
			set { _writtenResourceField = value; }
		}

		/// <remarks/>
		[XmlElementAttribute("Source")]
		public SourceType[] Source { get; set; }

		/// <remarks/>
		public AnonymsType Anonyms { get; set; }
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(AnonymousType=true, Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class SessionReferences : IMDIDescription
	{
	}

	/// <remarks/>
	[SerializableAttribute]
	[XmlTypeAttribute(Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public enum MetatranscriptValueType
	{
		// ReSharper disable InconsistentNaming
		/// <remarks/>
		SESSION,

		/// <remarks/>
		[XmlEnumAttribute("SESSION.Profile")]
		SESSIONProfile,

		/// <remarks/>
		LEXICON_RESOURCE_BUNDLE,

		/// <remarks/>
		[XmlEnumAttribute("LEXICON_RESOURCE_BUNDLE.Profile")]
		LEXICON_RESOURCE_BUNDLEProfile,

		/// <remarks/>
		CATALOGUE,

		/// <remarks/>
		[XmlEnumAttribute("CATALOGUE.Profile")]
		CATALOGUEProfile,

		/// <remarks/>
		CORPUS,

		/// <remarks/>
		[XmlEnumAttribute("CORPUS.Profile")]
		CORPUSProfile,
		// ReSharper restore InconsistentNaming
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(Namespace = "http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class TimePositionRangeType
	{
		/// <remarks/>
		public TimePositionRangeType()
		{
			Start = "Unspecified";
			End = "Unspecified";
		}

		/// <remarks/>
		public string Start { get; set; }

		/// <remarks/>
		public string End { get; set; }
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class SessionTypeReferences
	{
		/// <remarks/>
		public SessionTypeReferences()
		{
			Description = new DescriptionTypeCollection();
		}

		/// <remarks/>
		[XmlElement("Description")]
		public DescriptionTypeCollection Description { get; set; }
	}
}