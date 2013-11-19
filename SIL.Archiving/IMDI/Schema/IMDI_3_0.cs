using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Diagnostics;
using System.ComponentModel;
using Palaso.Xml;
using SIL.Archiving.Generic;
using SIL.Archiving.Generic.AccessProtocol;

namespace SIL.Archiving.IMDI.Schema
{
	public class IMDIMajorObject
	{
		private List<Description_Type> descriptionField;

		/// <summary>Name of object</summary>
		[XmlElement("Name")]
		public string Name { get; set; }

		/// <summary>Title of object</summary>
		[XmlElement("Title")]
		public string Title { get; set; }

		/// <summary>Description of object</summary>
		[XmlElementAttribute("Description")]
		public List<Description_Type> Description
		{
			get
			{
				if (descriptionField == null)
					descriptionField = new List<Description_Type>();
				return descriptionField;
			}
			set
			{
				descriptionField = value;
			}
		}

		/// <summary>
		/// Adds a description (in a particular language)
		/// </summary>
		public void AddDescription(LanguageString description)
		{
			Description.Add(description.ToIMDIDescriptionType());
		}
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlType(Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	[XmlRootAttribute("METATRANSCRIPT", Namespace="http://www.mpi.nl/IMDI/Schema/IMDI", IsNullable=false)]
	public class MetaTranscript
	{
		private DateTime dateField = DateTime.Today;

		private string originatorField = "Palaso IMDI 0.0.1";

		private string versionField = "0";

		private string formatIdField = "IMDI 3.03";

		private string schemaLocation = "http://www.mpi.nl/IMDI/Schema/IMDI http://www.mpi.nl/IMDI/Schema/IMDI_3.0.xsd";

		public MetaTranscript()
		{
		}

		public MetaTranscript(Metatranscript_Value_Type type)
		{
			Type = type;
			switch (type)
			{
				case Metatranscript_Value_Type.CORPUS:
					Items = new object[] { new Corpus() };
					break;
				case Metatranscript_Value_Type.SESSION:
					Items = new object[] { new Session() };
					break;
				case Metatranscript_Value_Type.CATALOGUE:
					Items = new object[] { new Catalogue() };
					break;
			}
		}

		/// <remarks/>
		[XmlAttribute("schemaLocation", Namespace = XmlSchema.InstanceNamespace)]
		public string SchemaLocation
		{
			get { return schemaLocation; }
			set { schemaLocation = value; }
		}

		/// <remarks/>
		public String_Type History { get; set; }

		/// <remarks/>
		[XmlElementAttribute("Catalogue", typeof(Catalogue))]
		[XmlElementAttribute("Corpus", typeof(Corpus))]
		[XmlElementAttribute("Session", typeof(Session))]
		public object[] Items { get; set; }

		/// <remarks/>
		[XmlAttributeAttribute]
		public string Profile { get; set; }

		/// <remarks/>
		[XmlAttributeAttribute(DataType="date")]
		public DateTime Date {
			get {
				return dateField;
			}
			set {
				dateField = value;
			}
		}

		/// <remarks/>
		[XmlAttributeAttribute]
		public string Originator {
			get {
				return originatorField;
			}
			set {
				originatorField = value;
			}
		}

		/// <remarks/>
		[XmlAttributeAttribute]
		public string Version {
			get {
				return versionField;
			}
			set {
				versionField = value;
			}
		}

		/// <remarks/>
		[XmlAttributeAttribute]
		public string FormatId {
			get {
				return formatIdField;
			}
			set {
				formatIdField = value;
			}
		}

		/// <remarks/>
		[XmlAttributeAttribute("History", DataType="anyURI")]
		public string History1 { get; set; }

		/// <remarks/>
		[XmlAttributeAttribute]
		public Metatranscript_Value_Type Type { get; set; }

		/// <remarks/>
		[XmlAttributeAttribute]
		public string ArchiveHandle { get; set; }

		/// <summary>
		/// Gets the XML representation of this object
		/// </summary>
		public override string ToString()
		{
			using (var strWriter = new StringWriter())
			{
				var settings = new XmlWriterSettings();
				settings.Indent = true;
				settings.IndentChars = "\t";
				settings.CheckCharacters = true;

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
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class String_Type
	{
		[XmlTextAttribute(DataType="token")]
		public string Value { get; set; }
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class CommaSeparatedString_Type
	{

		private string valueField;

		/// <remarks/>
		[XmlTextAttribute]
		public string Value {
			get {
				return valueField;
			}
			set {
				valueField = value;
			}
		}
	}

	/// <remarks/>
	[XmlIncludeAttribute(typeof(SubjectLanguageType))]
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class SimpleLanguageType {

		private LanguageId_Type idField;

		private LanguageName_Type nameField;

		/// <remarks/>
		public LanguageId_Type Id {
			get {
				return idField;
			}
			set {
				idField = value;
			}
		}

		/// <remarks/>
		public LanguageName_Type Name {
			get {
				return nameField;
			}
			set {
				nameField = value;
			}
		}
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class LanguageId_Type
	{

		private string valueField;

		/// <remarks/>
		[XmlTextAttribute(DataType="token")]
		public string Value {
			get {
				return valueField;
			}
			set {
				valueField = value;
			}
		}
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class LanguageName_Type : Vocabulary_Type
	{
	}

	/// <remarks/>
	[XmlIncludeAttribute(typeof(LanguageName_Type))]
	[XmlIncludeAttribute(typeof(Key_Type))]
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class Vocabulary_Type
	{

		private VocabularyType_Value_Type typeField;

		private string defaultLinkField;

		private string linkField;

		private string valueField;

		/// <remarks/>
		public Vocabulary_Type() {
			typeField = VocabularyType_Value_Type.OpenVocabulary;
		}

		/// <remarks/>
		[XmlAttributeAttribute]
		[DefaultValueAttribute(VocabularyType_Value_Type.OpenVocabulary)]
		public VocabularyType_Value_Type Type {
			get {
				return typeField;
			}
			set {
				typeField = value;
			}
		}

		/// <remarks/>
		[XmlAttributeAttribute(DataType="anyURI")]
		public string DefaultLink {
			get {
				return defaultLinkField;
			}
			set {
				defaultLinkField = value;
			}
		}

		/// <remarks/>
		[XmlAttributeAttribute(DataType="anyURI")]
		public string Link {
			get {
				return linkField;
			}
			set {
				linkField = value;
			}
		}

		/// <remarks/>
		[XmlTextAttribute]
		public string Value {
			get {
				return valueField;
			}
			set {
				valueField = value;
			}
		}
	}

	/// <remarks/>
	[SerializableAttribute]
	[XmlTypeAttribute(Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public enum VocabularyType_Value_Type
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
	public class Key_Type : Vocabulary_Type
	{

		private string nameField;

		/// <remarks/>
		[XmlAttributeAttribute]
		public string Name {
			get {
				return nameField;
			}
			set {
				nameField = value;
			}
		}
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class SubjectLanguageType : SimpleLanguageType {

		private Boolean_Type dominantField;

		private Boolean_Type sourceLanguageField;

		private Boolean_Type targetLanguageField;

		private List<Description_Type> descriptionField;

		/// <remarks/>
		public Boolean_Type Dominant {
			get {
				return dominantField;
			}
			set {
				dominantField = value;
			}
		}

		/// <remarks/>
		public Boolean_Type SourceLanguage {
			get {
				return sourceLanguageField;
			}
			set {
				sourceLanguageField = value;
			}
		}

		/// <remarks/>
		public Boolean_Type TargetLanguage {
			get {
				return targetLanguageField;
			}
			set {
				targetLanguageField = value;
			}
		}

		/// <remarks/>
		[XmlElementAttribute("Description")]
		public List<Description_Type> Description {
			get {
				if (descriptionField == null)
					descriptionField = new List<Description_Type>();
				return descriptionField;
			}
			set {
				descriptionField = value;
			}
		}
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class Boolean_Type
	{

		private VocabularyType_Value_Type typeField;

		private string defaultLinkField;

		private string linkField;

		private string valueField;

		/// <remarks/>
		public Boolean_Type() {
			typeField = VocabularyType_Value_Type.ClosedVocabulary;
		}

		/// <remarks/>
		[XmlAttributeAttribute]
		[DefaultValueAttribute(VocabularyType_Value_Type.ClosedVocabulary)]
		public VocabularyType_Value_Type Type {
			get {
				return typeField;
			}
			set {
				typeField = value;
			}
		}

		/// <remarks/>
		[XmlAttributeAttribute(DataType="anyURI")]
		public string DefaultLink {
			get {
				return defaultLinkField;
			}
			set {
				defaultLinkField = value;
			}
		}

		/// <remarks/>
		[XmlAttributeAttribute(DataType="anyURI")]
		public string Link {
			get {
				return linkField;
			}
			set {
				linkField = value;
			}
		}

		/// <remarks/>
		[XmlTextAttribute]
		public string Value {
			get {
				return valueField;
			}
			set {
				valueField = value;
			}
		}
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class Description_Type
	{

		private string languageIdField;

		private string nameField;

		private string archiveHandleField;

		private string linkField;

		private string valueField;

		/// <remarks/>
		[XmlAttributeAttribute(DataType="token")]
		public string LanguageId {
			get {
				return languageIdField;
			}
			set {
				languageIdField = value;
			}
		}

		/// <remarks/>
		[XmlAttributeAttribute]
		public string Name {
			get {
				return nameField;
			}
			set {
				nameField = value;
			}
		}

		/// <remarks/>
		[XmlAttributeAttribute]
		public string ArchiveHandle {
			get {
				return archiveHandleField;
			}
			set {
				archiveHandleField = value;
			}
		}

		/// <remarks/>
		[XmlAttributeAttribute(DataType="anyURI")]
		public string Link {
			get {
				return linkField;
			}
			set {
				linkField = value;
			}
		}

		/// <remarks/>
		[XmlTextAttribute]
		public string Value {
			get {
				return valueField;
			}
			set {
				valueField = value;
			}
		}
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class Catalogue : IMDIMajorObject
	{
		private String_Type[] idField;

		private CatalogueDocumentLanguages documentLanguagesField;

		private CatalogueSubjectLanguages subjectLanguagesField;

		private Location_Type[] locationField;

		private Vocabulary_Type[] contentTypeField;

		private CatalogueFormat formatField;

		private CatalogueQuality qualityField;

		private Vocabulary_Type smallestAnnotationUnitField;

		private Vocabulary_Type applicationsField;

		private Date_Type dateField;

		private Project[] projectField;

		private String_Type[] publisherField;

		private CommaSeparatedString_Type[] authorField;

		private String_Type sizeField;

		private Vocabulary_Type distributionFormField;

		private Access_Type accessField;

		private String_Type pricingField;

		private String_Type contactPersonField;

		private String_Type referenceLinkField;

		private String_Type metadataLinkField;

		private String_Type publicationsField;

		private Keys_Type keysField;

		/// <remarks/>
		[XmlElementAttribute("Id")]
		public String_Type[] Id {
			get {
				return idField;
			}
			set {
				idField = value;
			}
		}

		/// <remarks/>
		public CatalogueDocumentLanguages DocumentLanguages {
			get {
				return documentLanguagesField;
			}
			set {
				documentLanguagesField = value;
			}
		}

		/// <remarks/>
		public CatalogueSubjectLanguages SubjectLanguages {
			get {
				return subjectLanguagesField;
			}
			set {
				subjectLanguagesField = value;
			}
		}

		/// <remarks/>
		[XmlElementAttribute("Location")]
		public Location_Type[] Location {
			get {
				return locationField;
			}
			set {
				locationField = value;
			}
		}

		/// <remarks/>
		[XmlElementAttribute("ContentType")]
		public Vocabulary_Type[] ContentType {
			get {
				return contentTypeField;
			}
			set {
				contentTypeField = value;
			}
		}

		/// <remarks/>
		public CatalogueFormat Format {
			get {
				return formatField;
			}
			set {
				formatField = value;
			}
		}

		/// <remarks/>
		public CatalogueQuality Quality {
			get {
				return qualityField;
			}
			set {
				qualityField = value;
			}
		}

		/// <remarks/>
		public Vocabulary_Type SmallestAnnotationUnit {
			get {
				return smallestAnnotationUnitField;
			}
			set {
				smallestAnnotationUnitField = value;
			}
		}

		/// <remarks/>
		public Vocabulary_Type Applications {
			get {
				return applicationsField;
			}
			set {
				applicationsField = value;
			}
		}

		/// <remarks/>
		public Date_Type Date {
			get {
				return dateField;
			}
			set {
				dateField = value;
			}
		}

		/// <remarks/>
		[XmlElementAttribute("Project")]
		public Project[] Project {
			get {
				return projectField;
			}
			set {
				projectField = value;
			}
		}

		/// <remarks/>
		[XmlElementAttribute("Publisher")]
		public String_Type[] Publisher {
			get {
				return publisherField;
			}
			set {
				publisherField = value;
			}
		}

		/// <remarks/>
		[XmlElementAttribute("Author")]
		public CommaSeparatedString_Type[] Author {
			get {
				return authorField;
			}
			set {
				authorField = value;
			}
		}

		/// <remarks/>
		public String_Type Size {
			get {
				return sizeField;
			}
			set {
				sizeField = value;
			}
		}

		/// <remarks/>
		public Vocabulary_Type DistributionForm {
			get {
				return distributionFormField;
			}
			set {
				distributionFormField = value;
			}
		}

		/// <remarks/>
		public Access_Type Access {
			get {
				return accessField;
			}
			set {
				accessField = value;
			}
		}

		/// <remarks/>
		public String_Type Pricing {
			get {
				return pricingField;
			}
			set {
				pricingField = value;
			}
		}

		/// <remarks/>
		public String_Type ContactPerson {
			get {
				return contactPersonField;
			}
			set {
				contactPersonField = value;
			}
		}

		/// <remarks/>
		public String_Type ReferenceLink {
			get {
				return referenceLinkField;
			}
			set {
				referenceLinkField = value;
			}
		}

		/// <remarks/>
		public String_Type MetadataLink {
			get {
				return metadataLinkField;
			}
			set {
				metadataLinkField = value;
			}
		}

		/// <remarks/>
		public String_Type Publications {
			get {
				return publicationsField;
			}
			set {
				publicationsField = value;
			}
		}

		/// <remarks/>
		public Keys_Type Keys {
			get {
				return keysField;
			}
			set {
				keysField = value;
			}
		}
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(AnonymousType=true, Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class CatalogueDocumentLanguages {

		private List<Description_Type> descriptionField;

		private SimpleLanguageType[] languageField;

		/// <remarks/>
		[XmlElementAttribute("Description")]
		public List<Description_Type> Description {
			get {
				if (descriptionField == null)
					descriptionField = new List<Description_Type>();
				return descriptionField;
			}
			set {
				descriptionField = value;
			}
		}

		/// <remarks/>
		[XmlElementAttribute("Language")]
		public SimpleLanguageType[] Language {
			get {
				return languageField;
			}
			set {
				languageField = value;
			}
		}
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(AnonymousType=true, Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class CatalogueSubjectLanguages {

		private List<Description_Type> descriptionField;

		private SubjectLanguageType[] languageField;

		/// <remarks/>
		[XmlElementAttribute("Description")]
		public List<Description_Type> Description {
			get {
				if (descriptionField == null)
					descriptionField = new List<Description_Type>();
				return descriptionField;
			}
			set {
				descriptionField = value;
			}
		}

		/// <remarks/>
		[XmlElementAttribute("Language")]
		public SubjectLanguageType[] Language {
			get {
				return languageField;
			}
			set {
				languageField = value;
			}
		}
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class Location_Type
	{

		private Vocabulary_Type continentField;

		private Vocabulary_Type countryField;

		private String_Type[] regionField;

		private String_Type addressField;

		/// <remarks/>
		public Vocabulary_Type Continent {
			get {
				return continentField;
			}
			set {
				continentField = value;
			}
		}

		/// <remarks/>
		public Vocabulary_Type Country {
			get {
				return countryField;
			}
			set {
				countryField = value;
			}
		}

		/// <remarks/>
		[XmlElementAttribute("Region")]
		public String_Type[] Region {
			get {
				return regionField;
			}
			set {
				regionField = value;
			}
		}

		/// <remarks/>
		public String_Type Address {
			get {
				return addressField;
			}
			set {
				addressField = value;
			}
		}
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(AnonymousType=true, Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class CatalogueFormat {

		private Vocabulary_Type textField;

		private Vocabulary_Type audioField;

		private Vocabulary_Type videoField;

		private Vocabulary_Type imageField;

		/// <remarks/>
		public Vocabulary_Type Text {
			get {
				return textField;
			}
			set {
				textField = value;
			}
		}

		/// <remarks/>
		public Vocabulary_Type Audio {
			get {
				return audioField;
			}
			set {
				audioField = value;
			}
		}

		/// <remarks/>
		public Vocabulary_Type Video {
			get {
				return videoField;
			}
			set {
				videoField = value;
			}
		}

		/// <remarks/>
		public Vocabulary_Type Image {
			get {
				return imageField;
			}
			set {
				imageField = value;
			}
		}
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(AnonymousType=true, Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class CatalogueQuality {

		private string textField;

		private string audioField;

		private string videoField;

		private string imageField;

		/// <remarks/>
		public string Text {
			get {
				return textField;
			}
			set {
				textField = value;
			}
		}

		/// <remarks/>
		public string Audio {
			get {
				return audioField;
			}
			set {
				audioField = value;
			}
		}

		/// <remarks/>
		public string Video {
			get {
				return videoField;
			}
			set {
				videoField = value;
			}
		}

		/// <remarks/>
		public string Image {
			get {
				return imageField;
			}
			set {
				imageField = value;
			}
		}
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class Date_Type
	{

		private string valueField;

		/// <remarks/>
		[XmlTextAttribute]
		public string Value {
			get {
				return valueField;
			}
			set {
				valueField = value;
			}
		}
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class Project : IMDIMajorObject
	{
		private String_Type idField;

		private Contact_Type contactField;

		/// <remarks/>
		public String_Type Id {
			get {
				return idField;
			}
			set {
				idField = value;
			}
		}

		/// <remarks/>
		public Contact_Type Contact {
			get {
				return contactField;
			}
			set {
				contactField = value;
			}
		}
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class Contact_Type
	{
		private String_Type nameField;

		private String_Type addressField;

		private String_Type emailField;

		private String_Type organisationField;

		/// <remarks/>
		public String_Type Name {
			get {
				return nameField;
			}
			set {
				nameField = value;
			}
		}

		/// <remarks/>
		public String_Type Address {
			get {
				return addressField;
			}
			set {
				addressField = value;
			}
		}

		/// <remarks/>
		public String_Type Email {
			get {
				return emailField;
			}
			set {
				emailField = value;
			}
		}

		/// <remarks/>
		public String_Type Organisation {
			get {
				return organisationField;
			}
			set {
				organisationField = value;
			}
		}
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class Access_Type
	{
		private Date_Type dateField;

		private String_Type ownerField;

		private String_Type publisherField;

		private Contact_Type contactField;

		private List<Description_Type> descriptionField;

		/// <remarks/>
		public Vocabulary_Type Availability { get; set; }

		/// <remarks/>
		public Date_Type Date {
			get {
				return dateField;
			}
			set {
				dateField = value;
			}
		}

		/// <remarks/>
		public String_Type Owner {
			get {
				return ownerField;
			}
			set {
				ownerField = value;
			}
		}

		/// <remarks/>
		public String_Type Publisher {
			get {
				return publisherField;
			}
			set {
				publisherField = value;
			}
		}

		/// <remarks/>
		public Contact_Type Contact {
			get {
				return contactField;
			}
			set {
				contactField = value;
			}
		}

		/// <remarks/>
		[XmlElementAttribute("Description")]
		public List<Description_Type> Description {
			get {
				if (descriptionField == null)
					descriptionField = new List<Description_Type>();
				return descriptionField;
			}
			set {
				descriptionField = value;
			}
		}
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class Keys_Type
	{

		private Key_Type[] keyField;

		/// <remarks/>
		[XmlElementAttribute("Key")]
		public Key_Type[] Key {
			get {
				return keyField;
			}
			set {
				keyField = value;
			}
		}
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class Corpus : IMDIMajorObject
	{
		private MDGroupType mDGroupField;

		private CorpusLink_Type[] corpusLinkField;

		private string searchServiceField;

		private string corpusStructureServiceField;

		private string catalogueLinkField;

		private string catalogueHandleField;

		/// <remarks/>
		public MDGroupType MDGroup {
			get {
				if (mDGroupField == null)
					mDGroupField = new MDGroupType();
				return mDGroupField;
			}
			set {
				mDGroupField = value;
			}
		}

		/// <remarks/>
		[XmlElementAttribute("CorpusLink")]
		public CorpusLink_Type[] CorpusLink {
			get {
				return corpusLinkField;
			}
			set {
				corpusLinkField = value;
			}
		}

		/// <remarks/>
		[XmlAttributeAttribute(DataType="anyURI")]
		public string SearchService {
			get {
				return searchServiceField;
			}
			set {
				searchServiceField = value;
			}
		}

		/// <remarks/>
		[XmlAttributeAttribute(DataType="anyURI")]
		public string CorpusStructureService {
			get {
				return corpusStructureServiceField;
			}
			set {
				corpusStructureServiceField = value;
			}
		}

		/// <remarks/>
		[XmlAttributeAttribute(DataType="anyURI")]
		public string CatalogueLink {
			get {
				return catalogueLinkField;
			}
			set {
				catalogueLinkField = value;
			}
		}

		/// <remarks/>
		[XmlAttributeAttribute]
		public string CatalogueHandle {
			get {
				return catalogueHandleField;
			}
			set {
				catalogueHandleField = value;
			}
		}
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class MDGroupType {

		private Location_Type locationField;

		private Project[] projectField;

		private Keys_Type keysField;

		private Content_Type contentField;

		private Actors_Type actorsField;

		/// <remarks/>
		public Location_Type Location {
			get {
				return locationField;
			}
			set {
				locationField = value;
			}
		}

		/// <remarks/>
		[XmlElementAttribute("Project")]
		public Project[] Project {
			get {
				return projectField;
			}
			set {
				projectField = value;
			}
		}

		/// <remarks/>
		public Keys_Type Keys {
			get {
				return keysField;
			}
			set {
				keysField = value;
			}
		}

		/// <remarks/>
		public Content_Type Content {
			get {
				return contentField;
			}
			set {
				contentField = value;
			}
		}

		/// <remarks/>
		public Actors_Type Actors {
			get {
				return actorsField;
			}
			set {
				actorsField = value;
			}
		}
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class Content_Type
	{
		private Vocabulary_Type genreField;

		private Vocabulary_Type subGenreField;

		private Vocabulary_Type taskField;

		private Vocabulary_Type modalitiesField;

		private Content_TypeSubject subjectField;

		private Content_TypeCommunicationContext communicationContextField;

		private Languages_Type languagesField;

		private Keys_Type keysField;

		private List<Description_Type> descriptionField;

		/// <remarks/>
		public Vocabulary_Type Genre {
			get {
				return genreField;
			}
			set {
				genreField = value;
			}
		}

		/// <remarks/>
		public Vocabulary_Type SubGenre {
			get {
				return subGenreField;
			}
			set {
				subGenreField = value;
			}
		}

		/// <remarks/>
		public Vocabulary_Type Task {
			get {
				return taskField;
			}
			set {
				taskField = value;
			}
		}

		/// <remarks/>
		public Vocabulary_Type Modalities {
			get {
				return modalitiesField;
			}
			set {
				modalitiesField = value;
			}
		}

		/// <remarks/>
		public Content_TypeSubject Subject {
			get {
				return subjectField;
			}
			set {
				subjectField = value;
			}
		}

		/// <remarks/>
		public Content_TypeCommunicationContext CommunicationContext {
			get {
				return communicationContextField;
			}
			set {
				communicationContextField = value;
			}
		}

		/// <remarks/>
		public Languages_Type Languages {
			get {
				return languagesField;
			}
			set {
				languagesField = value;
			}
		}

		/// <remarks/>
		public Keys_Type Keys {
			get {
				return keysField;
			}
			set {
				keysField = value;
			}
		}

		/// <remarks/>
		[XmlElementAttribute("Description")]
		public List<Description_Type> Description {
			get {
				if (descriptionField == null)
					descriptionField = new List<Description_Type>();
				return descriptionField;
			}
			set {
				descriptionField = value;
			}
		}
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(AnonymousType=true, Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class Content_TypeSubject : Vocabulary_Type
	{

		private string encodingField;

		/// <remarks/>
		[XmlAttributeAttribute]
		public string Encoding {
			get {
				return encodingField;
			}
			set {
				encodingField = value;
			}
		}
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(AnonymousType=true, Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class Content_TypeCommunicationContext
	{
		private Vocabulary_Type interactivityField;

		private Vocabulary_Type planningTypeField;

		private Vocabulary_Type involvementField;

		private Vocabulary_Type socialContextField;

		private Vocabulary_Type eventStructureField;

		private Vocabulary_Type channelField;

		/// <remarks/>
		public Vocabulary_Type Interactivity {
			get {
				return interactivityField;
			}
			set {
				interactivityField = value;
			}
		}

		/// <remarks/>
		public Vocabulary_Type PlanningType {
			get {
				return planningTypeField;
			}
			set {
				planningTypeField = value;
			}
		}

		/// <remarks/>
		public Vocabulary_Type Involvement {
			get {
				return involvementField;
			}
			set {
				involvementField = value;
			}
		}

		/// <remarks/>
		public Vocabulary_Type SocialContext {
			get {
				return socialContextField;
			}
			set {
				socialContextField = value;
			}
		}

		/// <remarks/>
		public Vocabulary_Type EventStructure {
			get {
				return eventStructureField;
			}
			set {
				eventStructureField = value;
			}
		}

		/// <remarks/>
		public Vocabulary_Type Channel {
			get {
				return channelField;
			}
			set {
				channelField = value;
			}
		}
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class Languages_Type
	{
		private List<Description_Type> descriptionField;

		private List<Language_Type> languageField;

		/// <remarks/>
		[XmlElementAttribute("Description")]
		public List<Description_Type> Description {
			get {
				if (descriptionField == null)
					descriptionField = new List<Description_Type>();
				return descriptionField;
			}
			set {
				descriptionField = value;
			}
		}

		/// <remarks/>
		[XmlElementAttribute("Language")]
		public List<Language_Type> Language {
			get {
				if (languageField == null)
					languageField = new List<Language_Type>();
				return languageField;
			}
			set {
				languageField = value;
			}
		}
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class Language_Type
	{
		private LanguageId_Type idField;

		private LanguageName_Type[] nameField;

		private Boolean_Type motherTongueField;

		private Boolean_Type primaryLanguageField;

		private Boolean_Type dominantField;

		private Boolean_Type sourceLanguageField;

		private Boolean_Type targetLanguageField;

		private List<Description_Type> descriptionField;

		private string resourceRefField;

		/// <remarks/>
		public LanguageId_Type Id {
			get {
				return idField;
			}
			set {
				idField = value;
			}
		}

		/// <remarks/>
		[XmlElementAttribute("Name")]
		public LanguageName_Type[] Name {
			get {
				return nameField;
			}
			set {
				nameField = value;
			}
		}

		/// <remarks/>
		public Boolean_Type MotherTongue {
			get {
				return motherTongueField;
			}
			set {
				motherTongueField = value;
			}
		}

		/// <remarks/>
		public Boolean_Type PrimaryLanguage {
			get {
				return primaryLanguageField;
			}
			set {
				primaryLanguageField = value;
			}
		}

		/// <remarks/>
		public Boolean_Type Dominant {
			get {
				return dominantField;
			}
			set {
				dominantField = value;
			}
		}

		/// <remarks/>
		public Boolean_Type SourceLanguage {
			get {
				return sourceLanguageField;
			}
			set {
				sourceLanguageField = value;
			}
		}

		/// <remarks/>
		public Boolean_Type TargetLanguage {
			get {
				return targetLanguageField;
			}
			set {
				targetLanguageField = value;
			}
		}

		/// <remarks/>
		[XmlElementAttribute("Description")]
		public List<Description_Type> Description {
			get {
				if (descriptionField == null)
					descriptionField = new List<Description_Type>();
				return descriptionField;
			}
			set {
				descriptionField = value;
			}
		}

		/// <remarks/>
		[XmlAttributeAttribute]
		public string ResourceRef {
			get {
				return resourceRefField;
			}
			set {
				resourceRefField = value;
			}
		}
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class Actors_Type
	{
		private List<Description_Type> descriptionField;

		private List<Actor_Type> actorField;

		/// <remarks/>
		[XmlElementAttribute("Description")]
		public List<Description_Type> Description {
			get {
				if (descriptionField == null)
					descriptionField = new List<Description_Type>();
				return descriptionField;
			}
			set {
				descriptionField = value;
			}
		}

		/// <remarks/>
		[XmlElementAttribute("Actor")]
		public List<Actor_Type> Actor {
			get {
				if (actorField == null)
					actorField = new List<Actor_Type>();
				return actorField;
			}
			set {
				actorField = value;
			}
		}
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class Actor_Type
	{
		private Vocabulary_Type roleField;

		private String_Type[] nameField;

		private String_Type fullNameField;

		private String_Type codeField;

		private Vocabulary_Type familySocialRoleField;

		private Languages_Type languagesField;

		private Vocabulary_Type ethnicGroupField;

		private AgeRange_Type ageField;

		private Date_Type birthDateField;

		private Vocabulary_Type sexField;

		private String_Type educationField;

		private Boolean_Type anonymizedField;

		private Contact_Type contactField;

		private Keys_Type keysField;

		private List<Description_Type> descriptionField;

		private string resourceRefField;

		/// <remarks/>
		public Vocabulary_Type Role {
			get {
				return roleField;
			}
			set {
				roleField = value;
			}
		}

		/// <remarks/>
		[XmlElementAttribute("Name")]
		public String_Type[] Name {
			get {
				return nameField;
			}
			set {
				nameField = value;
			}
		}

		/// <remarks/>
		public String_Type FullName {
			get {
				return fullNameField;
			}
			set {
				fullNameField = value;
			}
		}

		/// <remarks/>
		public String_Type Code {
			get {
				return codeField;
			}
			set {
				codeField = value;
			}
		}

		/// <remarks/>
		public Vocabulary_Type FamilySocialRole {
			get {
				return familySocialRoleField;
			}
			set {
				familySocialRoleField = value;
			}
		}

		/// <remarks/>
		public Languages_Type Languages {
			get {
				return languagesField;
			}
			set {
				languagesField = value;
			}
		}

		/// <remarks/>
		public Vocabulary_Type EthnicGroup {
			get {
				return ethnicGroupField;
			}
			set {
				ethnicGroupField = value;
			}
		}

		/// <remarks/>
		public AgeRange_Type Age {
			get {
				return ageField;
			}
			set {
				ageField = value;
			}
		}

		/// <remarks/>
		public Date_Type BirthDate {
			get {
				return birthDateField;
			}
			set {
				birthDateField = value;
			}
		}

		/// <remarks/>
		public Vocabulary_Type Sex {
			get {
				return sexField;
			}
			set {
				sexField = value;
			}
		}

		/// <remarks/>
		public String_Type Education {
			get {
				return educationField;
			}
			set {
				educationField = value;
			}
		}

		/// <remarks/>
		public Boolean_Type Anonymized {
			get {
				return anonymizedField;
			}
			set {
				anonymizedField = value;
			}
		}

		/// <remarks/>
		public Contact_Type Contact {
			get {
				return contactField;
			}
			set {
				contactField = value;
			}
		}

		/// <remarks/>
		public Keys_Type Keys {
			get {
				return keysField;
			}
			set {
				keysField = value;
			}
		}

		/// <remarks/>
		[XmlElementAttribute("Description")]
		public List<Description_Type> Description {
			get {
				if (descriptionField == null)
					descriptionField = new List<Description_Type>();
				return descriptionField;
			}
			set {
				descriptionField = value;
			}
		}

		/// <remarks/>
		[XmlAttributeAttribute]
		public string ResourceRef {
			get {
				return resourceRefField;
			}
			set {
				resourceRefField = value;
			}
		}
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class AgeRange_Type
	{

		private string valueField;

		/// <remarks/>
		[XmlTextAttribute]
		public string Value {
			get {
				return valueField;
			}
			set {
				valueField = value;
			}
		}
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class CorpusLink_Type : ResourceLink_Type
	{

		private string nameField;

		/// <remarks/>
		[XmlAttributeAttribute]
		public string Name {
			get {
				return nameField;
			}
			set {
				nameField = value;
			}
		}
	}

	/// <remarks/>
	[XmlIncludeAttribute(typeof(CorpusLink_Type))]
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class ResourceLink_Type
	{
		private string archiveHandleField;

		private string valueField;

		/// <remarks/>
		[XmlAttributeAttribute]
		public string ArchiveHandle {
			get {
				return archiveHandleField;
			}
			set {
				archiveHandleField = value;
			}
		}

		/// <remarks/>
		[XmlTextAttribute(DataType="anyURI")]
		public string Value {
			get {
				return valueField;
			}
			set {
				valueField = value;
			}
		}
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class Anonyms_Type
	{
		private ResourceLink_Type resourceLinkField;

		private Access_Type accessField;

		/// <remarks/>
		public ResourceLink_Type ResourceLink {
			get {
				return resourceLinkField;
			}
			set {
				resourceLinkField = value;
			}
		}

		/// <remarks/>
		public Access_Type Access {
			get {
				return accessField;
			}
			set {
				accessField = value;
			}
		}
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class CounterPosition_Type
	{
		private Integer_Type startField;

		private Integer_Type endField;

		/// <remarks/>
		public Integer_Type Start {
			get {
				return startField;
			}
			set {
				startField = value;
			}
		}

		/// <remarks/>
		public Integer_Type End {
			get {
				return endField;
			}
			set {
				endField = value;
			}
		}
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class Integer_Type
	{
		private string valueField;

		/// <remarks/>
		[XmlTextAttribute]
		public string Value {
			get {
				return valueField;
			}
			set {
				valueField = value;
			}
		}
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class Source_Type
	{
		private Source_TypeID idField;

		private Vocabulary_Type formatField;

		private Quality_Type qualityField;

		private CounterPosition_Type counterPositionField;

		private TimePositionRange_Type timePositionField;

		private Access_Type accessField;

		private List<Description_Type> descriptionField;

		private Keys_Type keysField;

		private string resourceRefsField;

		/// <remarks/>
		public Source_TypeID Id {
			get {
				return idField;
			}
			set {
				idField = value;
			}
		}

		/// <remarks/>
		public Vocabulary_Type Format {
			get {
				return formatField;
			}
			set {
				formatField = value;
			}
		}

		/// <remarks/>
		public Quality_Type Quality {
			get {
				return qualityField;
			}
			set {
				qualityField = value;
			}
		}

		/// <remarks/>
		public CounterPosition_Type CounterPosition {
			get {
				return counterPositionField;
			}
			set {
				counterPositionField = value;
			}
		}

		/// <remarks/>
		public TimePositionRange_Type TimePosition {
			get {
				return timePositionField;
			}
			set {
				timePositionField = value;
			}
		}

		/// <remarks/>
		public Access_Type Access {
			get {
				return accessField;
			}
			set {
				accessField = value;
			}
		}

		/// <remarks/>
		[XmlElementAttribute("Description")]
		public List<Description_Type> Description {
			get {
				if (descriptionField == null)
					descriptionField = new List<Description_Type>();
				return descriptionField;
			}
			set {
				descriptionField = value;
			}
		}

		/// <remarks/>
		public Keys_Type Keys {
			get {
				return keysField;
			}
			set {
				keysField = value;
			}
		}

		/// <remarks/>
		[XmlAttributeAttribute]
		public string ResourceRefs {
			get {
				return resourceRefsField;
			}
			set {
				resourceRefsField = value;
			}
		}
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(AnonymousType=true, Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class Source_TypeID {

		private string valueField;

		/// <remarks/>
		[XmlTextAttribute]
		public string Value {
			get {
				return valueField;
			}
			set {
				valueField = value;
			}
		}
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class Quality_Type
	{

		private string linkField;

		private VocabularyType_Value_Type typeField;

		private string valueField;

		/// <remarks/>
		public Quality_Type() {
			typeField = VocabularyType_Value_Type.ClosedVocabulary;
		}

		/// <remarks/>
		[XmlAttributeAttribute(DataType="anyURI")]
		public string Link {
			get {
				return linkField;
			}
			set {
				linkField = value;
			}
		}

		/// <remarks/>
		[XmlAttributeAttribute]
		[DefaultValueAttribute(VocabularyType_Value_Type.ClosedVocabulary)]
		public VocabularyType_Value_Type Type {
			get {
				return typeField;
			}
			set {
				typeField = value;
			}
		}

		/// <remarks/>
		[XmlTextAttribute]
		public string Value {
			get {
				return valueField;
			}
			set {
				valueField = value;
			}
		}
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class TimePositionRange_Type
	{

		private TimePosition_Type startField;

		private TimePosition_Type endField;

		/// <remarks/>
		public TimePosition_Type Start {
			get {
				return startField;
			}
			set {
				startField = value;
			}
		}

		/// <remarks/>
		public TimePosition_Type End {
			get {
				return endField;
			}
			set {
				endField = value;
			}
		}
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class TimePosition_Type
	{

		private string valueField;

		/// <remarks/>
		[XmlTextAttribute]
		public string Value {
			get {
				return valueField;
			}
			set {
				valueField = value;
			}
		}
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class LexiconComponent_Type
	{

		private ResourceLink_Type resourceLinkField;

		private Date_Type dateField;

		private Vocabulary_Type typeField;

		private Vocabulary_Type formatField;

		private String_Type characterEncodingField;

		private LexiconComponent_TypeSize sizeField;

		private LexiconComponent_TypeComponent componentField;

		private LexiconComponent_TypeLexicalInfo lexicalInfoField;

		private LexiconComponent_TypeMetaLanguages metaLanguagesField;

		private Access_Type accessField;

		private List<Description_Type> descriptionField;

		private Keys_Type keysField;

		private string resourceIdField;

		/// <remarks/>
		public ResourceLink_Type ResourceLink {
			get {
				return resourceLinkField;
			}
			set {
				resourceLinkField = value;
			}
		}

		/// <remarks/>
		public Date_Type Date {
			get {
				return dateField;
			}
			set {
				dateField = value;
			}
		}

		/// <remarks/>
		public Vocabulary_Type Type {
			get {
				return typeField;
			}
			set {
				typeField = value;
			}
		}

		/// <remarks/>
		public Vocabulary_Type Format {
			get {
				return formatField;
			}
			set {
				formatField = value;
			}
		}

		/// <remarks/>
		public String_Type CharacterEncoding {
			get {
				return characterEncodingField;
			}
			set {
				characterEncodingField = value;
			}
		}

		/// <remarks/>
		public LexiconComponent_TypeSize Size {
			get {
				return sizeField;
			}
			set {
				sizeField = value;
			}
		}

		/// <remarks/>
		public LexiconComponent_TypeComponent Component {
			get {
				return componentField;
			}
			set {
				componentField = value;
			}
		}

		/// <remarks/>
		public LexiconComponent_TypeLexicalInfo LexicalInfo {
			get {
				return lexicalInfoField;
			}
			set {
				lexicalInfoField = value;
			}
		}

		/// <remarks/>
		public LexiconComponent_TypeMetaLanguages MetaLanguages {
			get {
				return metaLanguagesField;
			}
			set {
				metaLanguagesField = value;
			}
		}

		/// <remarks/>
		public Access_Type Access {
			get {
				return accessField;
			}
			set {
				accessField = value;
			}
		}

		/// <remarks/>
		[XmlElementAttribute("Description")]
		public List<Description_Type> Description {
			get {
				if (descriptionField == null)
					descriptionField = new List<Description_Type>();
				return descriptionField;
			}
			set {
				descriptionField = value;
			}
		}

		/// <remarks/>
		public Keys_Type Keys {
			get {
				return keysField;
			}
			set {
				keysField = value;
			}
		}

		/// <remarks/>
		[XmlAttributeAttribute]
		public string ResourceId {
			get {
				return resourceIdField;
			}
			set {
				resourceIdField = value;
			}
		}
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(AnonymousType=true, Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class LexiconComponent_TypeSize {

		private string valueField;

		/// <remarks/>
		[XmlTextAttribute]
		public string Value {
			get {
				return valueField;
			}
			set {
				valueField = value;
			}
		}
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(AnonymousType=true, Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class LexiconComponent_TypeComponent {

		private Vocabulary_Type possibleParentsField;

		private Vocabulary_Type preferredParentField;

		private LexiconComponent_TypeComponentChildNodes childNodesField;

		/// <remarks/>
		public Vocabulary_Type possibleParents {
			get {
				return possibleParentsField;
			}
			set {
				possibleParentsField = value;
			}
		}

		/// <remarks/>
		public Vocabulary_Type preferredParent {
			get {
				return preferredParentField;
			}
			set {
				preferredParentField = value;
			}
		}

		/// <remarks/>
		public LexiconComponent_TypeComponentChildNodes childNodes {
			get {
				return childNodesField;
			}
			set {
				childNodesField = value;
			}
		}
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(AnonymousType=true, Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class LexiconComponent_TypeComponentChildNodes {

		private Vocabulary_Type childComponentsField;

		private Vocabulary_Type childCategoriesField;

		/// <remarks/>
		public Vocabulary_Type childComponents {
			get {
				return childComponentsField;
			}
			set {
				childComponentsField = value;
			}
		}

		/// <remarks/>
		public Vocabulary_Type childCategories {
			get {
				return childCategoriesField;
			}
			set {
				childCategoriesField = value;
			}
		}
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(AnonymousType=true, Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class LexiconComponent_TypeLexicalInfo {

		private bool orthographyField;

		private bool orthographyFieldSpecified;

		private bool morphologyField;

		private bool morphologyFieldSpecified;

		private bool morphoSyntaxField;

		private bool morphoSyntaxFieldSpecified;

		private bool syntaxField;

		private bool syntaxFieldSpecified;

		private bool phonologyField;

		private bool phonologyFieldSpecified;

		private bool semanticsField;

		private bool semanticsFieldSpecified;

		private bool etymologyField;

		private bool etymologyFieldSpecified;

		private bool usageField;

		private bool usageFieldSpecified;

		private bool frequencyField;

		private bool frequencyFieldSpecified;

		/// <remarks/>
		public bool Orthography {
			get {
				return orthographyField;
			}
			set {
				orthographyField = value;
			}
		}

		/// <remarks/>
		[XmlIgnoreAttribute]
		public bool OrthographySpecified {
			get {
				return orthographyFieldSpecified;
			}
			set {
				orthographyFieldSpecified = value;
			}
		}

		/// <remarks/>
		public bool Morphology {
			get {
				return morphologyField;
			}
			set {
				morphologyField = value;
			}
		}

		/// <remarks/>
		[XmlIgnoreAttribute]
		public bool MorphologySpecified {
			get {
				return morphologyFieldSpecified;
			}
			set {
				morphologyFieldSpecified = value;
			}
		}

		/// <remarks/>
		public bool MorphoSyntax {
			get {
				return morphoSyntaxField;
			}
			set {
				morphoSyntaxField = value;
			}
		}

		/// <remarks/>
		[XmlIgnoreAttribute]
		public bool MorphoSyntaxSpecified {
			get {
				return morphoSyntaxFieldSpecified;
			}
			set {
				morphoSyntaxFieldSpecified = value;
			}
		}

		/// <remarks/>
		public bool Syntax {
			get {
				return syntaxField;
			}
			set {
				syntaxField = value;
			}
		}

		/// <remarks/>
		[XmlIgnoreAttribute]
		public bool SyntaxSpecified {
			get {
				return syntaxFieldSpecified;
			}
			set {
				syntaxFieldSpecified = value;
			}
		}

		/// <remarks/>
		public bool Phonology {
			get {
				return phonologyField;
			}
			set {
				phonologyField = value;
			}
		}

		/// <remarks/>
		[XmlIgnoreAttribute]
		public bool PhonologySpecified {
			get {
				return phonologyFieldSpecified;
			}
			set {
				phonologyFieldSpecified = value;
			}
		}

		/// <remarks/>
		public bool Semantics {
			get {
				return semanticsField;
			}
			set {
				semanticsField = value;
			}
		}

		/// <remarks/>
		[XmlIgnoreAttribute]
		public bool SemanticsSpecified {
			get {
				return semanticsFieldSpecified;
			}
			set {
				semanticsFieldSpecified = value;
			}
		}

		/// <remarks/>
		public bool Etymology {
			get {
				return etymologyField;
			}
			set {
				etymologyField = value;
			}
		}

		/// <remarks/>
		[XmlIgnoreAttribute]
		public bool EtymologySpecified {
			get {
				return etymologyFieldSpecified;
			}
			set {
				etymologyFieldSpecified = value;
			}
		}

		/// <remarks/>
		public bool Usage {
			get {
				return usageField;
			}
			set {
				usageField = value;
			}
		}

		/// <remarks/>
		[XmlIgnoreAttribute]
		public bool UsageSpecified {
			get {
				return usageFieldSpecified;
			}
			set {
				usageFieldSpecified = value;
			}
		}

		/// <remarks/>
		public bool Frequency {
			get {
				return frequencyField;
			}
			set {
				frequencyField = value;
			}
		}

		/// <remarks/>
		[XmlIgnoreAttribute]
		public bool FrequencySpecified {
			get {
				return frequencyFieldSpecified;
			}
			set {
				frequencyFieldSpecified = value;
			}
		}
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(AnonymousType=true, Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class LexiconComponent_TypeMetaLanguages {

		private Vocabulary_Type languageField;

		private List<Description_Type> descriptionField;

		/// <remarks/>
		public Vocabulary_Type Language {
			get {
				return languageField;
			}
			set {
				languageField = value;
			}
		}

		/// <remarks/>
		[XmlElementAttribute("Description")]
		public List<Description_Type> Description {
			get {
				if (descriptionField == null)
					descriptionField = new List<Description_Type>();
				return descriptionField;
			}
			set {
				descriptionField = value;
			}
		}
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class LexiconResource_Type
	{

		private ResourceLink_Type resourceLinkField;

		private Date_Type dateField;

		private Vocabulary_Type typeField;

		private Vocabulary_Type formatField;

		private String_Type characterEncodingField;

		private LexiconResource_TypeSize sizeField;

		private Integer_Type noHeadEntriesField;

		private Integer_Type noSubEntriesField;

		private LexiconResource_TypeLexicalEntry[] lexicalEntryField;

		private LexiconResource_TypeMetaLanguages metaLanguagesField;

		private Access_Type accessField;

		private List<Description_Type> descriptionField;

		private Keys_Type keysField;

		private string resourceIdField;

		/// <remarks/>
		public ResourceLink_Type ResourceLink {
			get {
				return resourceLinkField;
			}
			set {
				resourceLinkField = value;
			}
		}

		/// <remarks/>
		public Date_Type Date {
			get {
				return dateField;
			}
			set {
				dateField = value;
			}
		}

		/// <remarks/>
		public Vocabulary_Type Type {
			get {
				return typeField;
			}
			set {
				typeField = value;
			}
		}

		/// <remarks/>
		public Vocabulary_Type Format {
			get {
				return formatField;
			}
			set {
				formatField = value;
			}
		}

		/// <remarks/>
		public String_Type CharacterEncoding {
			get {
				return characterEncodingField;
			}
			set {
				characterEncodingField = value;
			}
		}

		/// <remarks/>
		public LexiconResource_TypeSize Size {
			get {
				return sizeField;
			}
			set {
				sizeField = value;
			}
		}

		/// <remarks/>
		public Integer_Type NoHeadEntries {
			get {
				return noHeadEntriesField;
			}
			set {
				noHeadEntriesField = value;
			}
		}

		/// <remarks/>
		public Integer_Type NoSubEntries {
			get {
				return noSubEntriesField;
			}
			set {
				noSubEntriesField = value;
			}
		}

		/// <remarks/>
		[XmlElementAttribute("LexicalEntry")]
		public LexiconResource_TypeLexicalEntry[] LexicalEntry {
			get {
				return lexicalEntryField;
			}
			set {
				lexicalEntryField = value;
			}
		}

		/// <remarks/>
		public LexiconResource_TypeMetaLanguages MetaLanguages {
			get {
				return metaLanguagesField;
			}
			set {
				metaLanguagesField = value;
			}
		}

		/// <remarks/>
		public Access_Type Access {
			get {
				return accessField;
			}
			set {
				accessField = value;
			}
		}

		/// <remarks/>
		[XmlElementAttribute("Description")]
		public List<Description_Type> Description {
			get {
				if (descriptionField == null)
					descriptionField = new List<Description_Type>();
				return descriptionField;
			}
			set {
				descriptionField = value;
			}
		}

		/// <remarks/>
		public Keys_Type Keys {
			get {
				return keysField;
			}
			set {
				keysField = value;
			}
		}

		/// <remarks/>
		[XmlAttributeAttribute]
		public string ResourceId {
			get {
				return resourceIdField;
			}
			set {
				resourceIdField = value;
			}
		}
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(AnonymousType=true, Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class LexiconResource_TypeSize {

		private string valueField;

		/// <remarks/>
		[XmlTextAttribute]
		public string Value {
			get {
				return valueField;
			}
			set {
				valueField = value;
			}
		}
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(AnonymousType=true, Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class LexiconResource_TypeLexicalEntry {

		private Vocabulary_Type headWordTypeField;

		private Vocabulary_Type orthographyField;

		private Vocabulary_Type morphologyField;

		private Vocabulary_Type morphoSyntaxField;

		private Vocabulary_Type syntaxField;

		private Vocabulary_Type phonologyField;

		private Vocabulary_Type semanticsField;

		private Vocabulary_Type etymologyField;

		private Vocabulary_Type usageField;

		private String_Type frequencyField;

		/// <remarks/>
		public Vocabulary_Type HeadWordType {
			get {
				return headWordTypeField;
			}
			set {
				headWordTypeField = value;
			}
		}

		/// <remarks/>
		public Vocabulary_Type Orthography {
			get {
				return orthographyField;
			}
			set {
				orthographyField = value;
			}
		}

		/// <remarks/>
		public Vocabulary_Type Morphology {
			get {
				return morphologyField;
			}
			set {
				morphologyField = value;
			}
		}

		/// <remarks/>
		public Vocabulary_Type MorphoSyntax {
			get {
				return morphoSyntaxField;
			}
			set {
				morphoSyntaxField = value;
			}
		}

		/// <remarks/>
		public Vocabulary_Type Syntax {
			get {
				return syntaxField;
			}
			set {
				syntaxField = value;
			}
		}

		/// <remarks/>
		public Vocabulary_Type Phonology {
			get {
				return phonologyField;
			}
			set {
				phonologyField = value;
			}
		}

		/// <remarks/>
		public Vocabulary_Type Semantics {
			get {
				return semanticsField;
			}
			set {
				semanticsField = value;
			}
		}

		/// <remarks/>
		public Vocabulary_Type Etymology {
			get {
				return etymologyField;
			}
			set {
				etymologyField = value;
			}
		}

		/// <remarks/>
		public Vocabulary_Type Usage {
			get {
				return usageField;
			}
			set {
				usageField = value;
			}
		}

		/// <remarks/>
		public String_Type Frequency {
			get {
				return frequencyField;
			}
			set {
				frequencyField = value;
			}
		}
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(AnonymousType=true, Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class LexiconResource_TypeMetaLanguages {

		private Vocabulary_Type[] languageField;

		private List<Description_Type> descriptionField;

		/// <remarks/>
		[XmlElementAttribute("Language")]
		public Vocabulary_Type[] Language {
			get {
				return languageField;
			}
			set {
				languageField = value;
			}
		}

		/// <remarks/>
		[XmlElementAttribute("Description")]
		public List<Description_Type> Description {
			get {
				if (descriptionField == null)
					descriptionField = new List<Description_Type>();
				return descriptionField;
			}
			set {
				descriptionField = value;
			}
		}
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class Validation_Type
	{

		private Vocabulary_Type typeField;

		private Vocabulary_Type methodologyField;

		private Integer_Type levelField;

		private List<Description_Type> descriptionField;

		/// <remarks/>
		public Vocabulary_Type Type {
			get {
				return typeField;
			}
			set {
				typeField = value;
			}
		}

		/// <remarks/>
		public Vocabulary_Type Methodology {
			get {
				return methodologyField;
			}
			set {
				methodologyField = value;
			}
		}

		/// <remarks/>
		public Integer_Type Level {
			get {
				return levelField;
			}
			set {
				levelField = value;
			}
		}

		/// <remarks/>
		[XmlElementAttribute("Description")]
		public List<Description_Type> Description {
			get {
				if (descriptionField == null)
					descriptionField = new List<Description_Type>();
				return descriptionField;
			}
			set {
				descriptionField = value;
			}
		}
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class WrittenResource_Type : IIMDISessionFile {

		private ResourceLink_Type resourceLinkField;

		private ResourceLink_Type mediaResourceLinkField;

		private WrittenResource_TypeDate dateField;

		private Vocabulary_Type typeField;

		private Vocabulary_Type subTypeField;

		private Vocabulary_Type formatField;

		private String_Type sizeField;

		private Validation_Type validationField;

		private Vocabulary_Type derivationField;

		private String_Type characterEncodingField;

		private String_Type contentEncodingField;

		private Vocabulary_Type languageIdField;

		private Boolean_Type anonymizedField;

		private Access_Type accessField;

		private List<Description_Type> descriptionField;

		private Keys_Type keysField;

		private string resourceIdField;

		/// <remarks/>
		public ResourceLink_Type ResourceLink {
			get {
				return resourceLinkField;
			}
			set {
				resourceLinkField = value;
			}
		}

		/// <remarks/>
		public ResourceLink_Type MediaResourceLink {
			get {
				return mediaResourceLinkField;
			}
			set {
				mediaResourceLinkField = value;
			}
		}

		/// <remarks/>
		public WrittenResource_TypeDate Date {
			get {
				return dateField;
			}
			set {
				dateField = value;
			}
		}

		/// <remarks/>
		public Vocabulary_Type Type {
			get {
				return typeField;
			}
			set {
				typeField = value;
			}
		}

		/// <remarks/>
		public Vocabulary_Type SubType {
			get {
				return subTypeField;
			}
			set {
				subTypeField = value;
			}
		}

		/// <remarks/>
		public Vocabulary_Type Format {
			get {
				return formatField;
			}
			set {
				formatField = value;
			}
		}

		/// <remarks/>
		public String_Type Size {
			get {
				return sizeField;
			}
			set {
				sizeField = value;
			}
		}

		/// <remarks/>
		public Validation_Type Validation {
			get {
				return validationField;
			}
			set {
				validationField = value;
			}
		}

		/// <remarks/>
		public Vocabulary_Type Derivation {
			get {
				return derivationField;
			}
			set {
				derivationField = value;
			}
		}

		/// <remarks/>
		public String_Type CharacterEncoding {
			get {
				return characterEncodingField;
			}
			set {
				characterEncodingField = value;
			}
		}

		/// <remarks/>
		public String_Type ContentEncoding {
			get {
				return contentEncodingField;
			}
			set {
				contentEncodingField = value;
			}
		}

		/// <remarks/>
		public Vocabulary_Type LanguageId {
			get {
				return languageIdField;
			}
			set {
				languageIdField = value;
			}
		}

		/// <remarks/>
		public Boolean_Type Anonymized {
			get {
				return anonymizedField;
			}
			set {
				anonymizedField = value;
			}
		}

		/// <remarks/>
		public Access_Type Access {
			get {
				return accessField;
			}
			set {
				accessField = value;
			}
		}

		/// <remarks/>
		[XmlElementAttribute("Description")]
		public List<Description_Type> Description {
			get {
				if (descriptionField == null)
					descriptionField = new List<Description_Type>();
				return descriptionField;
			}
			set {
				descriptionField = value;
			}
		}

		/// <remarks/>
		public Keys_Type Keys {
			get {
				return keysField;
			}
			set {
				keysField = value;
			}
		}

		/// <remarks/>
		[XmlAttributeAttribute]
		public string ResourceId {
			get {
				return resourceIdField;
			}
			set {
				resourceIdField = value;
			}
		}
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(AnonymousType=true, Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class WrittenResource_TypeDate : Date_Type
	{
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class MediaFile_Type : IIMDISessionFile {

		private ResourceLink_Type resourceLinkField;

		private Vocabulary_Type typeField;

		private Vocabulary_Type formatField;

		private String_Type sizeField;

		private Quality_Type qualityField;

		private String_Type recordingConditionsField;

		private TimePositionRange_Type timePositionField;

		private Access_Type accessField;

		private List<Description_Type> descriptionField;

		private Keys_Type keysField;

		private string resourceIdField;

		/// <remarks/>
		public ResourceLink_Type ResourceLink {
			get {
				return resourceLinkField;
			}
			set {
				resourceLinkField = value;
			}
		}

		/// <remarks/>
		public Vocabulary_Type Type {
			get {
				return typeField;
			}
			set {
				typeField = value;
			}
		}

		/// <remarks/>
		public Vocabulary_Type Format {
			get {
				return formatField;
			}
			set {
				formatField = value;
			}
		}

		/// <remarks/>
		public String_Type Size {
			get {
				return sizeField;
			}
			set {
				sizeField = value;
			}
		}

		/// <remarks/>
		public Quality_Type Quality {
			get {
				return qualityField;
			}
			set {
				qualityField = value;
			}
		}

		/// <remarks/>
		public String_Type RecordingConditions {
			get {
				return recordingConditionsField;
			}
			set {
				recordingConditionsField = value;
			}
		}

		/// <remarks/>
		public TimePositionRange_Type TimePosition {
			get {
				return timePositionField;
			}
			set {
				timePositionField = value;
			}
		}

		/// <remarks/>
		public Access_Type Access {
			get {
				return accessField;
			}
			set {
				accessField = value;
			}
		}

		/// <remarks/>
		[XmlElementAttribute("Description")]
		public List<Description_Type> Description {
			get {
				if (descriptionField == null)
					descriptionField = new List<Description_Type>();
				return descriptionField;
			}
			set {
				descriptionField = value;
			}
		}

		/// <remarks/>
		public Keys_Type Keys {
			get {
				return keysField;
			}
			set {
				keysField = value;
			}
		}

		/// <remarks/>
		[XmlAttributeAttribute]
		public string ResourceId {
			get {
				return resourceIdField;
			}
			set {
				resourceIdField = value;
			}
		}
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class ExternalResourceReference_Type
	{

		private Vocabulary_Type typeField;

		private Vocabulary_Type subTypeField;

		private Vocabulary_Type formatField;

		private string linkField;

		/// <remarks/>
		public Vocabulary_Type Type {
			get {
				return typeField;
			}
			set {
				typeField = value;
			}
		}

		/// <remarks/>
		public Vocabulary_Type SubType {
			get {
				return subTypeField;
			}
			set {
				subTypeField = value;
			}
		}

		/// <remarks/>
		public Vocabulary_Type Format {
			get {
				return formatField;
			}
			set {
				formatField = value;
			}
		}

		/// <remarks/>
		[XmlElementAttribute(DataType="anyURI")]
		public string Link {
			get {
				return linkField;
			}
			set {
				linkField = value;
			}
		}
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class DateRange_Type
	{

		private string valueField;

		/// <remarks/>
		[XmlTextAttribute]
		public string Value {
			get {
				return valueField;
			}
			set {
				valueField = value;
			}
		}
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class Session : IMDIMajorObject, IArchivingSession
	{
		private ExternalResourceReference_Type[] externalResourceReferenceField;

		private MDGroupType mDGroupField;

		private SessionResources resourcesField;

		private SessionReferences referencesField;

		/// <remarks/>
		public DateRange_Type Date { get; set; }

		/// <remarks/>
		[XmlElementAttribute("ExternalResourceReference")]
		public ExternalResourceReference_Type[] ExternalResourceReference {
			get {
				return externalResourceReferenceField;
			}
			set {
				externalResourceReferenceField = value;
			}
		}

		/// <remarks/>
		public MDGroupType MDGroup {
			get {
				if (mDGroupField == null)
					mDGroupField = new MDGroupType();
				return mDGroupField;
			}
			set {
				mDGroupField = value;
			}
		}

		/// <remarks/>
		public SessionResources Resources {
			get {
				return resourcesField;
			}
			set {
				resourcesField = value;
			}
		}

		/// <remarks/>
		public SessionReferences References {
			get {
				return referencesField;
			}
			set {
				referencesField = value;
			}
		}

		// Don't know which of the following we really need:

		[XmlIgnore]
		public DateTime DateCreatedFirst { get; set; }
		[XmlIgnore]
		public DateTime DateCreatedLast { get; set; }
		[XmlIgnore]
		public DateTime DateModified { get; set; }
		[XmlIgnore]
		public IAccessProtocol AccessProtocol { get; set; }
		[XmlIgnore]
		public ArchivingLocation Location { get; set; }
		[XmlIgnore]
		public ArchivingActorCollection Actors { get; set; }
		[XmlIgnore]
		public List<IArchivingFile> Files { get; set; }
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(AnonymousType=true, Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class SessionResources {

		private List<MediaFile_Type> mediaFileField;

		private List<WrittenResource_Type> writtenResourceField;

		private LexiconResource_Type[] lexiconResourceField;

		private LexiconComponent_Type[] lexiconComponentField;

		private Source_Type[] sourceField;

		private Anonyms_Type anonymsField;

		/// <remarks/>
		[XmlElementAttribute("MediaFile")]
		public List<MediaFile_Type> MediaFile {
			get {
				if (mediaFileField == null)
					mediaFileField = new List<MediaFile_Type>();
				return mediaFileField;
			}
			set {
				mediaFileField = value;
			}
		}

		/// <remarks/>
		[XmlElementAttribute("WrittenResource")]
		public List<WrittenResource_Type> WrittenResource {
			get {
				if (writtenResourceField == null)
					writtenResourceField = new List<WrittenResource_Type>();
				return writtenResourceField;
			}
			set {
				writtenResourceField = value;
			}
		}

		/// <remarks/>
		[XmlElementAttribute("LexiconResource")]
		public LexiconResource_Type[] LexiconResource {
			get {
				return lexiconResourceField;
			}
			set {
				lexiconResourceField = value;
			}
		}

		/// <remarks/>
		[XmlElementAttribute("LexiconComponent")]
		public LexiconComponent_Type[] LexiconComponent {
			get {
				return lexiconComponentField;
			}
			set {
				lexiconComponentField = value;
			}
		}

		/// <remarks/>
		[XmlElementAttribute("Source")]
		public Source_Type[] Source {
			get {
				return sourceField;
			}
			set {
				sourceField = value;
			}
		}

		/// <remarks/>
		public Anonyms_Type Anonyms {
			get {
				return anonymsField;
			}
			set {
				anonymsField = value;
			}
		}
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(AnonymousType=true, Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class SessionReferences {

		private List<Description_Type> descriptionField;

		/// <remarks/>
		[XmlElementAttribute("Description")]
		public List<Description_Type> Description {
			get {
				if (descriptionField == null)
					descriptionField = new List<Description_Type>();
				return descriptionField;
			}
			set {
				descriptionField = value;
			}
		}
	}

	/// <remarks/>
	[SerializableAttribute]
	[XmlTypeAttribute(Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public enum Metatranscript_Value_Type
	{

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
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	[XmlRootAttribute("VocabularyDef", Namespace="http://www.mpi.nl/IMDI/Schema/IMDI", IsNullable=false)]
	public class VocabularyDef_Type
	{

		private List<Description_Type> descriptionField;

		private VocabularyDef_TypeEntry[] entryField;

		private string nameField;

		private DateTime dateField;

		private DateTime tagField;

		private bool tagFieldSpecified;

		private string linkField;

		/// <remarks/>
		[XmlElementAttribute("Description")]
		public List<Description_Type> Description {
			get {
				if (descriptionField == null)
					descriptionField = new List<Description_Type>();
				return descriptionField;
			}
			set {
				descriptionField = value;
			}
		}

		/// <remarks/>
		[XmlElementAttribute("Entry")]
		public VocabularyDef_TypeEntry[] Entry {
			get {
				return entryField;
			}
			set {
				entryField = value;
			}
		}

		/// <remarks/>
		[XmlAttributeAttribute]
		public string Name {
			get {
				return nameField;
			}
			set {
				nameField = value;
			}
		}

		/// <remarks/>
		[XmlAttributeAttribute(DataType="date")]
		public DateTime Date {
			get {
				return dateField;
			}
			set {
				dateField = value;
			}
		}

		/// <remarks/>
		[XmlAttributeAttribute(DataType="date")]
		public DateTime Tag {
			get {
				return tagField;
			}
			set {
				tagField = value;
			}
		}

		/// <remarks/>
		[XmlIgnoreAttribute]
		public bool TagSpecified {
			get {
				return tagFieldSpecified;
			}
			set {
				tagFieldSpecified = value;
			}
		}

		/// <remarks/>
		[XmlAttributeAttribute(DataType="anyURI")]
		public string Link {
			get {
				return linkField;
			}
			set {
				linkField = value;
			}
		}
	}

	/// <remarks/>
	[SerializableAttribute]
	[DebuggerStepThroughAttribute]
	[XmlTypeAttribute(AnonymousType=true, Namespace="http://www.mpi.nl/IMDI/Schema/IMDI")]
	public class VocabularyDef_TypeEntry {

		private string tagField;

		private string valueField;

		private string value1Field;

		/// <remarks/>
		[XmlAttributeAttribute]
		public string Tag {
			get {
				return tagField;
			}
			set {
				tagField = value;
			}
		}

		/// <remarks/>
		[XmlAttributeAttribute]
		public string Value {
			get {
				return valueField;
			}
			set {
				valueField = value;
			}
		}

		/// <remarks/>
		[XmlTextAttribute]
		public string Value1 {
			get {
				return value1Field;
			}
			set {
				value1Field = value;
			}
		}
	}
}
