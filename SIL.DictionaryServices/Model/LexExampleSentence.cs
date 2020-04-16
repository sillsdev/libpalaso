using System;
using System.Collections.Generic;
using SIL.Lift;
using SIL.ObjectModel;

namespace SIL.DictionaryServices.Model
{
	public sealed class LexExampleSentence: PalasoDataObject, ICloneable<LexExampleSentence>, IEquatable<LexExampleSentence>
	{
		//!!What!! Is this done this way so that we don't end up storing
		//  the data in the object database?
		//Answer:
		//This was done to avoid magic strings for property retrieval That way we can say
		//lexSentence.GetProperty<MultiText>(LexExampleSentence.WellKnownProperties.ExampleSentence) instead of lexSentence.GetProperty<MultiText>("ExampleSentence") --TA 9/24/2012
		public new class WellKnownProperties: PalasoDataObject.WellKnownProperties
		{
			public static string ExampleSentence = "ExampleSentence";
			public static string Translation = "ExampleTranslation";
			public static string Source = "source";

			public new static bool Contains(string fieldName)
			{
				List<string> list =
					new List<string>(new string[] {ExampleSentence, Source, Translation});
				return list.Contains(fieldName);
			}
		} ;

		public LexExampleSentence(PalasoDataObject parent): base(parent)
		{
			var sentence = GetOrCreateProperty<MultiText>(WellKnownProperties.ExampleSentence);
			sentence.Parent = this;
			var translation = GetOrCreateProperty<MultiText>(WellKnownProperties.Translation);
			translation.Parent = this;
			WireUpEvents();
		}

		/// <summary>
		/// Used when a list of these items adds via "AddNew", where we have to have a default constructor.
		/// The parent is added in an even handler, on the parent, which is called by the list.
		/// </summary>
		public LexExampleSentence(): this(null) {}

		public MultiText Sentence => GetOrCreateProperty<MultiText>(WellKnownProperties.ExampleSentence);

		public MultiText Translation => GetOrCreateProperty<MultiText>(WellKnownProperties.Translation);

		public override bool IsEmpty => Sentence.Empty && Translation.Empty && !HasProperties;

		/// <summary>
		/// Supports round-tripping, though we don't use it
		/// </summary>
		public string TranslationType { get; set; }

		public LexExampleSentence Clone()
		{
			var clone = new LexExampleSentence();
			clone.TranslationType = TranslationType;
			//We clear the properties here because ExampleSentence comes with ExampleSentence and ExampleTranslation properties on construction
			//and it's just easier to treat them like any other property on clone
			clone.Properties.Clear();
			foreach (var keyValuePairToClone in Properties)
			{
				clone.AddProperty(keyValuePairToClone.Key, keyValuePairToClone.Value.Clone());
			}
			return clone;
		}

		public override bool Equals(Object obj)
		{
			return Equals(obj as LexExampleSentence);
		}

		public bool Equals(LexExampleSentence other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			if (!base.Equals(other)) return false;
			if ((TranslationType != null && !TranslationType.Equals(other.TranslationType)) || (other.TranslationType != null && !other.TranslationType.Equals(TranslationType))) return false;
			return true;
		}

		public override int GetHashCode()
		{
			// https://stackoverflow.com/a/263416/487503
			unchecked // Overflow is fine, just wrap
			{
				var hash = 47;
				hash *= 71 + TranslationType.GetHashCode();
				hash *= 71 + base.GetHashCode();
				return hash;
			}
		}
	}
}