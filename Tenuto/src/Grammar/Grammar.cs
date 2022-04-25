namespace Tenuto.Grammar {

using System.Collections;

public class ReferenceExp : Expression {
	
	// TODO: is there any method that give us the default hash code
	// of an object? Like System.IdentityHashCode() of Java.
	private static System.Random rnd = new System.Random();
	
	public ReferenceExp( string name, Expression exp )
			: base(rnd.Next(),false){
		this.name = name;
		this.exp = exp;
	}
	
	public ReferenceExp( string name ) : this(name,null) {}
	
	public readonly string name;
	public Expression exp;

	public override bool IsNullable {
		get {
			if(exp==null)		return false;
			return exp.IsNullable;
		}
	}
	public override object Visit( ExpVisitor visitor ) {
		return visitor.OnRef(this);
	}
	public override void Visit( ExpVisitorVoid visitor ) {
		visitor.OnRef(this);
	}
	public override Expression Visit( ExpVisitorExp visitor ) {
		return visitor.OnRef(this);
	}
}

public class Grammar : ReferenceExp {
	
	public Grammar( string name, Expression top,
						ExpBuilder builder, Grammar parent ) : base(name,top) {
		this.Parent = parent;
		this.Builder = builder;
	}
	public Grammar( Grammar parent, ExpBuilder builder ) : base(null) {
		this.Parent = parent;
		this.Builder = builder;
	}
	public Grammar( ExpBuilder builder ) : this(null,builder) {}
	
	
	private readonly IDictionary namedPatterns = new Hashtable();
	// parent grammar, if any.
	public readonly Grammar Parent;
	
	public readonly ExpBuilder Builder;
	
	// gets or creates a ReferenceExp which has the specified name.
	public ReferenceExp GetOrCreate( string name ) {
		ReferenceExp exp = (ReferenceExp)namedPatterns[name];
		if(exp==null)
			namedPatterns[name] = exp = new ReferenceExp(name);
		return exp;
	}
	
	public ReferenceExp Get( string name ) {
		return (ReferenceExp)namedPatterns[name];
	}
	
	public IEnumerable NamedPatterns {
		get {
			return namedPatterns.Values;
		}
	}
}

}
