using System.Collections.Generic;
using NUnit.Framework;
using SIL.DictionaryServices.Model;
using SIL.Lift;
using SIL.TestUtilities;

namespace SIL.DictionaryServices.Tests.Model
{
	[TestFixture]
	public class LexRelationCollectionCloneableTests : CloneableTests<IPalasoDataObjectProperty>
	{
		public override IPalasoDataObjectProperty CreateNewCloneable()
		{
			return new LexRelationCollection();
		}

		public override string ExceptionList =>
			//PropertyChanged: No good way to clone eventhandlers
			//_parent: We are doing top down clones. Children shouldn't make clones of their parents, but parents of their children.
			"|Parent|PropertyChanged|";

		protected override List<ValuesToSet> DefaultValuesForTypes =>
			new List<ValuesToSet>
			{
				new ValuesToSet(
					new List<LexRelation> { new LexRelation("id", "target", null) },
					new List<LexRelation> { new LexRelation("!id", "!target", null) }),
			};
	}
}