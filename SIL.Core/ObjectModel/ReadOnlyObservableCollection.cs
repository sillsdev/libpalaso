using System.Collections.Specialized;
using System.ComponentModel;

namespace SIL.ObjectModel
{
	public class ReadOnlyObservableCollection<T> : ReadOnlyCollection<T>, IReadOnlyObservableCollection<T>
	{
		public virtual event NotifyCollectionChangedEventHandler CollectionChanged;
		event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
		{
			add { PropertyChanged += value; }
			remove { PropertyChanged -= value; }
		}

		protected virtual event PropertyChangedEventHandler PropertyChanged;

		public ReadOnlyObservableCollection(IObservableCollection<T> collection)
			: base(collection)
		{
			collection.CollectionChanged += collection_CollectionChanged;
			collection.PropertyChanged += collection_PropertyChanged;
		}

		private void collection_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			OnPropertyChanged(e);
		}

		private void collection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			OnCollectionChanged(e);
		}

		protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			if (CollectionChanged != null)
				CollectionChanged(this, e);
		}

		protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, e);
		}
	}
}
