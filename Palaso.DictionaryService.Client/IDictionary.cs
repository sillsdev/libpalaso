using System;
using System.Collections.Generic;

namespace Palaso.DictionaryService.Client
{
	public interface IDictionary : IDisposable
	{
		bool CanAddEntries { get; }

		Entry CreateEntryLocally();
		void AddEntry(Entry entry);
		void Dispose();
		IList<Entry> FindEntries(string writingSystemId, string form, FindMethods method);
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