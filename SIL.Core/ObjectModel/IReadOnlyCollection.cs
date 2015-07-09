using System.Collections.Generic;

namespace SIL.ObjectModel
{
	public interface IReadOnlyCollection<out T> : IEnumerable<T>
	{
		int Count { get; }
	}
}
