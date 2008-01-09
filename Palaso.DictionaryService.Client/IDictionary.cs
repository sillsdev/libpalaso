using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace Palaso.DictionaryService.Client
{
	[ServiceContract]
	public interface IDictionaryService
	{
		[OperationContract]
		void GetMatchingEntries(string writingSystemId, string form, FindMethods method, out string[] ids, out string[] forms);

		[OperationContract]
		string GetHmtlForEntry(string entryId);

		[OperationContract]
		void RegisterClient(int clientProcessId);

		[OperationContract]
		void DeregisterClient(int clientProcessId);

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

		/// <summary>
		/// Given an array of ids, get an array of forms to show
		/// </summary>
		/// <param name="writingSytemId">The writing system you want the form in</param>
		/// <param name="ids"></param>
		/// <returns></returns>
		[OperationContract]
		string[] GetFormsFromIds(string writingSytemId, string[] ids);
	}

	public interface IDictionary : IDisposable
	{
		bool CanAddEntries();

		IEntry CreateEntryLocally();

		void AddEntry(IEntry entry);

		void Dispose();

		IList<IEntry> FindEntries(string writingSystemId, string form, FindMethods method);
	}
	public enum FindMethods
	{
		Exact,
		DefaultApproximate
	}
	public enum ArticleCompositionFlags
	{
		Simple = 3,
		Definition = 1,
		Example = 2,
		Synonyms = 4,
		Antonyms = 8,
		RelatedByDomain = 16,
		Everything = 255
	} ;
}