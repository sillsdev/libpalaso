using System;
using System.Collections.Generic;

namespace SIL.WritingSystems
{
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

	public class WritingSystemConflatedEventArgs : WritingSystemIdChangedEventArgs
	{
		public WritingSystemConflatedEventArgs(string oldId, string newId)
			: base(oldId, newId)
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

	/// <summary>
	/// The writing system repository interface.
	/// </summary>
	public interface IWritingSystemRepository
	{
		/// <summary>
		/// Notifies a consuming class of a changed writing system id on Set()
		/// </summary>
		event EventHandler<WritingSystemIdChangedEventArgs> WritingSystemIdChanged;

		/// <summary>
		/// Notifies a consuming class of a deleted writing system
		/// </summary>
		event EventHandler<WritingSystemDeletedEventArgs> WritingSystemDeleted;

		/// <summary>
		/// Notifies a consuming class of a conflated writing system
		/// </summary>
		event EventHandler<WritingSystemConflatedEventArgs> WritingSystemConflated;

		/// <summary>
		/// Adds the writing system to the store or updates the store information about
		/// an already-existing writing system.  Set should be called when there is a change
		/// that updates the RFC5646 information.
		/// </summary>
		void Set(WritingSystemDefinition ws);

		/// <summary>
		/// Returns true if a call to Set should succeed, false if a call to Set would throw
		/// </summary>
		bool CanSet(WritingSystemDefinition ws);

		/// <summary>
		/// Gets the writing system object for the given Store ID
		/// </summary>
		WritingSystemDefinition Get(string id);

		/// <summary>
		/// Gets the specified writing system if it exists.
		/// </summary>
		/// <param name="id">The identifier.</param>
		/// <param name="ws">The writing system.</param>
		/// <returns></returns>
		bool TryGet(string id, out WritingSystemDefinition ws);

		/// <summary>
		/// If the given writing system were passed to Set, this function returns the
		/// new Id that would be assigned.
		/// </summary>
		string GetNewIdWhenSet(WritingSystemDefinition ws);

		/// <summary>
		/// Returns true if a writing system with the given Store ID exists in the store
		/// </summary>
		bool Contains(string id);

		/// <summary>
		/// Gives the total number of writing systems in the store
		/// </summary>
		int Count { get; }

		/// <summary>
		/// Merges two writing systems into one.
		/// </summary>
		void Conflate(string wsToConflate, string wsToConflateWith);

		/// <summary>
		/// Removes the writing system with the specified Store ID from the store.
		/// </summary>
		void Remove(string id);

		/// <summary>
		/// Returns a list of all writing system definitions in the store.
		/// </summary>
		IEnumerable<WritingSystemDefinition> AllWritingSystems { get; }

		/// <summary>
		/// If a consumer has a writingSystem ID that is not contained in the
		/// repository he can query the repository as to whether the ID was once
		/// contained and has since changed.
		/// Note that changes are only logged on Save() i.e. changes made between
		/// saves are not tracked
		/// Use WritingSystemIDHasChangedTo to determine the new ID if there has
		/// been a change
		/// </summary>
		bool WritingSystemIdHasChanged(string id);

		/// <summary>
		/// If a consumer has a writing system ID that was once contained in this
		/// repository, but has since been changed, this method will return the
		/// new ID. If there are multiple possibilities or if the ID was never
		/// contained in the repo the method will return null. If the consumer
		/// queries for an ID that has not changed and is still contained in the
		/// repo it will return the ID.
		/// Note that changes are only logged on Save() i.e. changes made between
		/// saves are not tracked
		/// Use WritingSystemIDHasChanged to determine whether an ID has changed
		/// at all
		/// </summary>
		string WritingSystemIdHasChangedTo(string id);

		/// <summary>
		/// True if it is capable of saving changes to the specified WS.
		/// </summary>
		bool CanSave(WritingSystemDefinition ws);

		/// <summary>
		/// Writes the store to a persistable medium, if applicable.
		/// </summary>
		void Save();

		IWritingSystemFactory WritingSystemFactory { get; }
	}

	/// <summary>
	/// The generic writing system repository interface. This interface allows writing system repositories
	/// to subclass the WritingSystemDefinition class.
	/// </summary>
	public interface IWritingSystemRepository<T> : IWritingSystemRepository where T : WritingSystemDefinition
	{
		void Set(T ws);
		bool CanSet(T ws);
		new T Get(string id);
		bool TryGet(string id, out T ws);
		string GetNewIdWhenSet(T ws);
		new IEnumerable<T> AllWritingSystems { get; }
		bool CanSave(T ws);
		new IWritingSystemFactory<T> WritingSystemFactory { get; }
		void Replace(string wsLanguageTag, T newWs);
	}
}
