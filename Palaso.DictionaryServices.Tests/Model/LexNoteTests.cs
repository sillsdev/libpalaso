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
	public class LexNoteIClonableGenericTests : IClonableGenericTests<IPalasoDataObjectProperty>
	{
		public override IPalasoDataObjectProperty CreateNewClonable()
		{
			return new LexNote("type");
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
				var traits = new List<LexTrait> { new LexTrait("one", "eins"), new LexTrait("two", "zwei") };
				var fields = new List<LexField> { new LexField("one"), new LexField("two") };
				var xml = new List<string> { "one", "two" };
				var forms = new[] {new LanguageForm("en", "en_form", null)};
				return new Dictionary<Type, object>
						   {
							   {typeof(string), "text!"},
							   {typeof(List<LexTrait>), traits},
							   {typeof(List<LexField>), fields},
							   {typeof(List<string>), xml},
							   {typeof(LanguageForm[]), forms}
						   };
			}
		}
	}

	[TestFixture]
	public class LexNoteTests
	{
	}
}
