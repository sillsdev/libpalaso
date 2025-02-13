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
	public class MailToEmailProviderTests
	{
		private class MailToEmailProviderDummy : MailToEmailProvider
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

		[TestCase("foo@example.com",
			ExpectedResult = "mailto:foo@example.com?subject=Testing&body=Hi%20there")]
		[TestCase("Foo <foo@example.com>",
			ExpectedResult = "mailto:Foo <foo@example.com>?subject=Testing&body=Hi%20there")]
		[TestCase("foo@example.com;someone@example.com",
			ExpectedResult = "mailto:foo@example.com,someone@example.com?subject=Testing&body=Hi%20there")]
		public string SendMessage_ToOnly(string to)
		{
			// Setup
			var provider = new MailToEmailProviderDummy();
			var email = provider.CreateMessage();
			email.To.AddRange(to.Split(';'));
			email.Subject =  "Testing";
			email.Body = "Hi there";

			// Exercise
			provider.SendMessage(email);

			// Verify
			return provider.GetCommandLine();
		}

		[TestCase("foo@example.com", null,
			ExpectedResult = "mailto:bar@example.com?subject=Testing&cc=foo@example.com&body=Hi%20there")]
		[TestCase("foo@example.com;someone@example.com", null,
			ExpectedResult = "mailto:bar@example.com?subject=Testing&cc=foo@example.com,someone@example.com&body=Hi%20there")]
		[TestCase(null, "foo@example.com",
			ExpectedResult = "mailto:bar@example.com?subject=Testing&bcc=foo@example.com&body=Hi%20there")]
		[TestCase(null, "foo@example.com;someone@example.com",
			ExpectedResult = "mailto:bar@example.com?subject=Testing&bcc=foo@example.com,someone@example.com&body=Hi%20there")]
		[TestCase("a@example.com", "foo@example.com;someone@example.com",
			ExpectedResult = "mailto:bar@example.com?subject=Testing&cc=a@example.com&bcc=foo@example.com,someone@example.com&body=Hi%20there")]
		public string SendMessage_CcAndBcc(string cc, string bcc)
		{
			// Setup
			var provider = new MailToEmailProviderDummy();
			var email = provider.CreateMessage();
			email.To.Add("bar@example.com");
			if (cc != null)
				email.Cc.AddRange(cc.Split(';'));
			if (bcc != null)
				email.Bcc.AddRange(bcc.Split(';'));
			email.Subject =  "Testing";
			email.Body = "Hi there";

			// Exercise
			provider.SendMessage(email);

			// Verify
			return provider.GetCommandLine();
		}

		[TestCase(null, null, 1, "mailto:bar@example.com?subject=Testing $ATTACH&body=Hi%20there")]
		[TestCase(null, null, 2, "mailto:bar@example.com?subject=Testing $ATTACH&body=Hi%20there")]
		[TestCase("foo@example.com", null, 1,
			"mailto:bar@example.com?subject=Testing&cc=foo@example.com $ATTACH&body=Hi%20there")]
		[TestCase("foo@example.com", "a@example.com", 1,
			"mailto:bar@example.com?subject=Testing&cc=foo@example.com&bcc=a@example.com $ATTACH&body=Hi%20there")]
		public void SendMessage_Attachment(string cc, string bcc, int noOfAttachments, string expected)
		{
			// Setup
			var provider = new MailToEmailProviderDummy();
			var email = provider.CreateMessage();
			email.To.Add("bar@example.com");
			if (cc != null)
				email.Cc.AddRange(cc.Split(';'));
			if (bcc != null)
				email.Bcc.AddRange(bcc.Split(';'));
			email.Subject =  "Testing";
			email.Body = "Hi there";
			var tempFiles = new List<TempFile>();
			try
			{
				var bldr = new StringBuilder();
				for (var i = 0; i < noOfAttachments; i++)
				{
					var tempFile = new TempFile($"SendMessage_Attachment{i}");
					tempFiles.Add(tempFile);
					email.AttachmentFilePath.Add(tempFile.Path);
					if (bldr.Length > 0)
						bldr.Append(",");
					bldr.Append($"\"{tempFile.Path}\"");
				}

				// Exercise
				provider.SendMessage(email);

				// Verify
				var fullExpected = expected.Replace(" $ATTACH", $"&attachment={bldr}");
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
