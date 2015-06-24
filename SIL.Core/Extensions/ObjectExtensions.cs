using System.Collections.Generic;
using System.Linq;

namespace SIL.Extensions
{
	public static class ObjectExtensions
	{
		public static bool IsOneOf<T>(this T item, params T[] list)
		{
			return list.Contains(item);
		}

		public static IEnumerable<T> ToEnumerable<T>(this T item)
		{
			yield return item;
		}
	}
}
