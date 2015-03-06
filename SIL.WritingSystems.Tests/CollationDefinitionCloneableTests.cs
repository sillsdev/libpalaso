using System.Collections.Generic;
using Palaso.TestUtilities;

namespace SIL.WritingSystems.Tests
{
	public abstract class CollationDefinitionCloneableTests : CloneableTests<CollationDefinition>
	{
		protected override bool Equals(CollationDefinition x, CollationDefinition y)
		{
			if (x == null)
				return y == null;
			return x.ValueEquals(y);
		}

		public override string ExceptionList
		{
			get { return "|IsChanged|_collator|IsValid|PropertyChanged|PropertyChanging|"; }
		}

		public override string EqualsExceptionList
		{
			get { return "|_collationRules|"; }
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
