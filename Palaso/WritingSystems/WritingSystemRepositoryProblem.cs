using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Palaso.WritingSystems
{
	public class WritingSystemRepositoryProblem
	{
		public enum ConsequenceType
		{
			WSWillNotBeAvailable
		}

		public Exception Exception { get; private set; }
		public string WritingSystemTag { get; private set; }
		public string FilePath { get; private set; }
		public ConsequenceType Consequence { get; private set; }
	}
}
