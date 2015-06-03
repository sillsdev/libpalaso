using System.Collections.Generic;

namespace SIL.Lift.Parsing
{
	/// <summary>
	/// This class implements "grammatical-info" from the LIFT standard.
	/// </summary>
	public class LiftGrammaticalInfo
	{
		///<summary>
		/// Constructor.
		///</summary>
		public LiftGrammaticalInfo()
		{
			Traits = new List<LiftTrait>();
		}

		///<summary>
		///</summary>
		public string Value { get; set; }

		///<summary></summary>
		public List<LiftTrait> Traits { get; private set; }
	}
}
