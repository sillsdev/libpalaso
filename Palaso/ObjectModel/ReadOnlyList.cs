using System.Collections.Generic;

namespace Palaso.ObjectModel
{
	public class ReadOnlyList<T> : System.Collections.ObjectModel.ReadOnlyCollection<T>, IReadOnlyList<T>
	{
		public ReadOnlyList(IList<T> list)
			: base(list)
		{
		}
	}
}
