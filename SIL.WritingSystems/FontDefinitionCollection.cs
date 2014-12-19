namespace SIL.WritingSystems
{
	public class FontDefinitionCollection : ObservableKeyedCollection<string, FontDefinition>
	{
		protected override string GetKeyForItem(FontDefinition item)
		{
			return item.Name;
		}
	}
}
