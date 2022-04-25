namespace Tenuto.Grammar.Util {

public class ExpCloner : ExpVisitorExp {
	
	public ExpCloner( ExpBuilder builder ) {
		this.builder = builder;
	}
	
	protected readonly ExpBuilder builder;
	
	public virtual Expression OnElement( ElementExp exp ) {
		return exp;
	}
	public virtual Expression OnAttribute( AttributeExp exp ) {
		return exp;
	}
	public virtual Expression OnGroup( GroupExp exp ) {
		return builder.CreateSequence(
			exp.exp1.Visit(this), exp.exp2.Visit(this) );
	}
	public virtual Expression OnInterleave( InterleaveExp exp ) {
		return builder.CreateInterleave(
			exp.exp1.Visit(this), exp.exp2.Visit(this) );
	}
	public virtual Expression OnChoice( ChoiceExp exp ) {
		return builder.CreateChoice(
			exp.exp1.Visit(this),
			exp.exp2.Visit(this) );
	}
	public virtual Expression OnOneOrMore( OneOrMoreExp exp ) {
		return builder.CreateOneOrMore( exp.exp.Visit(this) );
	}
	public virtual Expression OnMixed( MixedExp exp ) {
		return builder.CreateMixed( exp.exp.Visit(this) );
	}
	public virtual Expression OnRef( ReferenceExp exp ) {
		return exp.exp.Visit(this);
	}

	public virtual Expression OnList( ListExp exp ) {
		return builder.CreateList( exp.exp.Visit(this) );
	}
	public virtual Expression OnData( DataExp exp ) {
		return exp;
	}
	public virtual Expression OnValue( ValueExp exp ) {
		return exp;
	}
	public virtual Expression OnEmpty() {
		return Expression.Empty;
	}
	public virtual Expression OnNotAllowed() {
		return Expression.NotAllowed;
	}
	public virtual Expression OnText() {
		return Expression.Text;
	}
}
}
