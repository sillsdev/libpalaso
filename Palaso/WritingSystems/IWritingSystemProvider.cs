using System.Collections.Generic;

namespace Palaso.WritingSystems
{
	/// <summary>
	/// this exists largely because in .net, the information we need is available
	/// under the system.windows.forms namespace,which this library intentionally
	/// avoids.
	/// </summary>
	public interface IWritingSystemProvider
	{
		IEnumerable<WritingSystemDefinition> ActiveOSLanguages
		{
			get;
		}
	}
}