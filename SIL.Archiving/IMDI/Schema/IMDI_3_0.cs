using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Diagnostics;
using Palaso.Extensions;
using SIL.Archiving.Generic;
using SIL.Archiving.Generic.AccessProtocol;
using SIL.Archiving.IMDI.Lists;

namespace SIL.Archiving.IMDI.Schema
{
	/// <summary>Shared properties and methods</summary>
	public class IMDIMajorObject : IMDIDescription
	{
		/// <summary>Name of object</summary>
		[XmlElement("Name")]
		public string Name { get; set; }

		/// <summary>Title of object</summary>
		[XmlElement("Title")]
		public string Title { get; set; }
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
			using (var strWriter = new StringWriter())
			{
				var settings = new XmlWriterSettings
				{
					Indent = true,
					IndentChars = "\t",
					CheckCharacters = true
				};

				using (var xmlWriter = XmlWriter.Create(strWriter, settings))
				{
					var serializer = new XmlSerializer(GetType());
					serializer.Serialize(xmlWriter, this);
					xmlWriter.Flush();
					xmlWriter.Close();
					strWriter.Flush();
					strWriter.Close();

					return strWriter.ToString();
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

		// **************** do we need these? ****************

		/// <remarks/>
		[XmlIgnore]
		public string History { get; set; }

		/// <remarks/>
		[XmlIgnore]
		[XmlAttributeAttribute("History", DataType = "anyURI")]
		public string History1 { get; set; }

		/// <remarks/>
		[XmlIgnore]
		[XmlAttributeAttribute]
		public string Profile { get; set; }

		/// <remarks/>
		[XmlIgnore]
		[XmlAttributeAttribute]
		public string ArchiveHandle { get; set; }
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
	public class Catalogue : IMDIMajorObject
	{
		private CatalogueDocumentLanguages _documentLanguages;
		private CatalogueSubjectLanguages _subjectLanguages;

		/// <remarks/>
		[XmlElement("Id")]
		public string[] Id { get; set; }

		/// <remarks/>
		public CatalogueDocumentLanguages DocumentLanguages
		{
			get { return _documentLanguages ?? (_documentLanguages = new CatalogueDocumentLanguages()); }
			set { _documentLanguages = value; }
		}

		/// <remarks/>
		public CatalogueSubjectLanguages SubjectLanguages
		{
			get { return _subjectLanguages ?? (_subjectLanguages = new CatalogueSubjectLanguages()); }
			set { _subjectLanguages = value; }
		}

		/// <remarks/>
		[XmlElement("Location")]
		public LocationType[] Location { get; set; }

		/// <remarks/>
		[XmlElement("ContentType")]
		public VocabularyType[] ContentType { get; set; }

		/// <remarks/>
		public CatalogueFormat Format { get; set; }

		/// <remarks/>
		public CatalogueQuality Quality { get; set; }

		/// <remarks/>
		public VocabularyType SmallestAnnotationUnit { get; set; }

		/// <remarks/>
		public VocabularyType Applications { get; set; }

		/// <remarks/>
		public string Date { get; set; }

		/// <remarks/>
		[XmlElement("Project")]
		public Project[] Project { get; set; }

		/// <remarks/>
		[XmlElement("Publisher")]
		public string[] Publisher { get; set; }

		/// <remarks/>
		[XmlElement("Author")]
		public CommaSeparatedStringType[] Author { get; set; }

		/// <remarks/>
		public string Size { get; set; }

		/// <remarks/>
		public VocabularyType DistributionForm { get; set; }

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
		private List<string> _regionField;

		/// <remarks/>
		public VocabularyType Continent { get; set; }

		/// <remarks>Closed vocabulary</remarks>
		public void SetContinent(string continent)
		{
			ClosedIMDIItemList continentList = ListConstructor.GetClosedList(ListType.Continents);
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
		public List<string> Region
		{
			get { return _regionField ?? (_regionField = new List<string>()); }
			set { _regionField = value; }
		}

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
	public class Project : IMDIMajorObject
	{
		/// <remarks/>
		public string Id { get; set; }

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
	public class Corpus : IMDIMajorObject
	{
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

		/// <remarks/>
		public Corpus()
		{
			CorpusLink = new List<CorpusLinkType>();
		}
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class MDGroupType
	{
		private LocationType _locationField;
		private ActorsType _actorsField;
		private ContentType _contentField;
		private KeysType _keysField;

		/// <remarks/>
		public LocationType Location
		{
			get { return _locationField ?? (_locationField = new LocationType()); }
			set { _locationField = value; }
		}

		/// <remarks/>
		[XmlElement("Project")]
		public Project[] Project { get; set; }

		/// <remarks/>
		public KeysType Keys
		{
			get { return _keysField ?? (_keysField = new KeysType()); }
			set { _keysField = value; }
		}

		/// <remarks/>
		public ContentType Content
		{
			get { return _contentField ?? (_contentField = new ContentType()); }
			set { _contentField = value; }
		}

		/// <remarks/>
		public ActorsType Actors
		{
			get { return _actorsField ?? (_actorsField = new ActorsType()); }
			set { _actorsField = value; }
		}
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class ContentType : IMDIDescription
	{
		private ContentTypeCommunicationContext _contextField;
		private LanguagesType _languagesField;
		private KeysType _keysField;

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
		public KeysType Keys
		{
			get { return _keysField ?? (_keysField = new KeysType()); }
			set { _keysField = value; }
		}

		/// <remarks/>
		[XmlElement("Languages")]
		public LanguagesType Languages
		{
			get { return _languagesField ?? (_languagesField = new LanguagesType()); }
			set { _languagesField = value; }
		}

		/// <summary>Adds a language, setting some attributes also</summary>
		/// <param name="iso3Code"></param>
		/// <param name="dominantLanguage"></param>
		/// <param name="sourceLanguage"></param>
		/// <param name="targetLanguage"></param>
		public void AddLanguage(string iso3Code, BooleanEnum dominantLanguage, BooleanEnum sourceLanguage, BooleanEnum targetLanguage)
		{
			var language = LanguageList.FindByISO3Code(iso3Code).ToLanguageType();
			if (language == null) return;

			language.Dominant = new BooleanType { Value = dominantLanguage };
			language.SourceLanguage = new BooleanType { Value = sourceLanguage };
			language.TargetLanguage = new BooleanType { Value = targetLanguage };
			Languages.Language.Add(language);
		}

		/// <remarks/>
		public ContentTypeCommunicationContext CommunicationContext
		{
			get { return _contextField ?? (_contextField = new ContentTypeCommunicationContext()); }
			set { _contextField = value; }
		}

		/// <remarks/>
		public void SetInteractivity(string interactivity)
		{
			ClosedIMDIItemList list = ListConstructor.GetClosedList(ListType.ContentInteractivity);
			CommunicationContext.Interactivity = list.FindByValue(interactivity).ToVocabularyType(VocabularyTypeValueType.ClosedVocabulary, ListType.Link(ListType.ContentInteractivity));
		}

		/// <remarks/>
		public void SetPlanningType(string planningType)
		{
			ClosedIMDIItemList list = ListConstructor.GetClosedList(ListType.ContentPlanningType);
			CommunicationContext.PlanningType = list.FindByValue(planningType).ToVocabularyType(VocabularyTypeValueType.ClosedVocabulary, ListType.Link(ListType.ContentPlanningType));
		}

		/// <remarks/>
		public void SetInvolvement(string involvement)
		{
			ClosedIMDIItemList list = ListConstructor.GetClosedList(ListType.ContentInvolvement);
			CommunicationContext.Involvement = list.FindByValue(involvement).ToVocabularyType(VocabularyTypeValueType.ClosedVocabulary, ListType.Link(ListType.ContentInvolvement));
		}

		/// <remarks/>
		public void SetSocialContext(string socialContext)
		{
			ClosedIMDIItemList list = ListConstructor.GetClosedList(ListType.ContentSocialContext);
			CommunicationContext.SocialContext = list.FindByValue(socialContext).ToVocabularyType(VocabularyTypeValueType.ClosedVocabulary, ListType.Link(ListType.ContentSocialContext));
		}

		/// <remarks/>
		public void SetEventStructure(string eventStructure)
		{
			ClosedIMDIItemList list = ListConstructor.GetClosedList(ListType.ContentEventStructure);
			CommunicationContext.EventStructure = list.FindByValue(eventStructure).ToVocabularyType(VocabularyTypeValueType.ClosedVocabulary, ListType.Link(ListType.ContentEventStructure));
		}

		/// <remarks/>
		public void SetChannel(string channel)
		{
			ClosedIMDIItemList list = ListConstructor.GetClosedList(ListType.ContentChannel);
			CommunicationContext.Channel = list.FindByValue(channel).ToVocabularyType(VocabularyTypeValueType.ClosedVocabulary, ListType.Link(ListType.ContentChannel));
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
			LanguageType langType;

			try
			{
				langType = LanguageList.FindByISO3Code(iso3CodeOrEnglishName).ToLanguageType();
			}
			catch (ArgumentException)
			{
				// not found by iso3 code, try by name
				langType = LanguageList.FindByEnglishName(iso3CodeOrEnglishName).ToLanguageType();
			}

			return langType;
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
	public class ActorType : IMDIDescription
	{
		private LanguagesType _languagesField;

		/// <summary>Default constructor</summary>
		public ActorType()
		{
		}

		/// <summary></summary>
		public ActorType(ArchivingActor actor)
		{
			Name = new[] {actor.GetName()};
			FullName = actor.GetFullName();

			if (!string.IsNullOrEmpty(actor.Age))
				Age = actor.Age;

			// languages
			bool hasPrimary = !string.IsNullOrEmpty(actor.PrimaryLanguageIso3Code);
			bool hasMother = !string.IsNullOrEmpty(actor.MotherTongueLanguageIso3Code);

			foreach (var langIso3 in actor.Iso3LanguageCodes)
			{
				BooleanEnum isPrimary = (hasPrimary)
					? (actor.PrimaryLanguageIso3Code == langIso3) ? BooleanEnum.@true : BooleanEnum.@false
					: BooleanEnum.Unspecified;

				BooleanEnum isMother = (hasMother)
					? (actor.MotherTongueLanguageIso3Code == langIso3) ? BooleanEnum.@true : BooleanEnum.@false
					: BooleanEnum.Unspecified;

				AddLanguage(langIso3, isPrimary, isMother);
			}


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

			// Occupation
			if (!string.IsNullOrEmpty(actor.Occupation))
				FamilySocialRole = actor.Occupation.ToVocabularyType(false, ListType.Link(ListType.ActorFamilySocialRole));
		}

		/// <remarks/>
		[XmlElementAttribute("Name")]
		public string[] Name { get; set; }

		/// <remarks/>
		public string FullName { get; set; }

		/// <remarks/>
		public string Code { get; set; }

		/// <remarks/>
		public string Age { get; set; }

		/// <remarks/>
		public string BirthDate { get; set; }

		/// <remarks/>
		public void SetBirthDate(DateTime birthDate)
		{
			BirthDate = birthDate.ToISO8601DateOnlyString();
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
			ClosedIMDIItemList genderList = ListConstructor.GetClosedList(ListType.ActorSex);
			Sex = genderList.FindByValue(gender).ToVocabularyType(VocabularyTypeValueType.ClosedVocabulary, ListType.Link(ListType.ActorSex));
		}

		/// <remarks/>
		public string Education { get; set; }

		/// <remarks/>
		[XmlElement("Languages")]
		public LanguagesType Languages
		{
			get { return _languagesField ?? (_languagesField = new LanguagesType()); }
			set { _languagesField = value; }
		}

		/// <summary>Adds a language, setting some attributes also</summary>
		/// <param name="iso3Code"></param>
		/// <param name="primaryLanguage"></param>
		/// <param name="motherTongue"></param>
		public void AddLanguage(string iso3Code, BooleanEnum primaryLanguage, BooleanEnum motherTongue)
		{
			var language = LanguageList.FindByISO3Code(iso3Code).ToLanguageType();
			if (language == null) return;

			language.PrimaryLanguage = new BooleanType { Value = primaryLanguage, Link = ListType.Link(ListType.Boolean) };
			language.MotherTongue = new BooleanType { Value = motherTongue, Link = ListType.Link(ListType.Boolean) };
			Languages.Language.Add(language);
		}

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

			if (!string.IsNullOrEmpty(BirthDate))
				actr.BirthDate = BirthDate;

			foreach (LanguageType lang in Languages.Language)
			{
				var iso3 = lang.Id.Substring(lang.Id.Length - 3);

				actr.Iso3LanguageCodes.Add(iso3);

				if (lang.PrimaryLanguage.Value == BooleanEnum.@true)
					actr.PrimaryLanguageIso3Code = iso3;

				if (lang.MotherTongue.Value == BooleanEnum.@true)
					actr.MotherTongueLanguageIso3Code = iso3;
			}

			return actr;
		}

		//*********************************************************************
		// these possibly will not be used

		/// <remarks/>
		public VocabularyType Role { get; set; }

		/// <remarks/>
		public VocabularyType FamilySocialRole { get; set; }

		/// <remarks/>
		public VocabularyType EthnicGroup { get; set; }

		/// <remarks/>
		public BooleanType Anonymized { get; set; }

		/// <remarks/>
		public ContactType Contact { get; set; }

		/// <remarks/>
		public KeysType Keys { get; set; }

		/// <remarks/>
		[XmlAttributeAttribute]
		public string ResourceRef { get; set; }
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

		// **************** do we need these? ****************

		///// <remarks/>
		//public TimePositionRange_Type TimePosition { get; set; }
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
	public class ValidationType : IMDIDescription
	{
		/// <remarks/>
		public VocabularyType Type { get; set; }

		/// <remarks/>
		public VocabularyType Methodology { get; set; }

		/// <remarks/>
		public IntegerType Level { get; set; }
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class WrittenResourceType : IMDIDescription, IIMDISessionFile
	{
		private string _dateField;

		/// <remarks/>
		public ResourceLinkType ResourceLink { get; set; }

		/// <remarks/>
		public ResourceLinkType MediaResourceLink { get; set; }

		/// <remarks/>
		public string Date {
			get {
				return _dateField;
			}
			set {
				_dateField = value;
			}
		}

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
		public VocabularyType LanguageId { get; set; }

		/// <remarks/>
		public BooleanType Anonymized { get; set; }

		/// <remarks/>
		public AccessType Access { get; set; }

		/// <remarks/>
		public KeysType Keys { get; set; }

		/// <remarks/>
		[XmlAttribute]
		public string ResourceId { get; set; }
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class MediaFileType : IMDIDescription, IIMDISessionFile
	{
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
		public AccessType Access { get; set; }

		/// <remarks/>
		public KeysType Keys { get; set; }

		/// <remarks/>
		[XmlAttribute]
		public string ResourceId { get; set; }

		// **************** do we need these? ****************

		///// <remarks/>
		//public NotUsed.TimePositionRange_Type TimePosition { get; set; }
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class Session : IMDIMajorObject, IArchivingSession
	{
		private MDGroupType _mDGroupField;
		private SessionResources _resourcesField;

		/// <remarks/>
		[XmlElement("Date")]
		public string Date { get; set; }

		/// <remarks/>
		public void SetDate(DateTime date)
		{
			Date = date.ToISO8601DateOnlyString();
		}

		/// <remarks/>
		public void SetDate(string date)
		{
			Date = date;
		}

		/// <remarks/>
		public void SetDate(int date)
		{
			Date = date.ToString(CultureInfo.InvariantCulture);
		}

		/// <remarks/>
		public MDGroupType MDGroup
		{
			get { return _mDGroupField ?? (_mDGroupField = new MDGroupType()); }
			set { _mDGroupField = value; }
		}

		/// <remarks/>
		public SessionResources Resources
		{
			get { return _resourcesField ?? (_resourcesField = new SessionResources()); }
			set { _resourcesField = value; }
		}

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
		public void AddActor(ArchivingActor actor)
		{
			MDGroup.Actors.Actor.Add(new ActorType(actor));
		}

		// **************** do we need these? ****************

		///// <remarks/>
		//public SessionReferences References { get; set; }

		///// <remarks/>
		//[XmlElementAttribute("ExternalResourceReference")]
		//public NotUsed.ExternalResourceReference_Type[] ExternalResourceReference { get; set; }

		// Don't know which of the following we really need:

		/// <remarks/>
		[XmlIgnore]
		public DateTime DateCreatedFirst { get; set; }
		/// <remarks/>
		[XmlIgnore]
		public DateTime DateCreatedLast { get; set; }
		/// <remarks/>
		[XmlIgnore]
		public DateTime DateModified { get; set; }
		/// <remarks/>
		[XmlIgnore]
		public IAccessProtocol AccessProtocol { get; set; }
		/// <remarks/>
		[XmlIgnore]
		public List<IArchivingFile> Files { get; set; }
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

		// **************** do we need these? ****************

		///// <remarks/>
		//[XmlElementAttribute("LexiconResource")]
		//public NotUsed.LexiconResource_Type[] LexiconResource { get; set; }

		///// <remarks/>
		//[XmlElementAttribute("LexiconComponent")]
		//public NotUsed.LexiconComponent_Type[] LexiconComponent { get; set; }
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
}