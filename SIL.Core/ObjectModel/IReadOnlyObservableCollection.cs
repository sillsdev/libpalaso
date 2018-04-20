using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace SIL.ObjectModel
{
	public interface IReadOnlyObservableCollection<out T> : INotifyCollectionChanged, INotifyPropertyChanged, IReadOnlyCollection<T>
	{
	}
}
