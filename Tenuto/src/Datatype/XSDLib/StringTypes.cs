namespace Tenuto.Datatype.XSDLib {

// using org.relaxng.datatype;

public class StringType : DatatypeImpl {
	public StringType(WSNormalizationMode mode) : base(mode) {}
	
	protected internal override Measure GetMeasure() {
		return new Measure(CalcLength);
	}
	
	private static int CalcLength( object s ) {
		int len = ((string)s).Length;
		foreach( char c in (string)s )
			// handle surrogate properly
			if((c&0xFC00)==0xD800)	len--;
		return len;
	}
}

public class LanguageType : StringType {
	public LanguageType() : base(WSNormalizationMode.Collapse) {}
	protected internal override bool LexicalCheck( string s ) {
		// TODO: check
		return true;
	}
}

public class NameType : StringType {
	public NameType() : base(WSNormalizationMode.Collapse) {}
	protected internal override bool LexicalCheck( string s ) {
		return XMLChar.IsName(s);
	}
}

public class NCNameType : StringType {
	public NCNameType() : base(WSNormalizationMode.Collapse) {}
	protected internal override bool LexicalCheck( string s ) {
		return XMLChar.IsNCName(s);
	}
}

public class IDType : NCNameType {
	public IDType() {}
	public override org.relaxng.datatype.IDType IdType {
		get { return org.relaxng.datatype.IDType.ID_TYPE_ID; }
	}
}

public class IDREFType : NCNameType {
	public IDREFType() {}
	public override org.relaxng.datatype.IDType IdType {
		get { return org.relaxng.datatype.IDType.ID_TYPE_IDREF; }
	}
}

public class NMTOKENType : StringType {
	public NMTOKENType() : base(WSNormalizationMode.Collapse) {}
	protected internal override bool LexicalCheck( string s ) {
		return XMLChar.IsNMTOKEN(s);
	}
}


}