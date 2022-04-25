namespace Tenuto.Datatype.XSDLib {

using org.relaxng.datatype;
using System;

public class ListType : DatatypeImpl {
	
	public ListType( Datatype _itemType ) : base(WSNormalizationMode.Collapse) {
		itemType = _itemType;
	}
	
	private readonly Datatype itemType;
	
	protected internal override
	bool ValueCheck( string str, ValidationContext ctxt ) {
		string[] tokens = StringTokenizer.Tokenize(str);
		if(tokens.Length==0)	return false;
		
		foreach( string s in tokens)
			if(!itemType.IsValid(s,ctxt))
				return false;
		return true;
	}
	protected internal override
	object GetValue( string str, ValidationContext ctxt ) {
		string[] tokens = StringTokenizer.Tokenize(str);
		object[] value = new object[tokens.Length];
		
		if(tokens.Length==0)	return null;	// don't allow 0-lenght item
		
		for( int i=tokens.Length-1; i>=0; i-- )
			if((value[i] = itemType.CreateValue(tokens[i],ctxt)) == null )
				return null;
		
		return value;
	}
	public override bool IsContextDependent {
		get { return itemType.IsContextDependent; }
	}
	
	public override org.relaxng.datatype.IDType IdType {
		get {
			switch(itemType.IdType) {
			case org.relaxng.datatype.IDType.ID_TYPE_NULL:
				return org.relaxng.datatype.IDType.ID_TYPE_NULL;
			case org.relaxng.datatype.IDType.ID_TYPE_IDREF:
				return org.relaxng.datatype.IDType.ID_TYPE_IDREF;
			default:
				// assertion failed
				throw new Exception();
			}
		}
	}
	
	public override int ValueHashCode( object o ) {
		int x=0;
		foreach( object v in (object[])o )
			x ^= itemType.ValueHashCode(v);
		return x;
	}
	
	public override bool SameValue( object o1, object o2 ) {
		object[] v1 = (object[])o1;
		object[] v2 = (object[])o2;
		
		if(v1.Length!=v2.Length)	return false;
		for( int i=v1.Length-1; i>=0; i-- )
			if(!itemType.SameValue(v1[i],v2[i]))
				return false;
		return true;
	}
	
	protected internal override
	Measure GetMeasure() { return new Measure(CalcLength); }
	
	private static int CalcLength( object o ) {
		return ((object[])o).Length;
	}
}

}
