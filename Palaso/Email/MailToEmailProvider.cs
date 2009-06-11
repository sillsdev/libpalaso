using System;
using System.Diagnostics;

namespace Palaso.Email
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
			var p = new Process
			{
				StartInfo =
				{
					FileName = String.Format("mailto:{0}?subject={1}&body={2}", message.To, subject, body),
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