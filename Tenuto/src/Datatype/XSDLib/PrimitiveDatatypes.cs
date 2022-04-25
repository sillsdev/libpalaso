namespace Tenuto.Datatype.XSDLib {

using org.relaxng.datatype;
using org.relaxng.datatype.helpers;
using Tenuto;


// length calculator
public delegate int Measure( object o );
// order comparison
public enum Order { LESS, EQUAL, GREATER, UNDECIDABLE };
public delegate Order Comparator( object o1, object o2 );

// base implementation
public abstract class DatatypeImpl : Datatype {
	
	protected DatatypeImpl( WSNormalizationMode mode ) {
		wsProcessor = WhitespaceNormalizer.GetProcessor(mode);
	}
	protected DatatypeImpl( WhitespaceNormalizer.Processor wsproc ) {
		wsProcessor = wsproc;
	}
	
	
	// methods that should be implemented by derived classes
	protected internal virtual bool LexicalCheck( string normalizedStr ) {
		return true;
	}
	protected internal virtual bool ValueCheck( string normalizedStr, ValidationContext ctxt ) {
		return true;
	}
	protected internal virtual object GetValue( string normalizedStr, ValidationContext ctxt ) {
		return normalizedStr;
	}
	
	
	
	public bool IsValid( string str, ValidationContext ctxt ) {
		str = wsProcessor(str);
		return LexicalCheck(str) && ValueCheck(str,ctxt);
	}
	
	public void CheckValid( string str, ValidationContext ctxt ) {
		if(!IsValid(str,ctxt))
			throw new DatatypeException();
	}
	
	public object CreateValue( string str, ValidationContext ctxt ) {
		str = wsProcessor(str);
		if(!LexicalCheck(str))	return null;
		return GetValue(str,ctxt);
	}
	
	public virtual bool IsContextDependent {
		get { return false; }
	}
	
	public virtual org.relaxng.datatype.IDType IdType {
		get { return org.relaxng.datatype.IDType.ID_TYPE_NULL; }
	}
	
	public virtual int ValueHashCode( object o ) {
		return o.GetHashCode();
	}
	
	public virtual bool SameValue( object o1, object o2 ) {
		return o1.Equals(o2);
	}
	
	public DatatypeStreamingValidator CreateStreamingValidator( ValidationContext ctxt ) {
		return new StreamingValidatorImpl(this,ctxt);
	}
	
	
	//
	// whitespace handling
	//
	
	public readonly WhitespaceNormalizer.Processor wsProcessor;
	
	
	protected internal virtual Measure GetMeasure() { return null; }
	protected internal virtual Comparator GetComparator() { return null; }
}



public class BooleanType : DatatypeImpl {
	
	public BooleanType() : base(WSNormalizationMode.Collapse) {}
	
	protected internal override
	bool LexicalCheck( string s ) {
		return s=="true" || s=="false" || s=="0" || s=="1";
	}
	
	protected internal override
	object GetValue( string s, ValidationContext ctxt ) {
		char ch = s[0];
		if(ch=='t' || ch=='1')	return true;
		else					return false;
	}
}



public class QNameType : DatatypeImpl {
	public QNameType() : base(WSNormalizationMode.Collapse) {}
	
	protected internal override
	bool LexicalCheck( string s ) {
		return XMLChar.IsQName(s);
	}
	
	protected internal override
	bool ValueCheck( string s, ValidationContext ctxt ) {
		int idx = s.IndexOf(':');
		if(idx<0)	return true;
		return ctxt.ResolveNamespacePrefix(s.Substring(0,idx))!=null;
	}
	protected internal override
	object GetValue( string s, ValidationContext ctxt ) {
		int idx = s.IndexOf(':');
		if(idx<0) {
			string uri = ctxt.ResolveNamespacePrefix("");
			if(uri==null)	uri="";
			return new XmlName(uri,s);
		} else {
			string uri = ctxt.ResolveNamespacePrefix(s.Substring(0,idx));
			if(uri==null)	return null;
			return new XmlName(uri,s.Substring(idx+1));
		}
	}
}

}
