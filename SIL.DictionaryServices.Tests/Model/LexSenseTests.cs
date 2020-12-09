using System.Collections.Generic;
using System.ComponentModel;
using NUnit.Framework;
using SIL.DictionaryServices.Model;
using SIL.Lift;
using SIL.TestUtilities;

namespace SIL.DictionaryServices.Tests.Model
{
	[TestFixture]
	public class LexSenseCloneableTests:CloneableTests<LexSense>
	{
		public override LexSense CreateNewCloneable()
		{
			return new LexSense();
		}

		public override string ExceptionList =>
			//_id: Ids should be unique, even between clones
			//_listEventHelpers: no good way to clone events.
			//_parent: We are doing top down clones. Children shouldn't make clones of their parents, but parents of their children.
			//PropertyChanged: No good way to clone eventhandlers
			//EmptyObjectsRemoved: No good way to clone eventhandlers. The parent should be taking care of this rather than the clone() method.
			//PropertyChanged: No good way to clone eventhandlers
			"|Id|_listEventHelpers|_parent|PropertyChanged|EmptyObjectsRemoved|";

		protected override List<ValuesToSet> DefaultValuesForTypes
		{
			get
			{
				return new List<ValuesToSet>
							 {
								new ValuesToSet(
									  new BindingList<LexExampleSentence>{
										  new LexExampleSentence {TranslationType = "sentence1"},
										  new LexExampleSentence {TranslationType = "sentence2"}},
									  new BindingList<LexExampleSentence>{
										  new LexExampleSentence {TranslationType = "sentence3"},
										  new LexExampleSentence {TranslationType = "sentence4"}}),
								new ValuesToSet(
									 new BindingList<LexNote>{
											  new LexNote("a note"),
											  new LexNote("another note!")},
									new BindingList<LexNote>{
											  new LexNote("a not equal note"),
											  new LexNote("another not equal note!")}),
								new ValuesToSet(
									new BindingList<LexReversal>{
												  new LexReversal{Type = "type"},
												  new LexReversal{EmbeddedXmlElements = new List<string> {"one", "xml2"}}},
									new BindingList<LexReversal>{
												  new LexReversal{Type = "type-O"},
												  new LexReversal{EmbeddedXmlElements = new List<string> {"two", "xml3"}}}),
								new ValuesToSet(
									new List<KeyValuePair<string, IPalasoDataObjectProperty>>(new[]{
											new KeyValuePair<string, IPalasoDataObjectProperty>("one", new LexNote()),
											new KeyValuePair<string, IPalasoDataObjectProperty>("two", new LexNote())}),
									new List<KeyValuePair<string, IPalasoDataObjectProperty>>(new[]{
											new KeyValuePair<string, IPalasoDataObjectProperty>("one", new LexField("type")),
											new KeyValuePair<string, IPalasoDataObjectProperty>("two", new LexField("type2"))}))
							 };
			}
		}
	}
}
