using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SIL.ObjectModel
{
	public class KeyedList<TKey, TItem> : KeyedCollection<TKey, TItem>, IKeyedCollection<TKey, TItem>
	{
		private readonly Func<TItem, TKey> _getKeyForItem; 

		public KeyedList(Func<TItem, TKey> getKeyForItem)
			: this(getKeyForItem, EqualityComparer<TKey>.Default)
		{
		}

		public KeyedList(Func<TItem, TKey> getKeyForItem, IEqualityComparer<TKey> comparer)
			: this(getKeyForItem, comparer, 0)
		{
		}

		public KeyedList(Func<TItem, TKey> getKeyForItem, IEqualityComparer<TKey> comparer, int dictionaryCreationThreshold)
			: base(comparer, dictionaryCreationThreshold)
		{
			_getKeyForItem = getKeyForItem;
		}

		public KeyedList(IEnumerable<TItem> items, Func<TItem, TKey> getKeyForItem)
			: this(items, getKeyForItem, null)
		{
		}

		public KeyedList(IEnumerable<TItem> items, Func<TItem, TKey> getKeyForItem, IEqualityComparer<TKey> comparer)
			: this(items, getKeyForItem, comparer, 0)
		{
		}

		public KeyedList(IEnumerable<TItem> items, Func<TItem, TKey> getKeyForItem, IEqualityComparer<TKey> comparer, int dictionaryCreationThreshold)
			: base(comparer, dictionaryCreationThreshold)
		{
			_getKeyForItem = getKeyForItem;
			foreach (TItem item in items)
				Add(item);
		}

		protected KeyedList()
		{
		}

		protected KeyedList(IEqualityComparer<TKey> comparer)
			: base(comparer)
		{
		}

		protected KeyedList(IEqualityComparer<TKey> comparer, int dictionaryCreationThreshold)
			: base(comparer, dictionaryCreationThreshold)
		{
		}

		protected override TKey GetKeyForItem(TItem item)
		{
			return _getKeyForItem(item);
		}

		public bool TryGet(TKey key, out TItem value)
		{
			if (Contains(key))
			{
				value = this[key];
				return true;
			}

			value = default(TItem);
			return false;
		}
	}
}
