using System;

namespace SIL.WritingSystems
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
