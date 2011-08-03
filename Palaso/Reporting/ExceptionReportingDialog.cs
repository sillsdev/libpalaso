using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Palaso.Email;
using Palaso.Reporting;

namespace Palaso.Reporting
{
	 public class ExceptionReportingDialog : Form, IDisposable
	{
		#region Member variables

		private Label label3;
		private TextBox _details;
		private TextBox _pleaseHelpText;
		private TextBox m_reproduce;
		private Label _attemptToContinueLabel;

		private bool _isLethal;

		private Button _sendAndCloseButton;
		 private LinkLabel _dontSendEmailLink;
		 private TextBox _notificationText;
		 private TextBox textBox1;
		 private ComboBox _methodCombo;
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
			this.m_reproduce = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this._details = new System.Windows.Forms.TextBox();
			this._sendAndCloseButton = new System.Windows.Forms.Button();
			this._pleaseHelpText = new System.Windows.Forms.TextBox();
			this._attemptToContinueLabel = new System.Windows.Forms.Label();
			this._dontSendEmailLink = new System.Windows.Forms.LinkLabel();
			this._notificationText = new System.Windows.Forms.TextBox();
			this.textBox1 = new System.Windows.Forms.TextBox();
			this._methodCombo = new System.Windows.Forms.ComboBox();
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
			// _attemptToContinueLabel
			//
			resources.ApplyResources(this._attemptToContinueLabel, "_attemptToContinueLabel");
			this._attemptToContinueLabel.ForeColor = System.Drawing.Color.Firebrick;
			this._attemptToContinueLabel.Name = "_attemptToContinueLabel";
			//
			// _dontSendEmailLink
			//
			resources.ApplyResources(this._dontSendEmailLink, "_dontSendEmailLink");
			this._dontSendEmailLink.Name = "_dontSendEmailLink";
			this._dontSendEmailLink.TabStop = true;
			this._dontSendEmailLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OnJustExit_LinkClicked);
			//
			// _notificationText
			//
			resources.ApplyResources(this._notificationText, "_notificationText");
			this._notificationText.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
			this._notificationText.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this._notificationText.ForeColor = System.Drawing.Color.Black;
			this._notificationText.Name = "_notificationText";
			this._notificationText.ReadOnly = true;
			this._notificationText.TextChanged += new System.EventHandler(this._notificationText_TextChanged);
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
			// ExceptionReportingDialog
			//
			this.AcceptButton = this._sendAndCloseButton;
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
			this.CancelButton = this._sendAndCloseButton;
			this.ControlBox = false;
			this.Controls.Add(this._methodCombo);
			this.Controls.Add(this.textBox1);
			this.Controls.Add(this._dontSendEmailLink);
			this.Controls.Add(this.m_reproduce);
			this.Controls.Add(this._notificationText);
			this.Controls.Add(this._pleaseHelpText);
			this.Controls.Add(this._details);
			this.Controls.Add(this._attemptToContinueLabel);
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

		 private void SetupMethodCombo()
		 {
			 _methodCombo.Items.Clear();
			 _methodCombo.Items.Add(new ReportingMethod("Send using my email program", "&Email", "mapiWithPopup", SendViaEmail));
			 _methodCombo.Items.Add(new ReportingMethod("Copy to clipboard", "&Copy", "clipboard", PutOnClipboard));
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
				dlg.Report(message, string.Empty, stack, null);
			}
		}
		internal static void ReportMessage(string message, Exception error, bool isLethal)
		{
			if (s_doIgnoreReport)
			{
				return;            // ignore message if we are showing from a previous error
			}

			using (ExceptionReportingDialog dlg = new ExceptionReportingDialog(isLethal))
			{
				dlg.Report(message, null, error,null);
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
			Report(null,null, error, owningForm);
		}


		 public void Report(string message, string messageBeforeStack, Exception error, Form owningForm)
		 {
			 try
			 {
				 if(!string.IsNullOrEmpty(message))
					UsageReporter.ReportExceptionString(message);
				 else if(error!=null)
					 UsageReporter.ReportException(error);
			 }
			 catch
			 {
				 //swallow
			 }

			 PrepareDialog();
			 _notificationText.Text = message;

			 if (!string.IsNullOrEmpty(message))
			 {
				_details.Text += "Message (not an exception): " + message + Environment.NewLine;
				_details.Text += Environment.NewLine;
			 }
			if (!string.IsNullOrEmpty(messageBeforeStack))
			 {
				 _details.Text += messageBeforeStack;
				 _details.Text += Environment.NewLine;
			 }

			 Exception innerMostException = null;
			 _details.Text += ErrorReport.GetHiearchicalExceptionInfo(error, ref innerMostException);

			 //if the exception had inner exceptions, show the inner-most exception first, since that is usually the one
			 //we want the developer to read.
			 if (innerMostException != null)
			 {
				 _details.Text += "Inner-most exception:\r\n" + ErrorReport.GetExceptionText(innerMostException) +
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

		 public void Report(string message, string messageBeforeStack, StackTrace stackTrace, Form owningForm)
		 {
			 PrepareDialog();
			 _notificationText.Text = message;

			_details.Text += "Message (not an exception): " + message + Environment.NewLine;
			 _details.Text += Environment.NewLine;
			 if(!string.IsNullOrEmpty(messageBeforeStack))
			 {
				_details.Text += messageBeforeStack;
				_details.Text += Environment.NewLine;
			 }
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
			 Font = SystemFonts.MessageBoxFont;

			 //
			 // Required for Windows Form Designer support
			 //
			 InitializeComponent();

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
				 _notificationText.BackColor = BackColor;
				 _pleaseHelpText.BackColor = BackColor;
				 textBox1.BackColor = BackColor;
			 }

			 SetupCloseButtonText();
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
					 emailMessage.Body = _details.Text;
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
				 ErrorReport.NotifyUserOfProblem(error,
					 "This program wasn't able to get your email program, if you have one, to send the error message.");
				 return false;
			 }
			 return false;
		 }

		 private void CloseUp()
		{
			if (!_isLethal || ModifierKeys.Equals(Keys.Shift))
			{
				try
				{
					Logger.WriteEvent("Error Dialog: Continuing...");
				}
				catch (Exception)
				{
					//really can't handle an embedded error related to logging
				}
				Close();
				return;
			}
			try
			{
				Logger.WriteEvent("Error Dialog: Exiting...");
			}
			catch (Exception)
			{
				//really can't handle an embedded error related to logging
			}

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

		 private void _notificationText_TextChanged(object sender, EventArgs e)
		 {

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
				 _dontSendEmailLink.Text = "Don't Send Email";
			 }
			 else
			 {
				 _sendAndCloseButton.Text += "&& Exit";
			 }
		 }

		 private ReportingMethod SelectedMethod
		 {
			 get { return ((ReportingMethod) _methodCombo.SelectedItem); }
			 set { _methodCombo.SelectedItem = value; }
		 }
	}
}