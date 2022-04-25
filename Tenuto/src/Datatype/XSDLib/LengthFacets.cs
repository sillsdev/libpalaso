namespace Tenuto.Datatype.XSDLib {

using org.relaxng.datatype;

//
//
// Length-related facets
//
//


public abstract class LengthTypeFacet : ValueFacet {
	protected LengthTypeFacet( DatatypeImpl _super ) : base(_super) {
		measure = _super.GetMeasure();
		if(measure==null)
			// TODO: localization
			throw new DatatypeException("unapplicable facet");
	}
	
	private readonly Measure measure;
	protected override sealed bool RestrictionCheck( object o ) {
		return LengthCheck(measure(o));
	}
	protected abstract bool LengthCheck( int itemLen );
}

public class LengthFacet : LengthTypeFacet {
	public LengthFacet( int _length, DatatypeImpl _super ) : base(_super) {
		this.length = _length;
	}
	private readonly int length;
	protected override bool LengthCheck( int itemLen ) {
		return itemLen==length;
	}
}

public class MaxLengthFacet : LengthTypeFacet {
	public MaxLengthFacet( int _length, DatatypeImpl _super ) : base(_super) {
		this.length = _length;
	}
	private readonly int length;
	protected override bool LengthCheck( int itemLen ) {
		return itemLen<=length;
	}
}

public class MinLengthFacet : LengthTypeFacet {
	public MinLengthFacet( int _length, DatatypeImpl _super ) : base(_super) {
		this.length = _length;
	}
	private readonly int length;
	protected override bool LengthCheck( int itemLen ) {
		return itemLen>=length;
	}
}

}