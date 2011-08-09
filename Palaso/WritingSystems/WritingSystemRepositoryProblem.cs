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

		public Exception Exception { get; set; }
		public string FilePath { get; set; }
		public ConsequenceType Consequence { get; set; }
	}
}
