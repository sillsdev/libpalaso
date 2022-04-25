namespace Tenuto.Verifier {

using Tenuto.Grammar;

public class Residual : ExpVisitorExp {
	
	public static Expression Calc( Expression exp, Token token, ExpBuilder builder ) {
		return exp.Visit( new Residual(token,builder) );
	}
	
	protected Residual( Token token, ExpBuilder builder ) {
		this.token = token;
		this.builder = builder;
	}
	
	private readonly Token token;
	private readonly ExpBuilder builder;
	
	public Expression OnGroup( GroupExp exp ) {
		Expression r1 = builder.CreateSequence(exp.exp1.Visit(this),exp.exp2);
		if( exp.exp1.IsNullable )
			return builder.CreateChoice( r1, exp.exp2.Visit(this) );
		else
			return r1;
	}
	
	public Expression OnChoice( ChoiceExp exp ) {
		return builder.CreateChoice(
			exp.exp1.Visit(this),
			exp.exp2.Visit(this) );
	}
	public Expression OnInterleave( InterleaveExp exp ) {
		return builder.CreateChoice(
			builder.CreateInterleave( exp.exp1, exp.exp2.Visit(this) ),
			builder.CreateInterleave( exp.exp1.Visit(this), exp.exp2 ));
	}
	
	public Expression OnOneOrMore( OneOrMoreExp exp ) {
		return builder.CreateSequence(
			exp.exp.Visit(this),
			builder.CreateZeroOrMore(exp.exp));
	}
	
	public Expression OnList( ListExp exp ) {
		if( token.Accepts(exp) )	return Expression.Empty;
		else						return Expression.NotAllowed;
	}
	public Expression OnMixed( MixedExp exp ) {
		if( token.AcceptsText() )	return exp;
		return builder.CreateMixed( exp.exp.Visit(this) );
	}
	
	public Expression OnElement( ElementExp exp ) {
		if( token.Accepts(exp) )	return Expression.Empty;
		else						return Expression.NotAllowed;
	}
	
	public Expression OnAttribute( AttributeExp exp ) {
		if( token.Accepts(exp) )	return Expression.Empty;
		else						return Expression.NotAllowed;
	}
	
	public Expression OnData( DataExp exp ) {
		if( token.Accepts(exp) )	return Expression.Empty;
		else						return Expression.NotAllowed;
	}
	
	public Expression OnValue( ValueExp exp ) {
		if( token.Accepts(exp) )	return Expression.Empty;
		else						return Expression.NotAllowed;
	}
	
	public Expression OnRef( ReferenceExp exp ) {
		return exp.exp.Visit(this);
	}
	
	public Expression OnEmpty() {
		return Expression.NotAllowed;
	}
	
	public Expression OnNotAllowed() {
		return Expression.NotAllowed;
	}
	
	public Expression OnText() {
		if( token.AcceptsText() )	return Expression.Text;	// return Text.
		else						return Expression.NotAllowed;
	}
}

}
