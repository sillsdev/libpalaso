using System;
using System.Collections.Generic;
using System.ComponentModel;
using NUnit.Framework;
using Palaso.DictionaryServices.Model;
using Palaso.Lift;
using Palaso.Tests.Code;

namespace Palaso.DictionaryServices.Tests.Model
{
	[TestFixture]
	public class LexSenseIClonableGenericTests:IClonableGenericTests<LexSense>
	{
		public override LexSense CreateNewClonable()
		{
			return new LexSense();
		}

		public override string ExceptionList
		{
			//_id: Ids should be unique, even between clones
			//_listEventHelpers: no good way to clone events.
			//_parent: We are doing top down clones. Children shouldn't make clones of their parents, but parents of their children.
			//PropertyChanged: No good way to clone eventhandlers
			//EmptyObjectsRemoved: No good way to clone eventhandlers. The parent should be taking care of this rather than the clone() method.
			//PropertyChanged: No good way to clone eventhandlers
			get { return "|_id|_listEventHelpers|_parent|PropertyChanged|EmptyObjectsRemoved|"; }
		}

		public override Dictionary<Type, object> DefaultValuesForTypes
		{
			get
			{
				var sentenceBindingList = new BindingList<LexExampleSentence>
									  {
										  new LexExampleSentence {TranslationType = "sentence1"},
										  new LexExampleSentence {TranslationType = "sentence2"}
									  };
				var noteBindingList = new BindingList<LexNote>
										  {
											  new LexNote("a note"),
											  new LexNote("another note!")
										  };
				var reversalBindingList = new BindingList<LexReversal>
											  {
												  new LexReversal(),
												  new LexReversal
													  {EmbeddedXmlElements = new List<string> {"one", "xml2"}}
											  };
				var listKvp =
					new List<KeyValuePair<string, IPalasoDataObjectProperty>>(new[]
															   {
																   new KeyValuePair<string, IPalasoDataObjectProperty>("one", new LexNote()),
																   new KeyValuePair<string, IPalasoDataObjectProperty>("two", new LexNote())
															   });
				return new Dictionary<Type, object>
						   {
							   {typeof(BindingList<LexExampleSentence>), sentenceBindingList},
							   {typeof(BindingList<LexNote>), noteBindingList},
							   {typeof(BindingList<LexReversal>), reversalBindingList},
							   {typeof(List<KeyValuePair<string, IPalasoDataObjectProperty>>), listKvp}
						   };
			}
		}
	}
}
