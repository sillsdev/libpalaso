using System;
using Palaso.Data;

namespace Palaso.Lift
{
	public interface ILiftReader<T> : IDisposable where T : class, new()
	{
		void Read(string filePath, MemoryDataMapper<T> backend);
	}
}