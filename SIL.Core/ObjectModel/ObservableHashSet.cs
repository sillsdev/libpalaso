using System.Collections.Generic;

namespace SIL.ObjectModel
{
	public class ObservableHashSet<T> : ObservableISet<T>
	{
		public ObservableHashSet() : base(new HashSet<T>())
		{}

		public ObservableHashSet(IEqualityComparer<T> comparer) : base(new HashSet<T>(comparer))
		{}

		public ObservableHashSet(IEnumerable<T> items) : base(new HashSet<T>(items))
		{}

		public ObservableHashSet(IEnumerable<T> items, IEqualityComparer<T> comparer) : base(new HashSet<T>(items, comparer))
		{}

		protected override IEqualityComparer<T> Comparer => Items.Comparer;

		protected HashSet<T> Items => (HashSet<T>) _set;
	}
}
