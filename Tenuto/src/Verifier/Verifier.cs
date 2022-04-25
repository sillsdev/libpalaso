#define	TRACE

namespace Tenuto.Verifier {

using System;
using System.Diagnostics;
using System.Resources;
using System.Text;
using System.Xml;
using Tenuto.Grammar;
using org.relaxng.datatype;

public class Verifier : ValidationContext {
	
	public static bool Verify(
		XmlReader document, Grammar grammar, ErrorHandler handler ) {
		
		while(!document.IsStartElement())
			document.Read();
		
		Verifier v = new Verifier(document,grammar.Builder,handler);
		v.Verify(grammar);
		return !v.hadError;
	}
	
	protected Verifier( XmlReader document, ExpBuilder builder, ErrorHandler handler ) {
		this.document = document;
		this.handler = handler;
		this.builder = builder;
		this.attPruner = new AttPruner(builder);
		
		emptyStringToken = new StringToken("",builder,this);
	}
	
	
	protected readonly XmlReader	document;
	protected ErrorHandler			handler;
	protected bool					hadError = false;
	protected readonly ExpBuilder	builder;
	protected readonly AttPruner	attPruner;
	
	private readonly StringToken	emptyStringToken;
	
	private Expression Verify( Expression contentModel ) {
		return Verify(contentModel,null);
	}

	// <a>
	//   <b/><c>...</c><d/>
	// </a>
	// XmlReader should be immediately after reading <a>.
	// When this method returns, it will be immediately after </a>.
	private Expression Verify( Expression contentModel, Expression[] atoms ) {
	
		while(!document.EOF) {
			switch( document.NodeType ) {
			case XmlNodeType.CDATA:
			case XmlNodeType.Text:
			case XmlNodeType.Whitespace:
			case XmlNodeType.SignificantWhitespace:
				OnText();
				break;
			
			case XmlNodeType.Element:
				contentModel = OnElement( contentModel, atoms );
				break;
				
			case XmlNodeType.EndElement:
				return contentModel;
				
			// nodes that are ignored
			case XmlNodeType.Comment:
			case XmlNodeType.ProcessingInstruction:
				document.Read();
				break;
				
			default:
				Console.WriteLine(document.NodeType);
				Debug.Assert(false);
				break;
			}
		}
		
		return contentModel;
	}

	private StringBuilder characters = new StringBuilder();
	private void OnText() {
		characters.Append( document.Value );
		document.Read();
	}
	
	private Expression VerifyText( Expression exp, Expression[] atoms ) {
		string literal;
		
		if(characters.Length==0) {
			literal = String.Empty;
		} else {
			literal = characters.ToString();
			characters = new StringBuilder();
		}
		bool ignorable = (literal.Trim().Length==0);
		
		
		Trace.WriteLine("characters: "+literal.Trim());
		
		StringToken token = new StringToken(literal,builder,this);
		
		Expression r = Residual.Calc( exp, token, builder );
		if(ignorable)	exp = builder.CreateChoice( exp, r );
		else			exp = r;
		
		if(exp==Expression.NotAllowed) {
			// error: unexpected literal
			if(literal.Length>20)	literal=literal.Substring(0,20)+" ...";
			ReportError( ERR_INVALID_TEXT, literal );
		}
		
		if(atoms!=null)
			for( int i=atoms.Length-1; i>=0; i-- ) {
				r = Residual.Calc( atoms[i], token, builder );
				if(ignorable)	atoms[i] = builder.CreateChoice( atoms[i], r );
				else			atoms[i] = r;
			}
		
		return exp;
	}
	
	private Expression OnElement( Expression contentModel, Expression[] atoms ) {
		contentModel = VerifyText(contentModel,atoms);
		
		Trace.WriteLine("<"+document.Name+">");
		Trace.Indent();
		Trace.WriteLine("expecting: "+ ExpPrinter.printContentModel(contentModel));
		
		ElementExp[] match = ContentModelCollector.Collect(
			contentModel, new XmlName(document.NamespaceURI,document.LocalName) );
		
		if(match[0]==null) {
			// error: unexpected element
			ReportError(ERR_UNEXPECTED_ELEMENT,document.Name);
		}
		
		Expression combinedChild = Expression.NotAllowed;
		int i;
		for( i=0; match[i]!=null; i++ )
			combinedChild = builder.CreateChoice( combinedChild, match[i].exp );
		int clen = i;
		
		Expression[] cp = null;
		if(clen>1) {
			Trace.WriteLine(string.Format("{0} elements are matched",clen));
			cp = new Expression[clen];
			for( i=0; i<clen; i++ )
				cp[i] = match[i].exp;
		}

		Trace.WriteLine("combined child: "+
			ExpPrinter.printContentModel(combinedChild));

		if(document.MoveToFirstAttribute())
			do {
				combinedChild = OnAttribute( combinedChild, cp );
			} while( document.MoveToNextAttribute() );
		
		document.MoveToElement();
		
		combinedChild = attPruner.prune(combinedChild);
		if(combinedChild==Expression.NotAllowed) {
			// error: required attribute is missing
			// TODO: name of required attributes
			ReportError(ERR_MISSING_ATTRIBUTE);
		}
			
		if( !document.IsEmptyElement ) {
			document.ReadStartElement();
			combinedChild = Verify(combinedChild,cp);
			combinedChild = VerifyText(combinedChild,cp);
			Trace.Unindent();
			Trace.WriteLine("</"+document.Name+">");
		} else {
			// treat it as ""
			combinedChild = VerifyText(combinedChild,cp);
			Trace.Unindent();
			Trace.WriteLine("</"+document.Name+">");
		}
		
		if( !combinedChild.IsNullable ) {
			// error: unexpected end of element.
			ReportError( ERR_CONTENTMODEL_INCOMPLETE, document.Name );
		}
		
		document.Read();	// read the end tag.
		
		
		if(cp!=null)
			for( i=0; i<clen; i++ )
				if(!cp[i].IsNullable)
					match[i]=null;
		
		ElementToken e = new ElementToken(match,clen);
		if(atoms!=null)
			for( i=0; i<atoms.Length; i++ )
				atoms[i] = Residual.Calc(atoms[i],e,builder);
		
		contentModel = Residual.Calc(contentModel,e,builder);
		Trace.WriteLine("residual: " + ExpPrinter.printContentModel(contentModel));
		return contentModel;
	}

	protected Expression OnAttribute( Expression exp, Expression[] atoms ) {
		if(document.Name!="xmlns"
		&& document.Prefix!="xmlns") {
			Trace.WriteLine("@"+document.Name);
			
			AttributeToken token = new AttributeToken(
					document.NamespaceURI,
					document.LocalName,
					document.Value,
					this, builder );
			AttFeeder feeder = new AttFeeder(builder,token);

			exp = exp.Visit(feeder);
			if( exp==Expression.NotAllowed ) {
				// error: bad attribute
				ReportError(ERR_BAD_ATTRIBUTE,document.Name);
			}
			
			if(atoms!=null)
				for( int i=0; i<atoms.Length; i++ )
					atoms[i] = atoms[i].Visit(feeder);
			
			Trace.WriteLine("residual: " + ExpPrinter.printContentModel(exp));
		}
		return exp;
	}

//
// ValidationContext implementation
//======================================
//	
	public string ResolveNamespacePrefix( string prefix ) {
		if(prefix=="xml")	return "http://www.w3.org/XML/1998/namespace";
		return document.LookupNamespace(prefix);
	}
	
	public bool IsUnparsedEntity( string entityName ) {
		// TODO: how we can obtain this information from System.Xml?
		return true;
	}
	public bool IsNotation( string notationName ) {
		// TODO: how we can obtain this information from System.Xml?
		return true;
	}

//
// Error handling
//======================================
//
	private ResourceManager resManager = null;
	
	public void ReportError( string property, params object[] args ) {
		if( resManager==null )
			resManager = new ResourceManager("Verifier",this.GetType().Assembly);
		
		hadError = true;
		
		handler.Error(
			string.Format( resManager.GetString(property), args ) );
	}
	
	protected const string ERR_UNEXPECTED_ELEMENT =
		"Verifier.UnexpectedElement";
	protected const string ERR_BAD_ATTRIBUTE =
		"Verifier.BadAttribute";
	protected const string ERR_MISSING_ATTRIBUTE =
		"Verifier.MissingAttribute";
	protected const string ERR_INVALID_TEXT =
		"Verifier.InvalidText";
	protected const string ERR_CONTENTMODEL_INCOMPLETE =
		"Verifier.ContentModelIncomplete";
}


public interface ErrorHandler {
	void Error( string msg );
}

}
