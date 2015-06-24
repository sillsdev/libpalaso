using System.Collections.Generic;

namespace SIL.ObjectModel
{
	public class ReadOnlyList<T> : System.Collections.ObjectModel.ReadOnlyCollection<T>, IReadOnlyList<T>
	{
		public ReadOnlyList(IList<T> list)
			: base(list)
		{
		}
	}
}
