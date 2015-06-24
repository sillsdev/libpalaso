namespace SIL.WritingSystems.Tests
{
	public class SystemCollationDefinitionCloneableTests : CollationDefinitionCloneableTests
	{
		public override CollationDefinition CreateNewCloneable()
		{
			return new SystemCollationDefinition();
		}
	}
}
