namespace Tenuto.Grammar {

using org.relaxng.datatype;

public sealed class DataExp : Expression {
	
	public readonly Expression except;
	public readonly Datatype dt;
	/**
	 * type name of this datatype. Can be null.
	 */
	public readonly string name;
	
	internal DataExp( Datatype _dt, Expression _except, string _name )
			: base(_dt.GetHashCode()*HASH_DATA, false ) {
		// TODO: probably the epsilon reducibility is wrong.
		// can we treat epsilon-reducibility as an approximation?
		// rather than the precise value?
		this.except = _except;
		this.dt = _dt;
		this.name = _name;
	}
	
	public override bool Equals( object o ) {
		if(o.GetType()!=this.GetType())	return false;
		DataExp rhs = (DataExp)o;
		return rhs.except==this.except && rhs.dt==this.dt;
	}
	public override object Visit( ExpVisitor visitor ) {
		return visitor.OnData(this);
	}
	public override void Visit( ExpVisitorVoid Visitor ) {
		Visitor.OnData(this);
	}
	public override Expression Visit( ExpVisitorExp visitor ) {
		return visitor.OnData(this);
	}
}


public sealed class ValueExp : Expression {
	
	public readonly Datatype dt;
	public readonly object value;
	
	internal ValueExp( Datatype dt, object value )
			: base(dt.GetHashCode()*dt.ValueHashCode(value)*HASH_VALUE, false ) {
		// TODO: probably the epsilon reducibility is wrong.
		// can we treat epsilon-reducibility as an approximation?
		// rather than the precise value?
		this.dt = dt;
		this.value = value;
	}
	
	public override bool Equals( object o ) {
		if(o.GetType()!=this.GetType())	return false;
		ValueExp rhs = (ValueExp)o;
		return rhs.dt==this.dt && dt.SameValue(this.value,rhs.value);
	}
	public override object Visit( ExpVisitor visitor ) {
		return visitor.OnValue(this);
	}
	public override void Visit( ExpVisitorVoid Visitor ) {
		Visitor.OnValue(this);
	}
	public override Expression Visit( ExpVisitorExp visitor ) {
		return visitor.OnValue(this);
	}
}





	
internal sealed class DummyValidationContext : ValidationContext {
	public static ValidationContext instance = new DummyValidationContext();
	
	public string ResolveNamespacePrefix( string prefix ) {
		return "";
	}
	public bool IsUnparsedEntity( string entityName ) {
		return entityName.Length!=0;
	}
	public bool IsNotation( string name ) {
		return name.Length!=0;
	}
}


}
