using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Palaso.Data
{
	public class MemoryDataMapper<T>: IDataMapper<T> where T : class, new()
	{
		protected readonly Dictionary<RepositoryId, T> idToObjectHashtable =
				new Dictionary<RepositoryId, T>();

		protected readonly Dictionary<T, RepositoryId> objectToIdHashtable =
				new Dictionary<T, RepositoryId>();

		private DateTime lastModified = new DateTime(DateTime.MinValue.Ticks, DateTimeKind.Utc);
#if DEBUG
		[CLSCompliant(false)]
		protected StackTrace _constructionStackTrace;
#endif

		public MemoryDataMapper()
		{
#if DEBUG
			_constructionStackTrace = new StackTrace();
#endif
			_disposed = false;
		}
		public DateTime LastModified
		{
			get { return lastModified; }
			set
			{
				value = value.ToUniversalTime();
				lastModified = value;
			}
		}

		public virtual bool CanQuery
		{
			get { return true; }
		}

		public virtual bool CanPersist
		{
			get { return false; }
		}

		public virtual T CreateItem()
		{
			T t = new T();
			MemoryRepositoryId id = new MemoryRepositoryId();
			idToObjectHashtable.Add(id, t);
			objectToIdHashtable.Add(t, id);
			return t;
		}

		public virtual void DeleteItem(T item)
		{
			if (item == null)
			{
				throw new ArgumentNullException("item");
			}
			if (!objectToIdHashtable.ContainsKey(item))
			{
				throw new ArgumentOutOfRangeException("item");
			}
			idToObjectHashtable.Remove(objectToIdHashtable[item]);
			objectToIdHashtable.Remove(item);
			LastModified = PreciseDateTime.UtcNow;
		}

		public virtual void DeleteItem(RepositoryId id)
		{
			if (id == null)
			{
				throw new ArgumentNullException("id");
			}
			if (!idToObjectHashtable.ContainsKey(id))
			{
				throw new ArgumentOutOfRangeException("id");
			}
			T item = GetItem(id);
			DeleteItem(item);
		}

		public virtual void DeleteAllItems()
		{
			idToObjectHashtable.Clear();
			objectToIdHashtable.Clear();
			LastModified = PreciseDateTime.UtcNow;
		}

		public RepositoryId[] GetAllItems()
		{
			int numberOfIds = idToObjectHashtable.Keys.Count;
			RepositoryId[] ids = new RepositoryId[numberOfIds];
			idToObjectHashtable.Keys.CopyTo(ids, 0);
			return ids;
		}

		public virtual void SaveItem(T item)
		{
			if (item == null)
			{
				throw new ArgumentNullException("item");
			}
			if (!objectToIdHashtable.ContainsKey(item))
			{
				throw new ArgumentOutOfRangeException("item",
													  "The item must exist in the repository before it can be saved.");
			}
			DateTime timeOfSave = PreciseDateTime.UtcNow;
			LastModified = timeOfSave;
		}

		public virtual void SaveItems(IEnumerable<T> items)
		{
			if (items == null)
			{
				throw new ArgumentNullException("items");
			}
			foreach (T t in items)
			{
				SaveItem(t);
			}
		}

		//public virtual ResultSet<T> GetItemsMatching(Query query)
		//{
		//    List<RecordToken<T>> results = new List<RecordToken<T>>();
		//    foreach (T item in objectToIdHashtable.Keys)
		//    {
		//        foreach (Dictionary<string, object> result in query.GetResults(item))
		//        {
		//            results.Add(new RecordToken<T>(this, result, GetId(item)));
		//        }
		//    }
		//    return new ResultSet<T>(this, results);
		//}

		public virtual ResultSet<T> GetItemsMatching(IQuery<T> query)
		{
			List<RecordToken<T>> results = new List<RecordToken<T>>();
			foreach (T item in objectToIdHashtable.Keys)
			{
				foreach (Dictionary<string, object> result in query.GetResults(item))
				{
					results.Add(new RecordToken<T>(this, result, GetId(item)));
				}
			}
			return new ResultSet<T>(this, results);
		}

		public virtual int CountAllItems()
		{
			return idToObjectHashtable.Count;
		}

		public virtual RepositoryId GetId(T item)
		{
			if (item == null)
			{
				throw new ArgumentNullException("item");
			}
			if (!objectToIdHashtable.ContainsKey(item))
			{
				throw new ArgumentOutOfRangeException("item");
			}
			return this.objectToIdHashtable[item];
		}

		public virtual T GetItem(RepositoryId id)
		{
			if (id == null)
			{
				throw new ArgumentNullException("id");
			}
			if (!idToObjectHashtable.ContainsKey(id))
			{
				throw new ArgumentOutOfRangeException("id");
			}
			return this.idToObjectHashtable[id];
		}

		#region IDisposable Members

#if DEBUG
		~MemoryDataMapper()
		{
			if (!_disposed)
			{
				throw new ApplicationException("Disposed not explicitly called on MemoryDataMapper." + "\n" + _constructionStackTrace.ToString());
			}
		}
#endif

		[CLSCompliantAttribute(false)]
		protected bool _disposed;

		public virtual void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					// dispose-only, i.e. non-finalizable logic
				}

				// shared (dispose and finalizable) cleanup logic
				_disposed = true;
			}
		}

		protected void VerifyNotDisposed()
		{
			if (!_disposed)
			{
				throw new ObjectDisposedException("MemoryRepository");
			}
		}

		#endregion

		private class MemoryRepositoryId: RepositoryId
		{
			private static int nextId = 1;
			private readonly int id;

			public MemoryRepositoryId()
			{
				id = nextId;
				++nextId;
			}

			public static bool operator !=(
					MemoryRepositoryId memoryRepositoryId1, MemoryRepositoryId memoryRepositoryId2)
			{
				return !Equals(memoryRepositoryId1, memoryRepositoryId2);
			}

			public static bool operator ==(
					MemoryRepositoryId memoryRepositoryId1, MemoryRepositoryId memoryRepositoryId2)
			{
				return Equals(memoryRepositoryId1, memoryRepositoryId2);
			}

			public bool Equals(MemoryRepositoryId memoryRepositoryId)
			{
				if (memoryRepositoryId == null)
				{
					return false;
				}

				return id == memoryRepositoryId.id;
			}

			public override bool Equals(object obj)
			{
				if (ReferenceEquals(this, obj))
				{
					return true;
				}
				return Equals(obj as MemoryRepositoryId);
			}

			public override int GetHashCode()
			{
				return id;
			}

			public int CompareTo(MemoryRepositoryId other)
			{
				if (other == null)
				{
					return -1;
				}
				return Comparer<int>.Default.Compare(id, other.id);
			}

			public override int CompareTo(RepositoryId other)
			{
				return CompareTo(other as MemoryRepositoryId);
			}

			public override bool Equals(RepositoryId other)
			{
				return Equals(other as MemoryRepositoryId);
			}
		}
	}
}
