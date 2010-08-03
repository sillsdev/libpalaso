using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Palaso.Data;
using Palaso.DictionaryServices.Model;

namespace Palaso.DictionaryServices.Queries
{
	class IdQuery:IQuery<LexEntry>
	{
		public override IEnumerable<IDictionary<string, object>> GetResults(LexEntry entryToQuery)
		{
			IDictionary<string, object> tokenFieldsAndValues = new Dictionary<string, object>();
			tokenFieldsAndValues.Add("Id", entryToQuery.Id);
			return new[] { tokenFieldsAndValues };
		}

		public override IEnumerable<SortDefinition> SortDefinitions
		{
			get
			{
				var sortOrder = new SortDefinition[1];
				sortOrder[0] = new SortDefinition("Id", Comparer<string>.Default);
				return sortOrder;
			}
		}

		public override string UniqueLabel
		{
			get { return "IdQuery"; }
		}

		public override bool IsUnpopulated(IDictionary<string, object> entryToCheckAgainst)
		{
			throw new NotImplementedException();
		}
	}
}
