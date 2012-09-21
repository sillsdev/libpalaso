using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Palaso.DictionaryServices.Model;
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
			//no good way to to test wether all the wiring has been cloned
			//parents shouldn't be cloned... usueally the clone is happening top down in the Lexentry tree and copying the parent would break the tree structure
			get { return "|_listEventHelpers|_parent|"; }
		}

		public override Dictionary<Type, object> DefaultValuesForTypes
		{
			get
			{
				var listKvp =
					new List<KeyValuePair<string, object>>(new[]
															   {
																   new KeyValuePair<string, object>("one",
																									new LexExampleSentence
																										()),
																   new KeyValuePair<string, object>("two",
																									new LexExampleSentence
																										())
															   });
				return new Dictionary<Type, object>
							 {
								 {typeof(string), "a string!"},
								 {typeof(List<KeyValuePair<string, object>>), listKvp}
							 };
			}
		}
	}

	[TestFixture]
	public class LexExampleSentenceTests
	{
	}
}
