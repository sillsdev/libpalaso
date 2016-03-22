using System;
using System.Collections.Generic;

namespace SIL.ObjectModel
{
	public static class ProjectionEqualityComparer
	{
		public static ProjectionEqualityComparer<TSource, TKey> Create<TSource, TKey>(Func<TSource, TKey> projection)
		{
			return new ProjectionEqualityComparer<TSource, TKey>(projection);
		}

		public static ProjectionEqualityComparer<TSource, TKey> Create<TSource, TKey>(TSource ignored, Func<TSource, TKey> projection)
		{
			return new ProjectionEqualityComparer<TSource, TKey>(projection);
		}
	}

	public static class ProjectionEqualityComparer<TSource>
	{
		public static ProjectionEqualityComparer<TSource, TKey> Create<TKey>(Func<TSource, TKey> projection)
		{
			return new ProjectionEqualityComparer<TSource, TKey>(projection);
		}
	}

	public class ProjectionEqualityComparer<TSource, TKey> : IEqualityComparer<TSource>
	{
		private readonly Func<TSource, TKey> _projection;
		private readonly IEqualityComparer<TKey> _comparer;

		public ProjectionEqualityComparer(Func<TSource, TKey> projection)
			: this(projection, null)
		{
		}

		public ProjectionEqualityComparer(Func<TSource, TKey> projection, IEqualityComparer<TKey> comparer)
		{
			_comparer = comparer ?? EqualityComparer<TKey>.Default;
			_projection = projection;
		}

		public bool Equals(TSource x, TSource y)
		{
			if (x == null && y == null)
				return true;
			if (x == null || y == null)
				return false;
			return _comparer.Equals(_projection(x), _projection(y));
		}

		public int GetHashCode(TSource obj)
		{
			if (obj == null)
				throw new ArgumentNullException("obj");
			return _comparer.GetHashCode(_projection(obj));
		}
	}
}
