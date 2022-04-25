namespace Tenuto.Grammar {

using System;
using System.Diagnostics;

public abstract class BinaryExp : Expression {
	
	public readonly Expression exp1;
	public readonly Expression exp2;
	
	protected BinaryExp( Expression exp1, Expression exp2, int opCode, bool eps )
					: base(exp1.GetHashCode()*exp2.GetHashCode()*opCode,eps) {
		this.exp1 = exp1;
		this.exp2 = exp2;
	}
	
	public override bool Equals( object o ) {
		if(o.GetType()!= this.GetType())	return false;
		BinaryExp rhs = (BinaryExp)o;
		return rhs.exp1==exp1 && rhs.exp2==exp2;
	}
	
	public Expression[] children {
		get {
			// count # of children
			int cnt=2;
			BinaryExp e = this;
			while(e.exp1.GetType()==this.GetType()) {
				cnt++;
				e = (BinaryExp)e.exp1;
			}
			
			Expression[] r = new Expression[cnt];
			
			// fill in the buffer
			e = this;
			while(true) {
				r[--cnt] = e.exp2;
				if(e.exp1.GetType()!=this.GetType()) {
					r[--cnt] = e.exp1;
					Debug.Assert(cnt==0);	// make sure that the buffer is fully filled.
					return r;
				}
				e = (BinaryExp)e.exp1;
			}
		}
	}
}

public sealed class GroupExp : BinaryExp {
	
	internal GroupExp( Expression exp1, Expression exp2 )
			: base(exp1,exp2,HASH_GROUP,
				exp1.IsNullable && exp2.IsNullable) {}
	public override object Visit( ExpVisitor visitor ) {
		return visitor.OnGroup(this);
	}
	public override void Visit( ExpVisitorVoid Visitor ) {
		Visitor.OnGroup(this);
	}
	public override Expression Visit( ExpVisitorExp visitor ) {
		return visitor.OnGroup(this);
	}
}

public sealed class ChoiceExp : BinaryExp {
	
	internal ChoiceExp( Expression exp1, Expression exp2 )
			: base(exp1,exp2,HASH_CHOICE,
				exp1.IsNullable || exp2.IsNullable) {}
	public override object Visit( ExpVisitor visitor ) {
		return visitor.OnChoice(this);
	}
	public override void Visit( ExpVisitorVoid Visitor ) {
		Visitor.OnChoice(this);
	}
	public override Expression Visit( ExpVisitorExp visitor ) {
		return visitor.OnChoice(this);
	}
}

public sealed class InterleaveExp : BinaryExp {
	
	internal InterleaveExp( Expression exp1, Expression exp2 )
			: base(exp1,exp2,HASH_INTERLEAVE,
				exp1.IsNullable && exp2.IsNullable) {}
	public override object Visit( ExpVisitor visitor ) {
		return visitor.OnInterleave(this);
	}
	public override void Visit( ExpVisitorVoid Visitor ) {
		Visitor.OnInterleave(this);
	}
	public override Expression Visit( ExpVisitorExp visitor ) {
		return visitor.OnInterleave(this);
	}
}

}
