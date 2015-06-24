using System;
using SIL.Data;

namespace SIL.Lift
{

	public interface ILiftReader<T> : IDisposable where T : class, new()
	{
		/// <summary>
		/// Subscribe to this event in order to do something (or do something to an entry) as soon as it has been parsed in.
		/// WeSay uses this to populate definitions from glosses.
		/// </summary>
		event EventHandler AfterEntryRead;

		void Read(string filePath, MemoryDataMapper<T> backend);
	}
}