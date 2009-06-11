using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Palaso.Email
{
	public class EmailMessage
	{
		//  private string _attachmentPath;

		public void Send()
		{
			//string body = _body.Replace(System.Environment.NewLine, "%0A").Replace("\"", "%22").Replace("&", "%26");
			string body = Uri.EscapeDataString(Body);
			string subject = Uri.EscapeDataString(Subject);
			System.Diagnostics.Process p = new Process();

			p.StartInfo.FileName =String.Format("mailto:{0}?subject={1}&body={2}", Address, subject, body);
			p.StartInfo.UseShellExecute = true;
			p.StartInfo.ErrorDialog = true;
			p.Start();
		}

		/// <summary>
		///
		/// </summary>
		public string Address { get; set; }

		/// <summary>
		/// the  e-mail subject
		/// </summary>
		public string Subject { get; set; }

		/// <summary>
		///
		/// </summary>
		public string Body { get; set; }

//        public string AttachmentPath
//        {
//            set
//            {
//                _attachmentPath = value;
//            }
//        }
		public EmailMessage()
		{
			Subject = "";
			Body = "";
			Address = "";
		}
	}
}