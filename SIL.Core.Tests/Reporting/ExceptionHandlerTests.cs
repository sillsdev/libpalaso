// Copyright (c) 2025 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using NUnit.Framework;
using SIL.Reporting;
// ReSharper disable ConvertClosureToMethodGroup

namespace SIL.Tests
{
	[TestFixture]
	public class ExceptionHandlerTests
	{
		private class TestExceptionHandler : ExceptionHandler
		{
			public static Exception ReceivedException { get; set; }

			protected override bool ShowUI => false;
			protected override bool DisplayError(Exception exception)
			{
				ReceivedException = exception;
				return true;
			}
		}

		private class ExceptionHandlerHelper : ExceptionHandler
		{
			public static void ReportException(Exception exception)
			{
				HandleUnhandledExceptionOnSingleton(new UnhandledExceptionEventArgs(exception, false));
			}

			public static void Reset()
			{
				ResetSingleton();
			}

			protected override bool ShowUI => false;
			protected override bool DisplayError(Exception exception)
			{
				return false;
			}
		}

		[TearDown]
		public void TearDown()
		{
			ExceptionHandlerHelper.Reset();
		}

		[Test]
		public void Init()
		{
			// Execute
			ExceptionHandler.Init(new TestExceptionHandler());

			// Verify
			var exception = new ArgumentOutOfRangeException();
			ExceptionHandlerHelper.ReportException(exception);
			Assert.That(TestExceptionHandler.ReceivedException, Is.EqualTo(exception));
		}

		[Test]
		public void Init_ThrowsIfAlreadyInitialized()
		{
			// Setup
			ExceptionHandler.Init(new ConsoleExceptionHandler());

			// Execute
			Assert.That(() => ExceptionHandler.Init(new TestExceptionHandler()),
				Throws.InvalidOperationException.With.Message.Contains(nameof(ConsoleExceptionHandler)));
		}

		[Test]
		public void TypeOfExistingHandler_NotInitialized_ReturnsNull()
		{
			Assert.That(ExceptionHandler.TypeOfExistingHandler, Is.Null);
		}

		[Test]
		public void TypeOfExistingHandler_Initialized_ReturnsTypeOfHandler()
		{
			// Setup
			ExceptionHandler.Init(new TestExceptionHandler());

			// Execute
			Assert.That(ExceptionHandler.TypeOfExistingHandler,
				Is.EqualTo(typeof(TestExceptionHandler)));
		}
	}
}
