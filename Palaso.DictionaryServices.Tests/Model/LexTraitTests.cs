using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Palaso.DictionaryServices.Model;
using Palaso.Lift;
using Palaso.Tests.Code;
using Palaso.Text;

namespace Palaso.DictionaryServices.Tests.Model
{
	[TestFixture]
	public class LexTraitIClonableGenericTests : IClonableGenericTests<LexTrait>
	{
		public override LexTrait CreateNewClonable()
		{
			return new LexTrait("type", "value");
		}

		public override string ExceptionList
		{
			get { return ""; }
		}

		public override Dictionary<Type, object> DefaultValuesForTypes
		{
			get
			{
				return new Dictionary<Type, object>
						   {
							   {typeof(string), "To Be!"}
						   };
			}
		}
	}

	[TestFixture]
	class LexTraitTests
	{
	}

	[TestFixture]
	public class LexFieldIClonableGenericTests : IClonableGenericTests<IPalasoDataObjectProperty>
	{
		public override IPalasoDataObjectProperty CreateNewClonable()
		{
			return new LexField("type");
		}

		public override string ExceptionList
		{
			//PropertyChanged: No good way to clone eventhandlers
			//_parent: We are doing top down clones. Children shouldn't make clones of their parents, but parents of their children.
			get { return "|_parent|PropertyChanged|"; }
		}

		public override Dictionary<Type, object> DefaultValuesForTypes
		{
			get
			{
				var traits = new List<LexTrait> {new LexTrait("one", "eins"), new LexTrait("two", "zwei")};
				return new Dictionary<Type, object>
						   {
							   {typeof(string), "a string"},
							   {typeof(List<string>), new List<string>{"one", "two"}},
							   {typeof(List<LexTrait>), traits},
							   {typeof(LanguageForm[]), new []{new LanguageForm("en", "en_form", null)}}
						   };
			}
		}
	}

	[TestFixture]
	class LexFieldTests
	{
	}
}
