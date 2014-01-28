
// ReSharper disable once EmptyNamespace
// ReSharper disable once CheckNamespace
namespace SIL.Archiving.IMDI.Schema.NotUsed
{
	///// <remarks/>
	//[SerializableAttribute]
	//[DebuggerStepThrough]
	//[XmlType(Namespace = "http://www.mpi.nl/IMDI/Schema/IMDI")]
	//[XmlRoot("VocabularyDef", Namespace = "http://www.mpi.nl/IMDI/Schema/IMDI", IsNullable = false)]
	//public class VocabularyDef_Type
	//{

	//    private List<DescriptionType> descriptionField;

	//    private VocabularyDef_TypeEntry[] entryField;

	//    private string nameField;

	//    private DateTime dateField;

	//    private DateTime tagField;

	//    private bool tagFieldSpecified;

	//    private string linkField;

	//    /// <remarks/>
	//    [XmlElement("Description")]
	//    public List<DescriptionType> Description
	//    {
	//        get
	//        {
	//            if (descriptionField == null)
	//                descriptionField = new List<DescriptionType>();
	//            return descriptionField;
	//        }
	//        set
	//        {
	//            descriptionField = value;
	//        }
	//    }

	//    /// <remarks/>
	//    [XmlElement("Entry")]
	//    public VocabularyDef_TypeEntry[] Entry
	//    {
	//        get
	//        {
	//            return entryField;
	//        }
	//        set
	//        {
	//            entryField = value;
	//        }
	//    }

	//    /// <remarks/>
	//    [XmlAttribute]
	//    public string Name
	//    {
	//        get
	//        {
	//            return nameField;
	//        }
	//        set
	//        {
	//            nameField = value;
	//        }
	//    }

	//    /// <remarks/>
	//    [XmlAttribute(DataType = "date")]
	//    public DateTime Date
	//    {
	//        get
	//        {
	//            return dateField;
	//        }
	//        set
	//        {
	//            dateField = value;
	//        }
	//    }

	//    /// <remarks/>
	//    [XmlAttribute(DataType = "date")]
	//    public DateTime Tag
	//    {
	//        get
	//        {
	//            return tagField;
	//        }
	//        set
	//        {
	//            tagField = value;
	//        }
	//    }

	//    /// <remarks/>
	//    [XmlIgnore]
	//    public bool TagSpecified
	//    {
	//        get
	//        {
	//            return tagFieldSpecified;
	//        }
	//        set
	//        {
	//            tagFieldSpecified = value;
	//        }
	//    }

	//    /// <remarks/>
	//    [XmlAttribute(DataType = "anyURI")]
	//    public string Link
	//    {
	//        get
	//        {
	//            return linkField;
	//        }
	//        set
	//        {
	//            linkField = value;
	//        }
	//    }
	//}

	///// <remarks/>
	//[SerializableAttribute]
	//[DebuggerStepThrough]
	//[XmlType(AnonymousType = true, Namespace = "http://www.mpi.nl/IMDI/Schema/IMDI")]
	//public class VocabularyDef_TypeEntry
	//{

	//    private string tagField;

	//    private string valueField;

	//    private string value1Field;

	//    /// <remarks/>
	//    [XmlAttribute]
	//    public string Tag
	//    {
	//        get
	//        {
	//            return tagField;
	//        }
	//        set
	//        {
	//            tagField = value;
	//        }
	//    }

	//    /// <remarks/>
	//    [XmlAttribute]
	//    public string Value
	//    {
	//        get
	//        {
	//            return valueField;
	//        }
	//        set
	//        {
	//            valueField = value;
	//        }
	//    }

	//    /// <remarks/>
	//    [XmlText]
	//    public string Value1
	//    {
	//        get
	//        {
	//            return value1Field;
	//        }
	//        set
	//        {
	//            value1Field = value;
	//        }
	//    }
	//}


	///// <remarks/>
	//[SerializableAttribute]
	//[DebuggerStepThroughAttribute]
	//[XmlTypeAttribute(Namespace = "http://www.mpi.nl/IMDI/Schema/IMDI")]
	//public class ExternalResourceReference_Type
	//{

	//    private VocabularyType typeField;

	//    private VocabularyType subTypeField;

	//    private VocabularyType formatField;

	//    private string linkField;

	//    /// <remarks/>
	//    public VocabularyType Type
	//    {
	//        get
	//        {
	//            return typeField;
	//        }
	//        set
	//        {
	//            typeField = value;
	//        }
	//    }

	//    /// <remarks/>
	//    public VocabularyType SubType
	//    {
	//        get
	//        {
	//            return subTypeField;
	//        }
	//        set
	//        {
	//            subTypeField = value;
	//        }
	//    }

	//    /// <remarks/>
	//    public VocabularyType Format
	//    {
	//        get
	//        {
	//            return formatField;
	//        }
	//        set
	//        {
	//            formatField = value;
	//        }
	//    }

	//    /// <remarks/>
	//    [XmlElementAttribute(DataType = "anyURI")]
	//    public string Link
	//    {
	//        get
	//        {
	//            return linkField;
	//        }
	//        set
	//        {
	//            linkField = value;
	//        }
	//    }
	//}


	///// <remarks/>
	//[SerializableAttribute]
	//[DebuggerStepThroughAttribute]
	//[XmlTypeAttribute(Namespace = "http://www.mpi.nl/IMDI/Schema/IMDI")]
	//public class LexiconResource_Type
	//{

	//    private ResourceLinkType resourceLinkField;

	//    private string dateField;

	//    private VocabularyType typeField;

	//    private VocabularyType formatField;

	//    private string characterEncodingField;

	//    private LexiconResource_TypeSize sizeField;

	//    private IntegerType noHeadEntriesField;

	//    private IntegerType noSubEntriesField;

	//    private LexiconResource_TypeLexicalEntry[] lexicalEntryField;

	//    private LexiconResourceTypeMetaLanguages metaLanguagesField;

	//    private AccessType accessField;

	//    private List<DescriptionType> descriptionField;

	//    private KeysType keysField;

	//    private string resourceIdField;

	//    /// <remarks/>
	//    public ResourceLinkType ResourceLink
	//    {
	//        get
	//        {
	//            return resourceLinkField;
	//        }
	//        set
	//        {
	//            resourceLinkField = value;
	//        }
	//    }

	//    /// <remarks/>
	//    public string Date
	//    {
	//        get
	//        {
	//            return dateField;
	//        }
	//        set
	//        {
	//            dateField = value;
	//        }
	//    }

	//    /// <remarks/>
	//    public VocabularyType Type
	//    {
	//        get
	//        {
	//            return typeField;
	//        }
	//        set
	//        {
	//            typeField = value;
	//        }
	//    }

	//    /// <remarks/>
	//    public VocabularyType Format
	//    {
	//        get
	//        {
	//            return formatField;
	//        }
	//        set
	//        {
	//            formatField = value;
	//        }
	//    }

	//    /// <remarks/>
	//    public string CharacterEncoding
	//    {
	//        get
	//        {
	//            return characterEncodingField;
	//        }
	//        set
	//        {
	//            characterEncodingField = value;
	//        }
	//    }

	//    /// <remarks/>
	//    public LexiconResource_TypeSize Size
	//    {
	//        get
	//        {
	//            return sizeField;
	//        }
	//        set
	//        {
	//            sizeField = value;
	//        }
	//    }

	//    /// <remarks/>
	//    public IntegerType NoHeadEntries
	//    {
	//        get
	//        {
	//            return noHeadEntriesField;
	//        }
	//        set
	//        {
	//            noHeadEntriesField = value;
	//        }
	//    }

	//    /// <remarks/>
	//    public IntegerType NoSubEntries
	//    {
	//        get
	//        {
	//            return noSubEntriesField;
	//        }
	//        set
	//        {
	//            noSubEntriesField = value;
	//        }
	//    }

	//    /// <remarks/>
	//    [XmlElementAttribute("LexicalEntry")]
	//    public LexiconResource_TypeLexicalEntry[] LexicalEntry
	//    {
	//        get
	//        {
	//            return lexicalEntryField;
	//        }
	//        set
	//        {
	//            lexicalEntryField = value;
	//        }
	//    }

	//    /// <remarks/>
	//    public LexiconResourceTypeMetaLanguages MetaLanguages
	//    {
	//        get
	//        {
	//            return metaLanguagesField;
	//        }
	//        set
	//        {
	//            metaLanguagesField = value;
	//        }
	//    }

	//    /// <remarks/>
	//    public AccessType Access
	//    {
	//        get
	//        {
	//            return accessField;
	//        }
	//        set
	//        {
	//            accessField = value;
	//        }
	//    }

	//    /// <remarks/>
	//    [XmlElementAttribute("Description")]
	//    public List<DescriptionType> Description
	//    {
	//        get
	//        {
	//            if (descriptionField == null)
	//                descriptionField = new List<DescriptionType>();
	//            return descriptionField;
	//        }
	//        set
	//        {
	//            descriptionField = value;
	//        }
	//    }

	//    /// <remarks/>
	//    public KeysType Keys
	//    {
	//        get
	//        {
	//            return keysField;
	//        }
	//        set
	//        {
	//            keysField = value;
	//        }
	//    }

	//    /// <remarks/>
	//    [XmlAttributeAttribute]
	//    public string ResourceId
	//    {
	//        get
	//        {
	//            return resourceIdField;
	//        }
	//        set
	//        {
	//            resourceIdField = value;
	//        }
	//    }
	//}

	///// <remarks/>
	//[SerializableAttribute]
	//[DebuggerStepThroughAttribute]
	//[XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.mpi.nl/IMDI/Schema/IMDI")]
	//public class LexiconResource_TypeSize
	//{
	//    private string valueField;

	//    /// <remarks/>
	//    [XmlTextAttribute]
	//    public string Value
	//    {
	//        get
	//        {
	//            return valueField;
	//        }
	//        set
	//        {
	//            valueField = value;
	//        }
	//    }
	//}


	///// <remarks/>
	//[SerializableAttribute]
	//[DebuggerStepThroughAttribute]
	//[XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.mpi.nl/IMDI/Schema/IMDI")]
	//public class LexiconResourceTypeMetaLanguages
	//{
	//    private List<DescriptionType> _descriptionField;

	//    /// <remarks/>
	//    [XmlElement("Language")]
	//    public VocabularyType[] Language { get; set; }

	//    /// <remarks/>
	//    [XmlElementAttribute("Description")]
	//    public List<DescriptionType> Description
	//    {
	//        get { return _descriptionField ?? (_descriptionField = new List<DescriptionType>()); }
	//        set { _descriptionField = value; }
	//    }
	//}


	///// <remarks/>
	//[SerializableAttribute]
	//[DebuggerStepThroughAttribute]
	//[XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.mpi.nl/IMDI/Schema/IMDI")]
	//public class LexiconResource_TypeLexicalEntry
	//{

	//    private VocabularyType headWordTypeField;

	//    private VocabularyType orthographyField;

	//    private VocabularyType morphologyField;

	//    private VocabularyType morphoSyntaxField;

	//    private VocabularyType syntaxField;

	//    private VocabularyType phonologyField;

	//    private VocabularyType semanticsField;

	//    private VocabularyType etymologyField;

	//    private VocabularyType usageField;

	//    private string frequencyField;

	//    /// <remarks/>
	//    public VocabularyType HeadWordType
	//    {
	//        get
	//        {
	//            return headWordTypeField;
	//        }
	//        set
	//        {
	//            headWordTypeField = value;
	//        }
	//    }

	//    /// <remarks/>
	//    public VocabularyType Orthography
	//    {
	//        get
	//        {
	//            return orthographyField;
	//        }
	//        set
	//        {
	//            orthographyField = value;
	//        }
	//    }

	//    /// <remarks/>
	//    public VocabularyType Morphology
	//    {
	//        get
	//        {
	//            return morphologyField;
	//        }
	//        set
	//        {
	//            morphologyField = value;
	//        }
	//    }

	//    /// <remarks/>
	//    public VocabularyType MorphoSyntax
	//    {
	//        get
	//        {
	//            return morphoSyntaxField;
	//        }
	//        set
	//        {
	//            morphoSyntaxField = value;
	//        }
	//    }

	//    /// <remarks/>
	//    public VocabularyType Syntax
	//    {
	//        get
	//        {
	//            return syntaxField;
	//        }
	//        set
	//        {
	//            syntaxField = value;
	//        }
	//    }

	//    /// <remarks/>
	//    public VocabularyType Phonology
	//    {
	//        get
	//        {
	//            return phonologyField;
	//        }
	//        set
	//        {
	//            phonologyField = value;
	//        }
	//    }

	//    /// <remarks/>
	//    public VocabularyType Semantics
	//    {
	//        get
	//        {
	//            return semanticsField;
	//        }
	//        set
	//        {
	//            semanticsField = value;
	//        }
	//    }

	//    /// <remarks/>
	//    public VocabularyType Etymology
	//    {
	//        get
	//        {
	//            return etymologyField;
	//        }
	//        set
	//        {
	//            etymologyField = value;
	//        }
	//    }

	//    /// <remarks/>
	//    public VocabularyType Usage
	//    {
	//        get
	//        {
	//            return usageField;
	//        }
	//        set
	//        {
	//            usageField = value;
	//        }
	//    }

	//    /// <remarks/>
	//    public string Frequency
	//    {
	//        get
	//        {
	//            return frequencyField;
	//        }
	//        set
	//        {
	//            frequencyField = value;
	//        }
	//    }
	//}


	///// <remarks/>
	//[SerializableAttribute]
	//[DebuggerStepThroughAttribute]
	//[XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.mpi.nl/IMDI/Schema/IMDI")]
	//public class LexiconComponent_TypeLexicalInfo
	//{

	//    private bool orthographyField;

	//    private bool orthographyFieldSpecified;

	//    private bool morphologyField;

	//    private bool morphologyFieldSpecified;

	//    private bool morphoSyntaxField;

	//    private bool morphoSyntaxFieldSpecified;

	//    private bool syntaxField;

	//    private bool syntaxFieldSpecified;

	//    private bool phonologyField;

	//    private bool phonologyFieldSpecified;

	//    private bool semanticsField;

	//    private bool semanticsFieldSpecified;

	//    private bool etymologyField;

	//    private bool etymologyFieldSpecified;

	//    private bool usageField;

	//    private bool usageFieldSpecified;

	//    private bool frequencyField;

	//    private bool frequencyFieldSpecified;

	//    /// <remarks/>
	//    public bool Orthography
	//    {
	//        get
	//        {
	//            return orthographyField;
	//        }
	//        set
	//        {
	//            orthographyField = value;
	//        }
	//    }

	//    /// <remarks/>
	//    [XmlIgnoreAttribute]
	//    public bool OrthographySpecified
	//    {
	//        get
	//        {
	//            return orthographyFieldSpecified;
	//        }
	//        set
	//        {
	//            orthographyFieldSpecified = value;
	//        }
	//    }

	//    /// <remarks/>
	//    public bool Morphology
	//    {
	//        get
	//        {
	//            return morphologyField;
	//        }
	//        set
	//        {
	//            morphologyField = value;
	//        }
	//    }

	//    /// <remarks/>
	//    [XmlIgnoreAttribute]
	//    public bool MorphologySpecified
	//    {
	//        get
	//        {
	//            return morphologyFieldSpecified;
	//        }
	//        set
	//        {
	//            morphologyFieldSpecified = value;
	//        }
	//    }

	//    /// <remarks/>
	//    public bool MorphoSyntax
	//    {
	//        get
	//        {
	//            return morphoSyntaxField;
	//        }
	//        set
	//        {
	//            morphoSyntaxField = value;
	//        }
	//    }

	//    /// <remarks/>
	//    [XmlIgnoreAttribute]
	//    public bool MorphoSyntaxSpecified
	//    {
	//        get
	//        {
	//            return morphoSyntaxFieldSpecified;
	//        }
	//        set
	//        {
	//            morphoSyntaxFieldSpecified = value;
	//        }
	//    }

	//    /// <remarks/>
	//    public bool Syntax
	//    {
	//        get
	//        {
	//            return syntaxField;
	//        }
	//        set
	//        {
	//            syntaxField = value;
	//        }
	//    }

	//    /// <remarks/>
	//    [XmlIgnoreAttribute]
	//    public bool SyntaxSpecified
	//    {
	//        get
	//        {
	//            return syntaxFieldSpecified;
	//        }
	//        set
	//        {
	//            syntaxFieldSpecified = value;
	//        }
	//    }

	//    /// <remarks/>
	//    public bool Phonology
	//    {
	//        get
	//        {
	//            return phonologyField;
	//        }
	//        set
	//        {
	//            phonologyField = value;
	//        }
	//    }

	//    /// <remarks/>
	//    [XmlIgnoreAttribute]
	//    public bool PhonologySpecified
	//    {
	//        get
	//        {
	//            return phonologyFieldSpecified;
	//        }
	//        set
	//        {
	//            phonologyFieldSpecified = value;
	//        }
	//    }

	//    /// <remarks/>
	//    public bool Semantics
	//    {
	//        get
	//        {
	//            return semanticsField;
	//        }
	//        set
	//        {
	//            semanticsField = value;
	//        }
	//    }

	//    /// <remarks/>
	//    [XmlIgnoreAttribute]
	//    public bool SemanticsSpecified
	//    {
	//        get
	//        {
	//            return semanticsFieldSpecified;
	//        }
	//        set
	//        {
	//            semanticsFieldSpecified = value;
	//        }
	//    }

	//    /// <remarks/>
	//    public bool Etymology
	//    {
	//        get
	//        {
	//            return etymologyField;
	//        }
	//        set
	//        {
	//            etymologyField = value;
	//        }
	//    }

	//    /// <remarks/>
	//    [XmlIgnoreAttribute]
	//    public bool EtymologySpecified
	//    {
	//        get
	//        {
	//            return etymologyFieldSpecified;
	//        }
	//        set
	//        {
	//            etymologyFieldSpecified = value;
	//        }
	//    }

	//    /// <remarks/>
	//    public bool Usage
	//    {
	//        get
	//        {
	//            return usageField;
	//        }
	//        set
	//        {
	//            usageField = value;
	//        }
	//    }

	//    /// <remarks/>
	//    [XmlIgnoreAttribute]
	//    public bool UsageSpecified
	//    {
	//        get
	//        {
	//            return usageFieldSpecified;
	//        }
	//        set
	//        {
	//            usageFieldSpecified = value;
	//        }
	//    }

	//    /// <remarks/>
	//    public bool Frequency
	//    {
	//        get
	//        {
	//            return frequencyField;
	//        }
	//        set
	//        {
	//            frequencyField = value;
	//        }
	//    }

	//    /// <remarks/>
	//    [XmlIgnoreAttribute]
	//    public bool FrequencySpecified
	//    {
	//        get
	//        {
	//            return frequencyFieldSpecified;
	//        }
	//        set
	//        {
	//            frequencyFieldSpecified = value;
	//        }
	//    }
	//}

	///// <remarks/>
	//[SerializableAttribute]
	//[DebuggerStepThroughAttribute]
	//[XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.mpi.nl/IMDI/Schema/IMDI")]
	//public class LexiconComponent_TypeMetaLanguages
	//{

	//    private VocabularyType languageField;

	//    private List<DescriptionType> descriptionField;

	//    /// <remarks/>
	//    public VocabularyType Language
	//    {
	//        get
	//        {
	//            return languageField;
	//        }
	//        set
	//        {
	//            languageField = value;
	//        }
	//    }

	//    /// <remarks/>
	//    [XmlElementAttribute("Description")]
	//    public List<DescriptionType> Description
	//    {
	//        get
	//        {
	//            if (descriptionField == null)
	//                descriptionField = new List<DescriptionType>();
	//            return descriptionField;
	//        }
	//        set
	//        {
	//            descriptionField = value;
	//        }
	//    }
	//}

	///// <remarks/>
	//[SerializableAttribute]
	//[DebuggerStepThroughAttribute]
	//[XmlTypeAttribute(Namespace = "http://www.mpi.nl/IMDI/Schema/IMDI")]
	//public class LexiconComponent_Type
	//{

	//    private ResourceLinkType resourceLinkField;

	//    private string dateField;

	//    private VocabularyType typeField;

	//    private VocabularyType formatField;

	//    private string characterEncodingField;

	//    private LexiconComponent_TypeSize sizeField;

	//    private LexiconComponent_TypeComponent componentField;

	//    private LexiconComponent_TypeLexicalInfo lexicalInfoField;

	//    private LexiconComponent_TypeMetaLanguages metaLanguagesField;

	//    private AccessType accessField;

	//    private List<DescriptionType> descriptionField;

	//    private KeysType keysField;

	//    private string resourceIdField;

	//    /// <remarks/>
	//    public ResourceLinkType ResourceLink
	//    {
	//        get
	//        {
	//            return resourceLinkField;
	//        }
	//        set
	//        {
	//            resourceLinkField = value;
	//        }
	//    }

	//    /// <remarks/>
	//    public string Date
	//    {
	//        get
	//        {
	//            return dateField;
	//        }
	//        set
	//        {
	//            dateField = value;
	//        }
	//    }

	//    /// <remarks/>
	//    public VocabularyType Type
	//    {
	//        get
	//        {
	//            return typeField;
	//        }
	//        set
	//        {
	//            typeField = value;
	//        }
	//    }

	//    /// <remarks/>
	//    public VocabularyType Format
	//    {
	//        get
	//        {
	//            return formatField;
	//        }
	//        set
	//        {
	//            formatField = value;
	//        }
	//    }

	//    /// <remarks/>
	//    public string CharacterEncoding
	//    {
	//        get
	//        {
	//            return characterEncodingField;
	//        }
	//        set
	//        {
	//            characterEncodingField = value;
	//        }
	//    }

	//    /// <remarks/>
	//    public LexiconComponent_TypeSize Size
	//    {
	//        get
	//        {
	//            return sizeField;
	//        }
	//        set
	//        {
	//            sizeField = value;
	//        }
	//    }

	//    /// <remarks/>
	//    public LexiconComponent_TypeComponent Component
	//    {
	//        get
	//        {
	//            return componentField;
	//        }
	//        set
	//        {
	//            componentField = value;
	//        }
	//    }

	//    /// <remarks/>
	//    public LexiconComponent_TypeLexicalInfo LexicalInfo
	//    {
	//        get
	//        {
	//            return lexicalInfoField;
	//        }
	//        set
	//        {
	//            lexicalInfoField = value;
	//        }
	//    }

	//    /// <remarks/>
	//    public LexiconComponent_TypeMetaLanguages MetaLanguages
	//    {
	//        get
	//        {
	//            return metaLanguagesField;
	//        }
	//        set
	//        {
	//            metaLanguagesField = value;
	//        }
	//    }

	//    /// <remarks/>
	//    public AccessType Access
	//    {
	//        get
	//        {
	//            return accessField;
	//        }
	//        set
	//        {
	//            accessField = value;
	//        }
	//    }

	//    /// <remarks/>
	//    [XmlElementAttribute("Description")]
	//    public List<DescriptionType> Description
	//    {
	//        get
	//        {
	//            if (descriptionField == null)
	//                descriptionField = new List<DescriptionType>();
	//            return descriptionField;
	//        }
	//        set
	//        {
	//            descriptionField = value;
	//        }
	//    }

	//    /// <remarks/>
	//    public KeysType Keys
	//    {
	//        get
	//        {
	//            return keysField;
	//        }
	//        set
	//        {
	//            keysField = value;
	//        }
	//    }

	//    /// <remarks/>
	//    [XmlAttributeAttribute]
	//    public string ResourceId
	//    {
	//        get
	//        {
	//            return resourceIdField;
	//        }
	//        set
	//        {
	//            resourceIdField = value;
	//        }
	//    }
	//}

	///// <remarks/>
	//[SerializableAttribute]
	//[DebuggerStepThroughAttribute]
	//[XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.mpi.nl/IMDI/Schema/IMDI")]
	//public class LexiconComponent_TypeSize
	//{
	//    private string valueField;

	//    /// <remarks/>
	//    [XmlTextAttribute]
	//    public string Value
	//    {
	//        get
	//        {
	//            return valueField;
	//        }
	//        set
	//        {
	//            valueField = value;
	//        }
	//    }
	//}

	///// <remarks/>
	//[SerializableAttribute]
	//[DebuggerStepThroughAttribute]
	//[XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.mpi.nl/IMDI/Schema/IMDI")]
	//public class LexiconComponent_TypeComponent
	//{

	//    private VocabularyType possibleParentsField;

	//    private VocabularyType preferredParentField;

	//    private LexiconComponent_TypeComponentChildNodes childNodesField;

	//    /// <remarks/>
	//    public VocabularyType possibleParents
	//    {
	//        get
	//        {
	//            return possibleParentsField;
	//        }
	//        set
	//        {
	//            possibleParentsField = value;
	//        }
	//    }

	//    /// <remarks/>
	//    public VocabularyType preferredParent
	//    {
	//        get
	//        {
	//            return preferredParentField;
	//        }
	//        set
	//        {
	//            preferredParentField = value;
	//        }
	//    }

	//    /// <remarks/>
	//    public LexiconComponent_TypeComponentChildNodes childNodes
	//    {
	//        get
	//        {
	//            return childNodesField;
	//        }
	//        set
	//        {
	//            childNodesField = value;
	//        }
	//    }
	//}

	///// <remarks/>
	//[SerializableAttribute]
	//[DebuggerStepThroughAttribute]
	//[XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.mpi.nl/IMDI/Schema/IMDI")]
	//public class LexiconComponent_TypeComponentChildNodes
	//{

	//    private VocabularyType childComponentsField;

	//    private VocabularyType childCategoriesField;

	//    /// <remarks/>
	//    public VocabularyType childComponents
	//    {
	//        get
	//        {
	//            return childComponentsField;
	//        }
	//        set
	//        {
	//            childComponentsField = value;
	//        }
	//    }

	//    /// <remarks/>
	//    public VocabularyType childCategories
	//    {
	//        get
	//        {
	//            return childCategoriesField;
	//        }
	//        set
	//        {
	//            childCategoriesField = value;
	//        }
	//    }
	//}


	///// <remarks/>
	//[SerializableAttribute]
	//[DebuggerStepThroughAttribute]
	//[XmlTypeAttribute(Namespace = "http://www.mpi.nl/IMDI/Schema/IMDI")]
	//public class TimePositionRange_Type
	//{

	//    private TimePosition_Type startField;

	//    private TimePosition_Type endField;

	//    /// <remarks/>
	//    public TimePosition_Type Start
	//    {
	//        get
	//        {
	//            return startField;
	//        }
	//        set
	//        {
	//            startField = value;
	//        }
	//    }

	//    /// <remarks/>
	//    public TimePosition_Type End
	//    {
	//        get
	//        {
	//            return endField;
	//        }
	//        set
	//        {
	//            endField = value;
	//        }
	//    }
	//}

	///// <remarks/>
	//[SerializableAttribute]
	//[DebuggerStepThroughAttribute]
	//[XmlTypeAttribute(Namespace = "http://www.mpi.nl/IMDI/Schema/IMDI")]
	//public class TimePosition_Type
	//{
	//    private string valueField;

	//    /// <remarks/>
	//    [XmlTextAttribute]
	//    public string Value
	//    {
	//        get
	//        {
	//            return valueField;
	//        }
	//        set
	//        {
	//            valueField = value;
	//        }
	//    }
	//}




}
