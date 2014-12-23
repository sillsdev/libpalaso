using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Palaso.Extensions
{
	public static class CollectionExtensions
	{
		public static string Concat<T>(this IEnumerable<T> list, string seperator)
		{
			string s = string.Empty;

			foreach (var part in list)
			{
				s += part + seperator;
			}
			if (s.Length > 0)
			{
				return s.Substring(0, s.Length - seperator.Length);
			}
			return string.Empty;
		}

		public static B GetOrCreate<A, B>(this IDictionary<A, B> dictionary, A key)
			where B : new()
		{
			B target;
			if (!dictionary.TryGetValue(key, out target))
			{
				target = new B();
				dictionary.Add(key, target);
			}
			return target;
		}

		public static bool TryGetItem<TKey, T>(this KeyedCollection<TKey, T> collection, TKey key, out T item)
		{
			if (collection.Contains(key))
			{
				item = collection[key];
				return true;
			}

			item = default(T);
			return false;
		}
	}
}
