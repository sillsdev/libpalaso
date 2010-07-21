using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Palaso.Data;
using Palaso.DictionaryServices.Model;
using Palaso.WritingSystems;

namespace Palaso.DictionaryServices.Queries
{
	class HeadwordQuery:IQuery<LexEntry>
	{
		private WritingSystemDefinition _writingSystemDefinition;

		public HeadwordQuery(WritingSystemDefinition writingSystemDefinition)
		{
			_writingSystemDefinition = writingSystemDefinition;
		}

		public IEnumerable<IDictionary<string, object>> GetResults(LexEntry entryToQuery)
		{
			IDictionary<string, object> tokenFieldsAndValues = new Dictionary<string, object>();
			string headWord = entryToQuery.VirtualHeadWord[_writingSystemDefinition.Id];
			if (String.IsNullOrEmpty(headWord))
			{
				headWord = null;
			}
			tokenFieldsAndValues.Add("Form", headWord);
			return new[] { tokenFieldsAndValues };
		}

		public SortDefinition[] SortDefinitions
		{
			get
			{
				var sortOrder = new SortDefinition[1];
				sortOrder[0] = new SortDefinition("Form", _writingSystemDefinition.Collator);
				return sortOrder;
			}
		}

		public string UniqueLabel
		{
			get { return "HeadwordQuery" + _writingSystemDefinition.Id; }
		}
	}
}
