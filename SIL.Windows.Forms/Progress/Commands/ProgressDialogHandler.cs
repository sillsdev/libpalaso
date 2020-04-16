//For UML diagram, see ProgressSystem.uml (done in StarUML)

using System;
using System.Diagnostics;
using System.Windows.Forms;
using SIL.Progress.Commands;
using SIL.Reporting;

namespace SIL.Windows.Forms.Progress.Commands
{
	/// <summary>
	/// This class can be used to display a ProgressDialog with multiple steps which are handled in a background thread.
	/// </summary>
	public class ProgressDialogHandler
	{
		#region Delegates

		/// <summary>
		/// A delegate for a method which takes an exception as its only parmeter
		/// </summary>
		public delegate void ExceptionMethodInvoker(Exception e);

		#endregion

		private readonly AsyncCommand _currentCommand;
		private readonly Form _parentForm;
		private ProgressDialog _progressDialog;

		/// <summary>
		/// This constructor will immediately show the ProgressDialog in a non-modal fashion.
		/// </summary>
		/// <param name="parentForm">The parent, or owning form for the ProgressDialog</param>
		/// <param name="command">The AsyncCommand c</param>
		[CLSCompliant (false)]
		public ProgressDialogHandler(Form parentForm, BasicCommand command)
			: this(parentForm, command, "From Handler for: " + command.GetType())
		{
		}

		/// <summary>
		/// This constructor will immediately show the ProgressDialog in a non-modal fashion.
		/// </summary>
		/// <param name="parentForm">The parent, or owning form for the ProgressDialog</param>
		/// <param name="command">The AsyncCommand c</param>
		/// <param name="progressTitleText">The title to use on the ProgressDialog</param>
		[CLSCompliant (false)]
		public ProgressDialogHandler(Form parentForm, BasicCommand command, string progressTitleText)
			: this(parentForm, command, progressTitleText, false)
		{
		}

		/// <summary>
		/// This constructor provides the option of making the ProgressDialog modal.
		/// </summary>
		/// <param name="parentForm">The parent, or owning form for the ProgressDialog</param>
		/// <param name="command">The AsyncCommand c</param>
		/// <param name="progressTitleText">The title to use on the ProgressDialog</param>
		/// <param name="showModally">true if you want to use this modally, false otherwise. If you pass true, you must use ShowModal later to show the ProgressDialog</param>
		[CLSCompliant (false)]
		public ProgressDialogHandler(Form parentForm, BasicCommand command, string progressTitleText, bool showModally)
		{
			_parentForm = parentForm;
			_currentCommand = command;
			command.InitializeCallback = InitializeProgress;
			command.ProgressCallback = UpdateProgress;
			command.PrimaryStatusTextCallback = UpdateStatus1;
			command.SecondaryStatusTextCallback = UpdateOverview;

			_currentCommand.BeginCancel += OnCommand_BeginCancel;
			_currentCommand.EnabledChanged += OnCommand_EnabledChanged;
			_currentCommand.Error += OnCommand_Error;
			_currentCommand.Finish += OnCommand_Finish;

			_progressDialog = new ProgressDialog
								{
									Text = progressTitleText
								};
			_progressDialog.CancelRequested += _progressDialog_Cancelled;
			_progressDialog.Owner = parentForm;
			_progressDialog.CanCancel = true;
			//To use this progress in a modal way you need to call ShowModal after you have setup the ProgressDialogState
			if (!showModally)
			{
				//if it is not modal then we can show the dialog and it won't inhibit the rest of the setup and calling.
				_progressDialog.Show();
			}
		}

		/// <summary>
		/// tests (or anything else that is rapidly chaining these things together)
		/// should use this to make sure we're ready to go on to the next activity
		/// </summary>
		public bool TestEverythingClosedUp
		{
			get { return _progressDialog == null || _progressDialog.Visible == false; }
		}

		public event EventHandler Finished;

		/// <summary>
		/// To be called when this handler was construced with showModally set to true.
		/// <param name="state"></param>
		/// </summary>
		public void ShowModal(ProgressDialogProgressState state)
		{
			Debug.Assert(_progressDialog != null, @"If this is null then the behavior of this class has changed, and this method needs to.");
			if (state == null)
			{
				throw new ArgumentNullException(@"state",
												@"State must be set to a ProgressDialogState object which is related to this handler.");
			}
			_currentCommand.BeginInvoke(state);
			_progressDialog.ShowDialog();
		}

		public void InitializeProgress(int minimum, int maximum)
		{
			if (NeedInvoke())
			{
				_parentForm.BeginInvoke(
					new InitializeProgressCallback(InitializeProgressCore),
					new object[] {minimum, maximum});
			}
			else
			{
				InitializeProgressCore(minimum, maximum);
			}
		}

		public void UpdateProgress(int progress)
		{
			if (NeedInvoke())
			{
				_parentForm.BeginInvoke(
					new ProgressCallback(UpdateProgressCore),
					new object[] {progress});
			}
			else
			{
				UpdateProgressCore(progress);
			}
		}

		public void UpdateStatus1(string statusText)
		{
			if (_parentForm == null)
				return;

			if (NeedInvoke())
			{
				_parentForm.BeginInvoke(
					new StatusCallback(UpdateStatusCore),
					new object[] {statusText});
			}
			else
			{
				UpdateStatusCore(statusText);
			}
		}

		private bool NeedInvoke()
		{
			return _progressDialog.InvokeRequired;
			//  return _parentForm != null && _parentForm.InvokeRequired;
		}

		private void UpdateOverview(string text)
		{
			if (NeedInvoke())
			{
				_parentForm.BeginInvoke(
					new StatusCallback(UpdateOverviewCore),
					new object[] {text});
			}
			else
			{
				UpdateOverviewCore(text);
			}
		}


		private void InitializeProgressCore(int minimum, int maximum)
		{
			_progressDialog.ProgressRangeMinimum = minimum;
			_progressDialog.ProgressRangeMaximum = maximum;
		}

		private void UpdateProgressCore(int progress)
		{
			_progressDialog.Progress = progress;
		}

		private void UpdateStatusCore(string statusText)
		{
			_progressDialog.StatusText = statusText;
		}

		private void UpdateOverviewCore(string text)
		{
			_progressDialog.Overview = text;
		}


		private void Finish()
		{
			Debug.WriteLine("ProgressDialogHandler:Finish");

			_progressDialog.ForceClose();
			_progressDialog = null;
			if (Finished != null)
			{
				Finished.BeginInvoke(this, null, null, null); //jh changed this from Invoke()
				// Finished.Invoke(this, null);//jh changed this from Invoke()
			}

			/* if (ParentFormIsClosing)
			{
				_parentForm.Close();
			}
			else
			*/
			{
				_currentCommand.Enabled = true;
			}
		}

		private void Finish(Exception e)
		{
			MessageBox.Show(e.ToString());
			Finish();
		}

		private void FinishWithUnspecifiedError()
		{
			ErrorReport.NotifyUserOfProblem("An error occurred while processing your request.");
			Finish();
		}

		public event EventHandler Cancelled;

		private void _progressDialog_Cancelled(object sender, EventArgs e)
		{
			if (Cancelled != null)
			{
				Cancelled.Invoke(this, null); //REVIEW jh wesay
			}
			_currentCommand.Cancel();
		}

		public void Close()
		{
			Debug.WriteLine("ProgressDialogHandler:Close");

			_progressDialog.Close();
		}


		private void OnCommand_BeginCancel(object sender, EventArgs e)
		{
			Close();
		}

		private void OnCommand_EnabledChanged(object sender, EventArgs e)
		{
			// button1.Enabled = _currentCommand.Enabled;
		}

		private void OnCommand_Error(object sender, ErrorEventArgs e)
		{
			if (e.Exception == null)
			{
				if (NeedInvoke())
				{
					_parentForm.BeginInvoke(new MethodInvoker(FinishWithUnspecifiedError));
				}
				else
				{
					FinishWithUnspecifiedError();
				}
			}
			else
			{
				if (_parentForm != null && NeedInvoke())
				{
					_parentForm.BeginInvoke(new ExceptionMethodInvoker(Finish), new object[] {e.Exception});
				}
				else
				{
					Finish(e.Exception);
				}
			}
		}

		private void OnCommand_Finish(object sender, EventArgs e)
		{
			Debug.WriteLine("ProgressDialogHandler:OnCommand_Finish");
			if (NeedInvoke())
			{
				Debug.WriteLine("ProgressDialogHandler: begin(Finish)");
				_parentForm.Invoke(new MethodInvoker(Finish));
				// _parentForm.BeginInvoke(new MethodInvoker(Finish));
				Debug.WriteLine("ProgressDialogHandler: end(Finish)");
			}
			else
			{
				Debug.WriteLine("ProgressDialogHandler: RAW(Finish)");
				Finish();
			}

			// trying disable a weird bug
			//  _currentCommand.Finish -= new EventHandler(OnCommand_Finish);
		}

		public void CloseByCancellingThenCloseParent()
		{
			Close();
		}
	}
}