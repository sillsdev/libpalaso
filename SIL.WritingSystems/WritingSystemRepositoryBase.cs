using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SIL.WritingSystems
{
	/// <summary>
	/// This class forms the bases for managing collections of WritingSystemDefinitions. WritingSystemDefinitions
	/// can be registered and then retrieved and deleted by ID. The preferred use when editting a WritingSystemDefinition stored
	/// in the WritingSystemRepository is to Get the WritingSystemDefinition in question and then to clone it via the
	/// Clone method on WritingSystemDefinition. This allows
	/// changes made to a WritingSystemDefinition to be registered back with the WritingSystemRepository via the Set method,
	/// or to be discarded by simply discarding the object.
	/// Internally the WritingSystemRepository uses the WritingSystemDefinition's StoreId property to establish the identity of
	/// a WritingSystemDefinition. This allows the user to change the IETF language tag components and thereby the ID of a
	/// WritingSystemDefinition and the WritingSystemRepository to update itself and the underlying store correctly.
	/// </summary>
	public abstract class WritingSystemRepositoryBase<T> : IWritingSystemRepository<T> where T : WritingSystemDefinition
	{
		private readonly Dictionary<string, T> _writingSystems;

		private readonly Dictionary<string, string> _idChangeMap;
		private IWritingSystemFactory<T> _writingSystemFactory;

		public event EventHandler<WritingSystemIdChangedEventArgs> WritingSystemIdChanged;
		public event EventHandler<WritingSystemDeletedEventArgs> WritingSystemDeleted;
		public event EventHandler<WritingSystemConflatedEventArgs> WritingSystemConflated;

		/// <summary>
		/// </summary>
		protected WritingSystemRepositoryBase()
		{
			_writingSystems = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);
			_idChangeMap = new Dictionary<string, string>();
		}

		/// <summary>
		/// Gets the changed IDs mapping.
		/// </summary>
		protected IDictionary<string, string> ChangedIds
		{
			get { return _idChangeMap; }
		}

		protected IDictionary<string, T> WritingSystems
		{
			get { return _writingSystems; }
		}

		public virtual void Conflate(string wsToConflate, string wsToConflateWith)
		{
			T ws = _writingSystems[wsToConflate];
			RemoveDefinition(ws);
			if (WritingSystemConflated != null)
				WritingSystemConflated(this, new WritingSystemConflatedEventArgs(wsToConflate, wsToConflateWith));
		}

		/// <summary>
		/// Remove the specified WritingSystemDefinition.
		/// </summary>
		/// <param name="id">the StoreId of the WritingSystemDefinition</param>
		/// <remarks>
		/// Note that ws.StoreId may differ from ws.Id.  The former is the key into the
		/// dictionary, but the latter is what gets persisted to disk (and shown to the
		/// user).
		/// </remarks>
		public virtual void Remove(string id)
		{
			if (id == null)
				throw new ArgumentNullException("id");
			if (!_writingSystems.ContainsKey(id))
				throw new ArgumentOutOfRangeException("id");

			T ws = _writingSystems[id];
			RemoveDefinition(ws);
			if (WritingSystemDeleted != null)
				WritingSystemDeleted(this, new WritingSystemDeletedEventArgs(id));
			//TODO: Could call the shared store to advise that one has been removed.
			//TODO: This may be useful if writing systems were reference counted.
		}

		public virtual IEnumerable<T> AllWritingSystems
		{
			get { return _writingSystems.Values; }
		}

		protected virtual void RemoveDefinition(T ws)
		{
			_writingSystems.Remove(ws.Id);
		}

		public abstract string WritingSystemIdHasChangedTo(string id);

		public virtual bool CanSave(T ws)
		{
			return true;
		}

		public IWritingSystemFactory<T> WritingSystemFactory
		{
			get
			{
				if (_writingSystemFactory == null)
					_writingSystemFactory = CreateWritingSystemFactory();
				return _writingSystemFactory;
			}
		}

		protected abstract IWritingSystemFactory<T> CreateWritingSystemFactory();

		/// <summary>
		/// Removes all writing systems.
		/// </summary>
		protected void Clear()
		{
			_writingSystems.Clear();
		}

		public abstract bool WritingSystemIdHasChanged(string id);

		public virtual bool Contains(string id)
		{
			// identifier should not be null, but some unit tests never define StoreId
			// on their temporary WritingSystemDefinition objects.
			return id != null && _writingSystems.ContainsKey(id);
		}

		public bool CanSet(T ws)
		{
			if (ws == null)
			{
				return false;
			}

			if (string.IsNullOrEmpty(ws.Id))
			{
				return !_writingSystems.ContainsKey(ws.LanguageTag);
			}

			if (IsLanguageIdChanging(ws, ws.Id))
			{
				// we are going to be changing the Id, check if we can set with
				// the new Id
				return !_writingSystems.ContainsKey(ws.LanguageTag);
			}
			// If the _writingSystems contains a writing system with this Id it is a duplicate,
			// but if the writing system is reference equal to the one in _writingSystems we are either
			// updating it or in the process of creating it for the first time. So return true.
			return !_writingSystems.ContainsKey(ws.Id) || _writingSystems[ws.Id].Equals(ws);
		}

		public virtual void Set(T ws)
		{
			if (ws == null)
			{
				throw new ArgumentNullException("ws");
			}

			string oldId = _writingSystems.Where(kvp => kvp.Value.Id == ws.Id).Select(kvp => kvp.Key).FirstOrDefault();

			//Check if this is a new writing system with a conflicting id
			if (!CanSet(ws))
				throw new ArgumentException(String.Format("Unable to set writing system '{0}' because this id already exists. Please change this writing system id before setting it.", ws.LanguageTag));

			// if there is no Id set, this is new, so mark it as changed
			if (string.IsNullOrEmpty(ws.Id))
			{
				ws.ForceChanged();
				ws.Id = ws.LanguageTag;
			}
			//??? How do we update
			//??? Is it sufficient to just set it, or can we not change the reference in case someone else has it too
			//??? i.e. Do we need a ws.Copy(WritingSystemDefinition)?
			if (!string.IsNullOrEmpty(oldId) && _writingSystems.ContainsKey(oldId))
			{
				_writingSystems.Remove(oldId);
			}

			string newId = ws.Id;
			if (IsLanguageIdChanging(ws, ws.Id))
			{
				newId = ws.LanguageTag;
			}
			_writingSystems[newId] = ws;

			if (!string.IsNullOrEmpty(oldId) && IsLanguageIdChanging(ws, oldId))
			{
				UpdateChangedIds(oldId, ws.LanguageTag);
				if (WritingSystemIdChanged != null)
					WritingSystemIdChanged(this, new WritingSystemIdChangedEventArgs(oldId, ws.LanguageTag));
			}

			ws.Id = newId;
		}

		private static bool IsLanguageIdChanging(T ws, string oldId)
		{
			return !IetfLanguageTag.AreTagsEquivalent(oldId, ws.LanguageTag);
		}

		/// <summary>
		/// Replace one writing system with another
		/// </summary>
		/// <remarks>The language tag could either change, or remain the same.</remarks>
		public virtual void Replace(string languageTag, T newWs)
		{
			Remove(languageTag);
			Set(newWs);
		}

		/// <summary>
		/// Updates the changed IDs mapping.
		/// </summary>
		protected void UpdateChangedIds(string oldId, string newId)
		{
			if (_idChangeMap.ContainsValue(oldId))
			{
				// if the oldid is in the value of key/value, then we can update the cooresponding key with the newId
				string keyToChange = _idChangeMap.First(pair => pair.Value == oldId).Key;
				_idChangeMap[keyToChange] = newId;
			}
			else if (_idChangeMap.ContainsKey(oldId))
			{
				// if oldId is already in the dictionary, set the result to be newId
				_idChangeMap[oldId] = newId;
			}
		}

		/// <summary>
		/// Loads the changed IDs mapping from the existing writing systems.
		/// </summary>
		protected void LoadChangedIdsFromExistingWritingSystems()
		{
			_idChangeMap.Clear();
			foreach (var pair in _writingSystems)
				_idChangeMap[pair.Key] = pair.Key;
		}

		public virtual bool TryGet(string id, out T ws)
		{
			if (Contains(id))
			{
				ws = Get(id);
				return true;
			}

			ws = null;
			return false;
		}

		public string GetNewIdWhenSet(T ws)
		{
			if (ws == null)
			{
				throw new ArgumentNullException("ws");
			}
			return String.IsNullOrEmpty(ws.Id) ? ws.LanguageTag : ws.Id;
		}

		public virtual T Get(string id)
		{
			if (id == null)
				throw new ArgumentNullException("id");
			if (!_writingSystems.ContainsKey(id))
				throw new ArgumentOutOfRangeException("id", String.Format("Writing system id '{0}' does not exist.", id));
			return _writingSystems[id];
		}

		public virtual int Count
		{
			get
			{
				return _writingSystems.Count;
			}
		}

		public virtual void Save()
		{
		}

		void IWritingSystemRepository.Set(WritingSystemDefinition ws)
		{
			Set((T) ws);
		}

		bool IWritingSystemRepository.CanSet(WritingSystemDefinition ws)
		{
			return CanSet((T) ws);
		}

		WritingSystemDefinition IWritingSystemRepository.Get(string id)
		{
			return Get(id);
		}

		bool IWritingSystemRepository.TryGet(string id, out WritingSystemDefinition ws)
		{
			T result;
			if (TryGet(id, out result))
			{
				ws = result;
				return true;
			}

			ws = null;
			return false;
		}

		string IWritingSystemRepository.GetNewIdWhenSet(WritingSystemDefinition ws)
		{
			return GetNewIdWhenSet((T) ws);
		}

		bool IWritingSystemRepository.CanSave(WritingSystemDefinition ws)
		{
			return CanSave((T) ws);
		}

		IEnumerable<WritingSystemDefinition> IWritingSystemRepository.AllWritingSystems
		{
			get { return AllWritingSystems; }
		}

		IWritingSystemFactory IWritingSystemRepository.WritingSystemFactory
		{
			get { return WritingSystemFactory; }
		}

		/// <summary>
		/// Used whenever writing a WS definition to disk. Reads contents of any existing data in the folder,
		/// falling back to any data in the SLDR Cache.
		/// </summary>
		/// <remarks>Does not attempt to pull the writing system into the SLDR Cache.</remarks>
		protected static MemoryStream GetDataToMergeWithInSave(string writingSystemFilePath)
		{
			MemoryStream oldData = null;
			if (File.Exists(writingSystemFilePath))
			{
				// load old data to preserve stuff in LDML that we don't use, but don't throw up an error if it fails
				try
				{
					oldData = new MemoryStream(File.ReadAllBytes(writingSystemFilePath), false);
				}
				catch
				{
				}
			}
			else
			{
				var source = Path.Combine(Sldr.SldrCachePath,
					Path.GetFileName(writingSystemFilePath));
				if (File.Exists(source))
				{
					oldData = new MemoryStream(File.ReadAllBytes(source), false);
				}
			}

			return oldData;
		}
	}
}
