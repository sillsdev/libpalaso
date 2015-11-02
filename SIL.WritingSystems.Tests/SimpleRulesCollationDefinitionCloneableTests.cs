namespace SIL.WritingSystems.Tests
{
	public class SimpleRulesCollationDefinitionCloneableTests : CollationDefinitionCloneableTests
	{
		public override CollationDefinition CreateNewCloneable()
		{
			return new SimpleRulesCollationDefinition("standard");
		}

		public override string EqualsExceptionList
		{
			get { return "|_collationRules|"; }
		}

	}
}
