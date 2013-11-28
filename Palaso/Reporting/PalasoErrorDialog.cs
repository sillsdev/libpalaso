#if MONO
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Palaso.Email;
using Palaso.Reporting;
using System.IO;

namespace Palaso.Reporting
{
	 public class PalasoErrorDialog : Form, IDisposable
	{
		#region Member variables

		private Label label3;
		private TextBox _details;
		private TextBox _pleaseHelpText;
		private TextBox m_reproduce;

		private bool _isLethal;

		private Button _sendAndCloseButton;
		 private TextBox _notificationText;
		 private TextBox textBox1;
		 private ComboBox _methodCombo;
		 private Button _privacyNoticeButton;
		 private Label _emailAddress;
		private static bool s_doIgnoreReport = false;
		private string _errorFileName;

		#endregion

		public PalasoErrorDialog(bool isLethal, string errorFileName, string emailAddress, string emailSubject)
		{
			_isLethal = isLethal;
			_errorFileName = errorFileName;
			ErrorReport.EmailAddress = emailAddress;
			ErrorReport.EmailSubject = emailSubject;
			PrepareDialog();

			using (StreamReader reader = File.OpenText(errorFileName))
			{
				_details.Text = reader.ReadToEnd();
			}
		}

		#region IDisposable override

		/// <summary>
		/// Check to see if the object has been disposed.
		/// All public Properties and Methods should call this
		/// before doing anything else.
		/// </summary>
		public void CheckDisposed()
		{
			if (IsDisposed)
			{
				throw new ObjectDisposedException(String.Format("'{0}' in use after being disposed.", GetType().Name));
			}
		}

		/// <summary>
		/// Executes in two distinct scenarios.
		///
		/// 1. If disposing is true, the method has been called directly
		/// or indirectly by a user's code via the Dispose method.
		/// Both managed and unmanaged resources can be disposed.
		///
		/// 2. If disposing is false, the method has been called by the
		/// runtime from inside the finalizer and you should not reference (access)
		/// other managed objects, as they already have been garbage collected.
		/// Only unmanaged resources can be disposed.
		/// </summary>
		/// <param name="disposing"></param>
		/// <remarks>
		/// If any exceptions are thrown, that is fine.
		/// If the method is being done in a finalizer, it will be ignored.
		/// If it is thrown by client code calling Dispose,
		/// it needs to be handled by fixing the bug.
		///
		/// If subclasses override this method, they should call the base implementation.
		/// </remarks>
		protected override void Dispose(bool disposing)
		{
			//Debug.WriteLineIf(!disposing, "****************** " + GetType().Name + " 'disposing' is false. ******************");
			// Must not be run more than once.
			if (IsDisposed)
			{
				return;
			}

			if (disposing)
			{
				// Dispose managed resources here.
			}

			// Dispose unmanaged resources here, whether disposing is true or false.

			base.Dispose(disposing);
		}

		#endregion IDisposable override

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PalasoErrorDialog));
			this.m_reproduce = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this._details = new System.Windows.Forms.TextBox();
			this._sendAndCloseButton = new System.Windows.Forms.Button();
			this._pleaseHelpText = new System.Windows.Forms.TextBox();
			this._notificationText = new System.Windows.Forms.TextBox();
			this.textBox1 = new System.Windows.Forms.TextBox();
			this._methodCombo = new System.Windows.Forms.ComboBox();
			this._privacyNoticeButton = new System.Windows.Forms.Button();
			this._emailAddress = new System.Windows.Forms.Label();
			this.SuspendLayout();
			//
			// m_reproduce
			//
			this.m_reproduce.AcceptsReturn = true;
			this.m_reproduce.AcceptsTab = true;
			resources.ApplyResources(this.m_reproduce, "m_reproduce");
			this.m_reproduce.Name = "m_reproduce";
			//
			// label3
			//
			resources.ApplyResources(this.label3, "label3");
			this.label3.Name = "label3";
			//
			// _details
			//
			resources.ApplyResources(this._details, "_details");
			this._details.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this._details.Name = "_details";
			this._details.ReadOnly = true;
			//
			// _sendAndCloseButton
			//
			resources.ApplyResources(this._sendAndCloseButton, "_sendAndCloseButton");
			this._sendAndCloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this._sendAndCloseButton.Name = "_sendAndCloseButton";
			this._sendAndCloseButton.Click += new System.EventHandler(this.btnClose_Click);
			//
			// _pleaseHelpText
			//
			resources.ApplyResources(this._pleaseHelpText, "_pleaseHelpText");
			this._pleaseHelpText.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
			this._pleaseHelpText.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this._pleaseHelpText.ForeColor = System.Drawing.Color.Black;
			this._pleaseHelpText.Name = "_pleaseHelpText";
			this._pleaseHelpText.ReadOnly = true;
			//
			// _notificationText
			//
			resources.ApplyResources(this._notificationText, "_notificationText");
			this._notificationText.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
			this._notificationText.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this._notificationText.ForeColor = System.Drawing.Color.Black;
			this._notificationText.Name = "_notificationText";
			this._notificationText.ReadOnly = true;
			//
			// textBox1
			//
			resources.ApplyResources(this.textBox1, "textBox1");
			this.textBox1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
			this.textBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.textBox1.ForeColor = System.Drawing.Color.Black;
			this.textBox1.Name = "textBox1";
			this.textBox1.ReadOnly = true;
			//
			// _methodCombo
			//
			resources.ApplyResources(this._methodCombo, "_methodCombo");
			this._methodCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this._methodCombo.FormattingEnabled = true;
			this._methodCombo.Name = "_methodCombo";
			this._methodCombo.SelectedIndexChanged += new System.EventHandler(this._methodCombo_SelectedIndexChanged);
			//
			// _privacyNoticeButton
			//
			resources.ApplyResources(this._privacyNoticeButton, "_privacyNoticeButton");
			this._privacyNoticeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this._privacyNoticeButton.Image = global::Palaso.Properties.Resources.spy16x16;
			this._privacyNoticeButton.Name = "_privacyNoticeButton";
			this._privacyNoticeButton.UseVisualStyleBackColor = true;
			this._privacyNoticeButton.Click += new System.EventHandler(this._privacyNoticeButton_Click);
			//
			// _emailAddress
			//
			resources.ApplyResources(this._emailAddress, "_emailAddress");
			this._emailAddress.ForeColor = System.Drawing.Color.DimGray;
			this._emailAddress.Name = "_emailAddress";
			//
			// PalasoErrorDialog
			//
			this.AcceptButton = this._sendAndCloseButton;
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
			//this.ControlBox = false;
			this.Controls.Add(this._emailAddress);
			this.Controls.Add(this._privacyNoticeButton);
			this.Controls.Add(this._methodCombo);
			this.Controls.Add(this.textBox1);
			this.Controls.Add(this.m_reproduce);
			this.Controls.Add(this._notificationText);
			this.Controls.Add(this._pleaseHelpText);
			this.Controls.Add(this._details);
			this.Controls.Add(this.label3);
			this.Controls.Add(this._sendAndCloseButton);
			this.KeyPreview = true;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "PalasoErrorDialog";
			this.TopMost = true;
			this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.PalasoErrorDialog_KeyPress);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		 private void SetupMethodCombo()
		 {
			 _methodCombo.Items.Clear();
			 _methodCombo.Items.Add(new ReportingMethod("Send using my email program", "&Email", "mapiWithPopup", SendViaEmail));
#if !MONO
			// DG May 2012: doesn't stay on clipboard after app closes on mono so not using it
			 _methodCombo.Items.Add(new ReportingMethod("Copy to clipboard", "&Copy", "clipboard", PutOnClipboard));
#endif
			 _methodCombo.Items.Add(new ReportingMethod("Open as text file", "&Open", "textfile", OpenTextFile));
		 }

		 class ReportingMethod
		 {
			 private readonly string _label;
			 public readonly string CloseButtonLabel;
			 public readonly string Id;
			 public readonly Func<bool> Method;

			 public ReportingMethod(string label, string closeButtonLabel, string id, Func<bool> method)
			 {
				 _label = label;
				 CloseButtonLabel = closeButtonLabel;
				 Id = id;
				 Method = method;
			 }
			 public override string ToString()
			 {
				 return _label;
			 }
		 }
		#endregion



		/// ------------------------------------------------------------------------------------
		/// <summary>
		///
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected void GatherData()
		{
			_details.Text += "\r\nTo Reproduce: " + m_reproduce.Text + "\r\n";
		}




		 private void PrepareDialog()
		 {
			 CheckDisposed();
			 Font = SystemFonts.MessageBoxFont;

			 //
			 // Required for Windows Form Designer support
			 //
			 InitializeComponent();
			 _emailAddress.Text = ErrorReport.EmailAddress;
			 SetupMethodCombo();

			 foreach (ReportingMethod  method in _methodCombo.Items)
			 {
				 if (ErrorReportSettings.Default.ReportingMethod == method.Id)
				 {
					SelectedMethod = method;
					break;
				 }
			 }

			 if (!_isLethal)
			 {
				 BackColor = Color.FromArgb(255, 255, 192); //yellow
				_notificationText.Text = "Take Courage. It'll work out.";
				 _notificationText.BackColor = BackColor;
				 _pleaseHelpText.BackColor = BackColor;
				 textBox1.BackColor = BackColor;
			 }

			 SetupCloseButtonText();
		 }



		 /// ------------------------------------------------------------------------------------
		/// <summary>
		///
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		/// ------------------------------------------------------------------------------------
		private void btnClose_Click(object sender, EventArgs e)
		{
			 ErrorReportSettings.Default.ReportingMethod = ((ReportingMethod) (_methodCombo.SelectedItem)).Id;
			 ErrorReportSettings.Default.Save();

			if (ModifierKeys.Equals(Keys.Shift))
			{
				return;
			}
			GatherData();

		   // Clipboard.SetDataObject(_details.Text, true);

			if (SelectedMethod.Method())
			{
				CloseUp();
			}
			else
			{
				PutOnClipboard();
				CloseUp();
			}
		}

		 private bool PutOnClipboard()
		 {
			if (ErrorReport.EmailAddress != null)
			{
				_details.Text = String.Format(ReportingStrings.ksPleaseEMailThisToUs, ErrorReport.EmailAddress, _details.Text);
			}
			Clipboard.SetDataObject(_details.Text, true);
			 return true;
		 }

		 private bool OpenTextFile()
		 {
			if (ErrorReport.EmailAddress != null)
			{
				_details.Text = String.Format(ReportingStrings.ksPleaseEMailThisToUs, ErrorReport.EmailAddress, _details.Text);
			}
			string tempdirPath = Path.GetDirectoryName(_errorFileName);
			string tempFileName = Path.Combine (tempdirPath, "Report.txt");

			File.WriteAllText(tempFileName, _details.Text);
			Process.Start(tempFileName);
			 return true;
		 }

		 private bool SendViaEmail()
		 {
			 try
			 {
				 var emailProvider = EmailProviderFactory.PreferredEmailProvider();
				 var emailMessage = emailProvider.CreateMessage();
				 emailMessage.To.Add(ErrorReport.EmailAddress);
				 emailMessage.Subject = ErrorReport.EmailSubject;
				 emailMessage.Body = _details.Text;
				 if (emailMessage.Send(emailProvider))
				 {
					 CloseUp();
					 return true;
				 }
			 }
			 catch (Exception)
			 {
				Console.WriteLine("First attempt at creating email failed");
				 //swallow it and go to the alternate method
			 }

			 try
			 {
				 //EmailMessage msg = new EmailMessage();
				 // This currently does not work. The main issue seems to be the length of the error report. mailto
				 // apparently has some limit on the length of the message, and we are exceeding that.
				 var emailProvider = EmailProviderFactory.PreferredEmailProvider();
				 var emailMessage = emailProvider.CreateMessage();
				 emailMessage.To.Add(ErrorReport.EmailAddress);
				 emailMessage.Subject = ErrorReport.EmailSubject;
				 if (Environment.OSVersion.Platform == PlatformID.Unix)
				 {
					string tempdirPath = Path.GetDirectoryName(_errorFileName);
					string tempFileName = Path.Combine (tempdirPath, "Report.txt");

					File.WriteAllText(tempFileName, _details.Text);
					 emailMessage.Body = "The error is in the attached file.\n\n<Please give a quick explanation for the developers here>";
					emailMessage.AttachmentFilePath.Add(tempFileName);
				 }
				 else
				 {
					 PutOnClipboard();
					 emailMessage.Body = "<Details of the crash have been copied to the clipboard. Please paste them here>";
				 }
				 if (emailMessage.Send(emailProvider))
				 {
					 CloseUp();
					 return true;
				 }
			 }
			 catch (Exception error)
			 {
				 PutOnClipboard();
				 ErrorReport.NotifyUserOfProblem(error,
					 "This program wasn't able to get your email program, if you have one, to send the error message.  The contents of the error message has been placed on your Clipboard.");
				 return false;
			 }
			 return false;
		 }



		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Shows the attempt to continue label if the shift key is pressed
		/// </summary>
		/// <param name="e"></param>
		/// ------------------------------------------------------------------------------------
		protected override void OnKeyDown(KeyEventArgs e)
		{
			if (e.KeyCode == Keys.ShiftKey && Visible)
			{
				_sendAndCloseButton.Text = "Continue";
			}
			base.OnKeyDown(e);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Hides the attempt to continue label if the shift key is pressed
		/// </summary>
		/// <param name="e"></param>
		/// ------------------------------------------------------------------------------------
		protected override void OnKeyUp(KeyEventArgs e)
		{
			if (e.KeyCode == Keys.ShiftKey && Visible)
			{
				SetupCloseButtonText();
			}
			base.OnKeyUp(e);
		}

		 private void OnJustExit_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		 {
			 CloseUp();
		 }

		 private void _methodCombo_SelectedIndexChanged(object sender, EventArgs e)
		 {
			 SetupCloseButtonText();
		 }

		 private void SetupCloseButtonText()
		 {
			_sendAndCloseButton.Text = SelectedMethod.CloseButtonLabel;
			 if (!_isLethal)
			 {
				// _dontSendEmailLink.Text = "Don't Send Email";
			 }
			 else
			 {
				 _sendAndCloseButton.Text += " and Exit";
			 }
		 }

		 private ReportingMethod SelectedMethod
		 {
			 get { return ((ReportingMethod) _methodCombo.SelectedItem); }
			 set { _methodCombo.SelectedItem = value; }
		 }

		 private void _privacyNoticeButton_Click(object sender, EventArgs e)
		 {
			MessageBox.Show(
				@"If you don't care who reads your bug report, you can skip this notice.

When you submit a crash report or other issue, the contents of your email go in our
issue tracking system, 'jira', which is available via the web at http://jira.palso.org/issues.
This is the normal way to handle issues in an open-source project.

Our issue-tracking system is not searchable by those without an account. Therefore, someone
searching via Google will not find your bug reports.

However, anyone can make an account and then read what you sent us. So if you have
something private to say, please send it to one of the developers privately with a
note that you don't want the issue in our issue tracking system. If need be, we'll
make some kind of sanitized place-holder for your issue so that we don't lose it.
");
		 }

		 private void PalasoErrorDialog_KeyPress(object sender, KeyPressEventArgs e)
		 {
			 if(e.KeyChar== 27)//ESCAPE
			 {
				CloseUp();
			 }
		 }

		private void CloseUp()
		{
			Close();
		}
	}
}
#endif
