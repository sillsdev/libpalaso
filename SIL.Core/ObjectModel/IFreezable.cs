namespace SIL.ObjectModel
{
	public interface IFreezable
	{
		bool IsFrozen { get; }
		void Freeze();
		int GetFrozenHashCode();
	}
}
