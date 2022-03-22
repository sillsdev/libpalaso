// Copyright (c) 2022 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.Collections.Generic;
using NUnit.Framework;
using SIL.Windows.Forms.Keyboarding.Linux;

namespace SIL.Windows.Forms.Keyboarding.Tests
{
	[TestFixture]
	[Platform(Include="Linux", Reason="Linux specific tests")]
	public class KeyboardRetrievingHelperTests
	{
		[Test]
		public void ToFlatpakSpawn_Works()
		{
			string program = "/usr/bin/myprogram";
			string origProgram = program;
			string arguments = "my args here";
			string origArguments = arguments;
			// SUT
			KeyboardRetrievingHelper.ToFlatpakSpawn(ref program, ref arguments);
			Assert.That(program, Is.EqualTo("flatpak-spawn"));
			Assert.That(arguments, Is.EqualTo($"--host --directory=/ {origProgram} {origArguments}"));
		}
	}
}