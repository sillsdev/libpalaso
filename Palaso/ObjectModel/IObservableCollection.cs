using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Palaso.ObjectModel
{
	public interface IObservableCollection<T> : INotifyCollectionChanged, INotifyPropertyChanged, ICollection<T>
	{
	}
}
