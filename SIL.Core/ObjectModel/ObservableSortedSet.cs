using System.Collections.Generic;

namespace SIL.ObjectModel
{
	public class ObservableSortedSet<T> : ObservableISet<T>
	{
		public ObservableSortedSet() : base(new SortedSet<T>())
		{}

		public ObservableSortedSet(IEnumerable<T> items) : base(new SortedSet<T>(items))
		{}

		protected override IEqualityComparer<T> Comparer => EqualityComparer<T>.Default;

		protected SortedSet<T> Items => (SortedSet<T>) _set;
	}
}
