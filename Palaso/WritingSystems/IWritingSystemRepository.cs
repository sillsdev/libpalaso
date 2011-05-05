using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Palaso.WritingSystems
{
	public interface IWritingSystemRepository
	{
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
		WritingSystemDefinition Get(string identifier);

		/// <summary>
		/// If the given writing system were passed to Set, this function returns the
		/// new StoreID that would be assigned.
		/// </summary>
		string GetNewStoreIDWhenSet(WritingSystemDefinition ws);

		/// <summary>
		/// Returns true if a writing system with the given Store ID exists in the store
		/// Contains is preferred
		/// </summary>
		[Obsolete("Use Contains instead")]
		bool Exists(string identifier);

		/// <summary>
		/// Returns true if a writing system with the given Store ID exists in the store
		/// </summary>
		bool Contains(string identifier);

		/// <summary>
		/// Gives the total number of writing systems in the store
		/// </summary>
		int Count { get; }

		/// <summary>
		/// Creates a new writing system object and returns it.  Set will need to be called
		/// once identifying information has been changed in order to save it in the store.
		/// </summary>
		WritingSystemDefinition CreateNew();

		/// <summary>
		/// Removes the writing system with the specified Store ID from the store.
		/// </summary>
		void Remove(string identifier);

		/// <summary>
		/// Returns a list of all writing system definitions in the store.
		/// </summary>
		[Obsolete("Deprecated: use AllWritingSystems instead")]
		IEnumerable<WritingSystemDefinition> WritingSystemDefinitions { get; }

		/// <summary>
		/// Returns a list of all writing system definitions in the store.
		/// </summary>
		IEnumerable<WritingSystemDefinition> AllWritingSystems { get; }

		/// <summary>
		/// Returns a list of *text* writing systems in the store
		/// </summary>
		IEnumerable<WritingSystemDefinition> TextWritingSystems { get; }

		/// <summary>
		/// Returns a list of *audio* writing systems in the store
		/// </summary>
		IEnumerable<WritingSystemDefinition> VoiceWritingSystems { get; }
		/// <summary>
		/// Makes a duplicate of an existing writing system definition.  Set will need
		/// to be called with this new duplicate once identifying information has been changed
		/// in order to place the new definition in the store.
		/// </summary>
		WritingSystemDefinition MakeDuplicate(WritingSystemDefinition definition);

		void LastChecked(string identifier, DateTime dateModified);

		/// <summary>
		/// Writes the store to a persistable medium, if applicable.
		/// </summary>
		void Save();

		/// <summary>
		/// Returns a list of writing systems from rhs which are newer than ones in the store.
		/// </summary>
		// TODO: Maybe this should be IEnumerable<string> .... which returns the identifiers.
		IEnumerable<WritingSystemDefinition> WritingSystemsNewerIn(IEnumerable<WritingSystemDefinition> rhs);


		/// <summary>
		/// Event Handler that updates the store when a writing system id has changed
		/// </summary>
		void OnWritingSystemIDChange(WritingSystemDefinition ws, string oldId);

		IEnumerable<string> FilterForTextIds(IEnumerable<string> idsToFilter);
	}
}
