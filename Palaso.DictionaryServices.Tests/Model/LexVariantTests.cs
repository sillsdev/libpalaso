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
	public class LexVariantIClonableGenericTests : IClonableGenericTests<IPalasoDataObjectProperty>
	{
		public override IPalasoDataObjectProperty CreateNewClonable()
		{
			return new LexVariant();
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
				return new Dictionary<Type, object>
						   {
							   {typeof(List<LexTrait>), new List<LexTrait>{new LexTrait("trait1", "very important")}},
							   {typeof(List<LexField>), new List<LexField>{new LexField("trivial")}},
							   {typeof(List<string>), new List<string>{"one", "two"}},
							   {typeof(LanguageForm[]), new []{new LanguageForm("en", "en_form", null)}}
						   };
			}
		}
	}
}
