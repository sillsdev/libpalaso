namespace Tenuto.Reader {

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Resources;
using System.Xml;
using Tenuto.Grammar;
using org.relaxng.datatype;

/*

	TODO: there seems to be a bug in the "xml" namespace handling.
	XmlReader doesn't consider the "xml" prefix to be bounded to
	"http://www.w3.org/XML/1998/namespace".

	TODO: XmlReader doesn't also properly honor xml:base

	Inheritance:
		You may want to override the ResolveEntity method to perform
		customized entity resolution.

		You may want to override the CreateXmlReader method to create
		your choice of XmlReader. By default, this class uses XmlValidatingReader
		with validating turned off (to properly expand entities), but
		you can also use XmlTextReader, or other XmlReader implementations.
*/
public class GrammarReader : ValidationContext {

	public Grammar parse( string sourceURL ) {

		Uri uri;
		try {
			uri = new Uri(sourceURL);
		} catch( UriFormatException ) {
			uri = new Uri(Path.GetFullPath(sourceURL));
		}

		return parse(
			CreateXmlReader( uri.AbsoluteUri,
			(Stream)Resolver.GetEntity( uri, null, typeof(Stream))));
	}

	public Grammar parse( XmlReader reader ) {

		// reset variables
		nsStack.Clear();
		nsStack.Push("");
		dtLibURIStack.Clear();
		baseUriStack.Clear();
		entityURLs.Clear();
		readerStack.Clear();
		readerStack.Push(null);

		PushEntity(reader);
		try {
			// skip xml declaration, DOCTYPE etc.
			while(!reader.IsStartElement())	reader.Read();

			HadError = false;
			Expression exp = ReadExp();
			if( HadError )	return null;	// there was an error.

			if( exp is Grammar )
				return (Grammar)exp;
			else {
				Grammar g = new Grammar(Builder);
				g.exp = exp;
				return g;
			}
		} finally {
			PopEntity();
		}
	}

//
// publicly accessible parameters
//========================================
// these parameters affects various aspects of the parsing process.
//
	// this controller receives error/warning messages
	public GrammarReaderController Controller;
	// this object is used to construct the grammar.
	public ExpBuilder Builder;
	// this resolver is used to resolve external references.
	public XmlResolver Resolver = new XmlUrlResolver();
	// string messages are resolved through this object.
	public ResourceManager ResManager;


	public GrammarReader( GrammarReaderController controller )
		: this(controller, new ExpBuilder()) {}


	protected XmlReader reader;
	protected Grammar grammar;

	// RELAX NG namespace
	public const string RELAXNGNamespace = "http://relaxng.org/ns/structure/1.0";

	public GrammarReader( GrammarReaderController controller, ExpBuilder builder ) {
		this.Controller = controller;
		this.Builder = builder;
//		this.ResManager = new ResourceManager(this.GetType());
		this.ResManager = new ResourceManager("GrammarReader",this.GetType().Assembly);

		{
			// derived classes can set additional ExpReader directly to
			// the ExpReaders field.
			IDictionary dic = new Hashtable();
			dic["notAllowed"]	= new ExpReader(NotAllowed);
			dic["empty"]		= new ExpReader(Empty);
			dic["group"]		= new ExpReader(Group);
			dic["choice"]		= new ExpReader(Choice);
			dic["interleave"]	= new ExpReader(Interleave);
			dic["optional"]		= new ExpReader(Optional);
			dic["zeroOrMore"]	= new ExpReader(ZeroOrMore);
			dic["oneOrMore"]	= new ExpReader(OneOrMore);
			dic["mixed"]		= new ExpReader(Mixed);
			dic["list"]			= new ExpReader(List);
			dic["element"]		= new ExpReader(Element);
			dic["attribute"]	= new ExpReader(Attribute);
			dic["externalRef"]	= new ExpReader(ExternalRef);
			dic["ref"]			= new ExpReader(Ref);
			dic["parentRef"]	= new ExpReader(ParentRef);
			dic["grammar"]		= new ExpReader(GrammarElm);
			dic["data"]			= new ExpReader(Data);
			dic["value"]		= new ExpReader(Value);
			dic["text"]			= new ExpReader(Text);
			ExpReaders = dic;
		}
		{
			IDictionary dic = new Hashtable();
			dic["choice"]	= new NCReader(ChoiceName);
			dic["name"]		= new NCReader(SimpleName);
			dic["nsName"]	= new NCReader(NsName);
			dic["anyName"]	= new NCReader(AnyName);
			NCReaders = dic;
		}
	}

//
// XmlReader-related low level utility methods
//================================================
// To handle "ns" and "datatypeLibrary" attributes,
// reader.ReadStartElement and reader.ReadEndElement methods may not be
// directly called. Instead, methods defined in this class should be called.
	private readonly Stack nsStack = new Stack();
	private readonly Stack dtLibURIStack = new Stack();

	// at least with Beta2 SDK, XmlReader.BaseURI doesn't handle
	// xml:base attribute, so we have to process them manually.
	private readonly Stack baseUriStack = new Stack();

	private readonly Stack readerStack = new Stack();

	// source URL of entities
	private readonly Stack entityURLs = new Stack();

	// Set up variables to process a next xml file
	protected bool PushEntity( XmlReader newReader ) {
		if(newReader==null)
			return false;	// error of ResolveEntity is handled here

		string systemId = newReader.BaseURI;

		if(entityURLs.Contains(systemId)) {
			ReportError( ERR_RECURSIVE_INCLUSION, systemId );
			return false;
		}

		entityURLs.Push(systemId);
		readerStack.Push(reader);
		// ns attribute will be propagated, so don't push it
		dtLibURIStack.Push("");
		baseUriStack.Push(new Uri(systemId));

		reader = newReader;
		Trace.WriteLine(string.Format("PushEntity({0})",reader.BaseURI));
		return true;
	}

	// Closes the current reader and gets back to the parent
	protected void PopEntity() {
		Trace.WriteLine(string.Format("PopEntity({0})",reader.BaseURI));
		reader.Close();

		dtLibURIStack.Pop();
		baseUriStack.Pop();
		reader = (XmlReader)readerStack.Pop();
		entityURLs.Pop();
	}


	/**
	 * return false if this element is an empty element
	 */
	protected bool ReadStartElement() {
		Trace.WriteLine("read <"+reader.Name+">");

		string ns = GetAttribute("ns");
		if(ns==null)		ns=(string)nsStack.Peek();
		nsStack.Push(ns);

		bool isEmpty = reader.IsEmptyElement;

		string dtLibURI = GetAttribute("datatypeLibrary");
		if(dtLibURI==null)
			dtLibURI=(string)dtLibURIStack.Peek();

		string xmlBase = GetAttribute("xml:base");
		Uri baseUri = (Uri)baseUriStack.Peek();
		if(xmlBase!=null)	baseUri = new Uri(baseUri,xmlBase);

		if(!isEmpty) {
			dtLibURIStack.Push(dtLibURI);
			baseUriStack.Push(baseUri);
		}
		reader.ReadStartElement();
		return !isEmpty;
	}

	protected void ReadEndElement() {
		Trace.WriteLine("read </"+reader.Name+">");

		nsStack.Pop();
		dtLibURIStack.Pop();
		baseUriStack.Pop();
		reader.ReadEndElement();
	}

	protected string GetAttribute( string name ) {
		string str = reader.GetAttribute(name);
		if(str==null)	return null;
		else			return str.Trim();
	}

	protected string GetAttribute( string ns, string local ) {
		string str = reader.GetAttribute(ns,local);
		if(str==null)	return null;
		else			return str.Trim();
	}

	// gets the propagated value of the ns attribute.
	protected string ns {
		get {
			string ns = GetAttribute("ns");
			if(ns!=null)	return ns;
			else			return (string)nsStack.Peek();
		}
	}
	protected Uri baseUri {
		get {
			Uri u = (Uri)baseUriStack.Peek();
			string v = GetAttribute("xml:base");
			if(v!=null)		return new Uri(u,v);
			else			return u;
		}
	}
	protected string datatypeLibraryURI {
		get {
			string uri = GetAttribute("datatypeLibrary");
			if(uri!=null)	return uri;
			else			return (string)dtLibURIStack.Peek();
		}
	}
	protected DatatypeLibrary datatypeLibrary {
		get {
			// TODO: use a map to cache DatatypeLibrary object
			return ResolveDatatypeLibrary(datatypeLibraryURI);
		}
	}

	// resolves the datatypeLibrary attribute value to a DatatypeLibrary object.
	protected virtual DatatypeLibrary ResolveDatatypeLibrary( string uri ) {
		if(uri=="")
			return org.relaxng.datatype.helpers.BuiltinDatatypeLibrary.theInstance;
		if(uri=="http://www.w3.org/2001/XMLSchema-datatypes"
		|| uri=="http://www.w3.org/2001/XMLSchema")
			return Tenuto.Datatype.XSDLib.XMLSchemaDatatypeLibrary.theInstance;
		throw new Exception(uri);
	}

	// converts a QName to an XmlName.
	protected XmlName ProcessQName( string qname ) {
		int idx = qname.IndexOf(':');
		if(idx<0)	return new XmlName(ns,qname);	// no prefix

		string uri = reader.LookupNamespace(qname.Substring(0,idx));
		if(uri==null) {
			ReportError( ERR_INVALID_QNAME, qname );
			return new XmlName("undefined","undefined");
		}
		return new XmlName(uri,qname.Substring(idx+1));
	}



	// skips any foreign elements
	// return true if XmlReader is on the start tag.
	// return false if XmlReader is on the end tag.
	protected bool SkipForeignElements() {
		while(reader.IsStartElement()) {
			if(reader.NamespaceURI==RELAXNGNamespace)
				return true;	// found a RELAX NG element.
			Trace.WriteLine("skip an element {"+reader.NamespaceURI+"}"+reader.LocalName );
			// foreign element. skip it.
			reader.Skip();
		}
		// found an end tag.
		return false;
	}

	// makes sure that the current element has no RELAX NG child elements.
	protected void EmptyContent() {
		Trace.WriteLine("EmptyContent()");
		if(!ReadStartElement()) {
			Trace.WriteLine("</>");
			return;	// empty content
		}
		// skip any elements from forein namespace
		while(SkipForeignElements()) {
			// elements from RELAX NG namespace. Error.
			ReportError( ERR_UNEXPECTED_ELEMENT, reader.Name );
			reader.Skip();
		}

		// TODO: how can we detect literal strings?
		ReadEndElement();
	}

	/**
	 * reads text inside an element and returns it.
	 *
	 * caller should call the ReadStartElement before
	 * calling this method. null is returned when an error occurs.
	 *
	 * caller is also responsible to call the ReadEndElement method
	 */
	protected string ReadPCDATA() {
		string s  = "";
		bool hadError = false;
		while(true) {
			switch(reader.NodeType) {
			case XmlNodeType.CDATA:
			case XmlNodeType.Text:
			case XmlNodeType.SignificantWhitespace:
			case XmlNodeType.Whitespace:
				s += reader.Value;
				reader.Read();
				break;

			case XmlNodeType.EndElement:
				if(hadError)	return null;
				else			return s;

			case XmlNodeType.Element:
				ReportError( ERR_UNEXPECTED_ELEMENT, reader.Name );
				hadError = true;
				reader.Skip();
				break;
			default:
				throw new Exception();
			}
		}
	}

	protected string GetRequiredAttribute( string attrName ) {
		string r = GetAttribute(attrName);
		if(r!=null)		return r;

		ReportError( ERR_MISSING_ATTRIBUTE, reader.Name, attrName );
		return null;
	}


//
// Expression Element Reader
//===================================================
// Each method parses one element (and its descendants) and returns an Expression.
// These methods assume that XmlReader is on a start tag when it's called.
// When the method returns, XmlReader is on a start tag of the next element.

	// XmlReader is on a start tag. Parses it and returns the result.
	protected delegate Expression ExpReader();
	protected readonly IDictionary ExpReaders;


	// XmlReader is on a start tag. Parses it and returns the result.
	protected virtual Expression ReadExp() {
		Trace.WriteLine("dispatching :"+reader.LocalName);
		ExpReader expreader = (ExpReader)ExpReaders[reader.LocalName];
		if(expreader==null) {
			// error: unknown element name
			ReportError( ERR_EXPRESSION_EXPECTED, reader.Name );
			reader.Skip();
			return Expression.NotAllowed;	// recover
		}

		Trace.Indent();
		Expression exp = expreader();	// dispatch the reader.
		Trace.Unindent();
		return exp;
	}


	protected virtual Expression Text() {
		EmptyContent();
		return Expression.Text;
	}
	protected virtual Expression Empty() {
		EmptyContent();
		return Expression.Empty;
	}
	protected virtual Expression NotAllowed() {
		EmptyContent();
		return Expression.NotAllowed;
	}
	protected virtual Expression ExternalRef() {
		XmlReader previous = reader;
		string href = GetRequiredAttribute("href");
		if(href==null) {
			EmptyContent();
			return Expression.NotAllowed;
		}

		// this method has to be called while we are at the start element.
		XmlReader newReader = ResolveEntity(href);

		EmptyContent();

		if(!PushEntity(newReader))
			return Expression.NotAllowed;
		try {
			// skip XML declarations, DOCTYPE, etc.
			while(!reader.IsStartElement())	reader.Read();
			return ReadExp();	// parse it.
		} finally {
			PopEntity();
		}
	}
	protected virtual Expression Ref() {
		string name = GetRequiredAttribute("name");
		EmptyContent();
		if(name==null)
			// error: missing attribute
			return Expression.NotAllowed;

		if(grammar==null) {
			// error: no-enclosing grammar
			ReportError( ERR_NO_GRAMMAR );
			return Expression.NotAllowed;
		}
		ReferenceExp exp = grammar.GetOrCreate(name);
		MemorizeReference(exp);
		return exp;
	}
	protected virtual Expression ParentRef() {
		string name = GetRequiredAttribute("name");
		EmptyContent();
		if(name==null)
			// error: missing attribute
			return Expression.NotAllowed;

		if(grammar==null || grammar.Parent==null) {
			// error: no-enclosing grammar
			ReportError( ERR_NO_GRAMMAR );
			return Expression.NotAllowed;
		}
		ReferenceExp exp = grammar.Parent.GetOrCreate(name);
		MemorizeReference(exp);
		return exp;
	}
	protected virtual Expression Group() {
		return ReadContainerExp( new ExpCombinator(Builder.CreateSequence) );
	}
	protected virtual Expression Interleave() {
		return ReadContainerExp( new ExpCombinator(Builder.CreateInterleave) );
	}
	protected virtual Expression Choice() {
		return ReadContainerExp( new ExpCombinator(Builder.CreateChoice) );
	}
	protected virtual Expression ReadContainerExp( ExpCombinator combinator ) {
		string name = reader.Name;
		if(ReadStartElement())
			return ReadChildExps( combinator );
		// no children. error
		ReportError( ERR_EXPRESSION_EXPECTED, name );
		return Expression.NotAllowed;
	}
	protected virtual Expression OneOrMore() {
		return Builder.CreateOneOrMore(Group());
	}
	protected virtual Expression ZeroOrMore() {
		return Builder.CreateZeroOrMore(Group());
	}
	protected virtual Expression Optional() {
		return Builder.CreateOptional(Group());
	}
	protected virtual Expression List() {
		return Builder.CreateList(Group());
	}
	protected virtual Expression Mixed() {
		return Builder.CreateMixed(Group());
	}
	protected virtual Expression Element() {
		bool isEmpty;
		NameClass nc = ReadNameClassOrNameAttr( false, out isEmpty );
		if(isEmpty) {
			ReportError( ERR_EXPRESSION_EXPECTED );
			return Expression.NotAllowed;
		}
		Expression contents = ReadChildExps( new ExpCombinator(Builder.CreateSequence) );
		return new ElementExp(nc,contents);
	}
	protected virtual Expression Attribute() {
		bool isEmpty;
		NameClass nc = ReadNameClassOrNameAttr( true, out isEmpty );
		Expression contents = Expression.Text;

		if(!isEmpty) {
			if( SkipForeignElements() )
				contents = ReadChildExps( new ExpCombinator(Builder.CreateSequence) );
			else
				// no child pattern. defaults to <text/>
				ReadEndElement();
		}
		return new AttributeExp(nc,contents);
	}
	protected virtual NameClass ReadNameClassOrNameAttr( bool isAttribute, out bool isEmpty ) {
		string name = GetAttribute("name");
		if(name!=null) {
			NameClass nc;

			// there is @name
			if( isAttribute
			&&  reader.GetAttribute("ns")==null	// there is no @ns
			&&  name.IndexOf(':')==-1 )	// name is NCName
				nc = new SimpleNameClass(new XmlName("",name));
			else
				nc = new SimpleNameClass(ProcessQName(name));

			isEmpty = !ReadStartElement();
			return nc;
		}
		isEmpty = !ReadStartElement();
		return ReadNameClass();
	}

	protected virtual Expression Value() {
		Datatype dt;
		string type = GetAttribute("type");
		if(type==null)
			dt = org.relaxng.datatype.helpers.TokenType.theInstance;
		else {
			try {
				dt = datatypeLibrary.CreateDatatype(type);
				Debug.Assert(dt!=null);
			} catch( DatatypeException e ) {
				ReportError( ERR_UNDEFINED_TYPENAME, type, e.Message );
				reader.Skip();
				return Expression.NotAllowed;
			}
		}

		try {
			object value;

			if(!ReadStartElement())
				value = dt.CreateValue("",this);
			else {
				string s = ReadPCDATA();
				if(s==null) {
					ReadEndElement();
					return Expression.NotAllowed;
				}

				// value must be created while we are still at the endElement
				value = dt.CreateValue(s,this);
				ReadEndElement();
			}

			return Builder.CreateValue(dt,value);
		} catch( DatatypeException ) {
			ReportError( ERR_BAD_VALUE_FOR_TYPE );
			return Expression.NotAllowed;
		}
	}

	protected virtual Expression Data() {
		string type = GetAttribute("type");
		if(type==null) {
			ReportError( ERR_MISSING_ATTRIBUTE, reader.Name, "type" );
			reader.Skip();
			return Expression.NotAllowed;
		}

		DatatypeBuilder builder;
		try {
			builder = datatypeLibrary.CreateDatatypeBuilder(type);
			Debug.Assert(builder!=null);
		} catch( DatatypeException e ) {
			ReportError( ERR_UNDEFINED_TYPENAME, type, e.Message );
			reader.Skip();
			return Expression.NotAllowed;
		}

		Expression except=null;

		if(ReadStartElement()) {
			// if the element has contents, parse them.
			while(SkipForeignElements()) {
				string name = reader.LocalName;
				if(name=="param")
					DataParam(builder);
				else
				if(name=="except") {
					if( except!=null ) {
						// only one "except" clause is allowed
						ReportError( ERR_MULTIPLE_EXCEPT );
						reader.Skip();
					} else {
						except = Choice();
					}
				} else {
					// error: unexpected element
					ReportError( ERR_EXCEPT_EXPECTED, reader.Name );
					reader.Skip();
				}
			}
			ReadEndElement();
		}

		try {
			// derive a type
			return Builder.CreateData(
				builder.CreateDatatype(), except, type );
		} catch( DatatypeException e ) {
			ReportError( ERR_DATATYPE_ERROR, type, e.Message );
			return Expression.NotAllowed;
		}
	}
	protected virtual void DataParam( DatatypeBuilder builder ) {
		string name = GetRequiredAttribute("name");
		if(name==null) {
			EmptyContent();
			return;
		}

		try {
			if(reader.IsEmptyElement) {
				builder.AddParameter(name,"",this);
				ReadStartElement();
			} else {
				ReadStartElement();
				string value = ReadPCDATA();
				if(value!=null)
					builder.AddParameter(name,value,this);
				ReadEndElement();
			}
		} catch( DatatypeException e ) {
			ReportError( ERR_BAD_DATATYPE_PARAMETER, name, e.Message );
		}
	}

	protected virtual Expression GrammarElm() {
		Grammar n = new Grammar(grammar,Builder);
		grammar = n;
		DivInGrammar();
		grammar = grammar.Parent;
		return n;
	}
	protected virtual void DivInGrammar() {
		if(!ReadStartElement())		return;

		while(SkipForeignElements()) {
			string name = reader.LocalName;
			if(name=="div")		DivInGrammar();
			else
			if(name=="start")	Start();
			else
			if(name=="define")	Define();
			else
			if(name=="include")	MergeGrammar();
			else {
				// error: unexpected element
				ReportError( ERR_UNEXPECTED_ELEMENT, reader.Name );
				reader.Skip();
			}
		}
		ReadEndElement();
	}
	protected virtual ReferenceExp Start() {
		string combine = GetAttribute("combine");
		Expression exp = null;
		if(ReadStartElement()) {
			while(SkipForeignElements()) {
				if(exp==null)	exp = ReadExp();
				else {
					// error: unexpected element. Only one child is allowed.
					ReportError( ERR_UNEXPECTED_ELEMENT, reader.Name );
					reader.Skip();
				}
			}
			ReadEndElement();
		} else {
			ReportError( ERR_EXPRESSION_EXPECTED );
			exp = Expression.NotAllowed;
		}
		CombineReferenceExp(grammar,exp,combine);
		return grammar;
	}
	protected virtual ReferenceExp Define() {
		string combine = GetAttribute("combine");
		string name = GetAttribute("name");
		if(name==null) {
			// error: missing attribute
			ReportError( ERR_MISSING_ATTRIBUTE, reader.Name, "name" );
			reader.Skip();
			return null;
		}
		Expression body = Group();	// parse the body.

		ReferenceExp exp = grammar.GetOrCreate(name);
		CombineReferenceExp(exp,body,combine);
		return exp;
	}

	private readonly Hashtable refParseInfos = new Hashtable();
	private class RefParseInfo {
		public string Combine;
		public bool HeadDefined;

		// this location specifies one of the referer to this expression
		public int LineNumber = -1;
		public int LinePosition = -1;
		public string SourceFile = null;
		public void MemorizeReference( XmlReader reader ) {
			if( reader is IXmlLineInfo ) {
				// source information is available only when the reader implements IXmlLineInfo
				IXmlLineInfo r = (IXmlLineInfo)reader;
				LineNumber = r.LineNumber;
				LinePosition = r.LinePosition;
				SourceFile = reader.BaseURI;	// TODO: is this correct?
			}
		}
	}
	protected void MemorizeReference( ReferenceExp exp ) {
		RefParseInfo pi = (RefParseInfo)refParseInfos[exp];
		if(pi==null)
			refParseInfos[exp] = pi = new RefParseInfo();
		pi.MemorizeReference(reader);
	}

	protected virtual void CombineReferenceExp(
				ReferenceExp r, Expression body, string combine ) {
		if( redefiningRefExps.ContainsKey(r) ) {
			// this pattern is currently being redefined.
			redefiningRefExps[r] = true;
			return;	// ignore the value
		}

		RefParseInfo pi = (RefParseInfo)refParseInfos[r];
		if(pi==null)	refParseInfos[r] = pi = new RefParseInfo();

		if( pi.Combine!=null && combine!=null && pi.Combine!=combine ) {
			// error: inconsistent combine method
			ReportError( ERR_INCONSISTENT_COMBINE, r.name );
			pi.Combine = null;
			return;
		}
		if( combine==null ) {
			if( pi.HeadDefined )
				// error: multiple heads
				ReportError( ERR_MULTIPLE_HEADS, r.name );
			pi.HeadDefined = true;
			combine = pi.Combine;
		} else {
			pi.Combine = combine;
		}

		if( r.exp==null )
			r.exp = body;
		else {
			if(combine=="interleave")	r.exp = Builder.CreateInterleave( r.exp, body );
			else
			if(combine=="choice")		r.exp = Builder.CreateChoice( r.exp, body );
			else {
				// error: invalid combine value
				ReportError( ERR_INVALID_COMBINE, combine );
			}
		}
	}

	// A set of ReferenceExps which are designated as being redefined.
	// This set is prepared by the MergeGrammar method, to check that
	// redefined expressions are in fact defined in the merged grammar.
	private Hashtable redefiningRefExps = new Hashtable();

	protected virtual void MergeGrammar() {
		string href = GetAttribute("href");
		if(href==null) {
			// error: missing attribute
			ReportError( ERR_MISSING_ATTRIBUTE, reader.Name, "href" );
			reader.Skip();
			return;
		}
		XmlReader newReader = ResolveEntity(href);
			// this method has to be called while we are at the start element state.

		Hashtable old = redefiningRefExps;
		redefiningRefExps = new Hashtable();

		try {
			// collect redefined patterns.
			DivInInclude();

			if(!PushEntity(newReader))	return;

			try {
				// skip XML declarations, DOCTYPE, etc.
				while(!reader.IsStartElement())	reader.Read();

				if(reader.LocalName!="grammar") {
					// error: unexpected tag name
					ReportError( ERR_GRAMMAR_EXPECTED, reader.Name );
					return;
				}

				// parse included pattern.
				DivInGrammar();

				// make sure that there were definitions for redefined exps.
				foreach( ReferenceExp exp in redefiningRefExps.Keys )
					if( (bool)redefiningRefExps[exp] == false ) {
						// error: this pattern was not defined.
						if( exp is Grammar )
							ReportError( ERR_REDEFINING_UNDEFINED_START );
						else
							ReportError( ERR_REDEFINING_UNDEFINED, exp.name );
					}
			} finally {
				PopEntity();
			}
		} finally {
			redefiningRefExps = old;
		}
	}
	protected virtual void DivInInclude() {
		if(!ReadStartElement())		return;
		while(SkipForeignElements()) {
			string name = reader.LocalName;
			if(name=="div")		DivInInclude();
			else
			if(name=="start")	redefiningRefExps[Start()] = false;
			else
			if(name=="define")	redefiningRefExps[Define()] = false;
			else {
				// error: unexpected element
				ReportError( ERR_UNEXPECTED_ELEMENT, reader.Name );
				reader.Skip();
			}
		}
		ReadEndElement();
	}


	// the function object that combines two expressions into one.
	protected delegate Expression ExpCombinator( Expression exp1, Expression exp2 );

	// reads child expressions and combines them by using the specified
	// combinator.
	protected virtual Expression ReadChildExps( ExpCombinator comb ) {
		Expression exp = null;

		while(SkipForeignElements()) {
			Expression child=ReadExp();
			if(exp==null)	exp=child;
			else
				exp = comb( exp, child );
		}
		ReadEndElement();
		if(exp==null) {
			// error: no children
			ReportError( ERR_EXPRESSION_EXPECTED );
			exp = Expression.NotAllowed;	// recovery.
		}
		return exp;
	}



	// resolves the "href" value and obtains XmlReader that reads that source.
	protected virtual XmlReader ResolveEntity( string href ) {

		Uri uri = this.baseUri;

		Trace.WriteLine(string.Format(
			"base:{0}\nhref:{1}",
			uri, href ));

		uri = new Uri( uri, href );

		Trace.WriteLine("after href :"+uri);


		if(uri.Fragment!=String.Empty) {
			ReportError( ERR_FRAGMENT_IN_URI, uri );
			return null;
		}

		return CreateXmlReader( uri.AbsoluteUri,
			(Stream)Resolver.GetEntity( uri, null, typeof(Stream)));
	}

	protected virtual XmlReader CreateXmlReader( string uri, Stream input ) {
//		return new XmlTextReader(uri,input);
		XmlValidatingReader reader = new XmlValidatingReader(new XmlTextReader(uri,input));
		reader.ValidationType = ValidationType.None;
		return reader;
	}

//
// Name Class Element Reader
//===================================================
// Each method parses one element (and its descendants) and returns a NameClass.
// These methods assume that XmlReader is on a start tag when it's called.
// When the method returns, XmlReader is on a start tag of the next element.

	protected delegate NameClass NCReader();
	protected readonly IDictionary NCReaders;

	// XmlReader is on a start tag. Parses it and returns the result.
	protected virtual NameClass ReadNameClass() {
		if(SkipForeignElements()) {
			Trace.WriteLine("dispatching NC: "+reader.LocalName);

			NCReader ncreader = (NCReader)NCReaders[reader.LocalName];
			if(ncreader==null) {
				// error: unknown element name
				ReportError( ERR_NAMECLASS_EXPECTED, reader.Name );
				reader.Skip();
				return new SimpleNameClass("foo","bar");	// recover
			}

			Trace.Indent();
			NameClass nc = ncreader();	// dispatch the reader.
			Trace.Unindent();
			return nc;
		} else {
			// there is no child element
			ReportError( ERR_MISSING_ATTRIBUTE, reader.Name, "name" );
			return AnyNameClass.theInstance;
		}
	}



	protected virtual NameClass AnyName() {
		return new AnyNameClass(ReadExceptName());
	}
	protected virtual NameClass NsName() {
		return new NsNameClass(ns,ReadExceptName());
	}
	protected virtual NameClass SimpleName() {
		if( reader.IsEmptyElement ) {
			ReadStartElement();
			// TODO: error message
			return new SimpleNameClass("undefined","undefined");
		} else {
			ReadStartElement();
			string name = ReadPCDATA();
			if(name==null)	name="undefined";
			NameClass nc = new SimpleNameClass(ProcessQName(name.Trim()));
			ReadEndElement();
			return nc;
		}
	}
	protected virtual NameClass ChoiceName() {
		NameClass nc=null;

		if(ReadStartElement()) {
			while(SkipForeignElements()) {
				NameClass child = ReadNameClass();
				if(nc==null)	nc=child;
				else			nc=new ChoiceNameClass(nc,child);
			}
			ReadEndElement();
		}
		if(nc==null) {
			// error: no children
			ReportError( ERR_NO_CHILD_NAMECLASS );
			nc = new SimpleNameClass("foo","bar");	// recover
		}
		return nc;
	}

	// reads <except> clause if if exists. Otherwise returns null.
	// This method skips any foreign elements.
	protected virtual NameClass ReadExceptName() {
		NameClass nc = null;

		if(ReadStartElement()) {
			while(SkipForeignElements()) {
				if(reader.LocalName=="except") {
					if(nc!=null)
						// error: multiple except
						ReportError( ERR_MULTIPLE_EXCEPT );
					nc = ChoiceName();
				} else {
					// error: unexpected elements
					ReportError( ERR_EXCEPT_EXPECTED, reader.Name );
					reader.Skip();
				}
			}
			ReadEndElement();
		}
		return nc;	// return null if <except> was not found.
	}

//
// ValidationContext implementation
//==============================
//
	public string ResolveNamespacePrefix( string prefix ) {
		return reader.LookupNamespace(prefix);
	}
	public bool IsUnparsedEntity( string str ) {
		// TODO: proper implementation
		return true;
	}
	public bool IsNotation( string str ) {
		// TODO: proper implementation
		return true;
	}

//
// Error message handling
//==============================
//
	protected bool HadError;

	protected void ReportError( string propKey, params object[] args ) {
		HadError = true;
		IXmlLineInfo li;
		if(reader is IXmlLineInfo)	li = (IXmlLineInfo)reader;
		else						li = null;

		Controller.error( string.Format( ResManager.GetString(propKey), args ), li );
	}

	protected const string ERR_MISSING_ATTRIBUTE = // arg:2
		"GrammarReader.MissingAttribute";
	protected const string ERR_NO_GRAMMAR =
		"GrammarReader.NoGrammar";
	protected const string ERR_UNEXPECTED_ELEMENT =
		"GrammarReader.UnexpectedElement";
	protected const string ERR_EXPRESSION_EXPECTED =
		"GrammarReader.ExpressionExpected";
	protected const string ERR_INCONSISTENT_COMBINE =
		"GrammarReader.InconsistentCombine";
	protected const string ERR_MULTIPLE_HEADS =
		"GrammarReader.MultipleHeads";
	protected const string ERR_INVALID_COMBINE =
		"GrammarReader.InvalidCombine";
	protected const string ERR_GRAMMAR_EXPECTED =
		"GrammarReader.GrammarExpected";
	protected const string ERR_REDEFINING_UNDEFINED_START =
		"GrammarReader.RedefiningUndefinedStart";
	protected const string ERR_REDEFINING_UNDEFINED =
		"GrammarReader.RedefiningUndefined";
	protected const string ERR_NAMECLASS_EXPECTED =
		"GrammarReader.NameClassExpected";
	protected const string ERR_NO_CHILD_NAMECLASS =
		"GrammarReader.NoChildNameClass";
	protected const string ERR_MULTIPLE_EXCEPT =
		"GrammarReader.MultipleExcept";
	protected const string ERR_EXCEPT_EXPECTED =
		"GrammarReader.ExceptExpected";
	protected const string ERR_UNDEFINED_TYPENAME =
		"GrammarReader.UndefinedTypeName";
	protected const string ERR_BAD_VALUE_FOR_TYPE =
		"GrammarReader.BadValueForType";
	protected const string ERR_DATATYPE_ERROR =
		"GrammarReader.DatatypeError";
	protected const string ERR_BAD_DATATYPE_PARAMETER =
		"GrammarReader.BadDatatypeParameter";
	protected const string ERR_FRAGMENT_IN_URI =
		"GrammarReader.FragmentInUri";
	protected const string ERR_RECURSIVE_INCLUSION =
		"GrammarReader.RecursiveInclusion";
	protected const string ERR_INVALID_QNAME =
		"GrammarReader.InvalidQName";
}


public interface GrammarReaderController {
	void error( string msg, IXmlLineInfo location );
	void warning( string msg, IXmlLineInfo location );
}

public class ConsoleController : GrammarReaderController {

	public void error( string msg, IXmlLineInfo loc ) {
		Console.WriteLine("Error: "+msg);
		PrintLocation(loc);
	}

	public void warning( string msg, IXmlLineInfo loc ) {
		Console.WriteLine("Warning: "+msg);
		PrintLocation(loc);
	}

	private void PrintLocation( IXmlLineInfo loc ) {
		if( loc!=null ) {
			Console.WriteLine("{0}({1}:{2})",
				((XmlReader)loc).BaseURI, loc.LineNumber, loc.LinePosition );
		}
	}
}

public class ErrorContainer : Verifier.ErrorHandler
{
	private readonly List<string> _errorMessages = new List<string>();

	public void Error(string msg) {
		_errorMessages.Add(msg);
	}

	public ICollection<string> GetErrors() {
		return _errorMessages.ToArray();
	}
}

}
