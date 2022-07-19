using System;
using System.Collections.Generic;
using System.Linq;

namespace SIL.Linq
{
	public static partial class Enumerable
	{
		public static IEnumerable<TSource> Reverse<TSource> (this IEnumerable<TSource> source)
		{
			if (source == null) throw new ArgumentNullException ("source");

			var list = source.ToList ();

			for (int i = list.Count - 1; i >= 0; i--)
				yield return list [i];
		}

		public static IEnumerable<TSource> DefaultIfEmpty<TSource> (this IEnumerable<TSource> source)
		{
			return source.DefaultIfEmpty (default (TSource));
		}

		public static IEnumerable<TSource> DefaultIfEmpty<TSource> (this IEnumerable<TSource> source, TSource defaultValue)
		{
			if (source == null) throw new ArgumentNullException ("source");

			bool empty = true;
			foreach (TSource element in source)
			{
				empty = false;
				yield return element;
			}
			if (empty) yield return defaultValue;
		}

		// A bonus query operator!  It allows you to do this:
		//     myQuery.ForEach (Console.WriteLine);

		public static void ForEach<TSource> (this IEnumerable<TSource> source, Action<TSource> action)
		{
			foreach (TSource element in source)
				action (element);
		}

		/// <summary>Like SingleOrDefault, but doesn't throw exception if more than one match is found.</summary>
		/// <returns>found item or null if item isn't found or there is more than one match</returns>
		public static TSource OnlyOrDefault<TSource>(this IEnumerable<TSource> source,
			Func<TSource, bool> predicate)
		{
			if (source == null) throw new ArgumentNullException(nameof(source));
			if (predicate == null) throw new ArgumentNullException(nameof(predicate));
			return source.Where(predicate).OnlyOrDefault();
		}

		/// <summary>Like SingleOrDefault, but doesn't throw exception if more than one match is found.</summary>
		/// <returns>first element if enumeration contains or null if enumeration is empty or contains more than one element</returns>
		public static TSource OnlyOrDefault<TSource>(this IEnumerable<TSource> source)
		{
			if (source == null) throw new ArgumentNullException(nameof(source));
			using (var enumerator = source.GetEnumerator())
			{
				if (!enumerator.MoveNext())
					return default(TSource);
				var result = enumerator.Current;
				return enumerator.MoveNext() ? default(TSource) : result;
			}
		}
	}
}
