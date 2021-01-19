// Copyright (c) 2021 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.Diagnostics;
using NUnit.Framework;
using SIL.PlatformUtilities;

namespace SIL.Extensions
{
	[TestFixture]
	public class ProcessExtensionsTests
	{
		[Test]
		public void RunProcess_NoError()
		{
			// Setup
			using var process = new Process();
			var errorTriggered = false;

			// Execute/Verify
			Assert.That(() => process.RunProcess("find", "blah", exception => {
				errorTriggered = true;
			}), Throws.Nothing);
			Assert.That(errorTriggered, Is.False);
		}

		[Test]
		public void RunProcess_Error()
		{
			// Setup
			using var process = new Process();
			var errorTriggered = false;

			// Execute/Verify
			Assert.That(() => process.RunProcess("NonExistingProgram_8473464", null, exception => {
				errorTriggered = true;
			}), Throws.Nothing);
			Assert.That(errorTriggered, Is.True);
		}

		[Test]
		public void RunProcess_NullErrorHandler()
		{
			// Setup
			using var process = new Process();

			// Execute/Verify
			Assert.That(() => process.RunProcess("NonExistingProgram_8473464", "", null),
				Throws.Nothing);
		}
	}
}
