using System;

namespace Palaso.Progress
{
	public class ConsoleProgress : ProgressState
	{
		public override string StatusLabel
		{
			set
			{
				Console.WriteLine(value);
				base.StatusLabel = value;
			}
		}
		public override int NumberOfStepsCompleted
		{
			set
			{
				Console.Write('.');
				base.NumberOfStepsCompleted = value;
			}
		}
	}
}