using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Palaso.Data
{
	public class SortedListAllowsDuplicates<T>:List<T>
	{
		private IComparer<T> _comparer;

		public SortedListAllowsDuplicates(IComparer<T> comparer)
		{
			_comparer = comparer;
		}

		public SortedListAllowsDuplicates()
		{
			_comparer = Comparer<T>.Default;
		}

		public void Add(T i)
		{
			bool listIsEmpty = (Count == 0);
			if (listIsEmpty)
			{
				base.Add(i);
				return;
			}
			bool itemToInsertIsGreaterThanLastitemInList =
				(_comparer.Compare(i, base[Count - 1]) > 0);
			if (itemToInsertIsGreaterThanLastitemInList)
			{
				base.Add(i);
				return;
			}
			int index = BinarySearch(i, _comparer);
			bool identicalItemAlreadyExistsInList = (index >= 0);
			if (identicalItemAlreadyExistsInList)
			{
				Insert(index + 1 , i);
			}
			else
			{
				int ind = ~index;
				Insert(ind, i);
			}
		}
	}
}
