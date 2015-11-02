using System;
using System.Collections;
using System.Collections.Generic;

namespace SIL.ObjectModel
{
	public class ReadOnlyCollection<T> : ICollection<T>, IReadOnlyCollection<T>
	{
		private readonly ICollection<T> _collection;

		public ReadOnlyCollection(ICollection<T> collection)
		{
			_collection = collection;
		}

		protected ICollection<T> Items
		{
			get { return _collection; }
		}
			
		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			return _collection.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _collection.GetEnumerator();
		}

		public void Add(T item)
		{
			throw new NotSupportedException();
		}

		public void Clear()
		{
			throw new NotSupportedException();
		}

		public bool Contains(T item)
		{
			return _collection.Contains(item);
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			_collection.CopyTo(array, arrayIndex);
		}

		public bool Remove(T item)
		{
			throw new NotSupportedException();
		}

		int ICollection<T>.Count
		{
			get { return _collection.Count; }
		}

		bool ICollection<T>.IsReadOnly
		{
			get { return true; }
		}

		public int Count
		{
			get { return _collection.Count; }
		}
	}
}
