using System.Collections.ObjectModel;

namespace SIL.WritingSystems
{
	public class ReadOnlyKeyedCollection<TKey, TItem> : ReadOnlyCollection<TItem>
	{
		public ReadOnlyKeyedCollection(KeyedCollection<TKey, TItem> list)
			: base(list)
		{
		}

		protected new KeyedCollection<TKey, TItem> Items
		{
			get { return (KeyedCollection<TKey, TItem>)base.Items; }
		}

		public TItem this[TKey key]
		{
			get { return Items[key]; }
		}

		public bool Contains(TKey key)
		{
			return Items.Contains(key);
		}

		public bool TryGetItem(TKey key, out TItem item)
		{
			if (Items.Contains(key))
			{
				item = Items[key];
				return true;
			}
			item = default(TItem);
			return false;
		}
	}
}
