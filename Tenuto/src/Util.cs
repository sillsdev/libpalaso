namespace Tenuto {

using System;
using System.Collections;

public struct XmlName {
	public XmlName( String uri, String local ) {
		this.uri = uri;
		this.local = local;
	}
	public readonly String uri;
	public readonly String local;

	public override bool Equals( object rhs ) {
		if(!(rhs is XmlName))	return false;
		return this==((XmlName)rhs);
	}
	public override int GetHashCode() {
		return uri.GetHashCode() ^ local.GetHashCode();
	}
	
	public static bool operator == ( XmlName lhs, XmlName rhs ) {
		return lhs.uri==rhs.uri && lhs.local==rhs.local;
	}
	public static bool operator != ( XmlName lhs, XmlName rhs ) {
		return !(lhs==rhs);
	}
}

internal sealed class StringTokenizer {
	private static readonly char[] whitespaces = {
		'\x9', '\xA', '\xB', '\xC', '\xD', '\x20', '\xA0', '\x2000',
		'\x2001', '\x2002', '\x2003', '\x2004', '\x2005', '\x2006',
		'\x2007', '\x2008', '\x2009', '\x200A', '\x200B', '\x3000', '\xFEFF' };
		
	// Tokenizes a string just like java.util.StringTokenizer
	internal static string[] Tokenize( string s ) {
		ArrayList tokens = new ArrayList();
		
		while(true) {
			s = s.TrimStart();
			int idx = s.IndexOfAny(whitespaces);
			if(idx<0) {
				if(s.Length!=0)		tokens.Add(s);
				return (string[])tokens.ToArray(typeof(string));
			}
			tokens.Add(s.Substring(0,idx));
			s = s.Substring(idx);
		}
	}
}

}
