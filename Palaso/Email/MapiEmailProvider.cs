namespace Palaso.Email
{
	public class MapiEmailProvider : IEmailProvider
	{
		public IEmailMessage CreateMessage()
		{
			return new EmailMessage();
		}


		public bool SendMessage(IEmailMessage message)
		{
#if MONO
			return false;
#else
			var mapi = new MAPI();
			foreach (string recipient in message.To)
			{
				mapi.AddRecipientTo(recipient);
			}
			foreach (string recipient in message.Cc)
			{
				mapi.AddRecipientCc(recipient);
			}
			foreach (string recipient in message.Bcc)
			{
				mapi.AddRecipientBcc(recipient);
			}
			foreach (string attachmentFilePath in message.AttachmentFilePath)
			{
				mapi.AddAttachment(attachmentFilePath);
			}
			//this one is better if it works (and it does for Microsoft emailers), but
			//return mapi.SendMailDirect(message.Subject, message.Body);

			//this one works for thunderbird, too. It opens a window rather than just sending:
			return mapi.SendMailPopup(message.Subject, message.Body);
#endif
		}
	}
}
