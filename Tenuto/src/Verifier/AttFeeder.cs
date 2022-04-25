namespace Tenuto.Verifier {

using Tenuto.Grammar;
using Tenuto.Grammar.Util;

public class AttFeeder : ExpCloner {
	
	public AttFeeder( ExpBuilder builder, Token _token ) : base(builder) {
		this.token = _token;
	}
	
	private readonly Token token;
	
	public override Expression OnElement( ElementExp exp ) {
		return Expression.NotAllowed;
	}
	public override Expression OnGroup( GroupExp exp ) {
		Expression e1 = exp.exp1.Visit(this);
		Expression e2 = exp.exp2.Visit(this);
		
		if(e1==Expression.NotAllowed)
			return builder.CreateSequence(exp.exp1,e2);
		if(e2==Expression.NotAllowed)
			return builder.CreateSequence(e1,exp.exp2);
		
		return builder.CreateChoice(
			builder.CreateSequence(exp.exp1,e2),
			builder.CreateSequence(e1,exp.exp2) );
	}
	public override Expression OnInterleave( InterleaveExp exp ) {
		Expression e1 = exp.exp1.Visit(this);
		Expression e2 = exp.exp2.Visit(this);
		
		if(e1==Expression.NotAllowed)
			return builder.CreateInterleave(exp.exp1,e2);
		if(e2==Expression.NotAllowed)
			return builder.CreateInterleave(e1,exp.exp2);
		
		return builder.CreateChoice(
			builder.CreateInterleave(exp.exp1,e2),
			builder.CreateInterleave(e1,exp.exp2) );
	}
	public override Expression OnOneOrMore( OneOrMoreExp exp ) {
		Expression e = exp.exp.Visit(this);
		return builder.CreateSequence( e,
			builder.CreateOptional(exp) );
	}
	public override Expression OnAttribute( AttributeExp exp ) {
		if(token.Accepts(exp))	return Expression.Empty;
		else					return Expression.NotAllowed;
	}
	public override Expression OnList( ListExp exp ) {
		return Expression.NotAllowed;
	}
	public override Expression OnData( DataExp exp ) {
		return Expression.NotAllowed;
	}
	public override Expression OnValue( ValueExp exp ) {
		return Expression.NotAllowed;
	}
	public override Expression OnEmpty() {
		return Expression.NotAllowed;
	}
	public override Expression OnText() {
		return Expression.NotAllowed;
	}
}


public class AttPruner {
	
	private readonly AttPrunerImpl pruner;
	public AttPruner( ExpBuilder builder ) {
		pruner = new AttPrunerImpl(builder);
	}

	public Expression prune( Expression exp ) {
		if(exp.attributePrunedExp==null)
			// if this is the first time to compute this value
			exp.attributePrunedExp=exp.Visit(pruner);
		return exp.attributePrunedExp;
	}

	private class AttPrunerImpl : ExpCloner {
		
		public AttPrunerImpl( ExpBuilder builder ) : base(builder) {}

		public override Expression OnAttribute( AttributeExp exp ) {
			return Expression.NotAllowed;
		}
	}
}

}
