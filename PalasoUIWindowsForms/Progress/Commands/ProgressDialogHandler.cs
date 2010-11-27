using System;
using System.Diagnostics;
using System.Windows.Forms;
using Palaso.Progress;

//For UML diagram, see ProgressSystem.uml (done in StarUML)

namespace Palaso.UI.WindowsForms.Progress.Commands
{
	public class ProgressDialogHandler
	{
		private ProgressDialog _progressDialog;
		private readonly System.Windows.Forms.Form _parentForm;
		private readonly AsyncCommand _currentCommand;
		public event EventHandler Finished;

		public ProgressDialogHandler(System.Windows.Forms.Form parentForm, BasicCommand  command)
		{
			_parentForm = parentForm;
			_currentCommand = command;
			command.InitializeCallback = new InitializeProgressCallback(InitializeProgress);
			command.ProgressCallback = new ProgressCallback(UpdateProgress);
			command.PrimaryStatusTextCallback = new StatusCallback(UpdateStatus1);
			command.SecondaryStatusTextCallback = new StatusCallback(UpdateOverview);

			_currentCommand.BeginCancel += new EventHandler(OnCommand_BeginCancel);
			_currentCommand.EnabledChanged += new EventHandler(OnCommand_EnabledChanged);
			_currentCommand.Error += new ErrorEventHandler(OnCommand_Error);
			_currentCommand.Finish += new EventHandler(OnCommand_Finish);

			_progressDialog = new ProgressDialog();
			_progressDialog.Text = "From Handler for"+command.GetType().ToString();
			_progressDialog.CancelRequested += new EventHandler(_progressDialog_Cancelled);
			_progressDialog.Owner = parentForm ;
			_progressDialog.CanCancel = true;
			//we don't yet have any actual background-safe stuff, but this dialog
			//doesn't seem to work (no progress) if it's called modally
			//_progressDialog.ShowDialog();
			//_progressDialog.DelayShow() <-- this one makes it come up only if the command turns out to be slow
			_progressDialog.Show();

		}

		/// <summary>
		/// tests (or anything else that is rapidly chaining these things together)
		/// should use this to make sure we're ready to go on to the next activity
		/// </summary>
		public bool TestEverythingClosedUp
		{
			get
			{
				return _progressDialog == null || _progressDialog.Visible == false;
			}
		}

		public void InitializeProgress(int minimum, int maximum)
		{

			if (NeedInvoke())
			{
				_parentForm.BeginInvoke(
					new InitializeProgressCallback(InitializeProgressCore),
					new object[] { minimum, maximum });
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
					new object[] { progress });
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
					new object[] { statusText });
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
					new object[] { text });
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
				Finished.BeginInvoke(this, null, null, null);//jh changed this from Invoke()
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
			Palaso.Reporting.ErrorReport.NotifyUserOfProblem("An error occurred while processing your request.");
			Finish();
		}

		/// <summary>
		/// A delegate for a method which takes an exception as its only parmeter
		/// </summary>
		public delegate void ExceptionMethodInvoker(Exception e);
		public event EventHandler Cancelled;

		private void _progressDialog_Cancelled(object sender, EventArgs e)
		{
			if (Cancelled != null)
			{
				Cancelled.Invoke(this, null);//REVIEW jh wesay
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
					_parentForm.BeginInvoke(new ProgressDialogHandler.ExceptionMethodInvoker(Finish), new object[] { e.Exception });
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