//originally from Matthew Adams, who was a thorough blog on these things at http://mwadams.spaces.live.com/blog/cns!652A0FB566F633D5!133.entry

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using Palaso.Progress;

namespace Palaso.UI.WindowsForms.Progress
{
	/// <summary>
	/// Provides a progress dialog similar to the one shown by Windows
	/// </summary>
	public class ProgressDialog : Form
	{
		private Label _statusLabel;
		private ProgressBar _progressBar;
		private Label _progressLabel;
		private Button _cancelButton;
		private Timer _showWindowIfTakingLongTimeTimer;
		private bool _showOnce;
		private Timer _progressTimer;
		private bool _isClosing;
		private Label _overviewLabel;
		private DateTime _startTime = DateTime.Now;
		private IContainer components;
		private BackgroundWorker _backgroundWorker;
//        private ProgressState _lastHeardFromProgressState;
		private ProgressState _progressState;

		/// <summary>
		/// Standard constructor
		/// </summary>
		public ProgressDialog()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			_statusLabel.BackColor = SystemColors.Control;
			_progressLabel.BackColor = SystemColors.Control;
			_overviewLabel.BackColor = SystemColors.Control;

			//avoids the client getting null errors if he checks this when there
			//has yet to be a callback from the worker
//            _lastHeardFromProgressState = new NullProgressState();
		}

		/// <summary>
		/// Get / set the time in ms to delay
		/// before showing the dialog
		/// </summary>
		private/*doesn't work yet public*/ int DelayShowInterval
		{
			get
			{
				return _showWindowIfTakingLongTimeTimer.Interval;
			}
			set
			{
				_showWindowIfTakingLongTimeTimer.Interval = value;
			}
		}

		/// <summary>
		/// Get / set the text to display in the first status panel
		/// </summary>
		public string StatusText
		{
			get
			{
				return _statusLabel.Text;
			}
			set
			{
				_statusLabel.Text = value;
			}
		}

		/// <summary>
		/// Description of why this dialog is even showing
		/// </summary>
		public string Overview
		{
			get
			{
				return _overviewLabel.Text;
			}
			set
			{
				_overviewLabel.Text = value;
			}
		}

		/// <summary>
		/// Get / set the minimum range of the progress bar
		/// </summary>
		public int ProgressRangeMinimum
		{
			get
			{
				return _progressBar.Minimum;
			}
			set
			{
				_progressBar.Minimum = value;
			}
		}

		/// <summary>
		/// Get / set the maximum range of the progress bar
		/// </summary>
		public int ProgressRangeMaximum
		{
			get
			{
				return _progressBar.Maximum;
			}
			set
			{
				_progressBar.Maximum = value;
			}
		}

		/// <summary>
		/// Get / set the current value of the progress bar
		/// </summary>
		public int Progress
		{
			get
			{
				return _progressBar.Value;
			}
			set
			{
				/* these were causing weird, hard to debug (because of threads)
				 * failures. The debugger would reprot that value == max, so why fail?

				 * Debug.Assert(value <= _progressBar.Maximum);
				 */
				Debug.WriteLineIf(value >  _progressBar.Maximum,
					"***Warning progres was " + value + " but max is " + _progressBar.Maximum);
				Debug.Assert(value >= _progressBar.Minimum);
				if (value > _progressBar.Maximum)
				{
					_progressBar.Maximum = value;//not worth crashing over in Release build
				}
				if (value < _progressBar.Minimum)
				{
					return; //not worth crashing over in Release build
				}
				_progressBar.Value = value;
			}
		}

		/// <summary>
		/// Get/set a boolean which determines whether the form
		/// will show a cancel option (true) or not (false)
		/// </summary>
		public bool CanCancel
		{
			get
			{
				return _cancelButton.Enabled;
			}
			set
			{
				_cancelButton.Enabled = value;
			}
		}

		/// <summary>
		/// If this is set before showing, the dialog will run the worker and respond
		/// to its events
		/// </summary>
		public BackgroundWorker BackgroundWorker
		{
			get
			{
				return _backgroundWorker;
			}
			set
			{
				_backgroundWorker = value;
			}
		}

		public ProgressState ProgressStateResult
		{
			get
			{
				return _progressState;// return _lastHeardFromProgressState;
			}
		}

		/// <summary>
		/// Optional; one will be created (of some class or subclass) if you don't set it.
		/// E.g. dlg.ProgressState = new BackgroundWorkerState(dlg.BackgroundWorker);
		/// Also, you can use the getter to gain access to the progressstate, in order to add arguments
		/// which the worker method can get at.
		/// </summary>
		public ProgressState ProgressState
		{
			get
			{
				if(_progressState ==null)
				{
					if(_backgroundWorker == null)
					{
						throw new ArgumentException("You must set BackgroundWorker before accessing this property.");
					}
					ProgressState  = new BackgroundWorkerState(_backgroundWorker);
				}
				return _progressState;
			}

			set
			{
				if (_progressState!=null)
				{
					CancelRequested -= _progressState.CancelRequested;
				}
				_progressState = value;
				CancelRequested += _progressState.CancelRequested;
			}
		}

		void OnBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			//BackgroundWorkerState progressState = e.Result as ProgressState;

			if(e.Cancelled )
			{
				DialogResult = DialogResult.Cancel;
			}
				//NB: I don't know how to actually let the BW know there was an error
				//else if (e.Error != null ||
			else if (ProgressStateResult != null && (ProgressStateResult.State == ProgressState.StateValue.StoppedWithError
													 || ProgressStateResult.ExceptionThatWasEncountered != null))
			{
				//this dialog really can't know whether this was an unexpected exception or not
				//so don't do this:  Reporting.ErrorReporter.ReportException(ProgressStateResult.ExceptionThatWasEncountered, this, false);
				DialogResult = DialogResult.Abort;//not really matching semantics
			}
			else
			{
				DialogResult = DialogResult.OK;
			}
			_isClosing = true;
			Close();
		}

		void OnBackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			ProgressState state = e.UserState as ProgressState;
			if (state != null)
			{
 //               _lastHeardFromProgressState = state;
				ProgressRangeMaximum = state.TotalNumberOfSteps;
				Progress = state.NumberOfStepsCompleted;
				StatusText = state.StatusLabel;

			}
			else
			{
				Progress = e.ProgressPercentage;
			}
		}

		/// <summary>
		/// Show the control, but honor the
		/// <see cref="DelayShowInterval"/>.
		/// </summary>
		private/*doesn't work yet public*/  void DelayShow()
		{
			// This creates the control, but doesn't
			// show it; you can't use CreateControl()
			// here, because it will return because
			// the control is not visible
			CreateHandle();
		}


		//************
		//the problem is that our worker reports progress, and we die because of a begininvoke with no window yet

		private/*doesn't work yet public*/  void ShowDialogIfTakesLongTime()
		{
			DelayShow();
			OnStartWorker(this, null);
		}

		/// <summary>
		/// Close the dialog, ignoring cancel status
		/// </summary>
		public void ForceClose()
		{
			_isClosing = true;
			Close();
		}

		/// <summary>
		/// Raised when the cancel button is clicked
		/// </summary>
		public event EventHandler CancelRequested;

		/// <summary>
		/// Raises the cancelled event
		/// </summary>
		/// <param name="e">Event data</param>
		protected virtual void OnCancelled( EventArgs e )
		{
			EventHandler cancelled = CancelRequested;
			if( cancelled != null )
			{
				cancelled( this, e );
			}
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		/// <summary>
		/// Custom handle creation code
		/// </summary>
		/// <param name="e">Event data</param>
		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated (e);
			if( !_showOnce )
			{
				// First, we don't want this to happen again
				_showOnce = true;
				// Then, start the timer which will determine whether
				// we are going to show this again
				_showWindowIfTakingLongTimeTimer.Start();
			}
		}

		/// <summary>
		/// Custom close handler
		/// </summary>
		/// <param name="e">Event data</param>
		protected override void OnClosing(CancelEventArgs e)
		{
			if( !_isClosing )
			{
				e.Cancel = true;
				_cancelButton.PerformClick();
			}
			base.OnClosing( e );
		}


		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this._statusLabel = new System.Windows.Forms.Label();
			this._progressBar = new System.Windows.Forms.ProgressBar();
			this._cancelButton = new System.Windows.Forms.Button();
			this._progressLabel = new System.Windows.Forms.Label();
			this._showWindowIfTakingLongTimeTimer = new System.Windows.Forms.Timer(this.components);
			this._progressTimer = new System.Windows.Forms.Timer(this.components);
			this._overviewLabel = new System.Windows.Forms.Label();
			this.SuspendLayout();
			//
			// _statusLabel
			//
			this._statusLabel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
			this._statusLabel.Location = new System.Drawing.Point(9, 52);
			this._statusLabel.Name = "_statusLabel";
			this._statusLabel.Size = new System.Drawing.Size(279, 18);
			this._statusLabel.TabIndex = 12;
			//
			// _progressBar
			//
			this._progressBar.Location = new System.Drawing.Point(9, 75);
			this._progressBar.Name = "_progressBar";
			this._progressBar.Size = new System.Drawing.Size(279, 18);
			this._progressBar.TabIndex = 11;
			this._progressBar.Value = 1;
			//
			// _cancelButton
			//
			this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this._cancelButton.Location = new System.Drawing.Point(234, 99);
			this._cancelButton.Name = "_cancelButton";
			this._cancelButton.Size = new System.Drawing.Size(54, 22);
			this._cancelButton.TabIndex = 10;
			this._cancelButton.Text = "&Cancel";
			this._cancelButton.Click += new System.EventHandler(this.OnCancelButton_Click);
			//
			// _progressLabel
			//
			this._progressLabel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
			this._progressLabel.Location = new System.Drawing.Point(9, 99);
			this._progressLabel.Name = "_progressLabel";
			this._progressLabel.Size = new System.Drawing.Size(210, 18);
			this._progressLabel.TabIndex = 9;
			//
			// _showWindowIfTakingLongTimeTimer
			//
			this._showWindowIfTakingLongTimeTimer.Interval = 2000;
			this._showWindowIfTakingLongTimeTimer.Tick += new System.EventHandler(this.OnTakingLongTimeTimerClick);
			//
			// _progressTimer
			//
			this._progressTimer.Enabled = true;
			this._progressTimer.Interval = 1000;
			this._progressTimer.Tick += new System.EventHandler(this.progressTimer_Tick);
			//
			// _overviewLabel
			//
			this._overviewLabel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
			this._overviewLabel.Location = new System.Drawing.Point(9, 7);
			this._overviewLabel.Name = "_overviewLabel";
			this._overviewLabel.Size = new System.Drawing.Size(280, 37);
			this._overviewLabel.TabIndex = 8;
			//
			// ProgressDialog
			//
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(298, 130);
			this.ControlBox = false;
			this.Controls.Add(this._overviewLabel);
			this.Controls.Add(this._progressLabel);
			this.Controls.Add(this._cancelButton);
			this.Controls.Add(this._progressBar);
			this.Controls.Add(this._statusLabel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ProgressDialog";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Text = "Palaso";
			this.Shown += new System.EventHandler(this.OnStartWorker);
			this.ResumeLayout(false);

		}
		#endregion


		private void OnTakingLongTimeTimerClick(object sender, EventArgs e)
		{
			// Show the window now the timer has elapsed, and stop the timer
			_showWindowIfTakingLongTimeTimer.Stop();
			Show();
		}

		private void OnCancelButton_Click(object sender, EventArgs e)
		{
			// Prevent further cancellation
			_cancelButton.Enabled = false;
			_progressTimer.Stop();
			_progressLabel.Text =  "Canceling...";
			// Tell people we're canceling
			OnCancelled( e );
			if (_backgroundWorker != null)
			{
				_backgroundWorker.CancelAsync();
			}
		}

		private void progressTimer_Tick(object sender, EventArgs e)
		{
			int range = _progressBar.Maximum - _progressBar.Minimum;
			if( range <= 0 )
			{
				return;
			}
			if( _progressBar.Value <= 0 )
			{
				return;
			}
			TimeSpan elapsed = DateTime.Now - _startTime;
			double estimatedSeconds = (elapsed.TotalSeconds * range) / _progressBar.Value;
			TimeSpan estimatedToGo = new TimeSpan(0,0,0,(int)(estimatedSeconds - elapsed.TotalSeconds),0);
//			_progressLabel.Text = String.Format(
//				System.Globalization.CultureInfo.CurrentUICulture,
//                "Elapsed: {0} Remaining: {1}",
//				GetStringFor(elapsed),
//				GetStringFor(estimatedToGo) );
			_progressLabel.Text = String.Format(
				CultureInfo.CurrentUICulture,
				"{0}",
				//GetStringFor(elapsed),
				GetStringFor(estimatedToGo));
		}

		private static string GetStringFor( TimeSpan span )
		{
			if( span.TotalDays > 1 )
			{
				return string.Format(CultureInfo.CurrentUICulture, "{0} day {1} hour", span.Days, span.Hours);
			}
			else if( span.TotalHours > 1 )
			{
				return string.Format(CultureInfo.CurrentUICulture, "{0} hour {1} minutes", span.Hours, span.Minutes);
			}
			else if( span.TotalMinutes > 1 )
			{
				return string.Format(CultureInfo.CurrentUICulture, "{0} minutes {1} seconds", span.Minutes, span.Seconds);
			}
			return string.Format( CultureInfo.CurrentUICulture, "{0} seconds", span.Seconds );
		}

		public void OnNumberOfStepsCompletedChanged(object sender, EventArgs e)
		{
			Progress = ((ProgressState) sender).NumberOfStepsCompleted;
			//in case there is no event pump showing us (mono-threaded)
			progressTimer_Tick(this, null);
			Refresh();
		}

		public void OnTotalNumberOfStepsChanged(object sender, EventArgs e)
		{
			ProgressRangeMaximum = ((ProgressState)sender).TotalNumberOfSteps;
			Refresh();
		}

		public void OnStatusLabelChanged(object sender, EventArgs e)
		{
			StatusText = ((ProgressState)sender).StatusLabel;
			Refresh();
		}

		private void OnStartWorker(object sender, EventArgs e)
		{
			if (_backgroundWorker != null)
			{
				 //BW uses percentages (unless it's using our custom ProgressState in the UserState member)
				ProgressRangeMinimum = 0;
				ProgressRangeMaximum = 100;

				//if the actual task can't take cancelling, the caller of this should set CanCancel to false;
				_backgroundWorker.WorkerSupportsCancellation = CanCancel;

				_backgroundWorker.ProgressChanged += new ProgressChangedEventHandler(OnBackgroundWorker_ProgressChanged);
				_backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(OnBackgroundWorker_RunWorkerCompleted);
				_backgroundWorker.RunWorkerAsync(ProgressState);
			}
		}
	}
}