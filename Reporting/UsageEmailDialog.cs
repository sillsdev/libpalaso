using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Palaso.Reporting
{
	/// <summary>
	/// Summary description for UsageEmailDialog.
	/// </summary>
	public class UsageEmailDialog : Form, IDisposable
	{
		private TabControl tabControl1;
		private TabPage tabPage1;
		private PictureBox pictureBox1;
		private RichTextBox richTextBox2;
		private Button btnSend;
		private LinkLabel btnNope;
		private RichTextBox m_topLineText;

		private EmailMessage _message = new EmailMessage();

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private Container components = null;

		internal UsageEmailDialog()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			this.richTextBox2.Text = "May we ask you a favor? We would like to send a tiny e-mail back to the software developers telling us of your progress.\nYou will be able to view the e-mail before it goes out. You do not need to be connected to the Internet right now...the e-mail will just open and you can save it in your outbox.";
			this.m_topLineText.Text = string.Format(this.m_topLineText.Text, UsageReporter.AppNameToUseInDialogs);

		}

		/// <summary>
		/// Check to see if the object has been disposed.
		/// All public Properties and Methods should call this
		/// before doing anything else.
		/// </summary>
		public void CheckDisposed()
		{
			if (IsDisposed)
				throw new ObjectDisposedException(String.Format("'{0}' in use after being disposed.", GetType().Name));
		}

		/// <summary>
		///
		/// </summary>
		public string TopLineText
		{
			set
			{
				CheckDisposed();
				m_topLineText.Text = value;
			}
			get
			{
				CheckDisposed();
				return m_topLineText.Text;
			}
		}

		public EmailMessage EmailMessage
		{
			get
			{
				return _message;
			}
			set
			{
				_message = value;
			}
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			//Debug.WriteLineIf(!disposing, "****************** " + GetType().Name + " 'disposing' is false. ******************");
			// Must not be run more than once.
			if (IsDisposed)
				return;

			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}



		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UsageEmailDialog));
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabPage1 = new System.Windows.Forms.TabPage();
			this.m_topLineText = new System.Windows.Forms.RichTextBox();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.richTextBox2 = new System.Windows.Forms.RichTextBox();
			this.btnSend = new System.Windows.Forms.Button();
			this.btnNope = new System.Windows.Forms.LinkLabel();
			this.tabControl1.SuspendLayout();
			this.tabPage1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.SuspendLayout();
			//
			// tabControl1
			//
			this.tabControl1.Controls.Add(this.tabPage1);
			this.tabControl1.Location = new System.Drawing.Point(9, 10);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(590, 240);
			this.tabControl1.TabIndex = 0;
			//
			// tabPage1
			//
			this.tabPage1.BackColor = System.Drawing.SystemColors.Window;
			this.tabPage1.Controls.Add(this.m_topLineText);
			this.tabPage1.Controls.Add(this.pictureBox1);
			this.tabPage1.Controls.Add(this.richTextBox2);
			this.tabPage1.Location = new System.Drawing.Point(4, 22);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Size = new System.Drawing.Size(582, 214);
			this.tabPage1.TabIndex = 0;
			this.tabPage1.Text = "Report";
			//
			// m_topLineText
			//
			this.m_topLineText.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.m_topLineText.Location = new System.Drawing.Point(218, 29);
			this.m_topLineText.Name = "m_topLineText";
			this.m_topLineText.Size = new System.Drawing.Size(339, 31);
			this.m_topLineText.TabIndex = 1;
			this.m_topLineText.Text = "Thank you for checking out this preview version of {0}.";
			//
			// pictureBox1
			//
			this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
			this.pictureBox1.Location = new System.Drawing.Point(18, 38);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(165, 71);
			this.pictureBox1.TabIndex = 2;
			this.pictureBox1.TabStop = false;
			//
			// richTextBox2
			//
			this.richTextBox2.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.richTextBox2.Location = new System.Drawing.Point(218, 74);
			this.richTextBox2.Name = "richTextBox2";
			this.richTextBox2.Size = new System.Drawing.Size(339, 147);
			this.richTextBox2.TabIndex = 1;
			this.richTextBox2.Text = "May we ask you a favor? We would like to send a tiny e-mail back to the software developers telling us of your progress.\nYou will be able to view the e-mail before it goes out. You do not need to be connected to the Internet right now...the e-mail will just open and you can save it in your outbox.";

			//
			// btnSend
			//
			this.btnSend.Location = new System.Drawing.Point(464, 263);
			this.btnSend.Name = "btnSend";
			this.btnSend.Size = new System.Drawing.Size(133, 23);
			this.btnSend.TabIndex = 1;
			this.btnSend.Text = "Create Email Message";
			this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
			//
			// btnNope
			//
			this.btnNope.Location = new System.Drawing.Point(14, 271);
			this.btnNope.Name = "btnNope";
			this.btnNope.Size = new System.Drawing.Size(278, 23);
			this.btnNope.TabIndex = 2;
			this.btnNope.TabStop = true;
			this.btnNope.Text = "I\'m unable to send this information.";
			this.btnNope.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.btnNope_LinkClicked);
			//
			// UsageEmailDialog
			//
			this.AcceptButton = this.btnSend;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.btnNope;
			this.ClientSize = new System.Drawing.Size(618, 301);
			this.ControlBox = false;
			this.Controls.Add(this.btnNope);
			this.Controls.Add(this.btnSend);
			this.Controls.Add(this.tabControl1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MinimizeBox = false;
			this.Name = "UsageEmailDialog";
			this.Text = "Field Usage Report System";
			this.tabControl1.ResumeLayout(false);
			this.tabPage1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion

		private void btnSend_Click(object sender, EventArgs e)
		{
			try
			{

				EmailMessage.Send();
			}
			catch
			{
				//swallow it
			}
			DialogResult = DialogResult.OK;
			Close();
		}


		private void btnNope_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			DialogResult = DialogResult.No;
			Close();
		}





	}
}
