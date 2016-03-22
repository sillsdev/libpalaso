using System;
using System.Collections.Generic;

namespace SIL.ObjectModel
{
	public static class AnonymousEqualityComparer
	{
		public static AnonymousEqualityComparer<T> Create<T>(Func<T, T, bool> equals, Func<T, int> getHashCode)
		{
			return new AnonymousEqualityComparer<T>(equals, getHashCode);
		}
	}

	public class AnonymousEqualityComparer<T> : EqualityComparer<T>
	{
		private readonly Func<T, T, bool> _equals;
		private readonly Func<T, int> _getHashCode;

		public AnonymousEqualityComparer(Func<T, T, bool> equals, Func<T, int> getHashCode)
		{
			_equals = equals;
			_getHashCode = getHashCode;
		}

		public override bool Equals(T x, T y)
		{
			return _equals(x, y);
		}

		public override int GetHashCode(T obj)
		{
			return _getHashCode(obj);
		}
	}
}
