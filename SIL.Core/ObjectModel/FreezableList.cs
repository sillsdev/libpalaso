using System;
using System.Collections;
using System.Collections.Generic;

namespace SIL.ObjectModel
{
	public class FreezableList<T> : IList<T>, IFreezable
	{
		private readonly List<T> _list;

		public FreezableList()
		{
			_list = new List<T>();
		}

		public FreezableList(IEnumerable<T> items)
		{
			_list = new List<T>(items);
		}
			
		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			return _list.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _list.GetEnumerator();
		}

		public void Add(T item)
		{
			CheckFrozen();
			_list.Add(item);
		}

		public void Clear()
		{
			CheckFrozen();
			_list.Clear();
		}

		public bool Contains(T item)
		{
			return _list.Contains(item);
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			_list.CopyTo(array, arrayIndex);
		}

		public bool Remove(T item)
		{
			CheckFrozen();

			return _list.Remove(item);
		}

		public int Count
		{
			get { return _list.Count; }
		}

		public bool IsReadOnly
		{
			get { return IsFrozen; }
		}

		public int IndexOf(T item)
		{
			return _list.IndexOf(item);
		}

		public void Insert(int index, T item)
		{
			CheckFrozen();

			_list.Insert(index, item);
		}

		public void RemoveAt(int index)
		{
			CheckFrozen();

			_list.RemoveAt(index);
		}

		public T this[int index]
		{
			get { return _list[index]; }
			set
			{
				CheckFrozen();
				_list[index] = value;
			}
		}

		private void CheckFrozen()
		{
			if (IsFrozen)
				throw new InvalidOperationException("The list is immutable.");
		}

		public bool IsFrozen { get; private set; }

		public void Freeze()
		{
			if (IsFrozen)
				return;

			IsFrozen = true;
		}

		public int GetFrozenHashCode()
		{
			return GetHashCode();
		}
	}
}
