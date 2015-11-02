using System;
using System.Diagnostics;
using System.Text;

namespace SIL.Email
{
	public class LinuxEmailProvider : IEmailProvider
	{
		public IEmailMessage CreateMessage()
		{
			return new EmailMessage();
		}

		/// <summary>Prepare strings that will later be surrounded in single quotes to be passed to xdg-email via Process.Start in Mono/Linux.</summary>
		public static string EscapeString(string input)
		{
			// Escape backslashes to prevent user from defeating the single-quote-escape below with \', to prevent
			// problems with a backslash at the end of a string (which would result in a backslash escaping the
			// closing quote when the string is later surrounded in single quotes), and also add enough backslashes
			// so they get through to Thunderbird as entered by the user, and sequences like \t\n\0 don't do anything unexpected.
			// 8 backslashes are needed because Process.Start loses 4 of them before calling xdg-email, and then
			// xdg-email loses 3 of them before calling gvfs-open.
			input = input.Replace(@"\", @"\\\\\\\\"); // !
			// Prevent unescaped single quotes from crashing Process.Start().
			input = input.Replace("'", @"\'");
			return input;
		}

		public bool SendMessage(IEmailMessage message)
		{
			string body = EscapeString(message.Body);
			string subject = EscapeString(message.Subject);
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
			string commandLine;
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
				string attachments = EscapeString(String.Format(FormatStringAttachFile, message.AttachmentFilePath[0]));
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

			return p.Start();
			// Always return true. The false from p.Start may only indicate that the email client
			// was already open.
			// DG: This may be different with xdg-email
			//return true;
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
				return "--subject '{1}' --body '{3}' --attach '{2}' {0}";
			}
		}

		protected virtual string FormatStringAttachFile
		{
			get
			{
				return "{0}";
			}
		}
	}
}
