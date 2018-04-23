using System.Collections.Generic;

namespace SIL.ObjectModel
{
	public interface IReadOnlyObservableList<out T> : IReadOnlyList<T>, IReadOnlyObservableCollection<T>
	{
	}
}
