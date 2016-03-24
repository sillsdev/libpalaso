using System.Collections.Generic;

namespace SIL.ObjectModel
{
	public static class ReverseComparer
	{
		public static ReverseComparer<T> Create<T>(IComparer<T> comparer)
		{
			return new ReverseComparer<T>(comparer);
		}
	}

	public class ReverseComparer<T> : Comparer<T>
	{
		private readonly IComparer<T> _comparer;
 
		public ReverseComparer(IComparer<T> comparer)
		{
			_comparer = comparer;
		}

		public override int Compare(T x, T y)
		{
			return _comparer.Compare(y, x);
		}
	}
}
