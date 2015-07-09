using System.Collections.Generic;

namespace SIL.WritingSystems
{
	/// <summary>
	/// The local writing system repository interface. A local writing system repository is a repository
	/// that updates a global repository whenever it is saved.
	/// </summary>
	public interface ILocalWritingSystemRepository : IWritingSystemRepository
	{
		/// <summary>
		/// Gets all newer shared writing systems.
		/// </summary>
		IEnumerable<WritingSystemDefinition> CheckForNewerGlobalWritingSystems();

		/// <summary>
		/// Gets the global writing system repository.
		/// </summary>
		IWritingSystemRepository GlobalWritingSystemRepository { get; }
	}

	/// <summary>
	/// The generic local writing system repository interface.
	/// </summary>
	public interface ILocalWritingSystemRepository<T> : ILocalWritingSystemRepository, IWritingSystemRepository<T> where T : WritingSystemDefinition
	{
		new IEnumerable<T> CheckForNewerGlobalWritingSystems();
		new IWritingSystemRepository<T> GlobalWritingSystemRepository { get; } 
	}
}
