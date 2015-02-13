using System.Collections.Generic;

namespace Palaso.ObjectModel
{
	public interface IObservableDictionary<TKey, TValue> : IObservableCollection<KeyValuePair<TKey, TValue>>, IDictionary<TKey, TValue>
	{
	}
}
