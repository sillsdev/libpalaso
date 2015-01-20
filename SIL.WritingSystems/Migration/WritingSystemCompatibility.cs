namespace SIL.WritingSystems.Migration
{
	///<summary>
	/// Specifies any comaptibiltiy modes that can be imposed on a WritingSystemRepository
	///</summary>
	public enum WritingSystemCompatibility
	{
		///<summary>
		/// Strict adherence to the current LDML standard (with extensions)
		///</summary>
		Strict,
		///<summary>
		/// Permits backward compatibility with Flex 7.0.x and 7.1.x V0 LDML
		/// notably custom language tags having all elements in private use.
		/// e.g. x-abc-Zxxx-x-audio
		///</summary>
		Flex7V0Compatible
	};
}
