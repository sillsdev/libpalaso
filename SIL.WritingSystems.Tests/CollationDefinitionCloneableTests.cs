using System.Collections.Generic;
using Palaso.TestUtilities;

namespace SIL.WritingSystems.Tests
{
	public class CollationDefinitionCloneableTests : CloneableTests<CollationDefinition>
	{
		public override CollationDefinition CreateNewCloneable()
		{
			return new CollationDefinition("standard");
		}

		protected override bool Equals(CollationDefinition x, CollationDefinition y)
		{
			if (x == null)
				return y == null;
			return x.ValueEquals(y);
		}

		public override string ExceptionList
		{
			get { return "|IsChanged|_collator|"; }
		}

		protected override List<ValuesToSet> DefaultValuesForTypes
		{
			get
			{
				return new List<ValuesToSet>
				{
					new ValuesToSet("to be", "!(to be)")
				};
			}
		}
	}
}
