using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace SIL.ObjectModel
{
	public class ReferenceEqualityComparer<T> : IEqualityComparer<T> where T : class
	{
		private static readonly ReferenceEqualityComparer<T> Comparer = new ReferenceEqualityComparer<T>(); 
		public static ReferenceEqualityComparer<T> Instance
		{
			get { return Comparer; }
		}

		public bool Equals(T x, T y)
		{
			return ReferenceEquals(x, y);
		}

		public int GetHashCode(T obj)
		{
			return RuntimeHelpers.GetHashCode(obj);
		}
	}
}
