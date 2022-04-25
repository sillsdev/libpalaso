namespace Tenuto.Datatype.XSDLib {

using System;
using System.Text;

public enum WSNormalizationMode {
	Preserve, Replace, Collapse
};

public class WhitespaceNormalizer {
	
	// whitespace normalization processor
	public delegate string Processor(string s);
	
	public static Processor GetProcessor(WSNormalizationMode mode) {
		switch(mode) {
		case WSNormalizationMode.Preserve:
			return new Processor(preserve);
		case WSNormalizationMode.Replace:
			return new Processor(replace);
		case WSNormalizationMode.Collapse:
			return new Processor(collapse);
		default:
			// assertion failed
			throw new Exception();
		}
	}
	
	public static string preserve( string s ) { return s; }
	
	public static string replace( string s ) {
		StringBuilder buf = new StringBuilder();
		foreach( char c in s ) {
			if( c=='\r' || c=='\n' || c=='\t' )
				buf.Append(' ');
			else
				buf.Append(c);
		}
		return buf.ToString();
	}
	
	public static string collapse( string s ) {
		StringBuilder buf = new StringBuilder();
		bool inWhiteSpace = true;
		
		foreach( char c in s ) {
			if( char.IsWhiteSpace(c) ) {
				if(!inWhiteSpace)	buf.Append(' ');
				inWhiteSpace = true;
			} else {
				buf.Append(c);
				inWhiteSpace = false;
			}
		}
		// remove trailing whitespace if any.
		// (there must be at most one)
		if(inWhiteSpace && buf.Length>0)	buf.Length--;
		
		return buf.ToString();
	}
}

}