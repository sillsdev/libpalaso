using System.Collections.Generic;

namespace SIL.Lift.Parsing
{
	/// <summary>
	/// This class implements "example" from the LIFT standard.
	/// It also corresponds to LexExampleSentence in the FieldWorks model.
	/// </summary>
	public class LiftExample : LiftObject
	{
		///<summary>
		/// Constructor.
		///</summary>
		public LiftExample()
		{
			Notes = new List<LiftNote>();
			Translations = new List<LiftTranslation>();
		}

		///<summary></summary>
		public string Source { get; set; }

		///<summary></summary>
		public LiftMultiText Content { get; set; }

		///<summary></summary>
		public List<LiftTranslation> Translations { get; private set; }

		///<summary></summary>
		public List<LiftNote> Notes { get; private set; }

		///<summary></summary>
		public override string XmlTag
		{
			get { return "example"; }
		}
	}
}
