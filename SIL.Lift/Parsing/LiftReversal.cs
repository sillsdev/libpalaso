namespace SIL.Lift.Parsing
{
	/// <summary>
	/// This class implements "reversal" from the LIFT standard.
	/// It also roughly corresponds to ReversalIndexEntry in the FieldWorks model.
	/// </summary>
	public class LiftReversal : LiftObject
	{
		///<summary></summary>
		public string Type { get; set; }

		///<summary></summary>
		public LiftMultiText Form { get; set; }

		///<summary></summary>
		public LiftReversal Main { get; set; }

		///<summary></summary>
		public LiftGrammaticalInfo GramInfo { get; set; }

		///<summary></summary>
		public override string XmlTag
		{
			get { return "reversal"; }
		}
	}
}
