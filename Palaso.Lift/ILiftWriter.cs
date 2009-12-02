using System;

namespace Palaso.Lift
{
	public interface ILiftWriter<T> : IDisposable where T : class, new()
	{
		void Add(T item);
		void AddNewEntry(T item);
		void AddDeletedEntry(T item);

		void End();
	}
}