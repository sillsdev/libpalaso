namespace SIL.Lift.Parsing
{
	/// <summary>
	/// This class implements "relation" from the LIFT standard.
	/// </summary>
	public class LiftRelation : LiftObject
	{
		///<summary>
		/// Constructor.
		///</summary>
		public LiftRelation()
		{
			Order = -1;
		}

		///<summary></summary>
		public string Type { get; set; }

		///<summary></summary>
		public string Ref { get; set; }

		///<summary></summary>
		public int Order { get; set; }

		///<summary></summary>
		public LiftMultiText Usage { get; set; }

		///<summary></summary>
		public override string XmlTag
		{
			get { return "relation"; }
		}
	}
}
