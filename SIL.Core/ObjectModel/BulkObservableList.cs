using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace SIL.ObjectModel
{
	public class BulkObservableList<T> : ObservableList<T>
	{
		private bool _updating;

		public BulkObservableList()
		{
		}

		public BulkObservableList(IEnumerable<T> collection)
			: base(collection)
		{
		}

		public void AddRange(IEnumerable<T> items)
		{
			InsertRange(Count, items);
		}

		protected bool Updating
		{
			get { return _updating; }
		}

		public void InsertRange(int index, IEnumerable<T> items)
		{
			if (index < 0 || index > Count)
				throw new ArgumentOutOfRangeException("index");

			IList added = InsertItems(index, items);
			if (!_updating && added.Count > 0)
			{
				OnPropertyChanged(new PropertyChangedEventArgs("Count"));
				OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, added, index));
			}
		}

		private IList InsertItems(int index, IEnumerable<T> items)
		{
			bool prevUpdating = _updating;
			_updating = true;
			var added = new List<T>();
			try
			{
				foreach (T item in items)
				{
					InsertItem(index++, item);
					added.Add(item);
				}
			}
			finally
			{
				_updating = prevUpdating;
			}
			return added;
		}

		public void RemoveRangeAt(int index, int count)
		{
			if (index < 0 || index >= Count)
				throw new ArgumentOutOfRangeException("index");
			if (count < 0 || count > Count - index)
				throw new ArgumentOutOfRangeException("count");

			if (count == 0)
				return;

			IList removed = RemoveItems(index, count);
			if (!_updating)
			{
				OnPropertyChanged(new PropertyChangedEventArgs("Count"));
				OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removed, index));
			}
		}

		private IList RemoveItems(int index, int count)
		{
			bool prevUpdating = _updating;
			_updating = true;
			var removed = new T[count];
			try
			{
				for (int i = 0; i < count; i++)
				{
					removed[i] = Items[index];
					RemoveItem(index);
				}
			}
			finally
			{
				_updating = prevUpdating;
			}
			return removed;
		}

		public void ReplaceRange(int index, int count, IEnumerable<T> items)
		{
			if (index < 0 || index > Count)
				throw new ArgumentOutOfRangeException("index");
			if (count < 0 || count > Count - index)
				throw new ArgumentOutOfRangeException("count");

			IList removed = RemoveItems(index, count);
			IList added = InsertItems(index, items);
			if (!_updating)
			{
				if (removed.Count != added.Count)
					OnPropertyChanged(new PropertyChangedEventArgs("Count"));
				OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, added, removed, index));
			}
		}

		public void ReplaceAll(IEnumerable<T> items)
		{
			ReplaceRange(0, Count, items);
		}

		public void MoveRange(int oldIndex, int count, int newIndex)
		{
			if (oldIndex < 0 || oldIndex >= Count)
				throw new ArgumentOutOfRangeException("oldIndex");
			if (count < 0 || count > Count - oldIndex)
				throw new ArgumentOutOfRangeException("count");
			if (newIndex < 0 || (newIndex >= oldIndex && newIndex < count - oldIndex))
				throw new ArgumentOutOfRangeException("newIndex");

			if (count == 0)
				return;

			IList moved = RemoveItems(oldIndex, count);
			int index = newIndex;
			if (newIndex - count > oldIndex)
				index -= count;

			bool prevUpdating = _updating;
			_updating = true;
			try
			{
				foreach (T item in moved)
					InsertItem(index++, item);
			}
			finally
			{
				_updating = prevUpdating;
			}

			if (!_updating)
			{
				OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, moved, newIndex, oldIndex));
			}
		}

		public IDisposable BulkUpdate()
		{
			_updating = true;
			return new BulkUpdater(this);
		}

		private void EndUpdate()
		{
			_updating = false;
			OnPropertyChanged(new PropertyChangedEventArgs("Count"));
			OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			if (!_updating)
				base.OnCollectionChanged(e);
		}

		protected override void OnPropertyChanged(PropertyChangedEventArgs e)
		{
			if (!_updating)
				base.OnPropertyChanged(e);
		}

		private class BulkUpdater : IDisposable
		{
			private readonly BulkObservableList<T> _coll;

			public BulkUpdater(BulkObservableList<T> coll)
			{
				_coll = coll;
			}

			public void Dispose()
			{
				_coll.EndUpdate();
			}
		}
	}
}
