namespace Tenuto.Datatype.XSDLib {

using org.relaxng.datatype;
using System.Text;

internal sealed class UnimplementedDatatype : DatatypeImpl {
	
	internal static Datatype theInstance = new UnimplementedDatatype();
	
	private UnimplementedDatatype() : base(WSNormalizationMode.Collapse) {}
	
	protected internal override
	object GetValue( string str, ValidationContext ctxt ) {
		return str;
	}
}

}