// Copyright (c) 2025 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using NUnit.Framework;
using SIL.Reporting;

namespace SIL.Tests
{
	/// <summary>
	/// Class with test initialization code that is valid for all tests
	/// </summary>
	[SetUpFixture]
	public class SetupFixture
	{
		[OneTimeSetUp]
		public void RunBeforeAnyTests()
		{
			ErrorReport.IsOkToInteractWithUser = false;
		}
	}
}

