using System;
using System.Collections.Generic;
using Palaso.Data;

namespace Palaso.Data
{
	public class ResultSetCache<T> where T : class, new()
	{
		private SortedListAllowsDuplicates<RecordToken<T>> _sortedTokens = null;
		private List<IQuery<T>> _cachedQueries = new List<IQuery<T>>();
		private readonly IDataMapper<T> _dataMapperQueried;

		public ResultSetCache(IDataMapper<T> dataMapperQueried, IEnumerable<SortDefinition> sortDefinitions)
		{
			if (dataMapperQueried == null)
			{
				throw new ArgumentNullException("dataMapperQueried");
			}
			_dataMapperQueried = dataMapperQueried;

			if (sortDefinitions == null)
			{
				_sortedTokens = new SortedListAllowsDuplicates<RecordToken<T>>(); //sort by RepositoryId
			}
			else
			{
				RecordTokenComparer<T> comparerForSorting = new RecordTokenComparer<T>(sortDefinitions);
				_sortedTokens = new SortedListAllowsDuplicates<RecordToken<T>>(comparerForSorting);
			}
		}

		public ResultSetCache(IDataMapper<T> dataMapperQueried,  SortDefinition[] sortDefinitions, ResultSet<T> resultSetToCache, IQuery<T> queryToCache)
			: this(dataMapperQueried, sortDefinitions)
		{
			Add(resultSetToCache, queryToCache);
		}

		public ResultSetCache(IDataMapper<T> dataMapperQueried, ResultSet<T> resultSetToCache, IQuery<T> queryToCache)
			: this(dataMapperQueried, queryToCache.SortDefinitions)
		{
			Add(resultSetToCache, queryToCache);
		}

		private void SortNewResultSetIntoCachedResultSet(ResultSet<T> resultSetToCache)
		{
			foreach (RecordToken<T> token in resultSetToCache)
			{
				_sortedTokens.Add(token);
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
			ResultSet<T> cachedResults = new ResultSet<T>(_dataMapperQueried, _sortedTokens);
			return cachedResults;
		}

		/// <summary>
		/// Call this method every time a cached item changes. This method verifies that the item you are trying to update exists in the repository.
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
					_sortedTokens.Add(token);
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
			List<RecordToken<T>> oldTokensToDelete = new List<RecordToken<T>>();
			foreach (RecordToken<T> token in _sortedTokens)
			{
				if (token.Id == itemId)
				{
					oldTokensToDelete.Add(token);
				}
			}
			foreach (RecordToken<T> token in oldTokensToDelete)
			{
				_sortedTokens.Remove(token);
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
