using CookComputing.XmlRpc;

namespace Palaso.Services.Dictionary
{
	public struct FindResult
	{
		public string[] ids ;
		public string[] forms;
	}

	public interface IDictionaryServiceBase
	{
		/// <summary>
		/// Search the dictionary for an ordered list of entries that may be what the user is looking for.
		/// </summary>
		/// <param name="writingSystemId"></param>
		/// <param name="form">The form to search on.  May be used to match on lexeme form, citation form, variants, etc.,
		/// depending on how the implementing dictionary services application.</param>
		/// <param name="findMethod">Controls how matching should happen</param>
		[XmlRpcMethod("Dictionary.GetMatchingEntries", Description = "Search the dictionary for an ordered list of entries that may be what the user is looking for.")]
		FindResult GetMatchingEntries(string writingSystemId, string form, string findMethod);

		/// <summary>
		/// Get an HTML representation of one or more entries.
		/// </summary>
		/// <remarks>I had originally planned this to just be a fragment, which is more composable,
		/// but that then left us with the problem of where to get the style definitions, if we
		/// don't have an html header.</remarks>
		/// <param name="entryIds"></param>
		/// <returns></returns>
		[XmlRpcMethod("Dictionary.GetHtmlForEntries", Description = "Get an HTML representation of one or more entries.")]
		string GetHtmlForEntries(string[] entryIds);

		/// <summary>
		/// Used to help the dictionary service app know when to quit
		/// </summary>
		/// <param name="clientProcessId"></param>
		[XmlRpcMethod("Dictionary.RegisterClient", Description = "Used to help the dictionary service app know when to quit")]
		void RegisterClient(int clientProcessId);

		/// <summary>
		/// Used to help the dictionary service app know when to quit
		/// </summary>
		/// <param name="clientProcessId"></param>
		[XmlRpcMethod("Dictionary.DeregisterClient", Description = "Used to help the dictionary service app know when to quit")]
		void DeregisterClient(int clientProcessId);

		/// <summary>
		/// Cause a gui application to come to the front, focussed on this entry, read to edit
		/// </summary>
		/// <param name="entryId"></param>
		[XmlRpcMethod("Dictionary.JumpToEntry", Description = "Cause a gui application to come to the front, focussed on this entry, read to edit")]
		void JumpToEntry(string entryId);

		/// <summary>
		/// Add a new entry to the lexicon
		/// </summary>
		/// <returns>the id that was assigned to the new entry</returns>
		[XmlRpcMethod("Dictionary.AddEntry", Description = "Add a new entry to the lexicon")]
		string AddEntry(string lexemeFormWritingSystemId, string lexemeForm,
						string definitionWritingSystemId, string definition,
						string exampleWritingSystemId, string example);

		/// <summary>
		/// this is useful for unit tests, to see if the app went where we asked
		/// </summary>
		/// <returns></returns>
		[XmlRpcMethod("Dictionary.GetCurrentUrl", Description = "this is useful for unit tests, to see if the app went where we asked")]
		string GetCurrentUrl();

		[XmlRpcMethod("Dictionary.ShowUIWithUrl", Description = "")]
		void ShowUIWithUrl(string url);

		/// <summary>
		/// mostly for unit testing
		/// </summary>
		[XmlRpcMethod("Dictionary.IsInServerMode", Description = "")]
		bool IsInServerMode();

		//todo        void AddInflectionalVariant(string writingSystemId, string variant);
	}

	public interface IDictionaryService : IDictionaryServiceBase, IXmlRpcProxy
	{
	}

	public enum FindMethods
	{
		Exact,
		DefaultApproximate
	}
}