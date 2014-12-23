using System.Collections.ObjectModel;

namespace SIL.WritingSystems.WindowsForms.Keyboarding
{
	internal class KeyboardDescriptionCollection : KeyedCollection<string, KeyboardDescription>
	{
		protected override string GetKeyForItem(KeyboardDescription item)
		{
			return item.Id;
		}
	}
}
