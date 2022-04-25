namespace Tenuto.Grammar {

public abstract class UnaryExp : Expression {
	
	public readonly Expression exp;
	
	protected UnaryExp( Expression exp, int opCode, bool eps )
			: base(exp.GetHashCode()*opCode,eps) {
		this.exp = exp;
	}
	
	public override bool Equals( object o ) {
		if(o.GetType()!=this.GetType())	return false;
		return ((UnaryExp)o).exp==this.exp;
	}
}

public sealed class OneOrMoreExp : UnaryExp {
	
	internal OneOrMoreExp( Expression exp )
			: base(exp,HASH_ONEORMORE,exp.IsNullable) {}
	public override object Visit( ExpVisitor visitor ) {
		return visitor.OnOneOrMore(this);
	}
	public override void Visit( ExpVisitorVoid Visitor ) {
		Visitor.OnOneOrMore(this);
	}
	public override Expression Visit( ExpVisitorExp visitor ) {
		return visitor.OnOneOrMore(this);
	}
}

public sealed class ListExp : UnaryExp {
	
	internal ListExp( Expression exp )
			: base(exp,HASH_LIST,exp.IsNullable) {}
	public override object Visit( ExpVisitor visitor ) {
		return visitor.OnList(this);
	}
	public override void Visit( ExpVisitorVoid visitor ) {
		visitor.OnList(this);
	}
	public override Expression Visit( ExpVisitorExp visitor ) {
		return visitor.OnList(this);
	}
}

public sealed class MixedExp : UnaryExp {
	
	internal MixedExp( Expression exp )
			: base(exp,HASH_MIXED,exp.IsNullable) {}
	public override object Visit( ExpVisitor visitor ) {
		return visitor.OnMixed(this);
	}
	public override void Visit( ExpVisitorVoid Visitor ) {
		Visitor.OnMixed(this);
	}
	public override Expression Visit( ExpVisitorExp visitor ) {
		return visitor.OnMixed(this);
	}
}


}
