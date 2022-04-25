namespace Tenuto.Grammar {

using System;

public abstract class Expression {
	
	protected Expression( int hashCode, bool nullability ) {
		cachedHashCode = hashCode;
		this.nullability = nullability;
	}
	
	private readonly int cachedHashCode;
	
	public sealed override int GetHashCode() {
		return cachedHashCode;
	}
	
	protected bool nullability;

	/// <summary>
	///  if the expression can match to nothing (an empty sequence.)
	/// </summary>
	public virtual bool IsNullable {
		get {
			return nullability;
		}
	}

	/**
	 * the expression that is equivalent to this.Visit(attPruner)
	 */
	internal Expression attributePrunedExp;
	
	public abstract Object Visit( ExpVisitor Visitor );
	public abstract void Visit( ExpVisitorVoid Visitor );
	public abstract Expression Visit( ExpVisitorExp Visitor );
	
	
	
	private sealed class EmptyExp : Expression {
		internal EmptyExp() : base(1,true) {}
		public override bool Equals( Object rhs ) {
			return this==rhs;
		}
		public override Object Visit( ExpVisitor Visitor ) {
			return Visitor.OnEmpty();
		}
		public override void Visit( ExpVisitorVoid Visitor ) {
			Visitor.OnEmpty();
		}
		public override Expression Visit( ExpVisitorExp Visitor ) {
			return Visitor.OnEmpty();
		}
	}
	public static readonly Expression Empty = new EmptyExp();
	
	
	private sealed class NotAllowedExp : Expression {
		internal NotAllowedExp() : base(3,false) {}
		public override bool Equals( Object rhs ) {
			return this==rhs;
		}
		public override Object Visit( ExpVisitor Visitor ) {
			return Visitor.OnNotAllowed();
		}
		public override void Visit( ExpVisitorVoid Visitor ) {
			Visitor.OnNotAllowed();
		}
		public override Expression Visit( ExpVisitorExp Visitor ) {
			return Visitor.OnNotAllowed();
		}
	}
	public static readonly Expression NotAllowed = new NotAllowedExp();
	
	
	private sealed class TextExp : Expression {
		internal TextExp() : base(5,true) {}
		public override bool Equals( Object rhs ) {
			return this==rhs;
		}
		public override Object Visit( ExpVisitor Visitor ) {
			return Visitor.OnText();
		}
		public override void Visit( ExpVisitorVoid Visitor ) {
			Visitor.OnText();
		}
		public override Expression Visit( ExpVisitorExp Visitor ) {
			return Visitor.OnText();
		}
	}
	public static readonly Expression Text = new TextExp();
	
	
	//
	// hash constants
	//
	internal const int HASH_GROUP	= 17;
	internal const int HASH_CHOICE	= 19;
	internal const int HASH_INTERLEAVE	= 23;
	internal const int HASH_ONEORMORE	= 29;
	internal const int HASH_LIST	= 31;
	internal const int HASH_KEY	= 37;
	internal const int HASH_MIXED	= 41;
	internal const int HASH_ELEMENT	= 43;
	internal const int HASH_ATTRIBUTE	= 47;
	internal const int HASH_DATA	= 51;
	internal const int HASH_VALUE	= 53;
}

}
