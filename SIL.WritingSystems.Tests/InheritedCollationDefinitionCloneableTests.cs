namespace SIL.WritingSystems.Tests
{
	public class InheritedCollationDefinitionCloneableTests : CollationDefinitionCloneableTests
	{
		public override CollationDefinition CreateNewCloneable()
		{
			return new InheritedCollationDefinition("standard");
		}

		public override string EqualsExceptionList
		{
			get { return "|_icuRules|"; }
		}
	}
}
