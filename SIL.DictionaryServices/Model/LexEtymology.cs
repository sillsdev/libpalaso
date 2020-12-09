using System;
using System.Collections.Generic;
using System.Linq;
using SIL.Lift;
using SIL.Text;

namespace SIL.DictionaryServices.Model
{
	/// <summary>
	/// Not implemented: media
	/// Not implemented: extensible.date
	/// </summary>
	public sealed class LexEtymology: MultiText, IExtensible
	{
		public string Type { get; set; }
		public string Source { get; set; }

		public LexEtymology(string type, string source)
		{
			Type = type;
			Source = source;
			Traits = new List<LexTrait>();
			Fields = new List<LexField>();
			Gloss = new MultiText();
			Comment = new MultiText();
		}
		public MultiText Gloss { get; set; }


		/// <summary>
		/// as of lift 0.13, this doesn't exist, so the writer will have to make a <field/> or something
		/// </summary>
		public MultiText Comment { get; set; }

		#region IExtensible
		public List<LexTrait> Traits{get;private set;}
		public List<LexField> Fields { get; private set; }
		#endregion

		public override IPalasoDataObjectProperty Clone()
		{
			var clone = new LexEtymology(Type, Source);
			clone.Gloss = (MultiText)Gloss.Clone();
			clone.Comment = (MultiText)Comment.Clone();
			clone.Traits = new List<LexTrait>(Traits.Select(t=>t.Clone()));
			clone.Fields = new List<LexField>(Fields.Select(f => (LexField)f.Clone()));
			//copies from MultiText
			clone.EmbeddedXmlElements = new List<string>(EmbeddedXmlElements);
			clone.Forms = Forms.Select(f => (LanguageForm) f.Clone()).ToArray();
			return clone;
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as LexEtymology);
		}

		public bool Equals(LexEtymology other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			if (!base.Equals(other)) return false;
			if (!Gloss.Equals(other.Gloss)) return false;
			if (!Comment.Equals(other.Comment)) return false;
			if (!Traits.SequenceEqual(other.Traits)) return false; //order matters because we expose a list interface
			if (!Fields.SequenceEqual(other.Fields)) return false; //order matters because we expose a list interface
			if (!Type.Equals(other.Type)) return false;
			if (!Source.Equals(other.Source)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			// For this class we want a hash code based on the the object's reference so that we
			// can store and retrieve the object in the LiftLexEntryRepository. However, this is
			// not ideal and Microsoft warns: "Do not use the hash code as the key to retrieve an
			// object from a keyed collection."
			// https://docs.microsoft.com/en-us/dotnet/api/system.object.gethashcode?view=netframework-4.8#remarks
			return base.GetHashCode();
		}
	}
}