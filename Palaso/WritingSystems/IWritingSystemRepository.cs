using System;
using System.Collections.Generic;

namespace Palaso.WritingSystems
{
	public delegate void WritingSystemIdChangedEventHandler(object sender, WritingSystemIdChangedEventArgs e);
	public delegate void WritingSystemDeleted(object sender, WritingSystemDeletedEventArgs e);
	public delegate void WritingSystemConflatedEventHandler(object sender, WritingSystemConflatedEventArgs e);
	public delegate void WritingSystemLoadProblemHandler(IEnumerable<WritingSystemRepositoryProblem> problems);

	///<summary>
	/// Specifies any comaptibiltiy modes that can be imposed on a WritingSystemRepository
	///</summary>
	public enum WritingSystemCompatibility
	{
		///<summary>
		/// Strict adherence to the current LDML standard (with extensions)
		///</summary>
		Strict,
		///<summary>
		/// Permits backward compatibility with Flex 7.0.x and 7.1.x V0 LDML
		/// notably custom language tags having all elements in private use.
		/// e.g. x-abc-Zxxx-x-audio
		///</summary>
		Flex7V0Compatible
	};

	public interface IWritingSystemRepository
	{
		/// <summary>
		/// Notifies a consuming class of a changed writing system id on Set()
		/// </summary>
		event WritingSystemIdChangedEventHandler WritingSystemIdChanged;

		/// <summary>
		/// Notifies a consuming class of a deleted writing system
		/// </summary>
		event WritingSystemDeleted WritingSystemDeleted;

		/// <summary>
		/// Notifies a consuming class of a conflated writing system
		/// </summary>
		event WritingSystemConflatedEventHandler WritingSystemConflated;

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
		/// Merges two writing systems into one.
		/// </summary>
		void Conflate(string wsToConflate, string wsToConflateWith);

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

		/// <summary>
		/// If a consumer has a writingSystemId that is not contained in the
		/// repository he can query the repository as to whether the id was once
		/// contained and has since changed.
		/// Note that changes are only logged on Save() i.e. changes made between
		/// saves are not tracked
		/// Use WritingSystemIdHasChangedTo to determine the new Id if there has
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
		/// Use WritingSystemIdHasChanged to determine whether an Id has changed
		/// at all
		/// </summary>
		string WritingSystemIdHasChangedTo(string id);

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

		///<summary>
		/// Returns a list of writing system tags that apply only to text based writing systems.
		/// i.e. audio writing systems (and all non written writing systems) are excluded.
		///</summary>
		///<param name="idsToFilter"></param>
		///<returns></returns>
		IEnumerable<string> FilterForTextIds(IEnumerable<string> idsToFilter);

		///<summary>
		/// Gets / Sets the compatibilitiy mode imposed on this repository.
		///</summary>
		WritingSystemCompatibility CompatibilityMode { get; }
	}
}
