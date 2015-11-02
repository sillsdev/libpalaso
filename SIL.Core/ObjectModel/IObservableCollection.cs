using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace SIL.ObjectModel
{
	public interface IObservableCollection<T> : INotifyCollectionChanged, INotifyPropertyChanged, ICollection<T>
	{
	}
}
