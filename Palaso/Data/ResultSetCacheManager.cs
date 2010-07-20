using System;
using System.Collections.Generic;
using Palaso.Data;

namespace Palaso.Data
{
	public class ResultSetCacheManager<T> where T:class, new()
	{
		private IDataMapper<T> _associatedDataMapper;
		Dictionary<string, ResultSetCache<T>> labelToResultSetCacheMap = new Dictionary<string, ResultSetCache<T>>();

		public ResultSetCacheManager(IDataMapper<T> associatedDataMapper)
		{
			_associatedDataMapper = associatedDataMapper;
		}

		public ResultSetCache<T> this[string cacheLabel]
		{
			get
			{
				if(!labelToResultSetCacheMap.ContainsKey(cacheLabel))
				{
					return null;
				}
				return labelToResultSetCacheMap[cacheLabel];
			}
		}

		public void Add(string cacheLabel, ResultSetCache<T> cacheToAdd)
		{
			labelToResultSetCacheMap.Add(cacheLabel, cacheToAdd);
		}

		public void Add(IQuery<T> query , ResultSet<T> resultSetToCache)
		{
			if(resultSetToCache.DataMapper != _associatedDataMapper)
			{
				throw new InvalidOperationException("The ResultSet that you are trying to Add to this Cache was created from a different IDataMapper.");
			}
			ResultSetCache<T>  cacheToAdd = new ResultSetCache<T>(_associatedDataMapper, query.SortDefinitions, resultSetToCache, query);
			labelToResultSetCacheMap.Add(query.Label, cacheToAdd);
		}

		public void Remove(string cacheLabel)
		{
			labelToResultSetCacheMap.Remove(cacheLabel);
		}

		public void AddItemToCaches(T item)
		{
			foreach (KeyValuePair<string, ResultSetCache<T>> pair in labelToResultSetCacheMap)
			{
				pair.Value.UpdateItemInCache(item);
			}
		}

		public void UpdateItemInCaches(T item)
		{
			foreach (KeyValuePair<string, ResultSetCache<T>> pair in labelToResultSetCacheMap)
			{
				pair.Value.UpdateItemInCache(item);
			}
		}

		public void DeleteItemFromCaches(T item)
		{
			foreach (KeyValuePair<string, ResultSetCache<T>> pair in labelToResultSetCacheMap)
			{
				pair.Value.DeleteItemFromCache(item);
			}
		}

		public void DeleteItemFromCaches(RepositoryId id)
		{
			foreach (KeyValuePair<string, ResultSetCache<T>> pair in labelToResultSetCacheMap)
			{
				pair.Value.DeleteItemFromCache(id);
			}
		}

		public void DeleteAllItemsFromCaches()
		{
			foreach (KeyValuePair<string, ResultSetCache<T>> pair in labelToResultSetCacheMap)
			{
				pair.Value.DeleteAllItemsFromCache();
			}
		}
	}
}
