using System;
using System.Collections.Generic;
using Palaso.Data;

namespace Palaso.Data
{

	public class DelegateQuery<T> : IQuery<T> where T: class, new()
	{
		public delegate IEnumerable<IDictionary<string, object>> DelegateMethod(T item);
		DelegateMethod _columnFilter;
		private IEnumerable<SortDefinition> _sortorder;
		private string _uniqueLabel;
		private Predicate<IDictionary<string, object>> _isUnpopulated;

		public DelegateQuery(DelegateMethod columnFilter, IEnumerable<SortDefinition> sortorder, string uniqueLabel, Predicate<IDictionary<string, object>> isUnpopulated)
		{
			_columnFilter = columnFilter;
			_sortorder = sortorder;
			_uniqueLabel = uniqueLabel;
			_isUnpopulated = isUnpopulated;
		}

		#region IQuery<T> Members

		public override IEnumerable<IDictionary<string, object>> GetResults(T item)
		{
			return _columnFilter(item);
		}

		public override IEnumerable<SortDefinition> SortDefinitions
		{
			get { return _sortorder; }
		}

		public override string UniqueLabel
		{
			get { return _uniqueLabel; }
		}

		public override bool IsUnpopulated(IDictionary<string, object> entryToCheckAgainst)
		{
			return _isUnpopulated(entryToCheckAgainst);
		}

		#endregion
	}
}
