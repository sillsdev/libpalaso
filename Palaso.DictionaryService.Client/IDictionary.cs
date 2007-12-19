using System;
using System.Collections.Generic;

namespace Palaso.DictionaryService.Client
{
	public interface IDictionary : IDisposable
	{
		bool CanAddEntries { get; }

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