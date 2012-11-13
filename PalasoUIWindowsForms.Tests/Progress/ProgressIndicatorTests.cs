using NUnit.Framework;
using Palaso.Progress;
using Palaso.UI.WindowsForms.Progress;

namespace PalasoUIWindowsForms.Tests.Progress
{
	[TestFixture]
	public class MultiPhaseProgressIndicatorTests
	{
		[Test]
		public void FivePhase_ProgressesNormally()
		{
			var globalIndicator = new SimpleProgressIndicator();
			var progress = new MultiPhaseProgressIndicator(globalIndicator
				, 5);
			progress.Initialize(); // phase 1
			Assert.That(globalIndicator.Value, Is.EqualTo(0));
			progress.PercentCompleted = 50;
			Assert.That(globalIndicator.Value, Is.EqualTo(10));
			progress.Finish();
			Assert.That(globalIndicator.PercentCompleted, Is.EqualTo(20));

			progress.Initialize(); // phase 2
			Assert.That(progress.PercentCompleted, Is.EqualTo(0));
			Assert.That(globalIndicator.PercentCompleted, Is.EqualTo(20));
			progress.PercentCompleted = 50;
			Assert.That(globalIndicator.PercentCompleted, Is.EqualTo(30));
			progress.Finish();
			Assert.That(progress.PercentCompleted, Is.EqualTo(100));
			Assert.That(globalIndicator.PercentCompleted, Is.EqualTo(40));

			progress.Initialize(); // phase 3
			progress.Finish();
			Assert.That(globalIndicator.PercentCompleted, Is.EqualTo(60));

			progress.Initialize(); // phase 4
			progress.PercentCompleted = 10;
			Assert.That(globalIndicator.PercentCompleted, Is.EqualTo(62));
			progress.Finish();

			progress.Initialize(); // phase 5
			progress.Finish();
		}
	}
}
