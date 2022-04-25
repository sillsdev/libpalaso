namespace Tenuto.Grammar {

public abstract class NameClass {
	
	public abstract bool Contains( string uri, string local );
	
	public bool Contains( XmlName name ) {
		return Contains( name.uri, name.local );
	}
}

public sealed class SimpleNameClass : NameClass {
	
	public readonly XmlName  name;
	
	public SimpleNameClass( XmlName name ) {
		this.name = name;
	}
	public SimpleNameClass( string uri, string local ) : this(new XmlName(uri,local)) {}
	
	public override bool Contains( string uri, string local ) {
		return name.uri==uri && name.local==local;
	}
	
	public override string ToString() { return name.local; }
}

public sealed class NsNameClass : NameClass {
	
	public readonly string uri;
	public readonly NameClass except;
	
	public NsNameClass( string uri, NameClass except ) {
		this.uri = uri;
		this.except = except;
	}
	
	public NsNameClass( string uri ) : this(uri,null) {
	}
	
	public override bool Contains( string uri, string local ) {
		if(this.uri!=uri)	return false;
		if(except!=null)
			return !except.Contains(uri,local);
		return true;
	}
	
	public override string ToString() {
		string r = uri+":*";
		if(except!=null)	r+="/("+except+")";
		return r;
	}
}

public sealed class AnyNameClass : NameClass {
	
	// anyName without except clause.
	public static NameClass theInstance = new AnyNameClass();
	
	public readonly NameClass except;
	
	public AnyNameClass() : this(null) {}
	public AnyNameClass( NameClass except ) {
		this.except = except;
	}
	
	public override bool Contains( string uri, string local ) {
		if(except!=null)
			return !except.Contains(uri,local);
		return true;
	}
	
	public override string ToString() {
		string r = "*:*";
		if(except!=null)	r+="/("+except+")";
		return r;
	}
}

public sealed class ChoiceNameClass : NameClass {
	
	public readonly NameClass nc1;
	public readonly NameClass nc2;
	
	public ChoiceNameClass( NameClass nc1, NameClass nc2 ) {
		this.nc1=nc1;
		this.nc2=nc2;
	}
	
	public override bool Contains( string uri, string local ) {
		return nc1.Contains(uri,local) || nc2.Contains(uri,local);
	}
}


}
