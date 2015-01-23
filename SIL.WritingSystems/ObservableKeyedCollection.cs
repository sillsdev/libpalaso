using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace SIL.WritingSystems
{
	public class ObservableKeyedCollection<TKey, T> : KeyedCollection<TKey, T>, INotifyCollectionChanged, INotifyPropertyChanged
	{
		public event NotifyCollectionChangedEventHandler CollectionChanged;
		event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
		{
			add { PropertyChanged += value; }
			remove { PropertyChanged -= value; }
		}

		protected event PropertyChangedEventHandler PropertyChanged;

		private readonly SimpleMonitor _reentrancyMonitor = new SimpleMonitor();
		private readonly Func<T, TKey> _keySelector;

		public ObservableKeyedCollection(Func<T, TKey> keySelector)
		{
			_keySelector = keySelector;
		}

		protected override TKey GetKeyForItem(T item)
		{
			return _keySelector(item);
		}

		protected override void ClearItems()
		{
			CheckReentrancy();
			base.ClearItems();
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		protected override void InsertItem(int index, T item)
		{
			CheckReentrancy();
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
			CheckReentrancy();
			T oldItem = this[index];
			base.RemoveItem(index);
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldItem, index));
		}

		protected override void SetItem(int index, T item)
		{
			CheckReentrancy();
			T oldItem = this[index];
			base.SetItem(index, item);
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, item, oldItem, index));
		}

		protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			if (CollectionChanged != null)
			{
				using (BlockReentrancy())
					CollectionChanged(this, e);
			}
		}

		protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
		{
			if (PropertyChanged != null)
			{
				using (BlockReentrancy())
					PropertyChanged(this, e);
			}
		}

		protected void CheckReentrancy()
		{
			if (_reentrancyMonitor.Busy)
				throw new InvalidOperationException("This collection cannot be changed during a CollectionChanged event.");
		}

		protected IDisposable BlockReentrancy()
		{
			return _reentrancyMonitor.Enter();
		}
	}
}
