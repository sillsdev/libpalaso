using System.Collections.Generic;

namespace SIL.Email
{
	public class EmailMessage : IEmailMessage
	{
		private List<string> _attachmentFilePaths;
		private List<string> _to;
		private List<string> _bcc;
		private List<string> _cc;

		/// <summary>
		/// the  e-mail subject
		/// </summary>
		public string Subject { get; set; }

		public IList<string> AttachmentFilePath
		{
			get { return _attachmentFilePaths; }
		}

		public IList<string> To
		{
			get { return _to; }
		}

		public IList<string> Cc
		{
			get { return _cc; }
		}

		public IList<string> Bcc
		{
			get { return _bcc; }
		}

		/// <summary>
		///
		/// </summary>
		public string Body { get; set; }

		public EmailMessage()
		{
			_to = new List<string>();
			_cc = new List<string>();
			_bcc = new List<string>();
			_attachmentFilePaths = new List<string>();
			Subject = "";
			Body = "";
		}

		public bool Send(IEmailProvider provider)
		{
			return provider.SendMessage(this);
		}

	}
}