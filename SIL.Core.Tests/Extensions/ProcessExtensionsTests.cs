// Copyright (c) 2025 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.Diagnostics;
using NUnit.Framework;

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
			// UseShellExecute defaults to true in .net framework (including .net 4) and to false in .net core (including .net 8)
			// We are explicitly setting it to true for consistency with the old behavior
			// but have not checked if it is necessary here.
			process.StartInfo.UseShellExecute = true;
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
			// UseShellExecute defaults to true in .net framework (including .net 4) and to false in .net core (including .net 8)
			// We are explicitly setting it to true for consistency with the old behavior
			// but have not checked if it is necessary here.
			process.StartInfo.UseShellExecute = true;
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
			// UseShellExecute defaults to true in .net framework (including .net 4) and to false in .net core (including .net 8)
			// We are explicitly setting it to true for consistency with the old behavior
			// but have not checked if it is necessary here.
			process.StartInfo.UseShellExecute = true;

			// Execute/Verify
			Assert.That(() => process.RunProcess("NonExistingProgram_8473464", "", null),
				Throws.Nothing);
		}
	}
}
