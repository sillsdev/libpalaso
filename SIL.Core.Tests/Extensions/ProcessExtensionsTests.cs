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
			var program = Platform.IsWindows ? "cmd" : "/bin/bash";
			var args = Platform.IsWindows ? "/c \"echo hello\"" : "-c \"echo hello\"";
			using var process = new Process();
			var processError = false;

			// Execute/Verify
			Assert.That(() => process.RunProcess(program, args, exception => {
				processError = true;
			}), Throws.Nothing);
			Assert.That(processError, Is.False);
		}

		[Test]
		public void RunProcess_Error()
		{
			// Setup
			using var process = new Process();
			var processError = false;

			// Execute/Verify
			Assert.That(() => process.RunProcess("NonExistingProgram_8473464", null, exception => {
				processError = true;
			}), Throws.Nothing);
			Assert.That(processError, Is.True);
		}
	}
}
