using System;
using System.Collections.Generic;

namespace SIL.Lift.Parsing
{
	/// <summary>
	/// This class implements "extensible" from the LIFT standard.
	/// </summary>
	public abstract class LiftObject
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		protected LiftObject()
		{
			Annotations = new List<LiftAnnotation>();
			Traits = new List<LiftTrait>();
			Fields = new List<LiftField>();
			DateCreated = DateTime.MinValue;
			DateModified = DateTime.MinValue;
		}

		///<summary></summary>
		public string Id { get; set; }

		///<summary></summary>
		public Guid Guid { get; set; }

		///<summary></summary>
		public DateTime DateCreated { get; set; }

		///<summary></summary>
		public DateTime DateModified { get; set; }

		///<summary></summary>
		public List<LiftField> Fields { get; private set; }

		///<summary></summary>
		public List<LiftTrait> Traits { get; private set; }

		///<summary></summary>
		public List<LiftAnnotation> Annotations { get; private set; }

		///<summary></summary>
		public abstract string XmlTag { get; }
	}
}
