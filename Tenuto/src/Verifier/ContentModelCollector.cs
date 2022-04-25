namespace Tenuto.Verifier {

using Tenuto.Grammar;

internal class ContentModelCollector : ExpVisitorVoid {
	
	internal static ElementExp[] Collect( Expression exp, XmlName tagName ) {
		ContentModelCollector col = new ContentModelCollector(tagName);
		exp.Visit(col);
		return col.matched;
	}
	
	
	private ContentModelCollector( XmlName tagName ) {
		this.tagName = tagName;
	}
	
	private readonly XmlName tagName;
	private ElementExp[] matched = new ElementExp[4];
	private int matchedSize = 0;
	
	
	public void OnElement( ElementExp exp ) {
		if( exp.Name.Contains(tagName) ) {
			if(matched.Length==matchedSize) {
				ElementExp[] buf = new ElementExp[matched.Length*2];
				matched.CopyTo(buf,0);
				matched = buf;
			}
			matched[matchedSize++] = exp;
		}
	}
	public void OnGroup( GroupExp exp ) {
		exp.exp1.Visit(this);
		if(exp.exp1.IsNullable)
			exp.exp2.Visit(this);
	}
	
	public void OnChoice( ChoiceExp exp ) {
		exp.exp1.Visit(this);
		exp.exp2.Visit(this);
	}
	public void OnInterleave( InterleaveExp exp ) {
		exp.exp1.Visit(this);
		exp.exp2.Visit(this);
	}
	public void OnOneOrMore( OneOrMoreExp exp ) {
		exp.exp.Visit(this);
	}
	public void OnMixed( MixedExp exp ) {
		exp.exp.Visit(this);
	}
	public void OnRef( ReferenceExp exp ) {
		exp.exp.Visit(this);
	}
	
	
	
	public void OnList( ListExp exp ) {}
//	public void OnKey( KeyExp exp ) {}
	public void OnAttribute( AttributeExp exp ) {}
	public void OnData( DataExp exp ) {}
	public void OnValue( ValueExp exp ) {}
	public void OnEmpty() {}
	public void OnNotAllowed() {}
	public void OnText() {}
}

}
