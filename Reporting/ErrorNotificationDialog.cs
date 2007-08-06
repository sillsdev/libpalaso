using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace Palaso.Reporting
{
	 public class ErrorNotificationDialog : Form, IDisposable
	{
		#region Member variables

		private Label label2;
		private Label label3;
		private TextBox m_details;
		private TextBox m_notification;
		private TextBox m_reproduce;
		private Label _attemptToContinueLabel;

		private bool _isLethal;

		private Button _closeButton;
		private static bool s_doIgnoreReport = false;

		#endregion

		protected ErrorNotificationDialog(bool isLethal)
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
			System.ComponentModel.ComponentResourceManager resources =
					new System.ComponentModel.ComponentResourceManager(typeof (ErrorNotificationDialog));
			this.label2 = new System.Windows.Forms.Label();
			this.m_reproduce = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.m_details = new System.Windows.Forms.TextBox();
			this._closeButton = new System.Windows.Forms.Button();
			this.m_notification = new System.Windows.Forms.TextBox();
			this._attemptToContinueLabel = new System.Windows.Forms.Label();
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
			// m_details
			//
			resources.ApplyResources(this.m_details, "m_details");
			this.m_details.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.m_details.Name = "m_details";
			this.m_details.ReadOnly = true;
			//
			// btnClose
			//
			resources.ApplyResources(this._closeButton, "btnClose");
			this._closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this._closeButton.Name = "_closeButton";
			this._closeButton.Click += new System.EventHandler(this.btnClose_Click);
			//
			// m_notification
			//
			resources.ApplyResources(this.m_notification, "m_notification");
			this.m_notification.BackColor =
					System.Drawing.Color.FromArgb(((int) (((byte) (192)))),
												  ((int) (((byte) (255)))),
												  ((int) (((byte) (192)))));
			this.m_notification.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.m_notification.ForeColor = System.Drawing.Color.Black;
			this.m_notification.Name = "m_notification";
			this.m_notification.ReadOnly = true;
			//
			// labelAttemptToContinue
			//
			resources.ApplyResources(this._attemptToContinueLabel, "labelAttemptToContinue");
			this._attemptToContinueLabel.ForeColor = System.Drawing.Color.Firebrick;
			this._attemptToContinueLabel.Name = "_attemptToContinueLabel";
			//
			// ErrorReporter
			//
			this.AcceptButton = this._closeButton;
			resources.ApplyResources(this, "$this");
			this.BackColor =
					System.Drawing.Color.FromArgb(((int) (((byte) (192)))),
												  ((int) (((byte) (255)))),
												  ((int) (((byte) (192)))));
			this.CancelButton = this._closeButton;
			this.ControlBox = false;
			this.Controls.Add(this.m_reproduce);
			this.Controls.Add(this.m_notification);
			this.Controls.Add(this.m_details);
			this.Controls.Add(this._attemptToContinueLabel);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label3);
			this.Controls.Add(this._closeButton);
			this.KeyPreview = true;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ErrorReporter";
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
		public static void ReportException(Exception error)
		{
			ReportException(error, null);
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="error"></param>
		/// <param name="parent"></param>
		public static void ReportException(Exception error, Form parent)
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
		public static void ReportException(Exception error, Form parent, bool isLethal)
		{
			if (s_doIgnoreReport)
			{
				return;            // ignore message if we are showing from a previous error
			}

			using (ErrorNotificationDialog e = new ErrorNotificationDialog(isLethal))
			{
				e.HandleError(error, parent);
			}
		}



		/// ------------------------------------------------------------------------------------
		/// <summary>
		///
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected void GatherData()
		{
			m_details.Text += "\r\nTo Reproduce: " + m_reproduce.Text + "\r\n";
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		///
		/// </summary>
		/// <param name="error"></param>
		/// <param name="owner"></param>
		/// ------------------------------------------------------------------------------------
		public void HandleError(Exception error, Form owner)
		{
			CheckDisposed();
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			if (!_isLethal)
			{
				_closeButton.Text = ReportingStrings.ks_Ok;
				BackColor = Color.FromArgb(255, 255, 192); //yellow
				m_notification.BackColor = BackColor;
			}

			Exception innerMostException = null;
			m_details.Text += ErrorReport.GetHiearchicalExceptionInfo(error, ref innerMostException);

			//if the exception had inner exceptions, show the inner-most exception first, since that is usually the one
			//we want the developer to read.
			if (innerMostException != null)
			{
				m_details.Text = "Inner-most exception:\r\n" + ErrorReport.GetExceptionText(innerMostException) +
								 "\r\n\r\nFull, hierarchical exception contents:\r\n" + m_details.Text;
			}

			m_details.Text += "\r\nError Reporting Properties:\r\n";
			foreach (string label in ErrorReport.Properties.Keys)
			{
				m_details.Text += label + ": " + ErrorReport.Properties[label] + "\r\n";
			}

			m_details.Text += Logger.LogText;
			Debug.WriteLine(m_details.Text);
			if (innerMostException != null)
			{
				error = innerMostException;
			}
			Logger.WriteEvent("Got exception " + error.GetType().Name);

			if (ErrorReport.IsOkToInteractWithUser)
			{
				s_doIgnoreReport = true;
				ShowDialog(owner);
				s_doIgnoreReport = false;
			}
			else //the test environment already prohibits dialogs but will save the contents of assertions in some log.
			{
				Debug.Fail(m_details.Text);
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

			Clipboard.SetDataObject(m_details.Text, true);

			try
			{
				MAPI msg = new MAPI();
				msg.AddRecipientTo(ErrorReport.EmailAddress);
				if (msg.SendMailDirect(ErrorReport.EmailSubject, m_details.Text))
				{
					CloseUp();
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
				m_details.Text = String.Format(ReportingStrings.ksPleaseEMailThisToUs, ErrorReport.EmailAddress, m_details.Text);
			}
			Clipboard.SetDataObject(m_details.Text, true);

			CloseUp();
		}

		private void CloseUp()
		{
			if (!_isLethal || ModifierKeys.Equals(Keys.Shift))
			{
				Logger.WriteEvent("Continuing...");
				return;
			}
			Logger.WriteEvent("Exiting...");
			Application.Exit();
			//still didn't work? Sheesh.
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

	}
}