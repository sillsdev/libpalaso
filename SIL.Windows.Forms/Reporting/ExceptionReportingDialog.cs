using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using L10NSharp;
using SIL.Email;
using SIL.Reporting;
using SIL.Windows.Forms.Miscellaneous;
using static System.String;

namespace SIL.Windows.Forms.Reporting
{
	/// <summary>
	/// Display exception reporting dialog.
	/// NOTE: It is recommended to call one of SIL.Reporting.ErrorReport.Report(Non)Fatal*
	/// methods instead of instantiating this class.
	/// </summary>
	public class ExceptionReportingDialog : Form
	{
		#region Local structs
		private struct ExceptionReportingData
		{
			public ExceptionReportingData(string message, string messageBeforeStack,
				Exception error, StackTrace stackTrace, Form owningForm, int threadId)
			{
				Message = message;
				MessageBeforeStack = messageBeforeStack;
				Error = error;
				StackTrace = stackTrace;
				OwningForm = owningForm;
				ThreadId = threadId;
			}

			public readonly string Message;
			public readonly string MessageBeforeStack;
			public readonly Exception Error;
			public readonly Form OwningForm;
			public readonly StackTrace StackTrace;
			public readonly int ThreadId;
		}
		#endregion

		#region Member variables

		private Label _detailsForDevelopers;
		private TextBox _details;
		private Label _lblPleaseHelp;
		private TextBox _reproduce;

		private readonly bool _isLethal;

		private Button _sendAndCloseButton;
		private Label _notificationText;
		private Label _lblStepsToReproduce;
		private ComboBox _methodCombo;
		private Button _privacyNoticeButton;
		private Label _emailAddress;
		private static bool s_doIgnoreReport;
		private TableLayoutPanel tableLayoutPanel1;
		private System.ComponentModel.IContainer components;

		/// <summary>
		/// Stack with exception data.
		/// </summary>
		/// <remarks>When an exception occurs on a background thread ideally it should be handled
		/// by the application. However, not all applications are always implemented to do it
		/// that way, so we need a safe fall back that doesn't pop up a dialog from a (non-UI)
		/// background thread.
		///
		/// This implementation creates a control on the UI thread
		/// (WinFormsExceptionHandler.ControlOnUIThread) in order to be able to check
		/// if invoke is required. When an exception occurs on a background thread we push the
		/// exception data to an exception data stack and try to invoke the exception dialog on
		/// the UI thread. In case that the UI thread already shows an exception dialog we skip
		/// the exception (similar to the behavior we already have when we get an exception on
		/// the UI thread while displaying the exception dialog). Otherwise we display the
		/// exception dialog, appending the messages from the exception data stack.</remarks>
		private static readonly Stack<ExceptionReportingData> s_reportDataStack =
			new Stack<ExceptionReportingData>();

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
				throw new ObjectDisposedException(
					$"'{GetType().Name}' in use after being disposed.");
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExceptionReportingDialog));
			this._reproduce = new System.Windows.Forms.TextBox();
			this._detailsForDevelopers = new System.Windows.Forms.Label();
			this._details = new System.Windows.Forms.TextBox();
			this._sendAndCloseButton = new System.Windows.Forms.Button();
			this._lblPleaseHelp = new System.Windows.Forms.Label();
			this._notificationText = new System.Windows.Forms.Label();
			this._lblStepsToReproduce = new System.Windows.Forms.Label();
			this._methodCombo = new System.Windows.Forms.ComboBox();
			this._privacyNoticeButton = new System.Windows.Forms.Button();
			this._emailAddress = new System.Windows.Forms.Label();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// m_reproduce
			// 
			this._reproduce.AcceptsReturn = true;
			this._reproduce.AcceptsTab = true;
			resources.ApplyResources(this._reproduce, "m_reproduce");
			this.tableLayoutPanel1.SetColumnSpan(this._reproduce, 2);
			this._reproduce.Name = "m_reproduce";
			// 
			// _detailsForDevelopers
			// 
			resources.ApplyResources(this._detailsForDevelopers, "_detailsForDevelopers");
			this.tableLayoutPanel1.SetColumnSpan(this._detailsForDevelopers, 2);
			this._detailsForDevelopers.Name = "_detailsForDevelopers";
			// 
			// _details
			// 
			resources.ApplyResources(this._details, "_details");
			this._details.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.tableLayoutPanel1.SetColumnSpan(this._details, 2);
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
			// _lblPleaseHelp
			// 
			resources.ApplyResources(this._lblPleaseHelp, "_lblPleaseHelp");
			this._lblPleaseHelp.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
			this.tableLayoutPanel1.SetColumnSpan(this._lblPleaseHelp, 2);
			this._lblPleaseHelp.ForeColor = System.Drawing.Color.Black;
			this._lblPleaseHelp.Name = "_lblPleaseHelp";
			// 
			// _notificationText
			// 
			resources.ApplyResources(this._notificationText, "_notificationText");
			this._notificationText.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
			this.tableLayoutPanel1.SetColumnSpan(this._notificationText, 2);
			this._notificationText.ForeColor = System.Drawing.Color.Black;
			this._notificationText.Name = "_notificationText";
			// 
			// _lblStepsToReproduce
			// 
			resources.ApplyResources(this._lblStepsToReproduce, "_lblStepsToReproduce");
			this._lblStepsToReproduce.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
			this.tableLayoutPanel1.SetColumnSpan(this._lblStepsToReproduce, 2);
			this._lblStepsToReproduce.ForeColor = System.Drawing.Color.Black;
			this._lblStepsToReproduce.Name = "_lblStepsToReproduce";
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
			this._privacyNoticeButton.Image = global::SIL.Windows.Forms.Properties.Resources.spy16x16;
			this._privacyNoticeButton.Name = "_privacyNoticeButton";
			this._privacyNoticeButton.UseVisualStyleBackColor = true;
			this._privacyNoticeButton.Click += new System.EventHandler(this._privacyNoticeButton_Click);
			// 
			// _emailAddress
			// 
			this._emailAddress.ForeColor = System.Drawing.Color.DimGray;
			resources.ApplyResources(this._emailAddress, "_emailAddress");
			this._emailAddress.Name = "_emailAddress";
			// 
			// tableLayoutPanel1
			// 
			resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
			this.tableLayoutPanel1.Controls.Add(this._notificationText, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this._sendAndCloseButton, 1, 7);
			this.tableLayoutPanel1.Controls.Add(this._privacyNoticeButton, 0, 7);
			this.tableLayoutPanel1.Controls.Add(this._emailAddress, 0, 6);
			this.tableLayoutPanel1.Controls.Add(this._methodCombo, 1, 6);
			this.tableLayoutPanel1.Controls.Add(this._lblPleaseHelp, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this._lblStepsToReproduce, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this._reproduce, 0, 3);
			this.tableLayoutPanel1.Controls.Add(this._details, 0, 5);
			this.tableLayoutPanel1.Controls.Add(this._detailsForDevelopers, 0, 4);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			// 
			// ExceptionReportingDialog
			// 
			this.AcceptButton = this._sendAndCloseButton;
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
			this.Controls.Add(this.tableLayoutPanel1);
			this.KeyPreview = true;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ExceptionReportingDialog";
			this.ShowIcon = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.TopMost = true;
			this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ExceptionReportingDialog_KeyPress);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.ResumeLayout(false);

		}

		private void SetupMethodCombo()
		{
			_methodCombo.Items.Clear();
			string reportingMethod, closeButtonLabelLethal, closeButtonLabelNonLethal;
			try
			{
				reportingMethod = LocalizationManager.GetString(
					"ExceptionReportingDialog.ReportingMethod.Email",
					"Send using my email program");
			}
			catch (Exception e)
			{
				try { Logger.WriteError(e); } catch { /* We tried. */ }
				reportingMethod = "Send using my email program";
			}
			try
			{
				closeButtonLabelLethal = LocalizationManager.GetString(
					"ExceptionReportingDialog.ReportingMethod.EmailCloseButtonLabelLethal",
					"&Email and Exit", "Ampersand is optional, to indicate accelerator key");
			}
			catch (Exception e)
			{
				try { Logger.WriteError(e); } catch { /* We tried. */ }
				closeButtonLabelLethal = "&Email and Exit";
			}
			try
			{
				closeButtonLabelNonLethal = LocalizationManager.GetString(
					"ExceptionReportingDialog.ReportingMethod.EmailCloseButtonLabel",
					"&Email", "Ampersand is optional, to indicate accelerator key");
			}
			catch (Exception e)
			{
				try { Logger.WriteError(e); } catch { /* We tried. */ }
				closeButtonLabelNonLethal = "&Email";
			}
			_methodCombo.Items.Add(new ReportingMethod(
				reportingMethod, closeButtonLabelLethal, closeButtonLabelNonLethal,
				"mapiWithPopup", SendViaEmail));
			
			try
			{
				reportingMethod = LocalizationManager.GetString(
					"ExceptionReportingDialog.ReportingMethod.CopyToClipboard",
					"Copy to clipboard");
			}
			catch (Exception e)
			{
				try { Logger.WriteError(e); } catch { /* We tried. */ }
				reportingMethod = "Copy to clipboard";
			}
			try
			{
				closeButtonLabelLethal = LocalizationManager.GetString(
					"ExceptionReportingDialog.ReportingMethod.CopyCloseButtonLabelLethal",
					"&Copy and Exit", "Ampersand is optional, to indicate accelerator key");
			}
			catch (Exception e)
			{
				try { Logger.WriteError(e); } catch { /* We tried. */ }
				closeButtonLabelLethal = "&Copy and Exit";
			}
			try
			{
				closeButtonLabelNonLethal = LocalizationManager.GetString(
					"ExceptionReportingDialog.ReportingMethod.CopyCloseButtonLabel",
					"&Copy", "Ampersand is optional, to indicate accelerator key");
			}
			catch (Exception e)
			{
				try { Logger.WriteError(e); } catch { /* We tried. */ }
				closeButtonLabelNonLethal = "&Copy";
			}
			_methodCombo.Items.Add(new ReportingMethod(
				reportingMethod, closeButtonLabelLethal, closeButtonLabelNonLethal,
				"clipboard", PutOnClipboard));
		}

		private class ReportingMethod
		{
			private readonly string _label;
			public readonly string CloseButtonLabelLethal;
			public readonly string CloseButtonLabelNonLethal;
			public readonly string Id;
			public readonly Func<bool> Method;

			public ReportingMethod(string label, string closeButtonLabelLethal,
				string closeButtonLabelNonLethal, string id, Func<bool> method)
			{
				_label = label;
				CloseButtonLabelLethal = closeButtonLabelLethal;
				CloseButtonLabelNonLethal = closeButtonLabelNonLethal;
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
				lock (s_reportDataStack)
				{
					s_reportDataStack.Push(new ExceptionReportingData(null, null, error,
						null, parent, Thread.CurrentThread.ManagedThreadId));
				}
				return; // ignore message if we are showing from a previous error
			}

			using (ExceptionReportingDialog dlg = new ExceptionReportingDialog(isLethal))
				dlg.Report(error, parent);
		}

		internal static void ReportMessage(string message, StackTrace stack, bool isLethal)
		{
			if (s_doIgnoreReport)
			{
				lock (s_reportDataStack)
				{
					s_reportDataStack.Push(new ExceptionReportingData(message, null, null,
						stack, null, Thread.CurrentThread.ManagedThreadId));
				}
				return; // ignore message if we are showing from a previous error
			}

			using (ExceptionReportingDialog dlg = new ExceptionReportingDialog(isLethal))
				dlg.Report(message, Empty, stack, null);
		}

		internal static void ReportMessage(string message, Exception error, bool isLethal)
		{
			if (s_doIgnoreReport)
			{
				lock (s_reportDataStack)
				{
					s_reportDataStack.Push(new ExceptionReportingData(message, null, error,
						null, null, Thread.CurrentThread.ManagedThreadId));
				}
				return; // ignore message if we are showing from a previous error
			}

			using (ExceptionReportingDialog dlg = new ExceptionReportingDialog(isLethal))
			{
				dlg.Report(message, null, error,null);
			}
		}

		protected void GatherData()
		{
			_details.Text += Environment.NewLine + "To Reproduce: " + _reproduce.Text + Environment.NewLine;
		}

		public void Report(Exception error, Form owningForm)
		{
			Report(null,null, error, owningForm);
		}

		public void Report(string message, string messageBeforeStack, Exception error, Form owningForm)
		{
			lock (s_reportDataStack)
			{
				s_reportDataStack.Push(new ExceptionReportingData(message, messageBeforeStack, error,
					null, owningForm, Thread.CurrentThread.ManagedThreadId));
			}

			if (WinFormsExceptionHandler.InvokeRequired)
			{
				// we got called from a background thread.
				WinFormsExceptionHandler.ControlOnUIThread.Invoke(
					new Action(ReportInternal));
				return;
			}

			ReportInternal();
		}

		public void Report(string message, string messageBeforeStack, StackTrace stackTrace, Form owningForm)
		{
			lock (s_reportDataStack)
			{
				s_reportDataStack.Push(new ExceptionReportingData(message, messageBeforeStack, null,
					stackTrace, owningForm, Thread.CurrentThread.ManagedThreadId));
			}

			if (WinFormsExceptionHandler.InvokeRequired)
			{
				// we got called from a background thread.
				WinFormsExceptionHandler.ControlOnUIThread.Invoke(
					new Action(ReportInternal));
				return;
			}

			ReportInternal();
		}

		private void ReportInternal()
		{
			// This method will/should always be called on the UI thread
			Debug.Assert(!WinFormsExceptionHandler.InvokeRequired);

			ExceptionReportingData reportingData;
			lock (s_reportDataStack)
			{
				if (s_reportDataStack.Count <= 0)
					return;

				reportingData = s_reportDataStack.Pop();
			}

			ReportExceptionToAnalytics(reportingData);

			if (s_doIgnoreReport)
				return; // ignore message if we are showing from a previous error

			PrepareDialog();

			if (!IsNullOrEmpty(reportingData.Message))
				_notificationText.Text = reportingData.Message;

			var bldr = new StringBuilder();
			var innerMostException = FormatMessage(bldr, reportingData);
			bldr.Append(AddMessagesFromBackgroundThreads());
			_details.Text += bldr.ToString();

			Debug.WriteLine(_details.Text);
			var error = reportingData.Error;
			if (error != null)
			{
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
					//We have more than one report of dying while logging an exception.
					_details.Text += "****Could not write to log (" + err.Message + ")" + Environment.NewLine;
					_details.Text += "Was trying to log the exception: " + error.Message + Environment.NewLine;
					_details.Text += "Recent events:" + Environment.NewLine;
					_details.Text += Logger.MinorEventsLog;
				}
			}
			else
			{
				try
				{
					Logger.WriteEvent("Got error message " + reportingData.Message);
				}
				catch (Exception err)
				{
					//We have more than one report of dying while logging an exception.
					_details.Text += "****Could not write to log (" + err.Message + ")" + Environment.NewLine;
				}
			}

			ShowReportDialogIfAppropriate(reportingData.OwningForm);
		}

		private static void ReportExceptionToAnalytics(ExceptionReportingData reportingData)
		{
			try
			{
				if (!IsNullOrEmpty(reportingData.Message))
					UsageReporter.ReportExceptionString(reportingData.Message);
				else if (reportingData.Error != null)
					UsageReporter.ReportException(reportingData.Error);
			}
			catch
			{
				//swallow
			}
		}

		private static string AddMessagesFromBackgroundThreads()
		{
			var bldr = new StringBuilder();
			for (bool messageOnStack = AddNextMessageFromStack(bldr); messageOnStack;)
				messageOnStack = AddNextMessageFromStack(bldr);
			return bldr.ToString();
		}

		private static bool AddNextMessageFromStack(StringBuilder bldr)
		{
			ExceptionReportingData data;
			lock (s_reportDataStack)
			{
				if (s_reportDataStack.Count <= 0)
					return false;

				data = s_reportDataStack.Pop();
			}

			ReportExceptionToAnalytics(data);
			bldr.AppendLine("---------------------------------");
			bldr.AppendFormat("The following exception occurred on a different thread ({0}) at about the same time:",
				data.ThreadId);
			bldr.AppendLine();
			bldr.AppendLine();
			FormatMessage(bldr, data);
			return true;
		}

		private static Exception FormatMessage(StringBuilder bldr, ExceptionReportingData data)
		{
			if (!IsNullOrEmpty(data.Message))
			{
				bldr.Append("Message (not an exception): ");
				bldr.AppendLine(data.Message);
				bldr.AppendLine();
			}
			if (!IsNullOrEmpty(data.MessageBeforeStack))
			{
				bldr.AppendLine(data.MessageBeforeStack);
			}

			if (data.Error != null)
			{
				Exception innerMostException = null;
				bldr.Append(ErrorReport.GetHiearchicalExceptionInfo(data.Error, ref innerMostException));
				// If the exception had inner exceptions, show the inner-most exception first,
				// since that is likely to be the one most useful for the developer to read first.
				if (innerMostException != null)
				{
					var oldText = bldr.ToString();
					bldr.Clear();
					bldr.AppendLine("Inner-most exception:");
					bldr.AppendLine(ErrorReport.GetExceptionText(innerMostException));
					bldr.AppendLine();
					bldr.AppendLine("Full, hierarchical exception contents:");
					bldr.Append(oldText);
				}
				AddErrorReportingPropertiesToDetails(bldr);
				return innerMostException;
			}
			if (data.StackTrace != null)
			{
				bldr.AppendLine("--Stack--");
				bldr.AppendLine(data.StackTrace.ToString());
			}

			return null;
		}

		private static void AddErrorReportingPropertiesToDetails(StringBuilder bldr)
		{
			bldr.AppendLine();
			bldr.AppendLine("--Error Reporting Properties--");
			foreach (string label in ErrorReport.Properties.Keys)
			{
				bldr.Append(label);
				bldr.Append(": ");
				bldr.AppendLine(ErrorReport.Properties[label]);
			}

			bldr.AppendLine();
			bldr.AppendLine("--Log--");
			try
			{
				bldr.Append(Logger.LogText);
			}
			catch (Exception err)
			{
				//We have more than one report of dieing while logging an exception.
				bldr.AppendLine("****Could not read from log: " + err.Message);
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
			_emailAddress.Text = ErrorReport.EmailAddress;
			SetupMethodCombo();

			foreach (ReportingMethod method in _methodCombo.Items)
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
				try
				{
					_notificationText.Text = LocalizationManager.GetString(
						"ExceptionReportingDialog.NotificationTextNonLethal",
						"Take Courage. It'll work out.",
						"For more formal cultures, something less cheeky might be better. In most " +
						"cases, this will be replaced by a more useful message, but there are " +
						"situations where a user could see this.");
				}
				catch (Exception e)
				{
					try { Logger.WriteError(e); } catch { /* We tried. */ }
				}

				_notificationText.BackColor = BackColor;
				_lblPleaseHelp.BackColor = BackColor;
				_lblStepsToReproduce.BackColor = BackColor;
			}

			SetupCloseButtonText();

			SetLocalizedText();
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
				string localizedEmailInstructions = null;
				try
				{
					if (LocalizationManager.UILanguageId != "en")
					{
						localizedEmailInstructions = LocalizationManager.GetString(
							"ExceptionReportingDialog.EmailInstructions",
							"(Please e-mail this to)");
						if (localizedEmailInstructions == "(Please e-mail this to)")
							localizedEmailInstructions = null;
					}
				}
				catch (Exception e)
				{
					try { Logger.WriteError(e); } catch { /* We tried. */ }
				}
				_details.Text = $"Please e-mail this to {localizedEmailInstructions} {ErrorReport.EmailAddress}" +
					$"{Environment.NewLine}{_details.Text}";
			}
			try
			{
				PortableClipboard.SetText(_details.Text);
			}
			catch(Exception e)
			{
				Logger.WriteError("PutOnClipboard failed.", e);
				return false;
			}

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
				PutOnClipboard();
				ErrorReport.NotifyUserOfProblem(error,
					"This program wasn't able to get your email program, if you have one, to send the error message.  " +
					"The contents of the error message has been placed on your Clipboard.");
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
				try
				{
					_sendAndCloseButton.Text = LocalizationManager.GetString(
						"ExceptionReportingDialog.ContinueButtonLabel", "Continue");
				}
				catch (Exception ex)
				{
					try { Logger.WriteError(ex); } catch { /* We tried. */ }
				}
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

		private void _methodCombo_SelectedIndexChanged(object sender, EventArgs e)
		{
			SetupCloseButtonText();
		}

		private void SetupCloseButtonText() =>
			_sendAndCloseButton.Text = _isLethal ? SelectedMethod.CloseButtonLabelLethal :
				SelectedMethod.CloseButtonLabelNonLethal;

		private void SetLocalizedText()
		{
			try
			{
				if (LocalizationManager.UILanguageId == "en")
					return;
			}
			catch (Exception e)
			{
				try { Logger.WriteError(e); } catch { /* We tried. */ }
				// If we couldn't get the current UILanguageId without crashing, we don't have much
				// hope of retrieving localized strings. Just use what Designer resources supplied.
				return;
			}

			// All of these strings should be kept in sync with the ones in the Designer-generated
			// We opted to do it this way instead of using the LocalizationExtender so we could
			// ensure that applying localizations to the error reporting form would not itself
			// result in a fatal error.
			try
			{
				Text = LocalizationManager.GetString("ExceptionReportingDialog.WindowTitle",
					"Error");
			}
			catch (Exception e)
			{
				try { Logger.WriteError(e); } catch { /* We tried. */ }
			}

			try
			{
				_detailsForDevelopers.Text = LocalizationManager.GetString(
					"ExceptionReportingDialog._detailsForDevelopers",
					"Details for the developers:");
			}
			catch (Exception e)
			{
				try { Logger.WriteError(e); } catch { /* We tried. */ }
			}

			try
			{
				_lblPleaseHelp.Text = LocalizationManager.GetString(
					"ExceptionReportingDialog._lblPleaseHelp",
					"To help us fix the problem, we need to gather information on what went wrong.");
			}
			catch (Exception e)
			{
				try { Logger.WriteError(e); } catch { /* We tried. */ }
			}

			if (_isLethal)
			{
				try
				{
					_notificationText.Text = LocalizationManager.GetString(
						"ExceptionReportingDialog.NotificationTextLethal",
						"Well, this is embarrassing.",
						"For more formal cultures, something less cheeky might be better. In most " +
						"cases, this will be replaced by a more useful message, but there are " +
						"situations where a user could see this.");
				}
				catch (Exception e)
				{
					try { Logger.WriteError(e); } catch { /* We tried. */ }
				}
			}

			try
			{
				_lblStepsToReproduce.Text = LocalizationManager.GetString(
					"ExceptionReportingDialog._lblStepsToReproduce",
					"Can you list the steps to take to make this happen again, or tell us anything that might be helpful?");
			}
			catch (Exception e)
			{
				try { Logger.WriteError(e); } catch { /* We tried. */ }
			}

			try
			{
				_privacyNoticeButton.Text = LocalizationManager.GetString(
					"ExceptionReportingDialog._privacyNoticeButton",
					"Privacy Notice...");
			}
			catch (Exception e)
			{
				try { Logger.WriteError(e); } catch { /* We tried. */ }
			}
		}

		private ReportingMethod SelectedMethod
		{
			get => ((ReportingMethod) _methodCombo.SelectedItem);
			set => _methodCombo.SelectedItem = value;
		}

		/// <summary>
		/// Although this is public and can be set externally for an application that has a custom
		/// policy (i.e., does not use Jira), be aware that if you want it to be localized, you
		/// will need to reset it if the UI locale changes.
		/// </summary>
		public static string PrivacyNotice;

		private static string LocalizedPrivacyNotice => PrivacyNotice ?? GetDefaultPrivacyNotice();

		private static string GetDefaultPrivacyNotice()
		{
			var bldr = new StringBuilder();
			string nextPara;
			try
			{
				nextPara = LocalizationManager.GetString("ExceptionReportingDialog.Privacy.Para1",
					"If you don't care who reads your bug report, you can disregard this notice.");
			}
			catch (Exception e)
			{
				try { Logger.WriteError(e); } catch { /* We tried. */ }

				nextPara = "If you don't care who reads your bug report, you can disregard this " +
					"notice.";
			}
			bldr.Append(nextPara);
			bldr.Append(Environment.NewLine);
			bldr.Append(Environment.NewLine);

			// REVIEW: Is the last line (which I have put in parentheses) true and/or helpful?
			try
			{
				nextPara = LocalizationManager.GetString("ExceptionReportingDialog.Privacy.Para2",
					"When you submit a crash report or other issue, it goes into our issue " +
					"tracking system, \"Jira\", which is available via the web at " +
					"https://jira.sil.org/issues." +
					"(This is a normal way to handle issues in an open-source project.)");
			}
			catch (Exception e)
			{
				try { Logger.WriteError(e); } catch { /* We tried. */ }

				nextPara = "When you submit a crash report or other issue, it goes into our issue " +
					"tracking system, \"Jira\", which is available via the web at " +
					"https://jira.sil.org/issues." +
					"(This is a normal way to handle issues in an open-source project.)";
			}
			bldr.Append(nextPara);
			bldr.Append(Environment.NewLine);
			bldr.Append(Environment.NewLine);

			try
			{
				nextPara = LocalizationManager.GetString("ExceptionReportingDialog.Privacy.Para3",
					"Our issue-tracking system is not searchable by those without an account. " +
					"Therefore, search engines (like Google) will not find your bug reports.");
			}
			catch (Exception e)
			{
				try { Logger.WriteError(e); } catch { /* We tried. */ }

				nextPara = "Our issue-tracking system is not searchable by those without an " +
					"account. Therefore, search engines (like Google) will not find your bug " +
					"reports.";
			}
			bldr.Append(nextPara);
			bldr.Append(Environment.NewLine);
			bldr.Append(Environment.NewLine);
			
			try
			{
				nextPara = LocalizationManager.GetString("ExceptionReportingDialog.Privacy.Para4",
					"However, anyone can make an account and then read your report. So if you " +
					"have something private to say, please send it to one of the developers " +
					"privately with a note that you don't want the issue in our issue tracking " +
					"system. If necessary, we can make a place-holder for your issue without " +
					"the private information so the issue can still be tracked and not get lost.");
			}
			catch (Exception e)
			{
				try { Logger.WriteError(e); } catch { /* We tried. */ }

				nextPara = "However, anyone can make an account and then read your report. So if" +
					" you have something private to say, please send it to one of the developers" +
					" privately with a note that you don't want the issue in our issue tracking " +
					"system. If necessary, we can make a place-holder for your issue without " +
					"the private information so the issue can still be tracked and not get lost.";
			}
			bldr.Append(nextPara);

			return bldr.ToString();
		}

		private void _privacyNoticeButton_Click(object sender, EventArgs e)
		{
			string caption;
			try
			{
				caption = LocalizationManager.GetString(
					"ExceptionReportingDialog.PrivacyNoticeCaption", "Privacy Notice");
			}
			catch (Exception ex)
			{
				try { Logger.WriteError(ex); } catch { /* We tried. */ }
				caption = "Privacy Notice";
			}
			MessageBox.Show(LocalizedPrivacyNotice, caption);
		}

		private void ExceptionReportingDialog_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar== 27)//ESCAPE
				CloseUp();
		}
	}
}