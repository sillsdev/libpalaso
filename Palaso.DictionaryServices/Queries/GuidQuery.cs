using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Palaso.Data;
using Palaso.DictionaryServices.Model;

namespace Palaso.DictionaryServices.Queries
{
	class GuidQuery:IQuery<LexEntry>
	{
		private Comparer<Guid> _comparer;

		public GuidQuery(Comparer<Guid> guidComparer)
		{
			_comparer = guidComparer;
		}

		public GuidQuery()
		{
			_comparer = Comparer<Guid>.Default;
		}

		public override IEnumerable<IDictionary<string, object>> GetResults(LexEntry entryToQuery)
		{
			IDictionary<string, object> tokenFieldsAndValues = new Dictionary<string, object>();
			tokenFieldsAndValues.Add("Guid", entryToQuery.Guid);
			return new[] { tokenFieldsAndValues };
		}

		public override IEnumerable<SortDefinition> SortDefinitions
		{
			get
			{
				var sortOrder = new SortDefinition[1];
				sortOrder[0] = new SortDefinition("Guid", _comparer);
				return sortOrder;
			}
		}

		public override string UniqueLabel
		{
			get { return "GuidQuery"; }
		}

		public override bool IsUnpopulated(IDictionary<string, object> entryToCheckAgainst)
		{
			throw new NotImplementedException();
		}
	}
}
