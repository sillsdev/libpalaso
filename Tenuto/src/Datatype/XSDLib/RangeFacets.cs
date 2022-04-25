namespace Tenuto.Datatype.XSDLib {

using org.relaxng.datatype;


public class RangeFacet : ValueFacet {
	public RangeFacet( object _limit, RangeChecker _checker, DatatypeImpl _super ) : base(_super) {
		comparator = _super.GetComparator();
		if(comparator==null)
			// TODO: localization
			throw new DatatypeException();
		
		limit = _limit;
		rangeCheck = _checker;
	}
	
	private readonly Comparator comparator;
	private readonly object limit;
	
	private readonly RangeChecker rangeCheck;
	protected override bool RestrictionCheck( object o ) {
		return rangeCheck(comparator(limit,o));
	}
	
	
	
	public delegate bool RangeChecker( Order result );
	
	public static bool MaxInclusive( Order o ) {
		return o==Order.GREATER || o==Order.EQUAL;
	}
	public static bool MaxExclusive( Order o ) {
		return o==Order.GREATER;
	}
	public static bool MinExclusive( Order o ) {
		return o==Order.LESS;
	}
	public static bool MinInclusive( Order o ) {
		return o==Order.LESS || o==Order.EQUAL;
	}
}


}