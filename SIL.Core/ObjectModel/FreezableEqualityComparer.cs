using System;
using System.Collections.Generic;

namespace SIL.ObjectModel
{
	public class FreezableEqualityComparer<T> : IEqualityComparer<T> where T : IFreezable, IValueEquatable<T>
	{
		private static readonly FreezableEqualityComparer<T> Comparer = new FreezableEqualityComparer<T>(); 
		public static FreezableEqualityComparer<T> Default
		{
			get { return Comparer; }
		}

		public bool Equals(T x, T y)
		{
			if (x == null && y == null)
				return true;
			if (x == null || y == null)
				return false;
			return x.ValueEquals(y);
		}

		public int GetHashCode(T obj)
		{
			if (obj == null)
				throw new ArgumentNullException("obj");
			return obj.GetFrozenHashCode();
		}
	}
}
