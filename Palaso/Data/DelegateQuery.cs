using System;
using System.Collections.Generic;
using Palaso.Data;

namespace Palaso.Data
{

	public class DelegateQuery<T> : IQuery<T> where T: class, new()
	{
		public delegate IEnumerable<IDictionary<string, object>> DelegateMethod(T item);
		DelegateMethod _method;
		private IEnumerable<SortDefinition> _sortorder;
		private string _uniqueLabel;

		public DelegateQuery(DelegateMethod method, IEnumerable<SortDefinition> sortorder, string uniqueLabel)
		{
			_method = method;
			_sortorder = sortorder;
			_uniqueLabel = uniqueLabel;
		}

		#region IQuery<T> Members

		public override IEnumerable<IDictionary<string, object>> GetResults(T item)
		{
			return _method(item);
		}

		public override IEnumerable<SortDefinition> SortDefinitions
		{
			get { return _sortorder; }
		}

		public override string UniqueLabel
		{
			get { return _uniqueLabel; }
		}

		#endregion
	}
}
