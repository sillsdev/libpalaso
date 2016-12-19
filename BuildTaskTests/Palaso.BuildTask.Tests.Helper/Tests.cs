// Copyright (c) 2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using NUnit.Framework;

namespace Palaso.BuildTask.Tests.Helper
{
	/// <summary>
	/// This class defines some tests used for testing the NUnitTask class
	/// </summary>
	[TestFixture]
	public class Tests
	{
		[Test]
		[Category("Success")]
		public void Success()
		{
			Assert.True(true, "This test always passes");
		}

		[Test]
		[Category("Failing")]
		public void Failing()
		{
			Assert.Fail("This test intentionally fails");
		}

		[Test]
		[Category("Exception")]
		public void Exception()
		{
			throw new ApplicationException("This test throws an exception");
		}

		#region ThrowsExceptionInFinalizer
		/// <summary>
		/// This class simulates a class that throws an exception when the finalizer runs
		/// </summary>
		class ThrowsExceptionInFinalizer
		{
			~ThrowsExceptionInFinalizer()
			{
				throw new ApplicationException();
			}

			public bool DoSomething()
			{
				// just some arbitrary method so that we can instantiate the class
				return true;
			}
		}
		#endregion

		[Test]
		[Category("Finalizer")]
		public void Finalizer()
		{
			Assert.That(new ThrowsExceptionInFinalizer().DoSomething(), Is.True);
			// will throw an exception sometime later when GC runs
		}
	}
}
