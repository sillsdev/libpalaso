using System.ServiceModel;

namespace Palaso.Services.Dictionary
{
	[ServiceContract]
	public interface IDictionaryService
	{
		/// <summary>
		/// Search the dictionary for an ordered list of entries that may be what the user is looking for.
		/// </summary>
		/// <param name="writingSystemId"></param>
		/// <param name="form">The form to search on.  May be used to match on lexeme form, citation form, variants, etc.,
		/// depending on how the implementing dictionary services application.</param>
		/// <param name="method">Controls how matching should happen</param>
		/// <param name="ids">The ids of the returned elements, for use in other calls.</param>
		/// <param name="forms">The headwords of the matched elements.</param>
		[OperationContract]
		void GetMatchingEntries(string writingSystemId, string form, FindMethods method, out string[] ids, out string[] forms);

		/// <summary>
		/// Get an HTML representation of one or more entries.
		/// </summary>
		/// <remarks>I had originally planned this to just be a fragment, which is more composable,
		/// but that then left us with the problem of where to get the style definitions, if we
		/// don't have an html header.</remarks>
		/// <param name="entryIds"></param>
		/// <returns></returns>
		[OperationContract]
		string GetHtmlForEntries(string[] entryIds);

		/// <summary>
		/// Used to help the dictionary service app know when to quit
		/// </summary>
		/// <param name="clientProcessId"></param>
		[OperationContract]
		void RegisterClient(int clientProcessId);

		/// <summary>
		/// Used to help the dictionary service app know when to quit
		/// </summary>
		/// <param name="clientProcessId"></param>
		[OperationContract]
		void DeregisterClient(int clientProcessId);

		/// <summary>
		/// Cause a gui application to come to the front, focussed on this entry, read to edit
		/// </summary>
		/// <param name="entryId"></param>
		[OperationContract]
		void JumpToEntry(string entryId);

		/// <summary>
		/// Add a new entry to the lexicon
		/// </summary>
		/// <returns>the id that was assigned to the new entry</returns>
		[OperationContract]
		string AddEntry(string lexemeFormWritingSystemId, string lexemeForm,
						string definitionWritingSystemId, string definition,
						string exampleWritingSystemId, string example);

		/// <summary>
		/// this is useful for unit tests, to see if the app went where we asked
		/// </summary>
		/// <returns></returns>
		[OperationContract]
		string GetCurrentUrl();

		[OperationContract]
		void ShowUIWithUrl(string url);

		/// <summary>
		/// mostly for unit testing
		/// </summary>
		[OperationContract]
		bool IsInServerMode();


//todo        void AddInflectionalVariant(string writingSystemId, string variant);

	}

	public enum FindMethods
	{
		Exact,
		DefaultApproximate
	}
}