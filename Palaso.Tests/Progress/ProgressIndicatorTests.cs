using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Palaso.Progress;

namespace Palaso.Tests.Progress
{
	[TestFixture]
	public class SimpleProgressIndicatorTests
	{
		[Test]
		public void NextStep_Zero_GoesToOne()
		{
			var progress = new SimpleProgressIndicator();
			progress.Initialize(10);
			progress.NextStep();
			Assert.That(progress.Value, Is.EqualTo(1));
		}

		[Test]
		public void PercentCompleted_ThreeNextStep_ThreePercent()
		{
			var progress = new SimpleProgressIndicator();
			progress.Initialize(10);
			progress.NextStep();
			progress.NextStep();
			progress.NextStep();
			Assert.That(progress.PercentCompleted, Is.EqualTo(30));
		}
	}

	[TestFixture]
	public class MultiPhaseProgressIndicatorTests
	{
		[Test]
		public void ThreePhase_ProgressesNormally()
		{
			var globalIndicator = new SimpleProgressIndicator();
			var progress = new MultiPhaseProgressIndicator(globalIndicator, 4);
			progress.Initialize(4);
			Assert.That(globalIndicator.Value, Is.EqualTo(0));
			progress.NextStep();
			progress.NextStep();
			progress.NextStep();
			Assert.That(progress.NumberOfStepsCompleted, Is.EqualTo(3));
			progress.Finish();
			Assert.That(globalIndicator.PercentCompleted, Is.EqualTo(25));
			Assert.That(progress.NumberOfStepsCompleted, Is.EqualTo(4));
			progress.Initialize(10);
			Assert.That(progress.NumberOfStepsCompleted, Is.EqualTo(0));
			progress.NextStep();
			Assert.That(progress.NumberOfStepsCompleted, Is.EqualTo(1));
			progress.Finish();
			Assert.That(progress.NumberOfStepsCompleted, Is.EqualTo(10));
			Assert.That(globalIndicator.PercentCompleted, Is.EqualTo(50));
			progress.Initialize(5);
			progress.NumberOfStepsCompleted = 4;
			progress.NextStep();
			progress.Finish();
			Assert.That(globalIndicator.PercentCompleted, Is.EqualTo(75));
		}
	}
}
