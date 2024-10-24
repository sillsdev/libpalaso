// Copyright (c) 2024 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace SIL.Email
{
	public class MailToEmailProvider : IEmailProvider
	{
		public IEmailMessage CreateMessage()
		{
			return new EmailMessage();
		}

		public bool SendMessage(IEmailMessage message)
		{
			// NOTE: attachments are not officially supported in the mailto: protocol, so they may
			// or may not work for the user

			//string body = _body.Replace(System.Environment.NewLine, "%0A").Replace("\"", "%22").Replace("&", "%26");
			var body = Uri.EscapeDataString(message.Body);
			var subject = Uri.EscapeDataString(message.Subject);
			// review CP: throw if AttachmentFilePath.Count > 0 ?
			// review CP: throw if file not present?
			var commandLine = $"mailto:{GetToRecipients(message.To)}?subject={subject}{GetCcRecipients(message.Cc)}{GetBccRecipients(message.Bcc)}{GetAttachments(message.AttachmentFilePath)}&body={body}";
			Console.WriteLine(commandLine);
			return StartEmailProcess(commandLine);
		}

		protected virtual bool StartEmailProcess(string commandLine)
		{
			var p = new Process {
				StartInfo = {
					FileName = commandLine,
					UseShellExecute = true,
					ErrorDialog = true
				}
			};

			p.Start();
			// Always return true. The false from p.Start may only indicate that the email client
			// was already open.
			return true;
		}

		private static string GetArguments(IList<string> arguments, string prefix = "", string postfix = "")
		{
			var toBuilder = new StringBuilder();

			foreach (var argument in arguments)
			{
				if (toBuilder.Length > 0)
					toBuilder.Append(",");
				toBuilder.Append($"{prefix}{argument}{postfix}");
			}

			return toBuilder.ToString();
		}

		private static string GetToRecipients(IList<string> recipientTo)
		{
			return GetArguments(recipientTo);
		}

		private static string GetCcRecipients(IList<string> recipients)
		{
			return recipients.Count > 0 ? $"&cc={GetArguments(recipients)}" : null;
		}

		private static string GetBccRecipients(IList<string> recipients)
		{
			return recipients.Count > 0 ? $"&bcc={GetArguments(recipients)}" : null;
		}

		private static string GetAttachments(IList<string> attachments)
		{
			return attachments.Count > 0 ? $"&attachment={GetArguments(attachments, "\"", "\"")}" : null;
		}
	}
}