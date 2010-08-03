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

		public void Add(T t)
		{
			bool listIsEmpty = (Count == 0);
			if (listIsEmpty)
			{
				base.Add(t);
				return;
			}
			bool itemToInsertIsGreaterThanLastitemInList =
				(_comparer.Compare(t, base[Count - 1]) > 0);
			if (itemToInsertIsGreaterThanLastitemInList)
			{
				base.Add(t);
				return;
			}
			int index = BinarySearch(t, _comparer);
			bool identicalItemAlreadyExistsInList = (index >= 0);
			if (identicalItemAlreadyExistsInList)
			{
				Insert(index + 1 , t);
			}
			else
			{
				int ind = ~index;
				Insert(ind, t);
			}
		}

		public bool Contains(T t)
		{
			if(BinarySearch(t, _comparer)>=0)
			{
				return true;
			}
			return false;
		}
	}
}
