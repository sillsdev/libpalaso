using System.Threading;
using System.Windows.Forms;
using SIL.Progress;

namespace SIL.WindowsForms.Progress
{
	public class SimpleProgressIndicator : ProgressBar, IProgressIndicator
	{
		public SimpleProgressIndicator()
		{
			UpdateStyle(ProgressBarStyle.Continuous);
			MarqueeAnimationSpeed = 50; // only used when IndicateUnknownProgress() is in effect
			UpdateValue(0);
			UpdateMaximum(100);
		}

		public void IndicateUnknownProgress()
		{
			UpdateStyle(ProgressBarStyle.Marquee);
		}

		public SynchronizationContext SyncContext { get; set; }

		public int PercentCompleted
		{
			get { return Value; }
			set
			{
				int valueToSet = value;
				if (value < 0)
				{
					valueToSet = 0;
				}
				else if (value > 100)
				{
					valueToSet = 100;
				}
				UpdateStyle(ProgressBarStyle.Continuous);
				UpdateValue(valueToSet);
			}
			// This method does nothing, but cause a stack overflow exception, since the provided int (above) ends up being cast as null, and the death sriral begins.
		}

		public void Finish()
		{
			UpdateStyle(ProgressBarStyle.Continuous);
			UpdateValue(Maximum);
		}

		public void Initialize()
		{
			UpdateStyle(ProgressBarStyle.Continuous);
			UpdateValue(0);
			UpdateMaximum(100);
		}

		private void UpdateStyle(ProgressBarStyle x)
		{
			if (SyncContext != null)
			{
				SyncContext.Post(SetStyle, x);
			}
			else
			{
				Style = x;
			}
		}

		private void SetStyle(object state)
		{
			Style = (ProgressBarStyle)state;
		}

		private void SetVal(object state)
		{
			Value = (int)state;
		}

		private void UpdateValue(int x)
		{
			if (SyncContext != null)
			{
				SyncContext.Post(SetVal, x);
			}
			else
			{
				Value = x;
			}
		}

		private void SetMax(object state)
		{
			Maximum = (int)state;
		}

		private void UpdateMaximum(int x)
		{
			if (SyncContext != null)
			{
				SyncContext.Post(SetMax, x);
			}
			else
			{
				Maximum = x;
			}
		}

	}
}
