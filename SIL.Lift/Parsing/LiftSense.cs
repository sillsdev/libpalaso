using System;
using System.Collections.Generic;

namespace SIL.Lift.Parsing
{
	/// <summary>
	/// This class implements "sense" from the LIFT standard.
	/// </summary>
	public class LiftSense : LiftObject
	{
		///<summary>
		/// Default constructor.
		///</summary>
		public LiftSense()
		{
			Subsenses = new List<LiftSense>();
			Illustrations = new List<LiftUrlRef>();
			Reversals = new List<LiftReversal>();
			Examples = new List<LiftExample>();
			Notes = new List<LiftNote>();
			Relations = new List<LiftRelation>();
		}

		///<summary>
		/// Constructor.
		///</summary>
		public LiftSense(Extensible info, Guid guid, LiftObject owner)
		{
			Subsenses = new List<LiftSense>();
			Illustrations = new List<LiftUrlRef>();
			Reversals = new List<LiftReversal>();
			Examples = new List<LiftExample>();
			Notes = new List<LiftNote>();
			Relations = new List<LiftRelation>();
			Id = info.Id;
			Guid = guid;
			DateCreated = info.CreationTime;
			DateModified = info.ModificationTime;
			Owner = owner;
		}

		///<summary></summary>
		public int Order { get; set; }

		///<summary></summary>
		public LiftGrammaticalInfo GramInfo { get; set; }

		///<summary></summary>
		public LiftMultiText Gloss { get; set; }

		///<summary></summary>
		public LiftMultiText Definition { get; set; }

		///<summary></summary>
		public List<LiftRelation> Relations { get; private set; }

		///<summary></summary>
		public List<LiftNote> Notes { get; private set; }

		///<summary></summary>
		public List<LiftExample> Examples { get; private set; }

		///<summary></summary>
		public List<LiftReversal> Reversals { get; private set; }

		///<summary></summary>
		public List<LiftUrlRef> Illustrations { get; private set; }

		///<summary></summary>
		public List<LiftSense> Subsenses { get; private set; }

		///<summary></summary>
		public LiftEntry OwningEntry
		{
			get
			{
				LiftObject owner;
				for (owner = Owner; owner is LiftSense; owner = (owner as LiftSense).Owner)
				{
				}
				return owner as LiftEntry;
			}
		}

		///<summary></summary>
		public LiftObject Owner { get; private set; }

		///<summary></summary>
		public override string XmlTag
		{
			get { return "sense/subsense"; }
		}
	}
}
