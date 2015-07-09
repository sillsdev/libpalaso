using System.Collections.Generic;

namespace SIL.Lift.Parsing
{
	/// <summary>
	/// This class implements "phonetic" from the LIFT standard.
	/// </summary>
	public class LiftPhonetic : LiftObject
	{
		///<summary>
		/// Constructor.
		///</summary>
		public LiftPhonetic()
		{
			Media = new List<LiftUrlRef>();
		}

		///<summary></summary>
		public LiftMultiText Form { get; set; }

		///<summary></summary>
		public List<LiftUrlRef> Media { get; private set; }

		///<summary></summary>
		public override string XmlTag
		{
			get { return "pronunciation"; }
		}
	}
}
