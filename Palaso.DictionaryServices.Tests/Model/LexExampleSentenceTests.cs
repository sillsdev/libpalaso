using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Palaso.DictionaryServices.Model;
using Palaso.Lift;
using Palaso.TestUtilities;

namespace Palaso.DictionaryServices.Tests.Model
{
	[TestFixture]
	public class LexExampleSentenceCloneableTests : CloneableTests<LexExampleSentence>
	{
		public override LexExampleSentence CreateNewCloneable()
		{
			return new LexExampleSentence();
		}

		public override string ExceptionList
		{
			//_listEventHelpers: No good way to clone eventhandlers
			//_parent: We are doing top down clones. Children shouldn't make clones of their parents, but parents of their children.
			//PropertyChanged: No good way to clone eventhandlers
			//EmptyObjectsRemoved: No good way to clone eventhandlers. The parent should be taking care of this rather than the clone() method.
			get { return "|_listEventHelpers|_parent|PropertyChanged|EmptyObjectsRemoved|"; }
		}

		protected override List<ValuesToSet> DefaultValuesForTypes
		{
			get
			{
				return new List<ValuesToSet>
							 {
								 new ValuesToSet(
									new List<KeyValuePair<string, IPalasoDataObjectProperty>>(new[]{
											new KeyValuePair<string, IPalasoDataObjectProperty>("one", new LexNote()),
											new KeyValuePair<string, IPalasoDataObjectProperty>("two", new LexNote())}),
									new List<KeyValuePair<string, IPalasoDataObjectProperty>>(new[]{
											new KeyValuePair<string, IPalasoDataObjectProperty>("three", new LexNote()),
											new KeyValuePair<string, IPalasoDataObjectProperty>("four", new LexNote())})),
								 new ValuesToSet("to be", "!(to be)")
							 };
			}
		}

		[TestFixture]
		public class LexExampleSentenceTests
		{
		}
	}
}
