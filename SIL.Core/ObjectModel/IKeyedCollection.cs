using System.Collections.Generic;

namespace SIL.ObjectModel
{
	public interface IKeyedCollection<in TKey, TItem> : ICollection<TItem>
	{
		TItem this[TKey key] { get; }
		bool TryGet(TKey key, out TItem item);
		bool Contains(TKey key);
		bool Remove(TKey key);
	}
}
