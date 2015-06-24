namespace SIL.Lift.Parsing
{
	/// <summary>
	/// This class implements "etymology" from the LIFT standard.
	/// </summary>
	public class LiftEtymology : LiftObject
	{
		///<summary></summary>
		public string Type { get; set; }

		///<summary></summary>
		public string Source { get; set; }

		///<summary></summary>
		public LiftMultiText Gloss { get; set; }

		///<summary></summary>
		public LiftMultiText Form { get; set; }

		///<summary></summary>
		public override string XmlTag
		{
			get { return "etymology"; }
		}
	}
}
