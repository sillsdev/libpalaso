using System;
using System.Collections.Generic;

namespace SIL.ObjectModel
{
	public static class ProjectionComparer
	{
		public static ProjectionComparer<TSource, TKey> Create<TSource, TKey>(Func<TSource, TKey> projection)
		{
			return new ProjectionComparer<TSource, TKey>(projection);
		}

		public static ProjectionComparer<TSource, TKey> Create<TSource, TKey>(Func<TSource, TKey> projection, IComparer<TKey> comparer)
		{
			return new ProjectionComparer<TSource, TKey>(projection, comparer);
		}
	}

	public static class ProjectionComparer<TSource>
	{
		public static ProjectionComparer<TSource, TKey> Create<TKey>(Func<TSource, TKey> projection)
		{
			return new ProjectionComparer<TSource, TKey>(projection);
		}

		public static ProjectionComparer<TSource, TKey> Create<TKey>(Func<TSource, TKey> projection, IComparer<TKey> comparer)
		{
			return new ProjectionComparer<TSource, TKey>(projection, comparer);
		}
	}

	public class ProjectionComparer<TSource, TKey> : Comparer<TSource>
	{
		private readonly Func<TSource, TKey> _projection;
		private readonly IComparer<TKey> _comparer;

		public ProjectionComparer(Func<TSource, TKey> projection)
			: this(projection, Comparer<TKey>.Default)
		{
		}

		public ProjectionComparer(Func<TSource, TKey> projection, IComparer<TKey> comparer)
		{
			_comparer = comparer;
			_projection = projection;
		}

		public override int Compare(TSource x, TSource y)
		{
			if (x == null && y == null)
				return 0;
			if (x == null)
				return -1;
			if (y == null)
				return 1;
			return _comparer.Compare(_projection(x), _projection(y));
		}
	}
}
