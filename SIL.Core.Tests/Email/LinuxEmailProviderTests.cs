// Copyright (c) 2025 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using SIL.Email;
using SIL.Extensions;
using SIL.IO;

namespace SIL.Tests.Email
{
	[TestFixture]
	public class LinuxEmailProviderTests
	{
		private class LinuxEmailProviderDummy : LinuxEmailProvider
		{
			private string _commandLine;

			public string GetCommandLine()
			{
				return _commandLine;
			}

			protected override bool StartEmailProcess(string commandLine)
			{
				_commandLine = commandLine;
				return true;
			}
		}

		// Does not change most things.
		[TestCase(@"hello1234 hello", ExpectedResult = @"hello1234 hello")]
		[TestCase("hello\"hello", ExpectedResult = "hello\"hello")]
		[TestCase(@"hello`~!@#$%^&*)(][}{/?=+-_|", ExpectedResult = @"hello`~!@#$%^&*)(][}{/?=+-_|")]
		// Escapes quote and backslash characters.
		[TestCase(@"\", ExpectedResult = @"\\\\\\\\")]
		[TestCase(@"hello\hello", ExpectedResult = @"hello\\\\\\\\hello")]
		[TestCase(@"hello\t\n\a\r\0end", ExpectedResult = @"hello\\\\\\\\t\\\\\\\\n\\\\\\\\a\\\\\\\\r\\\\\\\\0end")]
		[TestCase(@"hello'hello", ExpectedResult = @"hello\'hello")]
		[TestCase(@"hello'hello'", ExpectedResult = @"hello\'hello\'")]
		[TestCase("hello'\"hello", ExpectedResult = "hello\\'\"hello")]
		[TestCase(@"C:\'Data\0end", ExpectedResult = @"C:\\\\\\\\\'Data\\\\\\\\0end")]
		public string EscapeString(string input)
		{
			return LinuxEmailProvider.EscapeString(input);
		}

		[TestCase("foo@example.com",
			ExpectedResult = "--subject 'Testing' --body 'Hi there!' 'foo@example.com'")]
		[TestCase("Foo <foo@example.com>",
			ExpectedResult = "--subject 'Testing' --body 'Hi there!' 'Foo <foo@example.com>'")]
		[TestCase("foo@example.com;someone@example.com",
			ExpectedResult = "--subject 'Testing' --body 'Hi there!' 'foo@example.com' 'someone@example.com'")]
		public string SendMessage_ToOnly(string to)
		{
			// Setup
			var provider = new LinuxEmailProviderDummy();
			var email = provider.CreateMessage();
			email.To.AddRange(to.Split(';'));
			email.Subject =  "Testing";
			email.Body = "Hi there!";

			// Exercise
			provider.SendMessage(email);

			// Verify
			return provider.GetCommandLine();
		}

		[TestCase("foo@example.com", null,
			ExpectedResult = "--subject 'Testing' --body 'Hi there!' --cc 'foo@example.com' 'bar@example.com'")]
		[TestCase("foo@example.com;someone@example.com", null,
			ExpectedResult = "--subject 'Testing' --body 'Hi there!' --cc 'foo@example.com' --cc 'someone@example.com' 'bar@example.com'")]
		[TestCase(null, "foo@example.com",
			ExpectedResult = "--subject 'Testing' --body 'Hi there!' --bcc 'foo@example.com' 'bar@example.com'")]
		[TestCase(null, "foo@example.com;someone@example.com",
			ExpectedResult = "--subject 'Testing' --body 'Hi there!' --bcc 'foo@example.com' --bcc 'someone@example.com' 'bar@example.com'")]
		[TestCase("a@example.com", "foo@example.com;someone@example.com",
			ExpectedResult = "--subject 'Testing' --body 'Hi there!' --cc 'a@example.com' --bcc 'foo@example.com' --bcc 'someone@example.com' 'bar@example.com'")]
		public string SendMessage_CcAndBcc(string cc, string bcc)
		{
			// Setup
			var provider = new LinuxEmailProviderDummy();
			var email = provider.CreateMessage();
			email.To.Add("bar@example.com");
			if (cc != null)
				email.Cc.AddRange(cc.Split(';'));
			if (bcc != null)
				email.Bcc.AddRange(bcc.Split(';'));
			email.Subject =  "Testing";
			email.Body = "Hi there!";

			// Exercise
			provider.SendMessage(email);

			// Verify
			return provider.GetCommandLine();
		}

		[TestCase(null, null, 1, "--subject 'Testing' --body 'Hi there!' $ATTACH 'bar@example.com'")]
		[TestCase(null, null, 2, "--subject 'Testing' --body 'Hi there!' $ATTACH 'bar@example.com'")]
		[TestCase("foo@example.com", null, 1,
			"--subject 'Testing' --body 'Hi there!' --cc 'foo@example.com' $ATTACH 'bar@example.com'")]
		[TestCase("foo@example.com", "a@example.com", 1,
			"--subject 'Testing' --body 'Hi there!' --cc 'foo@example.com' --bcc 'a@example.com' $ATTACH 'bar@example.com'")]
		public void SendMessage_Attachment(string cc, string bcc, int noOfAttachments, string expected)
		{
			// Setup
			var provider = new LinuxEmailProviderDummy();
			var email = provider.CreateMessage();
			email.To.Add("bar@example.com");
			if (cc != null)
				email.Cc.AddRange(cc.Split(';'));
			if (bcc != null)
				email.Bcc.AddRange(bcc.Split(';'));
			email.Subject =  "Testing";
			email.Body = "Hi there!";
			var tempFiles = new List<TempFile>();
			try
			{
				var bldr = new StringBuilder();
				for (var i = 0; i < noOfAttachments; i++)
				{
					var tempFile = new TempFile($"SendMessage_Attachment{i}");
					tempFiles.Add(tempFile);
					email.AttachmentFilePath.Add(tempFile.Path);
					bldr.Append($" --attach '{tempFile.Path}'");
				}

				// Exercise
				provider.SendMessage(email);

				// Verify
				var fullExpected = expected.Replace(" $ATTACH", bldr.ToString());
				Assert.That(provider.GetCommandLine(), Is.EqualTo(fullExpected));
			}
			catch
			{
				foreach (var tempFile in tempFiles)
					tempFile.Dispose();
				throw;
			}
		}
	}
}