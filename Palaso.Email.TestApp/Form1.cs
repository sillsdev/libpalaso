using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Palaso.Email.TestApp
{
	public partial class Form1 : Form
	{
		private StringBuilder m_bldr;
		public Form1()
		{
			InitializeComponent();
			m_bldr = new StringBuilder();
		}

		private void Log(string format, params object[] args)
		{
			m_bldr.AppendFormat(format, args);
			m_bldr.AppendLine();
			status.Text = m_bldr.ToString();
		}

		private void ClearLog()
		{
			m_bldr = new StringBuilder();
			status.Text = string.Empty;
		}
		private IEmailMessage CreateMessage(IEmailProvider provider)
		{
			var message = provider.CreateMessage();
			if (!string.IsNullOrEmpty(to.Text))
				message.To.Add(to.Text);
			if (!string.IsNullOrEmpty(cc.Text))
				message.Cc.Add(cc.Text);
			if (!string.IsNullOrEmpty(bcc.Text))
				message.Bcc.Add(bcc.Text);
			if (!string.IsNullOrEmpty(subject.Text))
				message.Subject = subject.Text;
			if (!string.IsNullOrEmpty(body.Text))
				message.Body = body.Text;
			if (!string.IsNullOrEmpty(files.Text))
				message.AttachmentFilePath.Add(files.Text);
			return message;
		}

		private void OnPreferredClicked(object sender, EventArgs e)
		{
			ClearLog();
			var provider = EmailProviderFactory.PreferredEmailProvider();
			Log("provider: {0}", provider.GetType());
			var message = CreateMessage(provider);
			var result = message.Send(provider);
			Log(result ? "Sending email ok" : "Sending email failed");
		}

		private void OnAlternateClicked(object sender, EventArgs e)
		{
			ClearLog();
			var provider = EmailProviderFactory.AlternateEmailProvider();
			Log("provider: {0}", provider.GetType());
			var message = CreateMessage(provider);
			var result = message.Send(provider);
			Log(result ? "Sending email ok" : "Sending email failed");
		}
	}
}
