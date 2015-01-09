namespace SIL.WritingSystems.Tests
{
	public class SimpleCollationDefinitionCloneableTests : CollationDefinitionCloneableTests
	{
		public override CollationDefinition CreateNewCloneable()
		{
			return new SimpleCollationDefinition("standard");
		}

		public override string EqualsExceptionList
		{
			get { return "|_icuRules|"; }
		}
	}
}
