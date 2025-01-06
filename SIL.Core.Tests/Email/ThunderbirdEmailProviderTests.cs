// Copyright (c) 2024 SIL Global
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
	public class ThunderbirdEmailProviderTests
	{
		private class ThunderbirdEmailProviderDummy : ThunderbirdEmailProvider
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
			ExpectedResult = "-compose \"to='foo@example.com',subject='Testing',body='Hi there!'\"")]
		[TestCase("Foo <foo@example.com>",
			ExpectedResult = "-compose \"to='Foo <foo@example.com>',subject='Testing',body='Hi there!'\"")]
		[TestCase("foo@example.com;someone@example.com",
			ExpectedResult = "-compose \"to='foo@example.com,someone@example.com',subject='Testing',body='Hi there!'\"")]
		public string SendMessage_ToOnly(string to)
		{
			// Setup
			var provider = new ThunderbirdEmailProviderDummy();
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
			ExpectedResult = "-compose \"to='bar@example.com',subject='Testing',body='Hi there!',cc='foo@example.com'\"")]
		[TestCase("foo@example.com;someone@example.com", null,
			ExpectedResult = "-compose \"to='bar@example.com',subject='Testing',body='Hi there!',cc='foo@example.com,someone@example.com'\"")]
		[TestCase(null, "foo@example.com",
			ExpectedResult = "-compose \"to='bar@example.com',subject='Testing',body='Hi there!',bcc='foo@example.com'\"")]
		[TestCase(null, "foo@example.com;someone@example.com",
			ExpectedResult = "-compose \"to='bar@example.com',subject='Testing',body='Hi there!',bcc='foo@example.com,someone@example.com'\"")]
		[TestCase("a@example.com", "foo@example.com;someone@example.com",
			ExpectedResult = "-compose \"to='bar@example.com',subject='Testing',body='Hi there!',cc='a@example.com',bcc='foo@example.com,someone@example.com'\"")]
		public string SendMessage_CcAndBcc(string cc, string bcc)
		{
			// Setup
			var provider = new ThunderbirdEmailProviderDummy();
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

		[TestCase(null, null, 1, "-compose \"to='bar@example.com',subject='Testing',body='Hi there!' $ATTACH\"")]
		[TestCase(null, null, 2, "-compose \"to='bar@example.com',subject='Testing',body='Hi there!' $ATTACH\"")]
		[TestCase("foo@example.com", null, 1,
			"-compose \"to='bar@example.com',subject='Testing',body='Hi there!',cc='foo@example.com' $ATTACH\"")]
		[TestCase("foo@example.com", "a@example.com", 1,
			"-compose \"to='bar@example.com',subject='Testing',body='Hi there!',cc='foo@example.com',bcc='a@example.com' $ATTACH\"")]
		public void SendMessage_Attachment(string cc, string bcc, int noOfAttachments, string expected)
		{
			// Setup
			var provider = new ThunderbirdEmailProviderDummy();
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
					if (bldr.Length > 0)
						bldr.Append(",");
					bldr.Append($"file://{tempFile.Path}");
				}

				// Exercise
				provider.SendMessage(email);

				// Verify
				var fullExpected = expected.Replace(" $ATTACH", $",attachment='{bldr}'");
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
