using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Palaso.Data;
using Palaso.DictionaryServices.Model;
using Palaso.WritingSystems;

namespace Palaso.DictionaryServices.Queries
{
	class LexicalFormQuery:IQuery<LexEntry>
	{
		private WritingSystemDefinition _writingSystemDefinition;

		public LexicalFormQuery(WritingSystemDefinition wsDef)
		{
			_writingSystemDefinition = wsDef;
		}

		public IEnumerable<IDictionary<string, object>> GetResults(LexEntry entryToQuery)
		{
			var tokenFieldsAndValues = new Dictionary<string, object>();
			string headWord = entryToQuery.LexicalForm[_writingSystemDefinition.Id];
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

		public string Label
		{
			get { return "LexicalFormQuery"; }
		}
	}
}
