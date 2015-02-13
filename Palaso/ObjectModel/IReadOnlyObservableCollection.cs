using System.Collections.Specialized;
using System.ComponentModel;

namespace Palaso.ObjectModel
{
	public interface IReadOnlyObservableCollection<out T> : INotifyCollectionChanged, INotifyPropertyChanged, IReadOnlyCollection<T>
	{
	}
}
