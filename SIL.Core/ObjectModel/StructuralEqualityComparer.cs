using System.Collections;
using System.Collections.Generic;

namespace SIL.Collections
{
	public class StructuralEqualityComparer<T> : IEqualityComparer<T>
	{
		private static readonly StructuralEqualityComparer<T> Comparer = new StructuralEqualityComparer<T>(); 
		public static StructuralEqualityComparer<T> Default
		{
			get { return Comparer; }
		}

		public bool Equals(T x, T y)
		{
			return StructuralComparisons.StructuralEqualityComparer.Equals(x, y);
		}

		public int GetHashCode(T obj)
		{
			return StructuralComparisons.StructuralEqualityComparer.GetHashCode(obj);
		}
	}
}
