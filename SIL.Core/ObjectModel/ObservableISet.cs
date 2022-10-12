using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using SIL.Code;

namespace SIL.ObjectModel
{
	public abstract class ObservableISet<T> : IObservableSet<T>, IReadOnlySet<T>, IReadOnlyObservableCollection<T>
	{
		public virtual event NotifyCollectionChangedEventHandler CollectionChanged;
		event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
		{
			add => PropertyChanged += value;
			remove => PropertyChanged -= value;
		}

		protected virtual event PropertyChangedEventHandler PropertyChanged;

		private readonly SimpleMonitor _reentrancyMonitor = new SimpleMonitor();
		protected readonly ISet<T> _set;

		public ObservableISet(ISet<T> set)
		{
			_set = set;
		}

		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			return _set.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _set.GetEnumerator();
		}

		void ICollection<T>.Add(T item)
		{
			Add(item);
		}

		protected abstract IEqualityComparer<T> Comparer { get; }

		public virtual void UnionWith(IEnumerable<T> other)
		{
			CheckReentrancy();
			 T[] addedItems = other.Where(x => !_set.Contains(x)).ToArray();
			_set.UnionWith(addedItems);
			if (addedItems.Length > 0)
			{
				OnPropertyChanged(new PropertyChangedEventArgs("Count"));
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, addedItems));
			}
		}

		public virtual void IntersectWith(IEnumerable<T> other)
		{
			CheckReentrancy();
			T[] removedItems = _set.Where(x => !other.Contains(x)).ToArray();
			_set.ExceptWith(removedItems);
			if (removedItems.Length > 0)
			{
				OnPropertyChanged(new PropertyChangedEventArgs("Count"));
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removedItems));
			}
		}

		public virtual void ExceptWith(IEnumerable<T> other)
		{
			CheckReentrancy();
			T[] removedItems = other.Where(x => _set.Contains(x)).ToArray();
			_set.ExceptWith(removedItems);
			if (removedItems.Length > 0)
			{
				OnPropertyChanged(new PropertyChangedEventArgs("Count"));
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removedItems));
			}
		}

		public virtual void SymmetricExceptWith(IEnumerable<T> other)
		{
			CheckReentrancy();
			var addedItems = new List<T>();
			var removedItems = new List<T>();
			foreach (T item in other.Distinct(Comparer))
			{
				if (_set.Contains(item))
					removedItems.Add(item);
				else
					addedItems.Add(item);
			}

			_set.UnionWith(addedItems);
			_set.ExceptWith(removedItems);

			if (addedItems.Count > 0 || removedItems.Count > 0)
				OnPropertyChanged(new PropertyChangedEventArgs("Count"));
			if (addedItems.Count > 0)
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, addedItems));
			if (removedItems.Count > 0)
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removedItems));
		}

		public bool IsSubsetOf(IEnumerable<T> other)
		{
			return _set.IsSubsetOf(other);
		}

		public bool IsSupersetOf(IEnumerable<T> other)
		{
			return _set.IsSupersetOf(other);
		}

		public bool IsProperSupersetOf(IEnumerable<T> other)
		{
			return _set.IsProperSupersetOf(other);
		}

		public bool IsProperSubsetOf(IEnumerable<T> other)
		{
			return _set.IsProperSubsetOf(other);
		}

		public bool Overlaps(IEnumerable<T> other)
		{
			return _set.Overlaps(other);
		}

		public bool SetEquals(IEnumerable<T> other)
		{
			return _set.SetEquals(other);
		}

		public virtual bool Add(T item)
		{
			CheckReentrancy();
			if (_set.Add(item))
			{
				OnPropertyChanged(new PropertyChangedEventArgs("Count"));
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
				return true;
			}
			return false;
		}

		public virtual void Clear()
		{
			CheckReentrancy();
			int origCount = _set.Count;
			_set.Clear();
			if (origCount > 0)
			{
				OnPropertyChanged(new PropertyChangedEventArgs("Count"));
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
			}
		}

		public bool Contains(T item)
		{
			return _set.Contains(item);
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			_set.CopyTo(array, arrayIndex);
		}

		public virtual bool Remove(T item)
		{
			CheckReentrancy();
			if (_set.Remove(item))
			{
				OnPropertyChanged(new PropertyChangedEventArgs("Count"));
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
				return true;
			}
			return false;
		}

		public int Count
		{
			get { return _set.Count; }
		}

		bool ICollection<T>.IsReadOnly
		{
			get { return false; }
		}

		protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			NotifyCollectionChangedEventHandler handler = CollectionChanged;
			if (handler != null)
			{
				using (_reentrancyMonitor.Enter())
					handler(this, e);
			}
		}

		protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null)
			{
				using (_reentrancyMonitor.Enter())
					handler(this, e);
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
