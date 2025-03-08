using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using static System.Environment;

namespace SIL.Reporting.Tests
{
	[TestFixture]
	public class ConsoleErrorReporterTests
	{
		// Msg: (?<message>[^\r\n]*)
		private readonly Regex m_regexOutput = new Regex(
				@"^(?<datetime>.*? \d\d:\d\d:\d\d GMT:)\r?\n?Severity: (?<severity>\w*)\r?\n?" +
				@"(?<exceptionInfo>(.*\r?\n?)*?)\r?\n?--Error Reporting Properties--\r?\n?(?<properties>(.*\r?\n?)*)",
				RegexOptions.Compiled);
		private ConsoleErrorReporter m_reporter;
		private StringWriter m_consoleOutput;
		private TextWriter m_originalConsoleOutput;

		[SetUp]
		public void SetUp()
		{
			ErrorReport.IsOkToInteractWithUser = true;
			m_reporter = new ConsoleErrorReporter();
			m_consoleOutput = new StringWriter();
			m_originalConsoleOutput = Console.Out;
			Console.SetOut(m_consoleOutput);
		}

		[TearDown]
		public void TearDown()
		{
			ErrorReport.IsOkToInteractWithUser = true;
			Console.SetOut(m_originalConsoleOutput);
			m_consoleOutput.Dispose();
		}

		[Test]
		public void ReportFatalException_NoInnerException_WritesExceptionToConsole()
		{
			var exception = new Exception("Test exception");

			m_reporter.ReportFatalException(exception);

			var output = m_consoleOutput.ToString();

			var match = m_regexOutput.Match(output);
			Assert.That(match.Success, Is.True);
			Assert.That(match.Groups["severity"].Value, Is.EqualTo("Fatal"));
			Assert.That(match.Groups["exceptionInfo"].Value,
				Is.EqualTo(ExceptionHelper.GetExceptionText(exception)));
			Assert.That(match.Groups["properties"].Value.Trim(), Is.Empty);
		}

		[Test]
		public void NotifyUserOfProblem_AlwaysShow_WritesMessageToConsole()
		{
			var policy = new AlwaysShowPolicy();
			var message = "Test message";

			m_reporter.NotifyUserOfProblem(policy, null, message);
			m_reporter.NotifyUserOfProblem(policy, null, message);

			var output = m_consoleOutput.ToString().TrimEnd();
			Assert.That(output, Is.EqualTo(
				$"Test message{NewLine}{AlwaysShowPolicy.kReoccurrenceMessage}{NewLine}" +
					$"Test message{NewLine}{AlwaysShowPolicy.kReoccurrenceMessage}"));
		}

		[Test]
		public void NotifyUserOfProblem_NotOkayToInteract_Throws()
		{
			var policy = new AlwaysShowPolicy();
			var message = "Test message";

			ErrorReport.IsOkToInteractWithUser = false;

			Assert.That(() => m_reporter.NotifyUserOfProblem(policy, null, message),
				Throws.TypeOf<ErrorReport.ProblemNotificationSentToUserException>());

			var output = m_consoleOutput.ToString();
			Assert.That(output, Is.Empty);
		}

		[Test]
		public void NotifyUserOfProblem_ShowOnce_WritesMessageToConsole()
		{
			var policy = new ShowOncePolicy();
			var message1 = "Test message 1";
			var message2 = "Test message 2";

			m_reporter.NotifyUserOfProblem(policy, null, message1);
			m_reporter.NotifyUserOfProblem(policy, null, message1);
			m_reporter.NotifyUserOfProblem(policy, null, message2);
			m_reporter.NotifyUserOfProblem(policy, null, message1);

			var output = m_consoleOutput.ToString().TrimEnd();
			Assert.That(output,
				Is.EqualTo($"{message1}{NewLine}{ShowOncePolicy.kReoccurrenceMessage}{NewLine}" +
					$"{message2}{NewLine}{ShowOncePolicy.kReoccurrenceMessage}"));
		}

		[TestCase(true)]
		[TestCase(false)]
		public void NotifyUserOfProblem_WithResultIfAlternateButtonPressed_ReturnsOk(bool passOK)
		{
			var policy = new ShowOncePolicy();
			var message = "Test message";

			var result = m_reporter.NotifyUserOfProblem(policy, "ignore me",
				passOK ? ErrorResult.OK : ErrorResult.Cancel, message);
			Assert.That(result, Is.EqualTo(ErrorResult.OK));

			var output = m_consoleOutput.ToString().TrimEnd();
			Assert.That(output,
				Is.EqualTo($"{message}{NewLine}{ShowOncePolicy.kReoccurrenceMessage}"));
		}

		[Test]
		public void ReportNonFatalException_NoInnerException_WritesExceptionToConsole()
		{
			var exception = new Exception("Test exception");
			var policy = new AlwaysShowPolicy();

			m_reporter.ReportNonFatalException(exception, policy);

			var output = m_consoleOutput.ToString();
			var match = m_regexOutput.Match(output);
			Assert.That(match.Success, Is.True);
			Assert.That(match.Groups["severity"].Value, Is.EqualTo("Warning"));
			Assert.That(match.Groups["exceptionInfo"].Value,
				Is.EqualTo(ExceptionHelper.GetExceptionText(exception)));
			Assert.That(match.Groups["properties"].Value.Trim(), Is.Empty);
		}

		[Test]
		public void ReportNonFatalExceptionWithMessage_WithSomeProperties_WritesExceptionMessageAndPropertiesToConsole()
		{
			var exception = new Exception("Not too bad of an exception");
			var message = "Blah {1} - {0}";
			ErrorReport.Properties = new Dictionary<string, string> { { "name", "HankJ" }, {"country", "Guam"} };

			m_reporter.ReportNonFatalExceptionWithMessage(exception, message, "one", "two");

			var output = m_consoleOutput.ToString();
			var match = m_regexOutput.Match(output);
			Assert.That(match.Success, Is.True);
			Assert.That(match.Groups["severity"].Value, Is.EqualTo("Warning"));
			Assert.That(match.Groups["exceptionInfo"].Value,
				Is.EqualTo(ExceptionHelper.GetExceptionText(exception)));
			Assert.That(match.Groups["properties"].Value.Trim(),
				Is.EqualTo($"name: HankJ{NewLine}country: Guam{NewLine}Message (not an exception): Blah two - one"));
		}

		private class AlwaysShowPolicy : IRepeatNoticePolicy
		{
			public const string kReoccurrenceMessage = "This message will be shown again.";
			public bool ShouldShowErrorReportDialog(Exception exception) => true;
			public bool ShouldShowMessage(string message) => true;
			public string ReoccurrenceMessage => kReoccurrenceMessage;
		}

		private class ShowOncePolicy : IRepeatNoticePolicy
		{
			public const string kReoccurrenceMessage = "This message will show once.";
			private readonly HashSet<string> m_occurrences = new HashSet<string>();
			public bool ShouldShowErrorReportDialog(Exception exception) =>
				ShouldShowMessage(exception.Message);

			public bool ShouldShowMessage(string message) => m_occurrences.Add(message);

			public string ReoccurrenceMessage => kReoccurrenceMessage;
		}
	}
}
