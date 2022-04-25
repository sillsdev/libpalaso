namespace Tenuto.Datatype.XSDLib {

using System;
using org.relaxng.datatype;

// numeric types have fully-ordered value space
// value objects must implement IComparable
public abstract class NumericType : DatatypeImpl {
	protected NumericType() : base(WSNormalizationMode.Collapse) {}
	
	protected internal override sealed Comparator GetComparator() {
		return new Comparator(CompareNumber);
	}
	
	protected internal override sealed
	bool ValueCheck( string s, ValidationContext ctxt ) {
		return GetValue(s,ctxt)!=null;
	}
	
	private static Order CompareNumber( object o1, object o2 ) {
		int r = ((IComparable)o1).CompareTo(o2);
		if(r<0)		return Order.LESS;
		if(r==0)	return Order.EQUAL;
		else		return Order.GREATER;
	}
}

public class FloatType : NumericType {
	protected internal override
	object GetValue( string s, ValidationContext ctxt ) {
		return float.Parse(s);
	}
}

public class DoubleType : NumericType {
	protected internal override
	object GetValue( string s, ValidationContext ctxt ) {
		return double.Parse(s);
	}
}

public class DecimalType : NumericType {
	protected internal override
	object GetValue( string s, ValidationContext ctxt ) {
		// TODO: what's the equivalent of BigDecimal?
		throw new Exception();
	}
}

public class IntegerType : NumericType {
	protected internal override
	object GetValue( string s, ValidationContext ctxt ) {
		// TODO: use a proper class.
		// Decimal is just a 96-bit length integer,
		// not what we need for "integer"
		return decimal.Parse(s);
		// applicable to (non)?(positive|negative)Integer
	}
}

public class PositiveIntegerType : NumericType {
	protected internal override
	object GetValue( string s, ValidationContext ctxt ) {
		decimal d = decimal.Parse(s);
		if(d>0)		return d;
		else		return null;
	}
}

public class NonPositiveIntegerType : NumericType {
	protected internal override
	object GetValue( string s, ValidationContext ctxt ) {
		decimal d = decimal.Parse(s);
		if(d<=0)	return d;
		else		return null;
	}
}

public class NegativeIntegerType : NumericType {
	protected internal override
	object GetValue( string s, ValidationContext ctxt ) {
		decimal d = decimal.Parse(s);
		if(d<0)		return d;
		else		return null;
	}
}

public class NonNegativeIntegerType : NumericType {
	protected internal override
	object GetValue( string s, ValidationContext ctxt ) {
		decimal d = decimal.Parse(s);
		if(d>=0)	return d;
		else		return null;
	}
}

public class LongType : NumericType {
	protected internal override
	object GetValue( string s, ValidationContext ctxt ) {
		return long.Parse(s);
	}
}

public class UnsignedLongType : NumericType {
	protected internal override
	object GetValue( string s, ValidationContext ctxt ) {
		return ulong.Parse(s);
	}
}

public class IntType : NumericType {
	protected internal override
	object GetValue( string s, ValidationContext ctxt ) {
		return int.Parse(s);
	}
}

public class UnsignedIntType : NumericType {
	protected internal override
	object GetValue( string s, ValidationContext ctxt ) {
		return uint.Parse(s);
	}
}

public class ShortType : NumericType {
	protected internal override
	object GetValue( string s, ValidationContext ctxt ) {
		return short.Parse(s);
	}
}

public class UnsignedShortType : NumericType {
	protected internal override
	object GetValue( string s, ValidationContext ctxt ) {
		return ushort.Parse(s);
	}
}

public class ByteType : NumericType {
	protected internal override
	object GetValue( string s, ValidationContext ctxt ) {
		return sbyte.Parse(s);
	}
}

public class UnsignedByteType : NumericType {
	protected internal override
	object GetValue( string s, ValidationContext ctxt ) {
		return byte.Parse(s);
	}
}



}