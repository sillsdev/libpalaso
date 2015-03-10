namespace SIL.WritingSystems
{
	public class WritingSystemFactory : WritingSystemFactoryBase<WritingSystemDefinition>
	{
		protected override WritingSystemDefinition ConstructDefinition()
		{
			return new WritingSystemDefinition();
		}

		protected override WritingSystemDefinition ConstructDefinition(string ietfLanguageTag)
		{
			return new WritingSystemDefinition(ietfLanguageTag);
		}

		protected override WritingSystemDefinition ConstructDefinition(WritingSystemDefinition ws)
		{
			return new WritingSystemDefinition(ws);
		}
	}
}
