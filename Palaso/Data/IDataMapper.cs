using System;
using System.Collections;
using System.Collections.Generic;

namespace Palaso.Data
{
	public abstract class IQuery<T> where T: class, new()
	{
		abstract public IEnumerable<IDictionary<string, object>> GetResults(T item);
		abstract public IEnumerable<SortDefinition> SortDefinitions { get; }
		abstract public string UniqueLabel { get; }

		public static IQuery<T> operator &(IQuery<T> thisQuery, IQuery<T> thatQuery)
		{
			DelegateQuery<T>.DelegateMethod newGetResults =
				delegate(T t)
					{
						List<IDictionary<string, object>> combinedResults = new List<IDictionary<string, object>>();
						combinedResults.AddRange(thisQuery.GetResults(t));
						combinedResults.AddRange(thatQuery.GetResults(t));
						return combinedResults;
					};

			List<SortDefinition> newSortDefinitions = new List<SortDefinition>();
			newSortDefinitions.AddRange(thisQuery.SortDefinitions);
			newSortDefinitions.AddRange(thisQuery.SortDefinitions);

			string newUniqueLabel = thisQuery.UniqueLabel + " & " + thatQuery.UniqueLabel;

			return new DelegateQuery<T>(newGetResults, newSortDefinitions, newUniqueLabel);
		}
	}

	public interface IDataMapper<T>: IDisposable where T : class, new()
	{
		DateTime LastModified { get; }
		bool CanQuery { get; }
		bool CanPersist { get; }

		T CreateItem();
		int CountAllItems();
		RepositoryId GetId(T item);
		T GetItem(RepositoryId id);
		void DeleteItem(T item);
		void DeleteItem(RepositoryId id);
		void DeleteAllItems();
		RepositoryId[] GetAllItems();
		void SaveItem(T item);
		void SaveItems(IEnumerable<T> items);

		ResultSet<T> GetItemsMatching(IQuery<T> query);

	}
}