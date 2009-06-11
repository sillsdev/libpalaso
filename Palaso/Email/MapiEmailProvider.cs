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
			return mapi.SendMailDirect(message.Subject, message.Body);
		}
	}
}