using System;
using System.Collections;
using System.Collections.Generic;
using SIL.ObjectModel;

namespace SIL.Extensions
{
	public static class ComparerExtensions
	{
		public static IEqualityComparer<T> ToTypesafe<T>(this IEqualityComparer comparer)
		{
			return new WrapperEqualityComparer<T>(comparer);
		}

		public static IComparer<T> Reverse<T>(this IComparer<T> comparer)
		{
			if (comparer == null)
				throw new ArgumentNullException("comparer");
			return new ReverseComparer<T>(comparer);
		}
	}
}
