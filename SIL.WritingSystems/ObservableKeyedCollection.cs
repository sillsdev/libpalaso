using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace SIL.WritingSystems
{
	public abstract class ObservableKeyedCollection<TKey, T> : KeyedCollection<TKey, T>, INotifyCollectionChanged
	{
		public event NotifyCollectionChangedEventHandler CollectionChanged;

		public bool TryGetItem(TKey key, out T item)
		{
			if (Contains(key))
			{
				item = this[key];
				return true;
			}

			item = default(T);
			return false;
		}

		protected override void ClearItems()
		{
			base.ClearItems();
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		protected override void InsertItem(int index, T item)
		{
			TKey key = GetKeyForItem(item);
			if (Contains(key))
			{
				T oldItem = this[key];
				int oldIndex = IndexOf(oldItem);
				if (oldIndex == index - 1 && EqualityComparer<T>.Default.Equals(oldItem, item))
					return;
				RemoveAt(oldIndex);
				if (index > oldIndex)
					index--;
			}
			base.InsertItem(index, item);
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
		}

		protected override void RemoveItem(int index)
		{
			T oldItem = this[index];
			base.RemoveItem(index);
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldItem, index));
		}

		protected override void SetItem(int index, T item)
		{
			T oldItem = this[index];
			base.SetItem(index, item);
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, item, oldItem, index));
		}

		protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			if (CollectionChanged != null)
				CollectionChanged(this, e);
		}
	}
}
