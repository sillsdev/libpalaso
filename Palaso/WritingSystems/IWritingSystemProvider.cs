using System.Collections.Generic;

namespace Palaso.WritingSystems
{
	public interface IWritingSystemProvider
	{
		IEnumerable<Palaso.WritingSystems.WritingSystemDefinition> ActiveOSLanguages();
	}
}