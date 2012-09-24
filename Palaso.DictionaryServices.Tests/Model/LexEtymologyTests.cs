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
	public class LexEtymologyIClonableGenericTests : IClonableGenericTests<IPalasoDataObjectProperty>
	{
		public override IPalasoDataObjectProperty CreateNewClonable()
		{
			return new LexEtymology("type", "source");
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
				var m = new MultiText();
				m.SetAlternative("en", "en form");
				var traits = new List<LexTrait> { new LexTrait("trait1", "very important") };
				var fields = new List<LexField> { new LexField("trivial") };
				var xml = new List<string> {"xml", "moreXml"};
				return new Dictionary<Type, object>
						   {
							   {typeof(string), "Hark! A string!"},
							   {typeof(MultiText), m},
							   {typeof(List<LexTrait>), traits},
							   {typeof(List<LexField>), fields},
							   {typeof(List<string>), xml},
							   {typeof(LanguageForm[]), new []{new LanguageForm("en", "en_form", null)}}
						   };
			}
		}
	}

	[TestFixture]
	public class LexEtymologyTests
	{
	}
}
