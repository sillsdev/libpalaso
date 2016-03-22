using System;
using System.Collections.Generic;

namespace SIL.ObjectModel
{
	public static class AnonymousComparer
	{
		public static AnonymousComparer<T> Create<T>(Func<T, T, int> compare)
		{
			return new AnonymousComparer<T>(compare);
		}
	}

	public class AnonymousComparer<T> : Comparer<T>
	{
		private readonly Func<T, T, int> _compare;
 
		public AnonymousComparer(Func<T, T, int> compare)
		{
			_compare = compare;
		}

		public override int Compare(T x, T y)
		{
			return _compare(x, y);
		}
	}
}
