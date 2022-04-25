namespace Tenuto.Datatype.XSDLib {

using org.relaxng.datatype;
using System.Collections;

public class XMLSchemaDatatypeLibrary : DatatypeLibrary {
	
	public static XMLSchemaDatatypeLibrary theInstance =
		new XMLSchemaDatatypeLibrary();
	
	private XMLSchemaDatatypeLibrary() {
		Datatype dt;
		
		// TODO from here
		builtinTypes.Add("duration",UnimplementedDatatype.theInstance);
		builtinTypes.Add("dateTime",UnimplementedDatatype.theInstance);
		builtinTypes.Add("time",UnimplementedDatatype.theInstance);
		builtinTypes.Add("date",UnimplementedDatatype.theInstance);
		builtinTypes.Add("gYearMonth",UnimplementedDatatype.theInstance);
		builtinTypes.Add("gYear",UnimplementedDatatype.theInstance);
		builtinTypes.Add("gMonthDay",UnimplementedDatatype.theInstance);
		builtinTypes.Add("gDay",UnimplementedDatatype.theInstance);
		builtinTypes.Add("gMonth",UnimplementedDatatype.theInstance);
		// TODO until here
		builtinTypes.Add("string",new StringType(WSNormalizationMode.Preserve));
		builtinTypes.Add("normalizedString",new StringType(WSNormalizationMode.Replace));
		builtinTypes.Add("token",new StringType(WSNormalizationMode.Collapse));
		builtinTypes.Add("language",new LanguageType());
		builtinTypes.Add("Name",new NameType());
		builtinTypes.Add("NCName",new NCNameType());
		builtinTypes.Add("ID",new IDType());
		builtinTypes.Add("IDREF",dt=new IDREFType());
		builtinTypes.Add("IDREFS",new ListType(dt));
		builtinTypes.Add("ENTITY",dt=new StringType(WSNormalizationMode.Collapse));	// TODO
		builtinTypes.Add("ENTITIES",new ListType(dt));
		builtinTypes.Add("NMTOKEN",dt=new NMTOKENType());
		builtinTypes.Add("NMTOKENS",new ListType(dt));
		builtinTypes.Add("boolean",new BooleanType());
		builtinTypes.Add("base64Binary",UnimplementedDatatype.theInstance);	// TODO
		builtinTypes.Add("hexBinary",UnimplementedDatatype.theInstance);	// TODO
		builtinTypes.Add("float",new FloatType());
		builtinTypes.Add("decimal",new DecimalType());
		builtinTypes.Add("integer",new IntegerType());
		builtinTypes.Add("nonPositiveInteger",new NonPositiveIntegerType());
		builtinTypes.Add("negativeInteger",new NegativeIntegerType());
		builtinTypes.Add("long",new LongType());
		builtinTypes.Add("int",new IntType());
		builtinTypes.Add("short",new ShortType());
		builtinTypes.Add("byte",new ByteType());
		builtinTypes.Add("nonNegativeInteger",new NonNegativeIntegerType());
		builtinTypes.Add("unsignedLong",new UnsignedLongType());
		builtinTypes.Add("unsignedInt",new UnsignedIntType());
		builtinTypes.Add("unsignedShort",new UnsignedShortType());
		builtinTypes.Add("unsignedByte",new UnsignedByteType());
		builtinTypes.Add("positiveInteger",new PositiveIntegerType());
		builtinTypes.Add("double",new DoubleType());
		builtinTypes.Add("anyURI",new StringType(WSNormalizationMode.Collapse)); // TODO
		builtinTypes.Add("QName",new QNameType());
		builtinTypes.Add("NOTATION",new StringType(WSNormalizationMode.Collapse)); // TODO
	}
	
	public DatatypeBuilder CreateDatatypeBuilder( string name ) {
		return new DatatypeBuilderImpl((DatatypeImpl)CreateDatatype(name));
	}
	
	private readonly IDictionary builtinTypes = new Hashtable();
	
	public Datatype CreateDatatype( string name ) {
		Datatype dt = (Datatype)builtinTypes[name];
		if(dt!=null)	return dt;
		
		// TODO: localization
		throw new DatatypeException("undefined datatype: "+name);
	}
}

internal class DatatypeBuilderImpl : DatatypeBuilder {
	
	private DatatypeImpl type;
	
	internal DatatypeBuilderImpl( DatatypeImpl _type ) {
		this.type = _type;
	}
	
	public void AddParameter( string name, string value, ValidationContext context ) {
		
		if(type==UnimplementedDatatype.theInstance)
			// allow any facet for unimplemented datatype
			return;
		
		if(name=="length") {
			type = new LengthFacet( int.Parse(value), type );
			return;
		}
		if(name=="minLength") {
			type = new MinLengthFacet( int.Parse(value), type );
			return;
		}
		if(name=="maxLength") {
			type = new MaxLengthFacet( int.Parse(value), type );
			return;
		}
		if(name=="pattern") {
			// TODO: support
			// type = new PatternFacet( value, type );
			return;
		}
		if(name=="enumeration") {
			// enumeration: this facet will be rejected
			// TODO: specialized error message
			throw new DatatypeException();
		}
		if(name=="whiteSpace") {
			// whiteSpace facet is also not supported.
			// TODO: specialized error message
			throw new DatatypeException();
		}
		if(name=="maxInclusive") {
			type = new RangeFacet(
				type.CreateValue(value,context),
				new RangeFacet.RangeChecker(RangeFacet.MaxInclusive),
				type );
			return;
		}
		if(name=="maxExclusive") {
			type = new RangeFacet(
				type.CreateValue(value,context),
				new RangeFacet.RangeChecker(RangeFacet.MaxExclusive),
				type );
			return;
		}
		if(name=="minInclusive") {
			type = new RangeFacet(
				type.CreateValue(value,context),
				new RangeFacet.RangeChecker(RangeFacet.MinInclusive),
				type );
			return;
		}
		if(name=="minExclusive") {
			type = new RangeFacet(
				type.CreateValue(value,context),
				new RangeFacet.RangeChecker(RangeFacet.MinExclusive),
				type );
			return;
		}
		if(name=="totalDigits") {
			// TODO: support
			return;
		}
		if(name=="fractionDigits") {
			// TODO: support
			return;
		}
		
		// TODO: localization
		throw new DatatypeException("unsupported facet: "+name);
	}
	
	public Datatype CreateDatatype() {
		return type;
	}
}

}
