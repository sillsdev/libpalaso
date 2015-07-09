using System;
using System.Collections.Generic;

namespace SIL.ObjectModel
{
	public class ReadOnlyDictionary<TKey, TValue> : ReadOnlyCollection<KeyValuePair<TKey, TValue>>, IReadOnlyDictionary<TKey, TValue>, IDictionary<TKey, TValue>
	{
		private readonly IDictionary<TKey, TValue> _dictionary;
 
		public ReadOnlyDictionary(IDictionary<TKey, TValue> dictionary)
			: base(dictionary)
		{
			_dictionary = dictionary;
		}

		public bool ContainsKey(TKey key)
		{
			return _dictionary.ContainsKey(key);
		}

		void IDictionary<TKey, TValue>.Add(TKey key, TValue value)
		{
			throw new NotSupportedException();
		}

		bool IDictionary<TKey, TValue>.Remove(TKey key)
		{
			throw new NotSupportedException();
		}

		public bool TryGetValue(TKey key, out TValue value)
		{
			return _dictionary.TryGetValue(key, out value);
		}

		public TValue this[TKey key]
		{
			get { return _dictionary[key]; }
			set { throw new NotSupportedException(); }
		}

		public IEnumerable<TKey> Keys
		{
			get { return _dictionary.Keys; }
		}

		public IEnumerable<TValue> Values
		{
			get { return _dictionary.Values; }
		}

		ICollection<TKey> IDictionary<TKey, TValue>.Keys
		{
			get { return _dictionary.Keys; }
		}

		ICollection<TValue> IDictionary<TKey, TValue>.Values
		{
			get { return _dictionary.Values; }
		}
	}
}
