// Copyright (c) 2010-2025 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.Threading;

namespace SIL.Progress
{
	public class NullProgressIndicator : IProgressIndicator
	{
		public int PercentCompleted { get; set; }

		public void Finish()
		{
		}

		public void Initialize()
		{
		}

		public void IndicateUnknownProgress()
		{
		}

		public SynchronizationContext SyncContext { get; set; }
	}
}