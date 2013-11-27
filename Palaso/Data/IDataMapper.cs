using System;
using System.Collections.Generic;

namespace Palaso.Data
{
	public interface IQuery<T> where T: class, new()
	{
		IEnumerable<IDictionary<string, object>> GetResults(T item);
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