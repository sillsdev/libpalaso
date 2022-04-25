namespace Tenuto.Datatype.XSDLib {

public class XMLChar {

	public static bool IsNMTOKEN( string s ) {
		// TODO
		return true;
	}
	
	public static bool IsName( string s ) {
		// TODO
		return true;
	}
	
	public static bool IsNCName( string s ) {
		if(!IsName(s))	return false;
		return s.IndexOf(':')==-1;
	}
	
	public static bool IsQName( string s ) {
		if(!IsName(s))	return false;
		return s.IndexOf(':')==s.LastIndexOf(':');
	}
}

}
