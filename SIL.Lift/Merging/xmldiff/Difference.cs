/* From http://xmlunit.sourceforge.net/ Moved here because the original library is for testing, and
 * it is tied to nunit, which we don't want to ship in production
 */

using System.Xml;

namespace SIL.Lift.Merging.xmldiff
{
	///<summary></summary>
	public class Difference {
		private readonly DifferenceType _diffType;
		private readonly bool _hasMajorDifference;
		private XmlNodeType _controlNodeType;
		private XmlNodeType _testNodeType;

		///<summary>
		/// Constructor.
		///</summary>
		public Difference(DifferenceType id) {
			_diffType = id;
			_hasMajorDifference = Differences.isMajorDifference(id);
		}

		///<summary>
		/// Constructor.
		///</summary>
		public Difference(DifferenceType id, XmlNodeType controlNodeType, XmlNodeType testNodeType)
		: this(id) {
			_controlNodeType = controlNodeType;
			_testNodeType = testNodeType;
		}

		///<summary></summary>
		public DifferenceType DiffType {
			get {
				return _diffType;
			}
		}

		///<summary></summary>
		public bool HasMajorDifference {
			get {
				return _hasMajorDifference;
			}
		}

		///<summary></summary>
		public XmlNodeType ControlNodeType {
			get {
				return _controlNodeType;
			}
		}

		///<summary></summary>
		public XmlNodeType TestNodeType {
			get {
				return _testNodeType;
			}
		}

		///<summary></summary>
		public override string ToString()
		{
			string asString = base.ToString() + " type: " + (int) _diffType
				+ ", control Node: " + _controlNodeType
				+ ", test Node: " + _testNodeType;
			return asString;
		}
	}
}
