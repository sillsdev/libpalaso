using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.XPath;

using Palaso.WritingSystems;

namespace Palaso.WritingSystems
{
	public class WritingSystemStoreBase : IWritingSystemStore
	{
		// Note that language tags are supposed to be case-insensitive.  See section 2.1.1 of
		// RFC 5646.  This is the reason for a number of ugly ToLowerInvariant() method calls
		// in the code below.

		private Dictionary<string, WritingSystemDefinition> _writingSystems;
		private Dictionary<string, DateTime> _writingSystemsToIgnore;

		/// <summary>
		/// Use the default repository
		/// </summary>
		public WritingSystemStoreBase()
		{
			_writingSystems = new Dictionary<string, WritingSystemDefinition>();
			_writingSystemsToIgnore = new Dictionary<string, DateTime>();
			//_sharedStore = LdmlSharedWritingSystemCollection.Singleton;
		}

		public IEnumerable<WritingSystemDefinition> WritingSystemDefinitions
		{
			get
			{
				return _writingSystems.Values;
			}
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
			WritingSystemDefinition retval = new WritingSystemDefinition();

			//!!! TODO: Add to shared

			return retval;
		}

		virtual protected LdmlAdaptor CreateLdmlAdaptor()
		{
			return new LdmlAdaptor();
		}

		virtual public void Remove(string identifier)
		{
			if (identifier == null)
			{
				throw new ArgumentNullException("identifier");
			}
			if (!_writingSystems.ContainsKey(identifier.ToLowerInvariant()))
			{
				throw new ArgumentOutOfRangeException("identifier");
			}
			// Delete from us
			//??? Do we really delete or just mark for deletion?
			_writingSystems.Remove(identifier.ToLowerInvariant());
			_writingSystemsToIgnore.Remove(identifier.ToLowerInvariant());
			//TODO: Could call the shared store to advise that one has been removed.
			//TODO: This may be useful if writing systems were reference counted.
		}

		virtual public void LastChecked(string identifier, DateTime dateModified)
		{
			if (_writingSystemsToIgnore.ContainsKey(identifier.ToLowerInvariant()))
			{
				_writingSystemsToIgnore[identifier.ToLowerInvariant()] = dateModified;
			}
			else
			{
				_writingSystemsToIgnore.Add(identifier.ToLowerInvariant(), dateModified);
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


		public bool Exists(string identifier)
		{
			return _writingSystems.ContainsKey(identifier.ToLowerInvariant());
		}

		public virtual void Set(WritingSystemDefinition ws)
		{
			if (ws == null)
			{
				throw new ArgumentNullException("ws");
			}
			string newID = (!String.IsNullOrEmpty(ws.RFC5646)) ? ws.RFC5646 : "unknown";
			string oldIdLC = String.IsNullOrEmpty(ws.StoreID) ? String.Empty : ws.StoreID.ToLowerInvariant();
			if (_writingSystems.ContainsKey(newID.ToLowerInvariant()) && newID.ToLowerInvariant() != oldIdLC)
			{
				throw new ArgumentException(String.Format("Unable to store writing system '{0:s}' because this id already exists.  Please change this writing system before storing.", newID));
			}
			//??? How do we update
			//??? Is it sufficient to just set it, or can we not change the reference in case someone else has it too
			//??? i.e. Do we need a ws.Copy(WritingSystemDefinition)?
			if (!String.IsNullOrEmpty(oldIdLC) && _writingSystems.ContainsKey(oldIdLC))
			{
				_writingSystems.Remove(oldIdLC);
			}
			ws.StoreID = newID;
			_writingSystems[ws.StoreID.ToLowerInvariant()] = ws;
		}

		public string GetNewStoreIDWhenSet(WritingSystemDefinition ws)
		{
			if (ws == null)
			{
				throw new ArgumentNullException("ws");
			}
			return (!String.IsNullOrEmpty(ws.RFC5646)) ? ws.RFC5646 : "unknown";
		}

		public bool CanSet(WritingSystemDefinition ws)
		{
			if (ws == null)
			{
				return false;
			}
			string newIdLC = (!String.IsNullOrEmpty(ws.RFC5646)) ? ws.RFC5646.ToLowerInvariant() : "unknown";
			string oldIdLC = String.IsNullOrEmpty(ws.StoreID) ? String.Empty : ws.StoreID.ToLowerInvariant();
			return newIdLC == oldIdLC || !_writingSystems.ContainsKey(newIdLC);
		}

		public WritingSystemDefinition Get(string identifier)
		{
			if (identifier == null)
			{
				throw new ArgumentNullException("identifier");
			}
			if (!_writingSystems.ContainsKey(identifier.ToLowerInvariant()))
			{
				throw new ArgumentOutOfRangeException("identifier");
			}
			return _writingSystems[identifier.ToLowerInvariant()];
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
			if (_writingSystemsToIgnore.TryGetValue(ws.Id.ToLowerInvariant(), out lastDateModified) && ws.DateModified > lastDateModified)
				_writingSystemsToIgnore.Remove(ws.Id.ToLowerInvariant());
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
			List<WritingSystemDefinition> newerWritingSystems = new List<WritingSystemDefinition>();
			foreach (WritingSystemDefinition ws in rhs)
			{
				if (ws == null)
				{
					throw new ArgumentNullException("rhs", "rhs contains a null WritingSystemDefinition");
				}
				if (_writingSystems.ContainsKey(ws.RFC5646.ToLowerInvariant()))
				{
					DateTime lastDateModified;
					if ((!_writingSystemsToIgnore.TryGetValue(ws.RFC5646.ToLowerInvariant(), out lastDateModified) || ws.DateModified > lastDateModified)
						&& (ws.DateModified > _writingSystems[ws.RFC5646.ToLowerInvariant()].DateModified))
					{
						newerWritingSystems.Add(ws.Clone());
					}
				}
			}
			return newerWritingSystems;
		}

	}
}
