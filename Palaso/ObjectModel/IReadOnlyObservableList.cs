namespace Palaso.ObjectModel
{
	public interface IReadOnlyObservableList<out T> : IReadOnlyList<T>, IReadOnlyObservableCollection<T>
	{
	}
}
