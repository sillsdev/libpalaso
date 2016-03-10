using System.Collections.Specialized;
using System.ComponentModel;

namespace SIL.ObjectModel
{
	public class ReadOnlyObservableList<T> : ReadOnlyList<T>, IReadOnlyObservableList<T>, IObservableList<T>
	{
		public virtual event NotifyCollectionChangedEventHandler CollectionChanged;
		event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
		{
			add { PropertyChanged += value; }
			remove { PropertyChanged -= value; }
		}

		protected virtual event PropertyChangedEventHandler PropertyChanged;

		public ReadOnlyObservableList(IObservableList<T> list)
			: base(list)
		{
			list.CollectionChanged += list_CollectionChanged;
			list.PropertyChanged += list_PropertyChanged;
		}

		private void list_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			OnPropertyChanged(e);
		}

		private void list_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			OnCollectionChanged(e);
		}

		protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			NotifyCollectionChangedEventHandler handler = CollectionChanged;
			if (handler != null)
				handler(this, e);
		}

		protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null)
				handler(this, e);
		}
	}
}
