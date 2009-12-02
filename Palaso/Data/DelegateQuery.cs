using System.Collections.Generic;
using Palaso.Data;

namespace Palaso.Data
{

	public class DelegateQuery<T> : IQuery<T> where T: class, new()
	{
		public delegate IEnumerable<IDictionary<string, object>> DelegateMethod<T>(T item) where T : class, new();
		DelegateMethod<T> _method;

		public DelegateQuery(DelegateMethod<T> method)
		{
			_method = method;
		}

		#region IQuery<T> Members

		public IEnumerable<IDictionary<string, object>> GetResults(T item)
		{
			return _method(item);
		}

		#endregion
	}
}
