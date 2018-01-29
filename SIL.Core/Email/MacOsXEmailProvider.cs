// Copyright (c) 2018 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

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
			foreach (var recpt in message.To)
			{
				bldr.AppendLine(
					$"make new to recipient at end of to recipients with properties {{address:\"{recpt}\"}}");
			}
			foreach (var recpt in message.Cc)
			{
				bldr.AppendLine(
					$"make new to recipient at end of cc recipients with properties {{address:\"{recpt}\"}}");
			}
			foreach (var recpt in message.Bcc)
			{
				bldr.AppendLine(
					$"make new to recipient at end of bcc recipients with properties {{address:\"{recpt}\"}}");
			}

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
