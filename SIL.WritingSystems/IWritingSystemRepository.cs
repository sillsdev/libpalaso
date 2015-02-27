using System;
using System.Collections.Generic;
using System.Globalization;

namespace SIL.WritingSystems
{
	public class WritingSystemIDChangedEventArgs : EventArgs
	{
		public WritingSystemIDChangedEventArgs(string oldID, string newID)
		{
			OldID = oldID;
			NewID = newID;
		}
		public string OldID { get; private set; }
		public string NewID { get; private set; }
	}

	public class WritingSystemConflatedEventArgs : WritingSystemIDChangedEventArgs
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
			ID = id;
		}
		public string ID { get; private set; }
	}

	/// <summary>
	/// The writing system repository interface.
	/// </summary>
	public interface IWritingSystemRepository
	{
		/// <summary>
		/// Notifies a consuming class of a changed writing system id on Set()
		/// </summary>
		event EventHandler<WritingSystemIDChangedEventArgs> WritingSystemIDChanged;

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
		/// If the given writing system were passed to Set, this function returns the
		/// new StoreID that would be assigned.
		/// </summary>
		string GetNewStoreIDWhenSet(WritingSystemDefinition ws);

		/// <summary>
		/// Returns true if a writing system with the given Store ID exists in the store
		/// </summary>
		bool Contains(string id);

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
		/// Creates a new writing system object using the specified language tag and returns it.
		/// Set will need to be called once identifying information has been changed in order to
		/// save it in the store.
		/// </summary>
		WritingSystemDefinition CreateNew(string id);

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
		/// repository he can query the repository as to whether the ID was once
		/// contained and has since changed.
		/// Note that changes are only logged on Save() i.e. changes made between
		/// saves are not tracked
		/// Use WritingSystemIDHasChangedTo to determine the new ID if there has
		/// been a change
		/// </summary>
		bool WritingSystemIDHasChanged(string id);

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
		string WritingSystemIDHasChangedTo(string id);

		/// <summary>
		/// 
		/// </summary>
		void LastChecked(string id, DateTime dateModified);

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
		void OnWritingSystemIDChange(WritingSystemDefinition ws, string oldID);

		///<summary>
		/// Returns a list of writing system tags that apply only to text based writing systems.
		/// i.e. audio writing systems (and all non written writing systems) are excluded.
		///</summary>
		///<param name="idsToFilter"></param>
		///<returns></returns>
		IEnumerable<string> FilterForTextIDs(IEnumerable<string> idsToFilter);

		/// <summary>
		/// Get the writing system that is most probably intended by the user, when input language changes to the specified layout and cultureInfo,
		/// given the indicated candidates, and that wsCurrent is the preferred result if it is a possible WS for the specified culture.
		/// wsCurrent is also returned if none of the candidates is found to match the specified inputs.
		/// (Currrently, if wsCurrent matches on both layout and culture, it will be returned even if it is not a candidate.
		/// It is not clear whether this behavior is desirable, and it should not be relied on.)
		/// </summary>
		/// <remarks>It is tempting to make the first argument InputLanguageEventArgs, the object that an event handler
		/// for InputLangChanged will have most immediately available, or InputLanguage, one of its properties.
		/// However, those classes are both in a Windows namespace, and
		/// may be specific to Windows.Forms, and we are trying to avoid such dependencies. The LayoutName and CultureInfo which are properties
		/// of the InputLanguage property provide all the information we need, and CultureInfo is in System.Globalization, so
		/// portability should not be a problem.</remarks>
		/// <param name="layoutName">Name of the keyboard layout the user has selected, typically (in Windows.Forms)
		/// e.InputLanguage.LayoutName, where e is the InputLanguageChangedEventArgs from the InputLanguageChanged event</param>
		/// <param name="cultureInfo">Culture of the keyboard layout the user has selected, typically (in Windows.Forms)
		/// e.InputLanguage.Culture, where e is the InputLanguageChangedEventArgs from the InputLanguageChanged event</param>
		/// <param name="wsCurrent">The writing system that is currently active in the form. This serves as a default
		/// that will be returned if no writing system can be determined from the first two arguments. It may be null. Also, if
		/// there is more than one equally promising match in candidates, and wsCurrent is one of them, it will be preferred.
		/// This ensures that we don't change WS on the user unless the keyboard they have selected definitely indicates a
		/// different WS.</param>
		/// <param name="candidates">The writing systems that should be considered as possible return values.</param>
		/// <returns></returns>
		/// Typical usage for Windows (assuming ActiveWritingSystem is a field indicating what writing system we think the user is typing):
		/// protected virtual void OnInputLangChanged(object sender, InputLanguageChangedEventArgs e)
		/// {
		///		this.ActiveWritingSytem = wsRepo.GetWsForCulture(e.InputLanguage.LayoutName, e.InputLanguage.Culture,
		///			ActiveWritingSystem, wsRepo.AllWritingSystems.ToArray())
		/// }
		/// Linux usage will have to be determined, no InputLangChanged event gets raised in Mono.
		WritingSystemDefinition GetWsForInputLanguage(string layoutName, CultureInfo cultureInfo, WritingSystemDefinition wsCurrent, WritingSystemDefinition[] candidates);
	}
}
