namespace SIL.WritingSystems
{
	public class SpellCheckDictionaryDefinitionCollection : ObservableKeyedCollection<string, SpellCheckDictionaryDefinition>
	{
		protected override string GetKeyForItem(SpellCheckDictionaryDefinition item)
		{
			return item.Id;
		}
	}
}
