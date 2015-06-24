using System.Collections.Generic;

namespace SIL.ObjectModel
{
	public interface IObservableDictionary<TKey, TValue> : IObservableCollection<KeyValuePair<TKey, TValue>>, IDictionary<TKey, TValue>
	{
	}
}
