using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Palaso.DictionaryServices.Model;
using Palaso.Lift;
using Palaso.Tests.Code;

namespace Palaso.DictionaryServices.Tests.Model
{
	[TestFixture]
	public class LexExampleSentenceIClonableGenericTests:IClonableGenericTests<LexExampleSentence>
	{
		public override LexExampleSentence CreateNewClonable()
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

		public override Dictionary<Type, object> DefaultValuesForTypes
		{
			get
			{
				var listKvp =
					new List<KeyValuePair<string, IPalasoDataObjectProperty>>(new[]
															   {
																   new KeyValuePair<string, IPalasoDataObjectProperty>("one", new LexNote()),
																   new KeyValuePair<string, IPalasoDataObjectProperty>("two", new LexNote())
															   });
				return new Dictionary<Type, object>
							 {
								 {typeof(string), "a string!"},
								 {typeof(List<KeyValuePair<string, IPalasoDataObjectProperty>>), listKvp}
							 };
			}
		}
	}

	[TestFixture]
	public class LexExampleSentenceTests
	{
	}
}
