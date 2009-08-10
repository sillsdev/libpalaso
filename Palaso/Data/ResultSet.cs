using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace Palaso.Data
{
	public sealed class ResultSet<T>: IEnumerable<RecordToken<T>>, IEnumerable<RepositoryId>
			where T : class, new()
	{
		private readonly List<RecordToken<T>> _results;
		private readonly IDataMapper<T> _dataMapper;

		public ResultSet(IDataMapper<T> dataMapper, params IEnumerable<RecordToken<T>>[] results)
		{
			if (dataMapper == null)
			{
				throw new ArgumentNullException("dataMapper");
			}
			if (results == null)
			{
				throw new ArgumentNullException("results");
			}
			_results = new List<RecordToken<T>>();
			foreach (IEnumerable<RecordToken<T>> result in results)
			{
				_results.AddRange(result);
			}
			_dataMapper = dataMapper;
		}

		[Obsolete]
		private static List<RecordToken<T>> CreateResultsWithNoDuplicates(IEnumerable<IEnumerable<RecordToken<T>>> initialResults)
		{
			Dictionary<RecordToken<T>, object> alreadyUsedTokens = new Dictionary<RecordToken<T>, object>();
			List<RecordToken<T>> results = new List<RecordToken<T>>();
			foreach (IEnumerable<RecordToken<T>> resultSet in initialResults)
			{
				foreach (RecordToken<T> token in resultSet)
				{
					if (alreadyUsedTokens.ContainsKey(token))
					{
						continue;
					}

					alreadyUsedTokens.Add(token, null);
					results.Add(token);
				}
			}
			return results;
		}

		public RecordToken<T> this[int index]
		{
			get { return _results[index]; }
		}

		public void RemoveAll(Predicate<RecordToken<T>> match)
		{
			_results.RemoveAll(match);
		}


		public int Count
		{
			get { return _results.Count; }
		}

		private RecordToken<T> GetItemFromIndex(int index)
		{
			if (index < 0)
			{
				return null;
			}
			return this[index];
		}

		#region FindFirst(Index) Of DisplayString

		public RecordToken<T> FindFirst(Predicate<RecordToken<T>> match)
		{
			int index = FindFirstIndex(match);
			return GetItemFromIndex(index);
		}

		public RecordToken<T> FindFirst(int startIndex, Predicate<RecordToken<T>> match)
		{
			int index = FindFirstIndex(startIndex, match);
			return GetItemFromIndex(index);
		}

		public RecordToken<T> FindFirst(int startIndex, int count, Predicate<RecordToken<T>> match)
		{
			int index = FindFirstIndex(startIndex, count, match);
			return GetItemFromIndex(index);
		}

		public int FindFirstIndex(Predicate<RecordToken<T>> match)
		{
			return _results.FindIndex(match);
		}

		public int FindFirstIndex(int startIndex, int count, Predicate<RecordToken<T>> match)
		{
			return _results.FindIndex(startIndex, count, match);
		}

		public int FindFirstIndex(int startIndex, Predicate<RecordToken<T>> match)
		{
			return _results.FindIndex(startIndex, match);
		}

		#endregion

		#region FindFirst(Index) of RepositoryId

		public RecordToken<T> FindFirst(RepositoryId id)
		{
			int index = FindFirstIndex(id);
			return GetItemFromIndex(index);
		}

		public RecordToken<T> FindFirst(RepositoryId id, int startIndex)
		{
			int index = FindFirstIndex(id, startIndex);
			return GetItemFromIndex(index);
		}

		public RecordToken<T> FindFirst(RepositoryId id, int startIndex, int count)
		{
			int index = FindFirstIndex(id, startIndex, count);
			return GetItemFromIndex(index);
		}

		public int FindFirstIndex(RepositoryId id, int startIndex, int count)
		{
			return _results.FindIndex(startIndex,
									  count,
									  delegate(RecordToken<T> r) { return (r.Id == id); });
		}

		public int FindFirstIndex(RepositoryId id)
		{
			return _results.FindIndex(delegate(RecordToken<T> r) { return (r.Id == id); });
		}

		public int FindFirstIndex(RepositoryId id, int startIndex)
		{
			return _results.FindIndex(startIndex,
									  delegate(RecordToken<T> r) { return (r.Id == id); });
		}

		#endregion

		#region FindFirst(Index) of T

		public RecordToken<T> FindFirst(T item)
		{
			int index = FindFirstIndex(item);
			return GetItemFromIndex(index);
		}

		public RecordToken<T> FindFirst(T item, int startIndex)
		{
			int index = FindFirstIndex(item, startIndex);
			return GetItemFromIndex(index);
		}

		public RecordToken<T> FindFirst(T item, int startIndex, int count)
		{
			int index = FindFirstIndex(item, startIndex, count);
			return GetItemFromIndex(index);
		}

		public int FindFirstIndex(T item)
		{
			RepositoryId id = _dataMapper.GetId(item);
			return FindFirstIndex(id);
		}

		public int FindFirstIndex(T item, int startIndex)
		{
			RepositoryId id = _dataMapper.GetId(item);
			return FindFirstIndex(id, startIndex);
		}

		public int FindFirstIndex(T item, int startIndex, int count)
		{
			RepositoryId id = _dataMapper.GetId(item);
			return FindFirstIndex(id, startIndex, count);
		}

		#endregion

		#region FindFirstIndex of RecordToken<T>

		public int FindFirstIndex(RecordToken<T> token, int startIndex, int count)
		{
			return _results.FindIndex(startIndex,
									  count,
									  delegate(RecordToken<T> r) { return (r == token); });
		}

		public int FindFirstIndex(RecordToken<T> token)
		{
			return _results.FindIndex(delegate(RecordToken<T> r) { return (r == token); });
		}

		public int FindFirstIndex(RecordToken<T> token, int startIndex)
		{
			return _results.FindIndex(startIndex,
									  delegate(RecordToken<T> r) { return (r == token); });
		}

		#endregion

		public static explicit operator BindingList<RecordToken<T>>(ResultSet<T> results)
		{
			return new BindingList<RecordToken<T>>(results._results);
		}

		#region IEnumerable<RecordToken<T>> Members

		IEnumerator<RecordToken<T>> IEnumerable<RecordToken<T>>.GetEnumerator()
		{
			return _results.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		public IEnumerator GetEnumerator()
		{
			return ((IEnumerable<RecordToken<T>>) this).GetEnumerator();
		}

		#endregion

		#region IEnumerable<RepositoryId> Members

		IEnumerator<RepositoryId> IEnumerable<RepositoryId>.GetEnumerator()
		{
			foreach (RecordToken<T> recordToken in this)
			{
				yield return (recordToken.Id);
			}
		}

		#endregion

		public void Sort(params SortDefinition[] sortDefinitions)
		{
			_results.Sort(new RecordTokenComparer<T>(sortDefinitions));
		}

		public void SortByRepositoryId()
		{
			SortByRepositoryId(_results);
		}

		private static void SortByRepositoryId(List<RecordToken<T>> list)
		{
			list.Sort(new RecordTokenComparer<T>(new SortDefinition("RepositoryId", Comparer<RepositoryId>.Default)));
		}

		/// <summary>
		/// Removes any entries for which the predicate canBeRemoved is true
		///   and another record token with the same repository Id exists
		/// </summary>
		/// <param name="fieldName"></param>
		/// <param name="canBeRemoved"></param>
		public void Coalesce(string fieldName, Predicate<object> canBeRemoved)
		{
			List<RecordToken<T>> results = new List<RecordToken<T>>(_results);
			SortByRepositoryId(results);

			bool hasValidEntry = false;
			List<RecordToken<T>> removeable = new List<RecordToken<T>>();
			RecordToken<T> previousToken = null;
			foreach (RecordToken<T> token in results)
			{
				if ((previousToken != null) && (token.Id != previousToken.Id))
				{
					if(hasValidEntry)
					{
						RemoveTokens(removeable);
					}
					removeable.Clear();
					hasValidEntry = false;
				}

				if (canBeRemoved(token[fieldName]))
				{
					removeable.Add(token);
				}
				else
				{
					hasValidEntry = true;
				}
				previousToken = token;
			}
			if (hasValidEntry)
			{
				RemoveTokens(removeable);
			}
		}

		private void RemoveTokens(IEnumerable<RecordToken<T>> removeable)
		{
			foreach (RecordToken<T> recordToken in removeable)
			{
				_results.Remove(recordToken);
			}
		}
	}
	public class SortDefinition
	{
		private readonly string _field;
		private readonly IComparer _comparer;

		public SortDefinition(string field, IComparer comparer)
		{
			this._field = field;
			this._comparer = comparer;
		}

		public string Field
		{
			get { return this._field; }
		}

		public IComparer Comparer
		{
			get { return this._comparer; }
		}
	}
}
