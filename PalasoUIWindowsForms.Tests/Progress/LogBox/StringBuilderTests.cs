using System;
using NUnit.Framework;
using Palaso.Progress.LogBox;

namespace Palaso.Tests.Progress.LogBoxTests
{
	[TestFixture]
	public class StringBuilderTests
	{
		private StringBuilderProgress _progress;

		[SetUp]
		public void Setup()
		{
			_progress = new StringBuilderProgress();
		}

		[TearDown]
		public void TearDown()
		{
		}

		[Test]
		public void StringBuilder_Simple()
		{
			_progress.WriteMessage ("Simple test");
			_progress.ShowVerbose = true;
			for (int i = 0; i < 1000; i++)
			{
				_progress.WriteVerbose(".");
			}
			_progress.WriteMessage("done");
			Console.WriteLine(_progress.Text);
		}
	}
}
