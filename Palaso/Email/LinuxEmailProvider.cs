using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Palaso.Email
{
	public class LinuxEmailProvider : IEmailProvider
	{
		public IEmailMessage CreateMessage()
		{
			return new EmailMessage();
		}

		public bool SendMessage(IEmailMessage message)
		{
			string body = message.Body;
			string subject = message.Subject;
			var recipientTo = message.To;
			var toBuilder = new StringBuilder();

			for (int i = 0; i < recipientTo.Count; ++i)
			{
				if (i > 0)
				{
					toBuilder.Append(",");
				}
				toBuilder.Append(recipientTo[i]);
			}
			string commandLine = "";
			if (message.AttachmentFilePath.Count == 0)
			{
				commandLine = String.Format(
					FormatStringNoAttachments,
					toBuilder, subject, body
				);
			}
			else
			{
				// DG: attachments untested with xdg-email
				// review CP: throw if AttachmentFilePath.Count > 0 ?
				// review CP: throw if file not present?
				string attachments = String.Format("file://{0}", message.AttachmentFilePath[0]);
				commandLine = String.Format(
					FormatStringWithAttachments,
					toBuilder, subject, attachments, body
				);
			}
			Console.WriteLine(commandLine);
			var p = new Process
			{
				StartInfo =
				{
					FileName = EmailCommand,
					Arguments = commandLine,
					UseShellExecute = true,
					ErrorDialog = true
				}
			};

			p.Start();
			// Always return true. The false from p.Start may only indicate that the email client
			// was already open.
			// DG: This may be different with xdg-email
			return true;
		}

		protected virtual string EmailCommand
		{
			get
			{
				return "xdg-email";
			}
		}

		protected virtual string FormatStringNoAttachments
		{
			get
			{
				return "--subject '{1}' --body '{2}' {0}";
			}
		}

		protected virtual string FormatStringWithAttachments
		{
			get
			{
				return "--subject '{1}' --body '{2}' --attach {0}";
			}
		}
	}
}
