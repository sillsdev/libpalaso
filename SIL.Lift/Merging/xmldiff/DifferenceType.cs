/* From http://xmlunit.sourceforge.net/ Moved here because the original library is for testing, and
 * it is tied to nunit, which we don't want to ship in production
 */

namespace SIL.Lift.Merging.xmldiff
{
	///<summary>
	///</summary>
	public enum DifferenceType
	{
		///<summary>Comparing an implied attribute value against an explicit value</summary>
		ATTR_VALUE_EXPLICITLY_SPECIFIED_ID = 1,

		///<summary>Comparing 2 elements and one has an attribute the other does not</summary>
		ATTR_NAME_NOT_FOUND_ID = 2,

		///<summary>Comparing 2 attributes with the same name but different values</summary>
		ATTR_VALUE_ID = 3,

		///<summary>Comparing 2 attribute lists with the same attributes in different sequence</summary>
		ATTR_SEQUENCE_ID = 4,

		///<summary>Comparing 2 CDATA sections with different values</summary>
		CDATA_VALUE_ID = 5,

		///<summary>Comparing 2 comments with different values</summary>
		COMMENT_VALUE_ID = 6,

		///<summary>Comparing 2 document types with different names</summary>
		DOCTYPE_NAME_ID = 7,

		///<summary>Comparing 2 document types with different public identifiers</summary>
		DOCTYPE_PUBLIC_ID_ID = 8,

		///<summary>Comparing 2 document types with different system identifiers</summary>
		DOCTYPE_SYSTEM_ID_ID = 9,

		///<summary>Comparing 2 elements with different tag names</summary>
		ELEMENT_TAG_NAME_ID = 10,

		///<summary>Comparing 2 elements with different number of attributes</summary>
		ELEMENT_NUM_ATTRIBUTES_ID = 11,

		///<summary>Comparing 2 processing instructions with different targets</summary>
		PROCESSING_INSTRUCTION_TARGET_ID = 12,

		///<summary>Comparing 2 processing instructions with different instructions</summary>
		PROCESSING_INSTRUCTION_DATA_ID = 13,

		///<summary>Comparing 2 different text values</summary>
		TEXT_VALUE_ID = 14,

		///<summary>Comparing 2 nodes with different namespace prefixes</summary>
		NAMESPACE_PREFIX_ID = 15,

		///<summary>Comparing 2 nodes with different namespace URIs</summary>
		NAMESPACE_URI_ID = 16,

		///<summary>Comparing 2 nodes with different node types</summary>
		NODE_TYPE_ID = 17,

		///<summary>Comparing 2 nodes but only one has any children</summary>
		HAS_CHILD_NODES_ID = 18,

		///<summary>Comparing 2 nodes with different numbers of children</summary>
		CHILD_NODELIST_LENGTH_ID = 19,

		///<summary>Comparing 2 nodes with children whose nodes are in different sequence</summary>
		CHILD_NODELIST_SEQUENCE_ID = 20,

		///<summary>Comparing 2 Documents only one of which has a doctype</summary>
		HAS_DOCTYPE_DECLARATION_ID = 21,

		///<summary>Comparing 2 Documents only one of which has an XML Prefix Declaration</summary>
		HAS_XML_DECLARATION_PREFIX_ID = 22,
	} ;
}
