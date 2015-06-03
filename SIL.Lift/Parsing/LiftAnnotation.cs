using System;

namespace SIL.Lift.Parsing
{
	/// <summary>
	/// This class implements "annotation" from the LIFT standard.
	/// </summary>
	public class LiftAnnotation
	{
		/// <summary></summary>
		public string Name { get; set; }

		/// <summary></summary>
		public string Value { get; set; }

		/// <summary></summary>
		public string Who { get; set; }

		/// <summary></summary>
		public DateTime When { get; set; }

		/// <summary></summary>
		public LiftMultiText Comment { get; set; }
	}
}
