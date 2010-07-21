using System;
using System.Collections.Generic;
using Palaso.Data;

namespace Palaso.Data
{

	public class DelegateQuery<T> : IQuery<T> where T: class, new()
	{
		public delegate IEnumerable<IDictionary<string, object>> DelegateMethod(T item);
		DelegateMethod _method;
		private SortDefinition[] _sortorder;
		private string _uniqueLabel;

		public DelegateQuery(DelegateMethod method, SortDefinition[] sortorder, string uniqueLabel)
		{
			_method = method;
			_sortorder = sortorder;
			_uniqueLabel = uniqueLabel;
		}

		#region IQuery<T> Members

		public IEnumerable<IDictionary<string, object>> GetResults(T item)
		{
			return _method(item);
		}

		public SortDefinition[] SortDefinitions
		{
			get { return _sortorder; }
		}

		public string UniqueLabel
		{
			get { return _uniqueLabel; }
		}

		#endregion
	}
}
