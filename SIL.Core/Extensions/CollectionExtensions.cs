using System;
using System.Collections.Generic;
using System.Linq;
using SIL.ObjectModel;

namespace SIL.Extensions
{
	public static class CollectionExtensions
	{
		#region IEnumerable
		[Obsolete("Use String.Join() instead.")]
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

		public static IEnumerable<T> Concat<T>(this IEnumerable<T> source, T item)
		{
			foreach (T i in source)
				yield return i;
			yield return item;
		}

		public static IEnumerable<T> CloneItems<T>(this IEnumerable<T> source) where T : ICloneable<T>
		{
			foreach (T i in source)
				yield return i.Clone();
		}

		public static IEnumerable<Tuple<TFirst, TSecond>> Zip<TFirst, TSecond>(this IEnumerable<TFirst> first, IEnumerable<TSecond> second)
		{
			using (IEnumerator<TFirst> iterator1 = first.GetEnumerator())
			using (IEnumerator<TSecond> iterator2 = second.GetEnumerator())
			{
				while (iterator1.MoveNext() && iterator2.MoveNext())
					yield return Tuple.Create(iterator1.Current, iterator2.Current);
			} 
		}

		public static IEnumerable<Tuple<TFirst, TSecond, TThird>> Zip<TFirst, TSecond, TThird>(this IEnumerable<TFirst> first, IEnumerable<TSecond> second, IEnumerable<TThird> third)
		{
			using (IEnumerator<TFirst> iterator1 = first.GetEnumerator())
			using (IEnumerator<TSecond> iterator2 = second.GetEnumerator())
			using (IEnumerator<TThird> iterator3 = third.GetEnumerator())
			{
				while (iterator1.MoveNext() && iterator2.MoveNext() && iterator3.MoveNext())
					yield return Tuple.Create(iterator1.Current, iterator2.Current, iterator3.Current);
			}
		}

		public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector)
		{
			return source.MinBy(selector, Comparer<TKey>.Default);
		}

		public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector, IComparer<TKey> comparer)
		{
			using (IEnumerator<TSource> sourceIterator = source.GetEnumerator())
			{
				if (!sourceIterator.MoveNext())
				{
					throw new InvalidOperationException("Sequence was empty");
				}
				TSource min = sourceIterator.Current;
				TKey minKey = selector(min);
				while (sourceIterator.MoveNext())
				{
					TSource candidate = sourceIterator.Current;
					TKey candidateProjected = selector(candidate);
					if (comparer.Compare(candidateProjected, minKey) < 0)
					{
						min = candidate;
						minKey = candidateProjected;
					}
				}
				return min;
			}
		}

		public static TSource MaxBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector)
		{
			return source.MaxBy(selector, Comparer<TKey>.Default);
		}

		public static TSource MaxBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector, IComparer<TKey> comparer)
		{
			using (IEnumerator<TSource> sourceIterator = source.GetEnumerator())
			{
				if (!sourceIterator.MoveNext())
				{
					throw new InvalidOperationException("Sequence was empty");
				}
				TSource max = sourceIterator.Current;
				TKey maxKey = selector(max);
				while (sourceIterator.MoveNext())
				{
					TSource candidate = sourceIterator.Current;
					TKey candidateProjected = selector(candidate);
					if (comparer.Compare(candidateProjected, maxKey) > 0)
					{
						max = candidate;
						maxKey = candidateProjected;
					}
				}
				return max;
			}
		}

		public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
		{
			return source.DistinctBy(keySelector, EqualityComparer<TKey>.Default);
		}

		public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector,
			IEqualityComparer<TKey> comparer)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}
			if (keySelector == null)
			{
				throw new ArgumentNullException("keySelector");
			}
			if (comparer == null)
			{
				throw new ArgumentNullException("comparer");
			}

			var knownKeys = new HashSet<TKey>(comparer);
			foreach (TSource element in source)
			{
				if (knownKeys.Add(keySelector(element)))
					yield return element;
			}
		}

		public static int GetSequenceHashCode<T>(this IEnumerable<T> source)
		{
			return GetSequenceHashCode(source, EqualityComparer<T>.Default);
		}

		public static int GetSequenceHashCode<T>(this IEnumerable<T> source, IEqualityComparer<T> comparer)
		{
			return source.Aggregate(23, (code, item) => code * 31 + (item == null ? 0 : comparer.GetHashCode(item)));
		}

		public static int IndexOf<T>(this IEnumerable<T> source, T value)
		{
			return source.IndexOf(value, EqualityComparer<T>.Default);
		}

		public static int IndexOf<T>(this IEnumerable<T> source, T value, IEqualityComparer<T> comparer)
		{
			var found = source.Select((a, i) => new { a, i }).FirstOrDefault(x => comparer.Equals(x.a, value));
			return found == null ? -1 : found.i;
		}

		public static int IndexOf<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
		{
			var found = source.Select((a, i) => new { a, i }).FirstOrDefault(x => predicate(x.a));
			return found == null ? -1 : found.i;
		}

		public static bool SetEquals<T>(this IEnumerable<T> first, IEnumerable<T> second)
		{
			return SetEquals(first, second, EqualityComparer<T>.Default);
		}

		public static bool SetEquals<T>(this IEnumerable<T> first, IEnumerable<T> second, IEqualityComparer<T> comparer)
		{
			var set = new HashSet<T>(first, comparer);
			return set.SetEquals(second);
		}

		#endregion

		#region IList

		public static int BinarySearch<T>(this IList<T> list, T item)
		{
			return BinarySearch(list, item, Comparer<T>.Default);
		}

		public static int BinarySearch<T>(this IList<T> list, T item, IComparer<T> comparer)
		{
			return BinarySearch(list, item, i => i, comparer);
		}

		public static int BinarySearch<TItem, TKey>(this IList<TItem> list, TItem item, Func<TItem, TKey> keySelector)
		{
			return BinarySearch(list, item, keySelector, Comparer<TKey>.Default);
		}

		public static int BinarySearch<TItem, TKey>(this IList<TItem> list, TItem item, Func<TItem, TKey> keySelector, IComparer<TKey> comparer)
		{
			if (list == null)
			{
				throw new ArgumentNullException("list");
			}
			if (comparer == null)
			{
				throw new ArgumentNullException("comparer");
			}

			int lower = 0;
			int upper = list.Count - 1;

			while (lower <= upper)
			{
				int middle = lower + (upper - lower) / 2;
				int comparisonResult = comparer.Compare(keySelector(item), keySelector(list[middle]));
				if (comparisonResult < 0)
				{
					upper = middle - 1;
				}
				else if (comparisonResult > 0)
				{
					lower = middle + 1;
				}
				else
				{
					return middle;
				}
			}

			return ~lower;
		}

		public static ReadOnlyList<T> ToReadOnlyList<T>(this IList<T> list)
		{
			return new ReadOnlyList<T>(list);
		}

		#endregion

		#region IDictionary

		public static TValue GetOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
			where TValue : new()
		{
			TValue target;
			if (!dictionary.TryGetValue(key, out target))
			{
				target = new TValue();
				dictionary.Add(key, target);
			}
			return target;
		}

		public static ReadOnlyDictionary<TKey, TValue> ToReadOnlyDictionary<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
		{
			return new ReadOnlyDictionary<TKey, TValue>(dictionary);
		}

		#endregion

		#region ICollection

		public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
		{
			foreach (T item in items)
				collection.Add(item);
		}

		public static ReadOnlyCollection<T> ToReadOnlyCollection<T>(this ICollection<T> collection)
		{
			return new ReadOnlyCollection<T>(collection);
		}

		public static void RemoveAll<T>(this ICollection<T> collection, Func<T, bool> predicate)
		{
			foreach (T item in collection.Where(predicate).ToArray())
				collection.Remove(item);
		}

		#endregion

		#region ISet

		public static ReadOnlySet<T> ToReadOnlySet<T>(this ISet<T> set)
		{
			return new ReadOnlySet<T>(set);
		}

		#endregion

		#region IObservableList

		public static ReadOnlyObservableList<T> ToReadOnlyObservableList<T>(this IObservableList<T> list)
		{
			return new ReadOnlyObservableList<T>(list);
		}

		#endregion

		#region IObservableCollection

		public static ReadOnlyObservableCollection<T> ToReadOnlyObservableCollection<T>(this IObservableCollection<T> collection)
		{
			return new ReadOnlyObservableCollection<T>(collection);
		}

		#endregion

		#region IKeyedCollection

		public static ReadOnlyKeyedCollection<TKey, TItem> ToReadOnlyKeyedCollection<TKey, TItem>(this IKeyedCollection<TKey, TItem> collection)
		{
			return new ReadOnlyKeyedCollection<TKey, TItem>(collection);
		}

		#endregion
	}
}
