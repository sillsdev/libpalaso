// Copyright (c) 2015 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using NUnit.Framework;
using Palaso.Reporting;

namespace Palaso.Tests
{
	/// <summary>
	/// Class with test initialization code that is valid for all tests
	/// </summary>
	[SetUpFixture]
	public class SetupFixture
	{
		[SetUp]
		public void RunBeforeAnyTests()
		{
			ErrorReport.IsOkToInteractWithUser = false;
		}
	}
}

