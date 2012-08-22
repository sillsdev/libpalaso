using System;
using System.Collections.Generic;
using System.Linq;
using Palaso.Code;

namespace Palaso.WritingSystems
{
	/// <summary>
	/// This class forms the bases for managing collections of WritingSystemDefinitions. WritingSystemDefinitions
	/// can be registered and then retrieved and deleted by Id. The preferred use when editting a WritingSystemDefinition stored
	/// in the WritingSystemRepository is to Get the WritingSystemDefinition in question and then to clone it either via the
	/// Clone method on WritingSystemDefinition or via the MakeDuplicate method on the WritingSystemRepository. This allows
	/// changes made to a WritingSystemDefinition to be registered back with the WritingSystemRepository via the Set method,
	/// or to be discarded by simply discarding the object.
	/// Internally the WritingSystemRepository uses the WritingSystemDefinition's StoreId property to establish the identity of
	/// a WritingSystemDefinition. This allows the user to change the Rfc646Tag components and thereby the Id of a
	/// WritingSystemDefinition and the WritingSystemRepository to update itself and the underlying store correctly.
	/// </summary>
	abstract public class WritingSystemRepositoryBase : IWritingSystemRepository
	{

		private readonly Dictionary<string, WritingSystemDefinition> _writingSystems;
		private readonly Dictionary<string, DateTime> _writingSystemsToIgnore;

		protected Dictionary<string, string> _idChangeMap;

		public event WritingSystemIdChangedEventHandler WritingSystemIdChanged;
		public event WritingSystemDeleted WritingSystemDeleted;
		public event WritingSystemConflatedEventHandler WritingSystemConflated;

		/// <summary>
		/// Constructor, set the CompatibilityMode
		/// </summary>
		protected WritingSystemRepositoryBase(WritingSystemCompatibility compatibilityMode)
		{
			CompatibilityMode = compatibilityMode;
			_writingSystems = new Dictionary<string, WritingSystemDefinition>(StringComparer.OrdinalIgnoreCase);
			_writingSystemsToIgnore = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
			_idChangeMap = new Dictionary<string, string>();
			//_sharedStore = LdmlSharedWritingSystemRepository.Singleton;
		}

		[Obsolete("Deprecated: use AllWritingSystems instead")]
		public IEnumerable<WritingSystemDefinition> WritingSystemDefinitions
		{
			get { return AllWritingSystems; }
		}

		protected IDictionary<string, DateTime> WritingSystemsToIgnore
		{
			get
			{
				return _writingSystemsToIgnore;
			}
		}

		virtual public WritingSystemDefinition CreateNew()
		{
			var retval = new WritingSystemDefinition();
			return retval;
		}

		virtual protected LdmlDataMapper CreateLdmlAdaptor()
		{
			return new LdmlDataMapper();
		}

		virtual public void Conflate(WritingSystemDefinition wsToConflate, WritingSystemDefinition wsToConflateWith)
		{
			if(WritingSystemConflated != null)
			{
				WritingSystemConflated(this, new WritingSystemConflatedEventArgs(wsToConflate.Id, wsToConflateWith.Id));
			}
			Remove(wsToConflate.Id);
		}

		virtual public void Remove(string identifier)
		{
			if (identifier == null)
			{
				throw new ArgumentNullException("identifier");
			}
			if (!_writingSystems.ContainsKey(identifier))
			{
				throw new ArgumentOutOfRangeException("identifier");
			}
			// Delete from us
			//??? Do we really delete or just mark for deletion?
			_writingSystems.Remove(identifier);
			_writingSystemsToIgnore.Remove(identifier);
			if (WritingSystemDeleted != null)
			{
				WritingSystemDeleted(this, new WritingSystemDeletedEventArgs(identifier));
			}
			//TODO: Could call the shared store to advise that one has been removed.
			//TODO: This may be useful if writing systems were reference counted.
		}

		abstract public string WritingSystemIdHasChangedTo(string id);

		virtual public void LastChecked(string identifier, DateTime dateModified)
		{
			if (_writingSystemsToIgnore.ContainsKey(identifier))
			{
				_writingSystemsToIgnore[identifier] = dateModified;
			}
			else
			{
				_writingSystemsToIgnore.Add(identifier, dateModified);
			}
		}

		protected void Clear()
		{
			_writingSystems.Clear();
		}

		public WritingSystemDefinition MakeDuplicate(WritingSystemDefinition definition)
		{
			if (definition == null)
			{
				throw new ArgumentNullException("definition");
			}
			return definition.Clone();
		}

		public abstract bool WritingSystemIdHasChanged(string id);

		[Obsolete("Deprecated: use Contains instead")]
		public bool Exists(string identifier)
		{
			return Contains(identifier);
		}

		public bool Contains(string identifier)
		{
			return _writingSystems.ContainsKey(identifier);
		}

		public bool CanSet(WritingSystemDefinition ws)
		{
			if (ws == null)
			{
				return false;
			}
			return !(_writingSystems.Keys.Any(id => id.Equals(ws.Id, StringComparison.OrdinalIgnoreCase)) &&
				ws.StoreID != _writingSystems[ws.Id].StoreID);
		}

		public virtual void  Set(WritingSystemDefinition ws)
		{
			if (ws == null)
			{
				throw new ArgumentNullException("ws");
			}

			//Check if this is a new writing system with a conflicting id
			if (!CanSet(ws))
			{
				throw new ArgumentException(String.Format("Unable to set writing system '{0}' because this id already exists. Please change this writing system id before setting it.", ws.Id));
			}
			string oldId = _writingSystems.Where(kvp => kvp.Value.StoreID == ws.StoreID).Select(kvp => kvp.Key).FirstOrDefault();
			//??? How do we update
			//??? Is it sufficient to just set it, or can we not change the reference in case someone else has it too
			//??? i.e. Do we need a ws.Copy(WritingSystemDefinition)?
			if (!String.IsNullOrEmpty(oldId) && _writingSystems.ContainsKey(oldId))
			{
				_writingSystems.Remove(oldId);
			}
			_writingSystems[ws.Id] = ws;
			if (!String.IsNullOrEmpty(oldId) && (oldId != ws.Id))
			{
				UpdateIdChangeMap(oldId, ws.Id);
				if (WritingSystemIdChanged != null)
				{
					WritingSystemIdChanged(this, new WritingSystemIdChangedEventArgs(oldId, ws.Id));
				}
			}
			if (ws.StoreID != ws.Id)
			{
				ws.StoreID = ws.Id;
			}
		}

		protected void UpdateIdChangeMap(string oldId, string newId)
		{
			if (_idChangeMap.ContainsValue(oldId))
			{
				// if the oldid is in the value of key/value, then we can update the cooresponding key with the newId
				string keyToChange = _idChangeMap.Where(pair => pair.Value == oldId).First().Key;
				_idChangeMap[keyToChange] = newId;
			}
			else if (_idChangeMap.ContainsKey(oldId))
			{
				// if oldId is already in the dictionary, set the result to be newId
				_idChangeMap[oldId] = newId;
			}
		}

		protected void LoadIdChangeMapFromExistingWritingSystems()
		{
			_idChangeMap.Clear();
			foreach (var pair in _writingSystems)
			{
				_idChangeMap[pair.Key] = pair.Key;
			}
		}

		public string GetNewStoreIDWhenSet(WritingSystemDefinition ws)
		{
			if (ws == null)
			{
				throw new ArgumentNullException("ws");
			}
			return String.IsNullOrEmpty(ws.StoreID) ? ws.Id : ws.StoreID;
		}

		public WritingSystemDefinition Get(string identifier)
		{
			if (identifier == null)
			{
				throw new ArgumentNullException("identifier");
			}
			if (!_writingSystems.ContainsKey(identifier))
			{
				throw new ArgumentOutOfRangeException("identifier", String.Format("Writing system id '{0}' does not exist.", identifier));
			}
			return _writingSystems[identifier];
		}

		public int Count
		{
			get
			{
				return _writingSystems.Count;
			}
		}

		virtual public void Save()
		{
		}

		virtual protected void OnChangeNotifySharedStore(WritingSystemDefinition ws)
		{
			DateTime lastDateModified;
			if (_writingSystemsToIgnore.TryGetValue(ws.Id, out lastDateModified) && ws.DateModified > lastDateModified)
				_writingSystemsToIgnore.Remove(ws.Id);
		}

		virtual protected void OnRemoveNotifySharedStore()
		{
		}

		virtual public IEnumerable<WritingSystemDefinition> WritingSystemsNewerIn(IEnumerable<WritingSystemDefinition> rhs)
		{
			if (rhs == null)
			{
				throw new ArgumentNullException("rhs");
			}
			var newerWritingSystems = new List<WritingSystemDefinition>();
			foreach (var ws in rhs)
			{
				Guard.AgainstNull(ws, "ws in rhs");
				if (_writingSystems.ContainsKey(ws.Bcp47Tag))
				{
					DateTime lastDateModified;
					if ((!_writingSystemsToIgnore.TryGetValue(ws.Bcp47Tag, out lastDateModified) || ws.DateModified > lastDateModified)
						&& (ws.DateModified > _writingSystems[ws.Bcp47Tag].DateModified))
					{
						newerWritingSystems.Add(ws.Clone());
					}
				}
			}
			return newerWritingSystems;
		}

		public IEnumerable<WritingSystemDefinition> AllWritingSystems
		{
			get
			{
				return _writingSystems.Values;
			}
		}

		public IEnumerable<WritingSystemDefinition> TextWritingSystems
		{
			get { return _writingSystems.Values.Where(ws => !ws.IsVoice); }
		}

		public IEnumerable<WritingSystemDefinition> VoiceWritingSystems
		{
			get { return _writingSystems.Values.Where(ws => ws.IsVoice); }
		}

		public virtual void OnWritingSystemIDChange(WritingSystemDefinition ws, string oldId)
		{
			_writingSystems[ws.Id] = ws;
			_writingSystems.Remove(oldId);
		}

		/// <summary>
		/// filters the list down to those that are texts (not audio), while preserving their order
		/// </summary>
		/// <param name="idsToFilter"></param>
		/// <returns></returns>
		public IEnumerable<string> FilterForTextIds(IEnumerable<string> idsToFilter)
		{
			var textIds = TextWritingSystems.Select(ws => ws.Id);
			return idsToFilter.Where(id => textIds.Contains(id));
		}

		public WritingSystemCompatibility CompatibilityMode { get; private set; }

	}

	public class WritingSystemIdChangedEventArgs : EventArgs
	{
		public WritingSystemIdChangedEventArgs(string oldId, string newId)
		{
			OldId = oldId;
			NewId = newId;
		}
		public string OldId { get; private set; }
		public string NewId { get; private set; }
	}

	public class WritingSystemConflatedEventArgs:WritingSystemIdChangedEventArgs
	{
		public WritingSystemConflatedEventArgs(string oldId, string newId) : base(oldId, newId)
		{
		}
	}

	public class WritingSystemDeletedEventArgs : EventArgs
	{
		public WritingSystemDeletedEventArgs(string id)
		{
			Id = id;
		}
		public string Id { get; private set; }
	}
}
