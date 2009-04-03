using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Palaso.Reporting;

namespace Palaso.Reporting
{
	 public class ExceptionReportingDialog : Form, IDisposable
	{
		#region Member variables

		private Label label2;
		private Label label3;
		private TextBox _details;
		private TextBox _pleaseHelpText;
		private TextBox m_reproduce;
		private Label _attemptToContinueLabel;

		private bool _isLethal;

		private Button _sendAndCloseButton;
		 private LinkLabel _dontSendEmailLink;
		 private TextBox _notificationText;
		private static bool s_doIgnoreReport = false;

		#endregion

		protected ExceptionReportingDialog(bool isLethal)
		{
			_isLethal = isLethal;
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExceptionReportingDialog));
			this.label2 = new System.Windows.Forms.Label();
			this.m_reproduce = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this._details = new System.Windows.Forms.TextBox();
			this._sendAndCloseButton = new System.Windows.Forms.Button();
			this._pleaseHelpText = new System.Windows.Forms.TextBox();
			this._attemptToContinueLabel = new System.Windows.Forms.Label();
			this._dontSendEmailLink = new System.Windows.Forms.LinkLabel();
			this._notificationText = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			//
			// label2
			//
			resources.ApplyResources(this.label2, "label2");
			this.label2.Name = "label2";
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
			// _closeButton
			//
			resources.ApplyResources(this._sendAndCloseButton, "_closeButton");
			this._sendAndCloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this._sendAndCloseButton.Name = "_sendAndCloseButton";
			this._sendAndCloseButton.Click += new System.EventHandler(this.btnClose_Click);
			//
			// m_notification
			//
			resources.ApplyResources(this._pleaseHelpText, "m_notification");
			this._pleaseHelpText.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
			this._pleaseHelpText.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this._pleaseHelpText.ForeColor = System.Drawing.Color.Black;
			this._pleaseHelpText.Name = "_pleaseHelpText";
			this._pleaseHelpText.ReadOnly = true;
			//
			// _attemptToContinueLabel
			//
			resources.ApplyResources(this._attemptToContinueLabel, "_attemptToContinueLabel");
			this._attemptToContinueLabel.ForeColor = System.Drawing.Color.Firebrick;
			this._attemptToContinueLabel.Name = "_attemptToContinueLabel";
			//
			// _linkJustExit
			//
			resources.ApplyResources(this._dontSendEmailLink, "_linkJustExit");
			this._dontSendEmailLink.Name = "_dontSendEmailLink";
			this._dontSendEmailLink.TabStop = true;
			this._dontSendEmailLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OnJustExit_LinkClicked);
			//
			// _userMessage
			//
			resources.ApplyResources(this._notificationText, "_userMessage");
			this._notificationText.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
			this._notificationText.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this._notificationText.ForeColor = System.Drawing.Color.Black;
			this._notificationText.Name = "_notificationText";
			this._notificationText.ReadOnly = true;
			//
			// ExceptionReportingDialog
			//
			this.AcceptButton = this._sendAndCloseButton;
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
			this.CancelButton = this._sendAndCloseButton;
			this.ControlBox = false;
			this.Controls.Add(this._dontSendEmailLink);
			this.Controls.Add(this.m_reproduce);
			this.Controls.Add(this._notificationText);
			this.Controls.Add(this._pleaseHelpText);
			this.Controls.Add(this._details);
			this.Controls.Add(this._attemptToContinueLabel);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label3);
			this.Controls.Add(this._sendAndCloseButton);
			this.KeyPreview = true;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ExceptionReportingDialog";
			this.Load += new System.EventHandler(this.ErrorNotificationDialog_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion



		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// show a dialog or output to the error log, as appropriate.
		/// </summary>
		/// <param name="error">the exception you want to report</param>
		/// ------------------------------------------------------------------------------------
		internal static void ReportException(Exception error)
		{
			ReportException(error, null);
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="error"></param>
		/// <param name="parent"></param>
		internal static void ReportException(Exception error, Form parent)
		{
			ReportException(error, null, true);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// show a dialog or output to the error log, as appropriate.
		/// </summary>
		/// <param name="error">the exception you want to report</param>
		/// <param name="parent">the parent form that this error belongs to (i.e. the form
		/// show modally on)</param>
		/// ------------------------------------------------------------------------------------
		/// <param name="isLethal"></param>
		internal static void ReportException(Exception error, Form parent, bool isLethal)
		{
			if (s_doIgnoreReport)
			{
				return;            // ignore message if we are showing from a previous error
			}

			using (ExceptionReportingDialog dlg = new ExceptionReportingDialog(isLethal))
			{
				dlg.Report(error, parent);
			}
		}

		internal static void ReportMessage(string message, StackTrace stack, bool isLethal)
		{
			if (s_doIgnoreReport)
			{
				return;            // ignore message if we are showing from a previous error
			}

			using (ExceptionReportingDialog dlg = new ExceptionReportingDialog(isLethal))
			{
				dlg.Report(message, stack, null);
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		///
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected void GatherData()
		{
			_details.Text += "\r\nTo Reproduce: " + m_reproduce.Text + "\r\n";
		}

		 public void Report(Exception error, Form owningForm)
		{
			PrepareDialog();

			Exception innerMostException = null;
			_details.Text += ErrorReport.GetHiearchicalExceptionInfo(error, ref innerMostException);

			//if the exception had inner exceptions, show the inner-most exception first, since that is usually the one
			//we want the developer to read.
			if (innerMostException != null)
			{
				_details.Text = "Inner-most exception:\r\n" + ErrorReport.GetExceptionText(innerMostException) +
								 "\r\n\r\nFull, hierarchical exception contents:\r\n" + _details.Text;
			}

			AddErrorReportingPropertiesToDetails();


			Debug.WriteLine(_details.Text);
			if (innerMostException != null)
			{
				error = innerMostException;
			}


			try
			{
				Logger.WriteEvent("Got exception " + error.GetType().Name);
			}
			catch (Exception err)
			{
				//We have more than one report of dieing while logging an exception.
				_details.Text += "****Could not write to log (" + err.Message + ")" + Environment.NewLine;
				_details.Text += "Was try to log the exception: " + error.Message + Environment.NewLine;
			}

			ShowReportDialogIfAppropriate(owningForm);
		}

		 public void Report(string message, StackTrace stackTrace, Form owningForm)
		 {
			 PrepareDialog();
			 _notificationText.Text = message;

			 Exception innerMostException = null;
			 _details.Text += "Message (not an exception): " + message + Environment.NewLine;
			 _details.Text += Environment.NewLine;
			 _details.Text += "--Stack--"+ Environment.NewLine;;
			 _details.Text += stackTrace.ToString() + Environment.NewLine; ;


			 AddErrorReportingPropertiesToDetails();

			 Debug.WriteLine(_details.Text);


			 try
			 {
				 Logger.WriteEvent("Got error message " + message);
			 }
			 catch (Exception err)
			 {
				 //We have more than one report of dieing while logging an exception.
				 _details.Text += "****Could not write to log (" + err.Message + ")" + Environment.NewLine;
			 }

			 ShowReportDialogIfAppropriate(owningForm);
		 }

		 private void AddErrorReportingPropertiesToDetails()
		 {

			 _details.Text += Environment.NewLine+"--Error Reporting Properties--"+Environment.NewLine;
			 foreach (string label in ErrorReport.Properties.Keys)
			 {
				 _details.Text += label + ": " + ErrorReport.Properties[label] + Environment.NewLine;
			 }

			 _details.Text += Environment.NewLine+"--Log--"+Environment.NewLine;
			 try
			 {
				 _details.Text += Logger.LogText;
			 }
			 catch (Exception err)
			 {
				 //We have more than one report of dieing while logging an exception.
				 _details.Text += "****Could not read from log: " + err.Message + Environment.NewLine;
			 }
		 }

		 private void PrepareDialog()
		 {
			 CheckDisposed();
			 //
			 // Required for Windows Form Designer support
			 //
			 InitializeComponent();

			 if (!_isLethal)
			 {
				 _sendAndCloseButton.Text = "&Send Email";
				 BackColor = Color.FromArgb(255, 255, 192); //yellow
				 _notificationText.BackColor = BackColor;
				 _pleaseHelpText.BackColor = BackColor;
				 _dontSendEmailLink.Text = "Don't Send Email";
			 }
		 }

		 private void ShowReportDialogIfAppropriate(Form owningForm)
		 {


			 if (ErrorReport.IsOkToInteractWithUser)
			 {
				 s_doIgnoreReport = true;
				 ShowDialog(owningForm);
				 s_doIgnoreReport = false;
			 }
			 else //the test environment already prohibits dialogs but will save the contents of assertions in some log.
			 {
				 Debug.Fail(_details.Text);
			 }


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
			if (ModifierKeys.Equals(Keys.Shift))
			{
				return;
			}
			GatherData();

			Clipboard.SetDataObject(_details.Text, true);

			try
			{
				MAPI msg = new MAPI();
				msg.AddRecipientTo(ErrorReport.EmailAddress);
				if (msg.SendMailDirect(ErrorReport.EmailSubject, _details.Text))
				{
					CloseUp();
					return;
				}
			}
			catch (Exception)
			{
				//swallow it and go to the mailto method
			}

			try
			{
				//EmailMessage msg = new EmailMessage();
				// This currently does not work. The main issue seems to be the length of the error report. mailto
				// apparently has some limit on the length of the message, and we are exceeding that.
				//make it safe, but does too much (like replacing spaces with +'s)
				//string s = System.Web.HttpUtility.UrlPathEncode( m_details.Text);
				//msg.Body = m_details.Text.Replace(Environment.NewLine, "%0A").Replace("\"", "%22").Replace("&", "%26");
				EmailMessage msg = new EmailMessage();
				msg.Body = "<Please paste the details of the crash here>";
				msg.Address = ErrorReport.EmailAddress;
				msg.Subject = ErrorReport.EmailSubject;
				msg.Send();
				CloseUp();
			}
			catch (Exception)
			{
				//swallow it and go to the clipboard method
			}

			if (ErrorReport.EmailAddress != null)
			{
				_details.Text = String.Format(ReportingStrings.ksPleaseEMailThisToUs, ErrorReport.EmailAddress, _details.Text);
			}
			Clipboard.SetDataObject(_details.Text, true);

			CloseUp();
		}

		private void CloseUp()
		{
			if (!_isLethal || ModifierKeys.Equals(Keys.Shift))
			{
				Logger.WriteEvent("Continuing...");
				this.Close();
				return;
			}
			Logger.WriteEvent("Exiting...");
			Process.GetCurrentProcess().Kill();
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
				_attemptToContinueLabel.Visible = true;
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
				_attemptToContinueLabel.Visible = false;
			}
			base.OnKeyUp(e);
		}

		 private void OnJustExit_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		 {
			 CloseUp();
		 }

		 private void ErrorNotificationDialog_Load(object sender, EventArgs e)
		 {

		 }

	}
}