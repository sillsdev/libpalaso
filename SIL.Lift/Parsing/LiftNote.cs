namespace SIL.Lift.Parsing
{
	/// <summary>
	/// This class implements "note" from the LIFT standard.
	/// </summary>
	public class LiftNote : LiftObject
	{
		///<summary>
		/// Default constructor.
		///</summary>
		public LiftNote()
		{
		}

		///<summary>
		/// Constructor.
		///</summary>
		public LiftNote(string type, LiftMultiText contents)
		{
			Type = type;
			Content = contents;
		}

		///<summary></summary>
		public string Type { get; set; }

		///<summary></summary>
		public LiftMultiText Content { get; set; }

		///<summary></summary>
		public override string XmlTag
		{
			get { return "note"; }
		}
	}
}
