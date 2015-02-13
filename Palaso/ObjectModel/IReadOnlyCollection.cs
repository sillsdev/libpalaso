using System.Collections.Generic;

namespace Palaso.ObjectModel
{
	public interface IReadOnlyCollection<out T> : IEnumerable<T>
	{
		int Count { get; }
	}
}
