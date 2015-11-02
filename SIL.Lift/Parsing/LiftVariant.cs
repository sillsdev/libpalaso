using System.Collections.Generic;

namespace SIL.Lift.Parsing
{
	/// <summary>
	/// This class implements "variant" from the LIFT standard.  (It represents an allomorph, not what
	/// FieldWorks understands to be a Variant.)
	/// </summary>
	public class LiftVariant : LiftObject
	{
		///<summary>
		/// Constructor.
		///</summary>
		public LiftVariant()
		{
			Relations = new List<LiftRelation>();
			Pronunciations = new List<LiftPhonetic>();
		}

		///<summary></summary>
		public string Ref { get; set; }

		///<summary></summary>
		public LiftMultiText Form { get; set; }

		///<summary></summary>
		public List<LiftPhonetic> Pronunciations { get; private set; }

		///<summary></summary>
		public List<LiftRelation> Relations { get; private set; }

		///<summary></summary>
		public string RawXml { get; set; }

		///<summary></summary>
		public override string XmlTag
		{
			get { return "variant"; }
		}
	}
}
