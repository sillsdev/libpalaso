namespace Tenuto.Verifier {

using System;
using System.Collections;
using System.Diagnostics;
using Tenuto;
using Tenuto.Grammar;
using org.relaxng.datatype;

public interface Token {
	bool Accepts( ListExp exp );
	bool Accepts( ElementExp exp );
	bool Accepts( AttributeExp exp );
	bool Accepts( DataExp exp );
	bool Accepts( ValueExp exp );
	bool AcceptsText();
}

internal class TokenImpl : Token {
	public virtual bool Accepts( ListExp exp ) { return false; }
	public virtual bool Accepts( DataExp exp ) { return false; }
	public virtual bool Accepts( ValueExp exp ) { return false; }
	public virtual bool AcceptsText() { return false; }
	public virtual bool Accepts( ElementExp exp ) { return false; }
	public virtual bool Accepts( AttributeExp exp ) { return false; }
}

internal class StringToken : TokenImpl {

	internal StringToken( string literal, ExpBuilder builder, ValidationContext context ) {
		this.literal = literal;
		this.builder = builder;
		this.context = context;
	}
	
	private readonly string literal;
	private StringToken[] listItems = null;
	
	private readonly ExpBuilder builder;
	private readonly ValidationContext context;
	
	
	public override bool Accepts( ListExp exp ) {
		if( listItems==null ) {
			// split the literal by whitespace.
			string[] tokens = StringTokenizer.Tokenize(literal);
			
//			Trace.WriteLine(string.Format("literal:'{0}' token size:{1}",literal,tokens.Length));
			listItems = new StringToken[tokens.Length];
			for( int i=0; i<tokens.Length; i++ )
				listItems[i] = new StringToken(tokens[i],builder,context);
		}
		
		Expression body = exp.exp;
		for( int i=0; i<listItems.Length; i++ ) {
			body = Residual.Calc( body, listItems[i], builder );
//			Trace.WriteLine("list residual: "+ ExpPrinter.printContentModel(body));
			if( body==Expression.NotAllowed )	return false;
		}
		return body.IsNullable;
	}
	public override bool Accepts( DataExp exp ) {
		return exp.dt.IsValid(literal,context)
		&& (
			exp.except==null
		||	!Residual.Calc(exp.except,this,builder).IsNullable);
	}
	
	public override bool Accepts( ValueExp exp ) {
		object o = exp.dt.CreateValue(literal,context);
		if(o==null)		return false;
//		System.Diagnostics.Trace.WriteLine(
//			string.Format("value compare:{0}:{1}",o,exp.value));
		return exp.dt.SameValue(o,exp.value);
	}
	
	public override bool AcceptsText() {
		return true;
	}
}

internal class ElementToken : TokenImpl {
	
	internal ElementToken( ElementExp[] matched, int len ) {
		this.matched = matched;
		this.len = len;
	}
	
	private readonly ElementExp[] matched;
	private readonly int len;
	
	public override bool Accepts( ElementExp exp ) {
		return Array.IndexOf(matched,exp,0,len)>=0;
/*		int idx = Array.IndexOf(matched,exp,0,len);
		if(idx>=0)	Trace.WriteLine("hit :"+ExpPrinter.printContentModel(exp)+" "+idx);
		else		Trace.WriteLine("miss:"+ExpPrinter.printContentModel(exp));
		return idx>=0;
*/	}
}

internal class AttributeToken : TokenImpl {
	
	internal AttributeToken( string uri, string localName, string value, ValidationContext context, ExpBuilder builder ) {
		this.uri = uri;
		this.localName = localName;
		this.value = value;
		this.context = context;
		this.builder = builder;
		this.ignorable = (value.Trim().Length==0);
	}
	
	private readonly string uri;
	private readonly string localName;
	private readonly string value;
	private readonly bool ignorable;
	private readonly ValidationContext context;
	private readonly ExpBuilder builder;
	private StringToken valueToken = null;
	
	public override bool Accepts( AttributeExp exp ) {
		if( !exp.Name.Contains(uri,localName) )		return false;
		
		if( valueToken==null )
			valueToken = new StringToken( value, builder, context );
		
		if( ignorable && exp.exp.IsNullable )	return true;
		
		return Residual.Calc( exp.exp, valueToken, builder ).IsNullable;
	}
}



}
