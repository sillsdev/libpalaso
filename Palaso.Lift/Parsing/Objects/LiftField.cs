using System;
using System.Collections.Generic;

namespace Palaso.Lift.Parsing
{
	/// <summary>
	/// This class implements "field" from the LIFT standard.
	/// </summary>
	public class LiftField
	{
		///<summary>
		/// Default Constructor.
		///</summary>
		public LiftField()
		{
			Annotations = new List<LiftAnnotation>();
			Traits = new List<LiftTrait>();
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		public LiftField(string name, LiftMultiText contents)
		{
			Annotations = new List<LiftAnnotation>();
			Traits = new List<LiftTrait>();
			Name = name;
			Content = contents;
		}

		///<summary></summary>
		public string Name { get; set; }

		///<summary></summary>
		public DateTime DateCreated { get; set; }

		///<summary></summary>
		public DateTime DateModified { get; set; }

		///<summary></summary>
		public List<LiftTrait> Traits { get; private set; }

		///<summary></summary>
		public List<LiftAnnotation> Annotations { get; private set; }

		///<summary></summary>
		public LiftMultiText Content { get; set; }
	}

	/// <summary>
	/// This class holds all the information from a &lt;field-definition&gt; element.
	/// </summary>
	public class LiftFieldDefinition
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public LiftFieldDefinition(string name, string classes, string type, string range,
			string langs, LiftMultiText description, LiftMultiText label)
		{
			Name = name;
			Classes = classes;
			Type = type;
			Range = range;
			LanguageTags = langs;
			Description = description;
			Label = label;
		}

		///<summary></summary>
		public string Name { get; set; }

		///<summary></summary>
		public string Classes { get; set; }

		///<summary></summary>
		public string Type { get; set; }

		///<summary></summary>
		public string Range { get; set; }

		///<summary></summary>
		public string LanguageTags { get; set; }

		///<summary></summary>
		public LiftMultiText Description { get; set; }

		///<summary></summary>
		public LiftMultiText Label { get; set; }
	}
}
