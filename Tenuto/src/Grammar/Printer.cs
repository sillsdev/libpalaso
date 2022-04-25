namespace Tenuto.Grammar {

using System.IO;

public class ExpPrinter : ExpVisitorVoid {
	
	public static string printContentModel( Expression exp ) {
		return print(exp,0,true);
	}
	public static string printFragment( Expression exp ) {
		return print(exp,-1,false);
	}
	public static string printGrammar( Grammar grammar ) {
		using( StringWriter str = new StringWriter() ) {
			ExpPrinter printer = new ExpPrinter(str,-1,false);
			str.Write("(start): ");
			grammar.exp.Visit(printer);
			str.Write("\n");
			
			foreach( ReferenceExp exp in grammar.NamedPatterns ) {
				str.Write(exp.name);
				str.Write(" : ");
				exp.exp.Visit(printer);
				str.Write("\n");
			}
			
			return str.ToString();
		}
	}
	
	
	
	public static string print( Expression exp, int elemDepth, bool visitRef ) {
		using( StringWriter str = new StringWriter() ) {
			exp.Visit( new ExpPrinter(str,elemDepth,visitRef) );
			return str.ToString();
		}
	}
	
	protected ExpPrinter( TextWriter _writer, int _elemDepth, bool _recurseRefExp ) {
		this.writer = _writer;
		this.visitDepth = _elemDepth;
		this.recurseRefExp = _recurseRefExp;
	}
	
	private TextWriter writer;
	private int visitDepth;
	private bool recurseRefExp;
	
	public void OnGroup( GroupExp exp ) {
		OnBinExp(exp,",");
	}
	public void OnChoice( ChoiceExp exp ) {
		if(exp.exp1==Expression.Empty) {
			OptimizedChoice(exp.exp2);
			return;
		}
		if(exp.exp2==Expression.Empty) {
			OptimizedChoice(exp.exp1);
			return;
		}
		OnBinExp(exp,"|");
	}
	protected void OptimizedChoice( Expression exp ) {
		if( exp is OneOrMoreExp ) {
			Visit( ((OneOrMoreExp)exp).exp );
			writer.Write('*');
			return;
		} else {
			Visit(exp);
			writer.Write('?');
		}
	}
	public void OnInterleave( InterleaveExp exp ) {
		OnBinExp(exp,"&");
	}
	protected void OnBinExp( BinaryExp exp, string separator ) {
		bool first = true;
		foreach( Expression e in exp.children ) {
			if(!first)
				writer.Write(separator);
			first=false;
			Visit(e);
		}
	}
	public void OnOneOrMore( OneOrMoreExp exp ) {
		Visit(exp.exp);
		writer.Write('+');
	}
	protected void Visit( Expression e ) {
		bool isComplex = false;
		if( e is BinaryExp )
			isComplex = true;
		if( e is ReferenceExp )
			isComplex = true;
		
		if(isComplex)	writer.Write('(');
		e.Visit(this);
		if(isComplex)	writer.Write(')');
	}
	
	
	public void OnList( ListExp exp ) {
		writer.Write('$');
		exp.exp.Visit(this);
		writer.Write('$');
	}
	public void OnMixed( MixedExp exp ) {
		writer.Write("##mixed[");
		exp.exp.Visit(this);
		writer.Write(']');
	}
	public void OnElement( ElementExp exp ) {
		writer.Write( exp.Name );
		if( visitDepth==0 )	return;
		
		visitDepth--;
		writer.Write('[');
		exp.exp.Visit(this);
		writer.Write(']');
		visitDepth++;
	}
	public void OnAttribute( AttributeExp exp ) {
		writer.Write('@');
		writer.Write( exp.Name );
		
		writer.Write('[');
		exp.exp.Visit(this);
		writer.Write(']');
	}
	public void OnData( DataExp exp ) {
		writer.Write('\\');
		writer.Write(exp.name);
		if(exp.except!=null) {
			writer.Write("~(");
			exp.except.Visit(this);
			writer.Write(')');
		}
	}
	public void OnValue( ValueExp exp ) {
		writer.Write('"');
		writer.Write(exp.value);
		writer.Write('"');
	}
	public void OnRef( ReferenceExp exp ) {
		if(recurseRefExp) {
			exp.exp.Visit(this);
		} else {
			writer.Write("{%");
			writer.Write(exp.name);
			writer.Write("}");
		}
	}
	public void OnEmpty() {
		writer.Write("##empty");
	}
	public void OnNotAllowed() {
		writer.Write("##notAllowed");
	}
	public void OnText() {
		writer.Write("##text");
	}
	
}

}
