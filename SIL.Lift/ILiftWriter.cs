using System;

namespace SIL.Lift
{
	public interface ILiftWriter<T> : IDisposable where T : class, new()
	{
		void Add(T item);
		void AddNewEntry(T item);
		void AddDeletedEntry(T item);

		void End();
	}
}