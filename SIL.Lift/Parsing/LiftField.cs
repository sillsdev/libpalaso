using System;
using System.Collections.Generic;

namespace SIL.Lift.Parsing
{
	/// <summary>
	/// This class implements "field" from the LIFT standard.
	/// </summary>
	public class LiftField
	{
		///<summary>
		/// Default Constructor.
		///</summary>
		public LiftField()
		{
			Annotations = new List<LiftAnnotation>();
			Traits = new List<LiftTrait>();
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		public LiftField(string type, LiftMultiText contents)
		{
			Annotations = new List<LiftAnnotation>();
			Traits = new List<LiftTrait>();
			Type = type;
			Content = contents;
		}

		///<summary></summary>
		public string Type { get; set; }

		///<summary></summary>
		public DateTime DateCreated { get; set; }

		///<summary></summary>
		public DateTime DateModified { get; set; }

		///<summary></summary>
		public List<LiftTrait> Traits { get; private set; }

		///<summary></summary>
		public List<LiftAnnotation> Annotations { get; private set; }

		///<summary></summary>
		public LiftMultiText Content { get; set; }
	}
}
