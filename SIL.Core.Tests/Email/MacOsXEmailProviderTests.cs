// Copyright (c) 2018 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Diagnostics;
using NUnit.Framework;
using SIL.Email;
using SIL.IO;

namespace SIL.Tests.Email
{
	[TestFixture]
	public class MacOsXEmailProviderTests
	{
		private class MacOsXEmailProviderDummy : MacOsXEmailProvider
		{
			public ProcessStartInfo StartInfo { get; private set; }

			protected override bool SendEmail(ProcessStartInfo startInfo)
			{
				StartInfo = startInfo;
				return true;
			}
		}

		[Test]
		public void SendMessage_SingleRecptNoAttachment()
		{
			// Setup
			var provider = new MacOsXEmailProviderDummy();
			var email = provider.CreateMessage();
			email.To.Add("someone@example.com");
			email.Subject = "My subject";
			email.Body = "This is a test email.";

			// Execute
			provider.SendMessage(email);

			// Verify
			var startInfo = provider.StartInfo;
			Assert.That(startInfo.FileName, Is.EqualTo("osascript"));
			Assert.That(startInfo.Arguments, Is.EqualTo(
				@"-e 'tell application ""Mail""
tell (make new outgoing message)
set subject to ""My subject""
set content to ""This is a test email.""
-- set visible to true
make new to recipient at end of to recipients with properties {address:""someone@example.com""}
end tell
end tell'
".Replace("\r", "").Replace("\n", Environment.NewLine)));
		}

		[Test]
		public void SendMessage_MultipleRecptNoAttachment()
		{
			// Setup
			var provider = new MacOsXEmailProviderDummy();
			var email = provider.CreateMessage();
			email.To.Add("someone@example.com");
			email.To.Add("foo@example.com");
			email.To.Add("whoever@example.com");
			email.Subject = "My subject";
			email.Body = "This is a test email.";

			// Execute
			provider.SendMessage(email);

			// Verify
			var startInfo = provider.StartInfo;
			Assert.That(startInfo.FileName, Is.EqualTo("osascript"));
			Assert.That(startInfo.Arguments, Is.EqualTo(
				@"-e 'tell application ""Mail""
tell (make new outgoing message)
set subject to ""My subject""
set content to ""This is a test email.""
-- set visible to true
make new to recipient at end of to recipients with properties {address:""someone@example.com""}
make new to recipient at end of to recipients with properties {address:""foo@example.com""}
make new to recipient at end of to recipients with properties {address:""whoever@example.com""}
end tell
end tell'
".Replace("\r", "").Replace("\n", Environment.NewLine)));
		}

		[Test]
		public void SendMessage_ToCcAndBccNoAttachment()
		{
			// Setup
			var provider = new MacOsXEmailProviderDummy();
			var email = provider.CreateMessage();
			email.To.Add("someone@example.com");
			email.Cc.Add("foo@example.com");
			email.Bcc.Add("whoever@example.com");
			email.Subject = "My subject";
			email.Body = "This is a test email.";

			// Execute
			provider.SendMessage(email);

			// Verify
			var startInfo = provider.StartInfo;
			Assert.That(startInfo.FileName, Is.EqualTo("osascript"));
			Assert.That(startInfo.Arguments, Is.EqualTo(
				@"-e 'tell application ""Mail""
tell (make new outgoing message)
set subject to ""My subject""
set content to ""This is a test email.""
-- set visible to true
make new to recipient at end of to recipients with properties {address:""someone@example.com""}
make new to recipient at end of cc recipients with properties {address:""foo@example.com""}
make new to recipient at end of bcc recipients with properties {address:""whoever@example.com""}
end tell
end tell'
".Replace("\r", "").Replace("\n", Environment.NewLine)));
		}

		[Test]
		public void SendMessage_SingleRecptSingleAttachment()
		{
			// Setup
			var provider = new MacOsXEmailProviderDummy();
			var email = provider.CreateMessage();
			email.To.Add("someone@example.com");
			email.Subject = "My subject";
			email.Body = "This is a test email.";
			using (var tempFile = new TempFile("SendMessage_SingleRecptSingleAttachment"))
			{
				email.AttachmentFilePath.Add(tempFile.Path);

				// Execute
				provider.SendMessage(email);

				// Verify
				var startInfo = provider.StartInfo;
				Assert.That(startInfo.FileName, Is.EqualTo("osascript"));
				Assert.That(startInfo.Arguments, Is.EqualTo(
					$@"-e 'tell application ""Mail""
tell (make new outgoing message)
set subject to ""My subject""
set content to ""This is a test email.""
-- set visible to true
make new to recipient at end of to recipients with properties {{address:""someone@example.com""}}
make new attachment with properties {{file name:""{tempFile.Path}""}} at after the last paragraph
end tell
end tell'
".Replace("\r", "").Replace("\n", Environment.NewLine)));
			}
		}

		[Test]
		public void SendMessage_SingleRecptMultipleAttachments()
		{
			// Setup
			var provider = new MacOsXEmailProviderDummy();
			var email = provider.CreateMessage();
			email.To.Add("someone@example.com");
			email.Subject = "My subject";
			email.Body = "This is a test email.";
			using (var tempFile1 = new TempFile("SendMessage_SingleRecptMultipleAttachments1"))
			using (var tempFile2 = new TempFile("SendMessage_SingleRecptMultipleAttachments2"))
			{
				email.AttachmentFilePath.Add(tempFile1.Path);
				email.AttachmentFilePath.Add(tempFile2.Path);

				// Execute
				provider.SendMessage(email);

				// Verify
				var startInfo = provider.StartInfo;
				Assert.That(startInfo.FileName, Is.EqualTo("osascript"));
				Assert.That(startInfo.Arguments, Is.EqualTo(
					$@"-e 'tell application ""Mail""
tell (make new outgoing message)
set subject to ""My subject""
set content to ""This is a test email.""
-- set visible to true
make new to recipient at end of to recipients with properties {{address:""someone@example.com""}}
make new attachment with properties {{file name:""{tempFile1.Path}""}} at after the last paragraph
make new attachment with properties {{file name:""{tempFile2.Path}""}} at after the last paragraph
end tell
end tell'
".Replace("\r", "").Replace("\n", Environment.NewLine)));
			}
		}
	}
}
