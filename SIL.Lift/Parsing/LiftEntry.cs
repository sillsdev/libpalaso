using System;
using System.Collections.Generic;

namespace SIL.Lift.Parsing
{
	/// <summary>
	/// This class implements "entry" from the LIFT standard.
	/// </summary>
	public class LiftEntry : LiftObject
	{
		///<summary>
		/// Default constructor.
		///</summary>
		public LiftEntry()
		{
			Etymologies = new List<LiftEtymology>();
			Relations = new List<LiftRelation>();
			Notes = new List<LiftNote>();
			Senses = new List<LiftSense>();
			Variants = new List<LiftVariant>();
			Pronunciations = new List<LiftPhonetic>();
			Order = 0;
			DateDeleted = DateTime.MinValue;
		}

		///<summary>
		/// Constructor.
		///</summary>
		public LiftEntry(Extensible info, Guid guid, int order)
		{
			Etymologies = new List<LiftEtymology>();
			Relations = new List<LiftRelation>();
			Notes = new List<LiftNote>();
			Senses = new List<LiftSense>();
			Variants = new List<LiftVariant>();
			Pronunciations = new List<LiftPhonetic>();
			Id = info.Id;
			Guid = guid;
			DateCreated = info.CreationTime;
			DateModified = info.ModificationTime;
			DateDeleted = DateTime.MinValue;
			Order = order;
		}

		///<summary></summary>
		public int Order { get; set; }

		///<summary></summary>
		public DateTime DateDeleted { get; set; }

		///<summary></summary>
		public LiftMultiText LexicalForm { get; set; }

		///<summary></summary>
		public LiftMultiText CitationForm { get; set; }

		///<summary></summary>
		public List<LiftPhonetic> Pronunciations { get; private set; }

		///<summary></summary>
		public List<LiftVariant> Variants { get; private set; }

		///<summary></summary>
		public List<LiftSense> Senses { get; private set; }

		///<summary></summary>
		public List<LiftNote> Notes { get; private set; }

		///<summary></summary>
		public List<LiftRelation> Relations { get; private set; }

		///<summary></summary>
		public List<LiftEtymology> Etymologies { get; private set; }

		///<summary></summary>
		public override string XmlTag
		{
			get { return "entry"; }
		}
	}
}
