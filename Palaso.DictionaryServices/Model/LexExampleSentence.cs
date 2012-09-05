using System.Collections.Generic;
using System.Linq;
using Palaso.Lift;

namespace Palaso.DictionaryServices.Model
{
	public sealed class LexExampleSentence: PalasoDataObject
	{
		private string _translationType;

		//!!What!! Is this done this way so that we don't end up storing
		//  the data in the object database?
		public new class WellKnownProperties: PalasoDataObject.WellKnownProperties
		{
			public static string ExampleSentence = "ExampleSentence";
			public static string Translation = "ExampleTranslation";
			public static string Source = "source";

			public static bool Contains(string fieldName)
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

		public MultiText Sentence
		{
			get { return GetOrCreateProperty<MultiText>(WellKnownProperties.ExampleSentence); }
		}

		public MultiText Translation
		{
			get { return GetOrCreateProperty<MultiText>(WellKnownProperties.Translation); }
		}

		public override bool IsEmpty
		{
			get { return Sentence.Empty && Translation.Empty && !HasProperties; }
		}

		/// <summary>
		/// Supports round-tripping, though we don't use it
		/// </summary>
		public string TranslationType
		{
			get { return _translationType; }
			set { _translationType = value; }
		}
	}
}