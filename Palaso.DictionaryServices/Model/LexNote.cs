using System;
using System.Collections.Generic;
using System.Linq;
using Palaso.Lift;
using SIL.Text;

namespace Palaso.DictionaryServices.Model
{
	/// <summary>
	/// A note is used for storing descriptive information of many kinds including comments, bibliographic information and domain specific notes. Notes are used to hold informational content rather than meta-information about an element, for which an annotation should be used.
	/// </summary>
	public sealed class LexNote:  MultiText, IExtensible
	{
		/// <summary>
		/// Not implemented: extensible.date
		/// </summary>
		public string Type { get; set; }

		public LexNote()
		{
			Type = string.Empty;
			Traits = new List<LexTrait>();
			Fields = new List<LexField>();
		}

		public LexNote(string type):this()
		{
			Type = type;
		}

		#region IExtensible
		public List<LexTrait> Traits { get; private set; }
		public List<LexField> Fields { get; private set; }
		#endregion

		public override IPalasoDataObjectProperty Clone()
		{
			var clone = new LexNote(Type);
			clone.Traits = new List<LexTrait>(Traits.Select(t => t.Clone()));
			clone.Fields = new List<LexField>(Fields.Select(t => (LexField) t.Clone()));
			clone.EmbeddedXmlElements = new List<string>(EmbeddedXmlElements);
			clone.Forms = Forms.Select(f => (LanguageForm)f.Clone()).ToArray();
			return clone;
		}

		public override bool Equals(Object obj)
		{
			if (!(obj is LexNote)) return false;
			return Equals((LexNote)obj);
		}

		public bool Equals(LexNote other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			if (!Traits.SequenceEqual(other.Traits)) return false;
			if (!Fields.SequenceEqual(other.Fields)) return false;
			if (!base.Equals(other)) return false;
			if (!Type.Equals(other.Type)) return false;
			return true;
		}
	}
}