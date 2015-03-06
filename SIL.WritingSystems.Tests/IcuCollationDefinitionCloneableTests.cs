namespace SIL.WritingSystems.Tests
{
	public class IcuCollationDefinitionCloneableTests : CollationDefinitionCloneableTests
	{
		public override CollationDefinition CreateNewCloneable()
		{
			return new IcuCollationDefinition("standard");
		}

		public override string ExceptionList
		{
			get { return base.ExceptionList + "_imports|WritingSystemFactory|"; }
		}
	}
}
