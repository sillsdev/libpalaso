using System;
using System.Collections.Generic;

namespace SIL.ObjectModel
{
	public class ReadOnlySet<T> : ReadOnlyCollection<T>, ISet<T>, IReadOnlySet<T>
	{
		private readonly ISet<T> _set; 

		public ReadOnlySet(ISet<T> set)
			: base(set)
		{
			_set = set;
		}

		bool ISet<T>.Add(T item)
		{
			throw new NotSupportedException();
		}

		void ISet<T>.UnionWith(IEnumerable<T> other)
		{
			throw new NotSupportedException();
		}

		void ISet<T>.IntersectWith(IEnumerable<T> other)
		{
			throw new NotSupportedException();
		}

		void ISet<T>.ExceptWith(IEnumerable<T> other)
		{
			throw new NotSupportedException();
		}

		void ISet<T>.SymmetricExceptWith(IEnumerable<T> other)
		{
			throw new NotSupportedException();
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
	}
}
