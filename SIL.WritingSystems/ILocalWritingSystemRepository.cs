using System.Collections.Generic;

namespace SIL.WritingSystems
{
	/// <summary>
	/// A local writing system repository is a repository that updates a global
	/// repository whenever it is saved.
	/// </summary>
	public interface ILocalWritingSystemRepository : IWritingSystemRepository
	{
		/// <summary>
		/// Gets all newer shared writing systems.
		/// </summary>
		IEnumerable<WritingSystemDefinition> CheckForNewerGlobalWritingSystems();
	}
}
