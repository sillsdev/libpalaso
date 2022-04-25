namespace Tenuto.Grammar {

public abstract class XmlItemExp : UnaryExp {
	
	public readonly NameClass Name;
	
	internal XmlItemExp( NameClass nc, Expression contentModel, int opCode )
			: base(contentModel,opCode,false) {
		this.Name = nc;
	}
	public override bool Equals( object o ) {
		// use object identity
		return this==o;
	}
}

public class ElementExp : XmlItemExp {
	
	protected internal ElementExp( NameClass name, Expression contentModel )
		: base(name,contentModel,HASH_ELEMENT) {}
	public override object Visit( ExpVisitor Visitor ) {
		return Visitor.OnElement(this);
	}
	public override void Visit( ExpVisitorVoid Visitor ) {
		Visitor.OnElement(this);
	}
	public override Expression Visit( ExpVisitorExp Visitor ) {
		return Visitor.OnElement(this);
	}
}

public class AttributeExp : XmlItemExp {
	
	protected internal AttributeExp( NameClass name, Expression contentModel )
		: base(name,contentModel,HASH_ATTRIBUTE) {}
	public override object Visit( ExpVisitor Visitor ) {
		return Visitor.OnAttribute(this);
	}
	public override void Visit( ExpVisitorVoid Visitor ) {
		Visitor.OnAttribute(this);
	}
	public override Expression Visit( ExpVisitorExp Visitor ) {
		return Visitor.OnAttribute(this);
	}
}


}
