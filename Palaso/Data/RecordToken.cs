using System;
using System.Collections.Generic;

namespace Palaso.Data
{
	//todo test IComparable
	public sealed class RecordToken<T>: IComparable<RecordToken<T>>, IEquatable<RecordToken<T>> where T:class ,new()
	{
		public delegate IEnumerable<string[]> DisplayStringGenerator(T item);

		private SortedDictionary<string, object> _queryResults;
		private readonly RepositoryId _id;
		private readonly IDataMapper<T> _dataMapper;

		public RecordToken(IDataMapper<T> dataMapper, RepositoryId id)
		{
			if (dataMapper == null)
			{
				throw new ArgumentNullException("dataMapper");
			}
			if (id == null)
			{
				throw new ArgumentNullException("id");
			}
			_dataMapper = dataMapper;
			_id = id;
		}

		public RecordToken(IDataMapper<T> dataMapper,
						   IDictionary<string, object> queryResults,
						   RepositoryId id): this(dataMapper, id)
		{
			if (queryResults == null)
			{
				throw new ArgumentNullException("queryResults");
			}
			_dataMapper = dataMapper;
			_queryResults = new SortedDictionary<string, object>(queryResults); // we need to own this
		}

		public RepositoryId Id
		{
			get { return _id; }
		}

		// proxy object
		public T RealObject
		{
			get { return _dataMapper.GetItem(Id); }
		}

		public bool TryGetValue(string fieldName, out object value)
		{
			value = null;
			if (_queryResults == null)
			{
				return false;
			}
			if (!_queryResults.ContainsKey(fieldName))
			{
				return false;
			}
			value = _queryResults[fieldName];
			return true;
			//return _queryResults.TryGetValue(fieldName, out value);
		}

		public object this[string fieldName]
		{
			get
			{
				object value;
				if (TryGetValue(fieldName, out value))
				{
					return value;
				}
				return null;
			}
			set
			{
				if (_queryResults == null)
				{
					_queryResults = new SortedDictionary<string, object>();
				}
				_queryResults[fieldName] = value;
			}
		}

		public static bool operator !=(RecordToken<T> recordToken1, RecordToken<T> recordToken2)
		{
			return !Equals(recordToken1, recordToken2);
		}

		public static bool operator ==(RecordToken<T> recordToken1, RecordToken<T> recordToken2)
		{
			return Equals(recordToken1, recordToken2);
		}

		public bool Equals(RecordToken<T> recordToken)
		{
			if (recordToken == null)
			{
				return false;
			}
			return Equals(_id, recordToken._id) &&
				   new DictionaryEqualityComparer<string, object>().Equals(_queryResults,
																		   recordToken._queryResults);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(this, obj))
			{
				return true;
			}
			return Equals(obj as RecordToken<T>);
		}

		public override int GetHashCode()
		{
			int queryResultsHash = 0;
			if (_queryResults != null)
			{
				queryResultsHash = new DictionaryEqualityComparer<string, object>()
					.GetHashCode(_queryResults);
			}
			return queryResultsHash + 29 * _id.GetHashCode();
		}

		public int CompareTo(RecordToken<T> other)
		{
			if (other == null)
				return 1;

			int order = Id.CompareTo(other.Id);
			if (order != 0)
				return order;

			SortedDictionary<string, object> theirResults = other._queryResults;
			if(_queryResults == null)
			{
				if(theirResults == null)
				{
					return 0;
				}
				return -1;
			}

			if(theirResults == null)
			{
				return 1;
			}

			order = _queryResults.Count.CompareTo(theirResults.Count);
			if (order != 0)
			{
				return order;
			}

			List<string> theirKeys = new List<string>(theirResults.Keys);

			int i = 0;
			foreach (string key in _queryResults.Keys)
			{
				order = key.CompareTo(theirKeys[i]);
				if (order != 0)
				{
					return order;
				}

				order = ((IComparable) _queryResults[key]).CompareTo(theirResults[key]);
				if(order != 0)
				{
					return order;
				}
				++i;
			}
			return 0;
		}

		public override string ToString()
		{
			return base.ToString() + " " + Id;
		}

	}
}
