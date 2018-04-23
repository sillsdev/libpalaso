using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SIL.ObjectModel
{
	public class ObservableList<T> : ObservableCollection<T>, IObservableList<T>, IReadOnlyObservableList<T>
	{
		public ObservableList()
		{
		}

		public ObservableList(IEnumerable<T> items)
		{
			foreach (T item in items)
				Add(item);
		}
	}
}
