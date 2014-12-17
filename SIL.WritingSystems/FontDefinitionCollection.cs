namespace SIL.WritingSystems
{
	public class FontDefinitionCollection : ObservableKeyedCollection<string, FontDefinition>
	{
		protected override string GetKeyForItem(FontDefinition item)
		{
			return item.Name;
		}

		protected override void InsertItem(int index, FontDefinition item)
		{
			if (Contains(item.Name))
			{
				int oldIndex = IndexOf(this[item.Name]);
				RemoveAt(oldIndex);
				if (index > oldIndex)
					index--;
			}
			base.InsertItem(index, item);
		}

		public bool TryGetFontDefinition(string name, out FontDefinition fd)
		{
			if (Contains(name))
			{
				fd = this[name];
				return true;
			}

			fd = null;
			return false;
		}
	}
}
