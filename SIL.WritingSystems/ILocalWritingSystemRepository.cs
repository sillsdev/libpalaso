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
		/// Returns copies of all of the global writing systems that are newer than their corresponding local writing systems. These
		/// writing systems can be used to replace the existing local writing systems.
		/// </summary>
		/// <param name="languageTags">Only check these language tags</param>
		IEnumerable<WritingSystemDefinition> CheckForNewerGlobalWritingSystems(IEnumerable<string> languageTags = null);

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
		new IEnumerable<T> CheckForNewerGlobalWritingSystems(IEnumerable<string> languageTags = null);
		new IWritingSystemRepository<T> GlobalWritingSystemRepository { get; } 
	}
}
