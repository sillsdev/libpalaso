using System;
using System.Collections.Generic;

namespace SIL.Data
{
	public class ResultSetCache<T> where T : class, new()
	{
		private SortedDictionary<RecordToken<T>, object> _sortedTokens = null;
		private List<IQuery<T>> _cachedQueries = new List<IQuery<T>>();
		private readonly IDataMapper<T> _dataMapperQueried;

		public ResultSetCache(IDataMapper<T> dataMapperQueried, SortDefinition[] sortDefinitions)
		{
			if (dataMapperQueried == null)
			{
				throw new ArgumentNullException("dataMapperQueried");
			}
			_dataMapperQueried = dataMapperQueried;

			if (sortDefinitions == null)
			{
				_sortedTokens = new SortedDictionary<RecordToken<T>, object>(); //sort by RepositoryId
			}
			else
			{
				RecordTokenComparer<T> comparerForSorting = new RecordTokenComparer<T>(sortDefinitions);
				_sortedTokens = new SortedDictionary<RecordToken<T>, object>(comparerForSorting);
			}
		}

		public ResultSetCache(IDataMapper<T> dataMapperQueried,  SortDefinition[] sortDefinitions, ResultSet<T> resultSetToCache, IQuery<T> queryToCache)
			: this(dataMapperQueried, sortDefinitions)
		{
			Add(resultSetToCache, queryToCache);
		}

		private void SortNewResultSetIntoCachedResultSet(ResultSet<T> resultSetToCache)
		{
			foreach (RecordToken<T> token in resultSetToCache)
			{
					_sortedTokens.Add(token, null);
			}
		}

		public void Add(ResultSet<T> resultSetToCache, IQuery<T> queryToCache)
		{
			if(resultSetToCache == null)
			{
				throw new ArgumentNullException("resultSetToCache");
			}
			if (queryToCache == null)
			{
				throw new ArgumentNullException("queryToCache");
			}
			_cachedQueries.Add(queryToCache);
			SortNewResultSetIntoCachedResultSet(resultSetToCache);
		}

		public ResultSet<T> GetResultSet()
		{
			ResultSet<T> cachedResults = new ResultSet<T>(_dataMapperQueried, _sortedTokens.Keys);
			return cachedResults;
		}

		/// <summary>
		/// Call this method every time a cached item changes. This method verifies that the item you are trying to update exists int he repository.
		/// </summary>
		/// <param name="item"></param>
		public void UpdateItemInCache(T item)
		{
			if(item == null)
			{
				throw new ArgumentNullException("item");
			}

			RepositoryId itemId = _dataMapperQueried.GetId(item);

			RemoveOldTokensWithId(itemId);

			ResultSet<T> itemsQueryResults = QueryNewItem(item);

			foreach (RecordToken<T> token in itemsQueryResults)
			{
				if(!_sortedTokens.ContainsKey(token))
				{
					_sortedTokens.Add(token, null);
				}
			}
		}

		private ResultSet<T> QueryNewItem(T item)
		{
			List<RecordToken<T>> results = new List<RecordToken<T>>();
			foreach (IQuery<T> query in _cachedQueries)
			{
				foreach (Dictionary<string, object> result in query.GetResults(item))
				{
					results.Add(new RecordToken<T>(_dataMapperQueried, result, _dataMapperQueried.GetId(item)));
				}
			}
			return new ResultSet<T>(_dataMapperQueried, results);
		}

		private void RemoveOldTokensWithId(RepositoryId itemId)
		{
			List<KeyValuePair<RecordToken<T>, object>> oldTokensToDelete = new List<KeyValuePair<RecordToken<T>, object>>();
			foreach (KeyValuePair<RecordToken<T>, object> token in _sortedTokens)
			{
				if (token.Key.Id == itemId)
				{
					oldTokensToDelete.Add(token);
				}
			}
			foreach (KeyValuePair<RecordToken<T>, object> pair in oldTokensToDelete)
			{
				_sortedTokens.Remove(pair.Key);
			}
		}

		public void DeleteItemFromCache(T item)
		{
			if (item == null)
			{
				throw new ArgumentNullException("item");
			}
			RepositoryId id = _dataMapperQueried.GetId(item);
			DeleteItemFromCache(id);
		}

		public void DeleteItemFromCache(RepositoryId id)
		{
			if (id == null)
			{
				throw new ArgumentNullException("id");
			}
			CheckIfItemIsInRepository(id);
			RemoveOldTokensWithId(id);
		}

		public void DeleteAllItemsFromCache()
		{
			_sortedTokens.Clear();
		}

		private void CheckIfItemIsInRepository(RepositoryId id)
		{
			_dataMapperQueried.GetItem(id);
		}

	}
}
