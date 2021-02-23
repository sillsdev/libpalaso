using System.Collections.Generic;
using NUnit.Framework;
using SIL.DictionaryServices.Model;
using SIL.TestUtilities;

namespace SIL.DictionaryServices.Tests.Model
{
	[TestFixture]
	public class LexTraitCloneableTests : CloneableTests<LexTrait>
	{
		public override LexTrait CreateNewCloneable()
		{
			return new LexTrait("type", "value");
		}

		public override string ExceptionList => "";

		protected override List<ValuesToSet> DefaultValuesForTypes =>
			new List<ValuesToSet>
			{
				new ValuesToSet("to be", "!(to be)")
			};
	}
}
