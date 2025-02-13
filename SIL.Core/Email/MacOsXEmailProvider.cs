// Copyright (c) 2025 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace SIL.Email
{
	public class MacOsXEmailProvider: IEmailProvider
	{
		public IEmailMessage CreateMessage()
		{
			return new EmailMessage();
		}

		public bool SendMessage(IEmailMessage message)
		{
			var startInfo = new ProcessStartInfo { FileName = "osascript" };
			var bldr = new StringBuilder();
			bldr.AppendLine("-e 'tell application \"Mail\"");
			bldr.AppendLine("tell (make new outgoing message)");
			bldr.AppendLine($"set subject to \"{message.Subject}\"");
			bldr.AppendLine($"set content to \"{message.Body}\"");
			bldr.AppendLine("-- set visible to true");

			bldr.Append(AddRecipients("to", message.To));
			bldr.Append(AddRecipients("cc", message.Cc));
			bldr.Append(AddRecipients("bcc", message.Bcc));

			foreach (var attachment in message.AttachmentFilePath)
			{
				bldr.AppendLine(
					$"make new attachment with properties {{file name:\"{attachment}\"}} at after the last paragraph");
			}

			bldr.AppendLine("end tell");
			bldr.AppendLine("end tell'");
			startInfo.Arguments = bldr.ToString();

			return SendEmail(startInfo);
		}

		private static string AddRecipients(string what, IEnumerable<string> recipients)
		{
			var bldr = new StringBuilder();
			foreach (var recpt in recipients)
			{
				bldr.AppendLine(
					$"make new to recipient at end of {what} recipients with properties {{address:\"{recpt}\"}}");
			}

			return bldr.ToString();
		}

		protected virtual bool SendEmail(ProcessStartInfo startInfo)
		{
			using (var process = new Process())
			{
				process.StartInfo = startInfo;
				return process.Start();
			}
		}
	}
}
