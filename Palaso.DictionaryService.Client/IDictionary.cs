using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace Palaso.DictionaryService.Client
{
	[ServiceContract]
	public interface IDictionaryService
	{
		[OperationContract]
		string[] GetIdsOfMatchingEntries(string writingSystemId, string form, FindMethods method);

		[OperationContract]
		string GetHmtlForEntry(string entryId);

		[OperationContract]
		void RegisterClient(int clientProcessId);

		[OperationContract]
		void DeregisterClient(int clientProcessId);

		[OperationContract]
		void JumpToEntry(string entryId);

		/// <summary>
		/// mostly for unit testing
		/// </summary>
		[OperationContract]
		bool IsInServerMode();
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