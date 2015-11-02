using System.Collections.Generic;

namespace SIL.Lift.Parsing
{
	/// <summary>
	/// This class implements "trait" from the LIFT standard.
	/// </summary>
	public class LiftTrait
	{
		///<summary>
		/// Constructor.
		///</summary>
		public LiftTrait()
		{
			Annotations = new List<LiftAnnotation>();
		}

		///<summary></summary>
		public string Name { get; set; }

		///<summary></summary>
		public string Value { get; set; }

		///<summary></summary>
		public string Id { get; set; }

		///<summary></summary>
		public List<LiftAnnotation> Annotations { get; private set; }
	}
}
