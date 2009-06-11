using System.Collections.Generic;

namespace Palaso.Email
{
	public interface IEmailMessage
	{
		string Body { get; set; }
		string Subject { get; set; }
		IList<string> AttachmentFilePath { get; }
		IList<string> To { get; }
		IList<string> Cc { get; }
		IList<string> Bcc { get; }

		bool Send(IEmailProvider provider);

	}
}