using System;

namespace SIL.Progress
{
	/// <summary>
	/// Use this when you don't have an actual progressstate installed and don't
	/// want to litter the code with if(_progressState != null)'s
	/// </summary>
	// Should be deprecated - [Obsolete("Use SIL.Progress.NullProgress instead.")]
	public class NullProgressState : ProgressState
	{
		public NullProgressState():base()
		{
		}


		/// <summary>
		/// How much the task is done
		/// </summary>
		public override int NumberOfStepsCompleted
		{
			get
			{
				return 0;
			}
			set
			{
			}
		}

		/// <summary>
		/// a label which describes what we are busy doing
		/// </summary>
		public override string StatusLabel
		{
			get
			{
				return "";
			}

			set
			{
			}
		}

		public override int TotalNumberOfSteps
		{
			get
			{
				return 0;
			}
			set
			{
			}
		}



	}
}