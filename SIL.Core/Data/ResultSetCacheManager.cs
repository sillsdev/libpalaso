using System.Collections.Generic;

namespace SIL.Data
{
	public class ResultSetCacheManager<T> where T:class, new()
	{
		Dictionary<string, ResultSetCache<T>> labelToResultSetCacheMap = new Dictionary<string, ResultSetCache<T>>();

		public ResultSetCacheManager()
		{
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
