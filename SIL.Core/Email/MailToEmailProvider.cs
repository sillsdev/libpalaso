using System;
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
			//string body = _body.Replace(System.Environment.NewLine, "%0A").Replace("\"", "%22").Replace("&", "%26");
			string body = Uri.EscapeDataString(message.Body);
			string subject = Uri.EscapeDataString(message.Subject);
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
				commandLine = String.Format("mailto:{0}?subject={1}&body={2}",
											toBuilder, subject, body);
			}
			else
			{
				// review CP: throw if AttachmentFilePath.Count > 0 ?
				// review CP: throw if file not present?
				string attachments = message.AttachmentFilePath[0];
				commandLine = String.Format("mailto:{0}?subject={1}&attachment={2}&body={3}",
											toBuilder, subject, attachments, body);
			}
			Console.WriteLine(commandLine);
			var p = new Process
			{
				StartInfo =
				{
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

	}
}