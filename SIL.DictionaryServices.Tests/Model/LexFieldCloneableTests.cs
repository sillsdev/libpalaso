using System.Collections.Generic;
using NUnit.Framework;
using SIL.DictionaryServices.Model;
using SIL.Lift;
using SIL.TestUtilities;
using SIL.Text;

namespace SIL.DictionaryServices.Tests.Model
{
	[TestFixture]
	public class LexFieldCloneableTests : CloneableTests<IPalasoDataObjectProperty>
	{
		public override IPalasoDataObjectProperty CreateNewCloneable()
		{
			return new LexField("type");
		}

		public override string ExceptionList =>
			//PropertyChanged: No good way to clone eventhandlers
			//_parent: We are doing top down clones. Children shouldn't make clones of their parents, but parents of their children.
			"|Parent|PropertyChanged|";

		protected override List<ValuesToSet> DefaultValuesForTypes
		{
			get
			{
				return new List<ValuesToSet>
				{
					new ValuesToSet("to be", "!(to be)"),
					new ValuesToSet(new[] {new LanguageForm("en", "en_form", null)},
						new[] {new LanguageForm("de", "de_form", null)}),
					new ValuesToSet(
						new List<LexTrait> {new LexTrait("one", "eins"), new LexTrait("two", "zwei")},
						new List<LexTrait> {new LexTrait("three", "drei"), new LexTrait("four", "vier")}),
					new ValuesToSet(new List<string> {"to", "be"}, new List<string> {"!", "to", "be"})
				};
			}
		}
	}
}