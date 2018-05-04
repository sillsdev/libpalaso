//originally from Matthew Adams, who was a thorough blog on these things at http://mwadams.spaces.live.com/blog/cns!652A0FB566F633D5!133.entry

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using SIL.Progress;
using SIL.Reporting;

namespace SIL.Windows.Forms.Progress
{
	/// <summary>
	/// Provides a progress dialog similar to the one shown by Windows
	/// </summary>
	public class ProgressDialog : Form
	{

		public delegate void ProgressCallback(int progress);

		private Label _statusLabel;
		private ProgressBar _progressBar;
		private Label _progressLabel;
		private Button _cancelButton;
		private Timer _showWindowIfTakingLongTimeTimer;
		private Timer _progressTimer;
		private bool _isClosing;
		private Label _overviewLabel;
		private DateTime _startTime;
		private IContainer components;
		private BackgroundWorker _backgroundWorker;
//        private ProgressState _lastHeardFromProgressState;
		private ProgressState _progressState;
		private TableLayoutPanel tableLayout;
		private bool _workerStarted;
		private bool _appUsingWaitCursor;

		/// <summary>
		/// Standard constructor
		/// </summary>
		public ProgressDialog()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			_statusLabel.BackColor = Color.Transparent;
			_progressLabel.BackColor = Color.Transparent;
			_overviewLabel.BackColor = Color.Transparent;
			_startTime = default(DateTime);
			Text = UsageReporter.AppNameToUseInDialogs;

			_statusLabel.Font = SystemFonts.MessageBoxFont;
			_progressLabel.Font = SystemFonts.MessageBoxFont;
			_overviewLabel.Font = SystemFonts.MessageBoxFont;

			_statusLabel.Text = string.Empty;
			_progressLabel.Text = string.Empty;
			_overviewLabel.Text = string.Empty;

			_cancelButton.MouseEnter += delegate
			{
				_appUsingWaitCursor = Application.UseWaitCursor;
				_cancelButton.Cursor = Cursor = Cursors.Arrow;
				Application.UseWaitCursor = false;
			};

			_cancelButton.MouseLeave += delegate
			{
				Application.UseWaitCursor = _appUsingWaitCursor;
			};

			//avoids the client getting null errors if he checks this when there
			//has yet to be a callback from the worker
//            _lastHeardFromProgressState = new NullProgressState();
		}

		private void HandleTableLayoutSizeChanged(object sender, EventArgs e)
		{
			if (!IsHandleCreated)
				CreateHandle();

			var desiredHeight = tableLayout.Height + Padding.Top + Padding.Bottom + (Height - ClientSize.Height);
			var scn = Screen.FromControl(this);
			Height = Math.Min(desiredHeight, scn.WorkingArea.Height - 20);
			AutoScroll = (desiredHeight > scn.WorkingArea.Height - 20);
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
				if (_backgroundWorker == null)
				{
					_progressBar.Minimum = value;
				}
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
				if (_backgroundWorker != null)
				{
					return;
				}
				if (InvokeRequired)
				{
					Invoke(new ProgressCallback(SetMaximumCrossThread), new object[] { value });
				}
				else
				{
					_progressBar.Maximum = value;
				}
			}
		}

		private void SetMaximumCrossThread(int amount)
		{
			ProgressRangeMaximum = amount;
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
				_progressBar.Minimum = 0;
				_progressBar.Maximum = 100;
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
		/// Gets or sets the manner in which progress should be indicated on the progress bar.
		/// </summary>
		public ProgressBarStyle BarStyle { get { return _progressBar.Style; } set { _progressBar.Style = value; } }

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
				_progressState.TotalNumberOfStepsChanged += OnTotalNumberOfStepsChanged;
			}
		}

		void OnBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			//BackgroundWorkerState progressState = e.Result as ProgressState;

			if(e.Cancelled )
			{
				DialogResult = DialogResult.Cancel;
				//_progressState.State = ProgressState.StateValue.Finished;
			}
				//NB: I don't know how to actually let the BW know there was an error
				//else if (e.Error != null ||
			else if (ProgressStateResult != null && (ProgressStateResult.State == ProgressState.StateValue.StoppedWithError
													 || ProgressStateResult.ExceptionThatWasEncountered != null))
			{
				//this dialog really can't know whether this was an unexpected exception or not
				//so don't do this:  Reporting.ErrorReporter.ReportException(ProgressStateResult.ExceptionThatWasEncountered, this, false);
				DialogResult = DialogResult.Abort;//not really matching semantics
			   // _progressState.State = ProgressState.StateValue.StoppedWithError;
			}
			else
			{
				DialogResult = DialogResult.OK;
			  //  _progressState.State = ProgressState.StateValue.Finished;
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
				StatusText = state.StatusLabel;
			}

			if (state == null
				|| state is BackgroundWorkerState)
			{
				Progress = e.ProgressPercentage;
			}
			else
			{
				ProgressRangeMaximum = state.TotalNumberOfSteps;
				Progress = state.NumberOfStepsCompleted;
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
		//the problem is that our worker reports progress, and we die (only in some circumstance not nailed-down yet)
		//because of a begininvoke with no window yet. Sometimes, we don't get the callback to
		//the very important OnBackgroundWorker_RunWorkerCompleted

	   private/*doesn't work yet public*/  void ShowDialogIfTakesLongTime()
		{
			DelayShow();
			OnStartWorker(this, null);
		   while((_progressState.State == ProgressState.StateValue.NotStarted
				  || _progressState.State == ProgressState.StateValue.Busy) && !this.Visible)
		   {
			   Application.DoEvents();
		   }
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
				if (_showWindowIfTakingLongTimeTimer != null)
				{
					_showWindowIfTakingLongTimeTimer.Stop();
				}
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		///// <summary>
		///// Custom handle creation code
		///// </summary>
		///// <param name="e">Event data</param>
//        protected override void OnHandleCreated(EventArgs e)
//        {
//            base.OnHandleCreated (e);
//            if( !_showOnce )
//            {
//                // First, we don't want this to happen again
//                _showOnce = true;
//                // Then, start the timer which will determine whether
//                // we are going to show this again
//                _showWindowIfTakingLongTimeTimer.Start();
//            }
//        }

		///// <summary>
		///// Custom close handler
		///// </summary>
		///// <param name="e">Event data</param>
//        protected override void OnClosing(CancelEventArgs e)
//        {
//            Debug.WriteLine("Dialog:OnClosing");
//            if (_showWindowIfTakingLongTimeTimer != null)
//            {
//                _showWindowIfTakingLongTimeTimer.Stop();
//            }
//
//            if( !_isClosing )
//            {
//                Debug.WriteLine("Warning: OnClosing called but _isClosing=false, attempting cancel click");
//                e.Cancel = true;
//                _cancelButton.PerformClick();
//            }
//            base.OnClosing( e );
//        }


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
			this.tableLayout = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayout.SuspendLayout();
			this.SuspendLayout();
			//
			// _statusLabel
			//
			this._statusLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this._statusLabel.AutoSize = true;
			this._statusLabel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
			this.tableLayout.SetColumnSpan(this._statusLabel, 2);
			this._statusLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._statusLabel.Location = new System.Drawing.Point(0, 35);
			this._statusLabel.Margin = new System.Windows.Forms.Padding(0, 0, 0, 5);
			this._statusLabel.Name = "_statusLabel";
			this._statusLabel.Size = new System.Drawing.Size(355, 15);
			this._statusLabel.TabIndex = 12;
			this._statusLabel.Text = "#";
			this._statusLabel.UseMnemonic = false;
			//
			// _progressBar
			//
			this._progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.tableLayout.SetColumnSpan(this._progressBar, 2);
			this._progressBar.Location = new System.Drawing.Point(0, 55);
			this._progressBar.Margin = new System.Windows.Forms.Padding(0, 0, 0, 12);
			this._progressBar.Name = "_progressBar";
			this._progressBar.Size = new System.Drawing.Size(355, 18);
			this._progressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
			this._progressBar.TabIndex = 11;
			this._progressBar.Value = 1;
			//
			// _cancelButton
			//
			this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this._cancelButton.AutoSize = true;
			this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this._cancelButton.Location = new System.Drawing.Point(280, 85);
			this._cancelButton.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
			this._cancelButton.Name = "_cancelButton";
			this._cancelButton.Size = new System.Drawing.Size(75, 23);
			this._cancelButton.TabIndex = 10;
			this._cancelButton.Text = "&Cancel";
			this._cancelButton.Click += new System.EventHandler(this.OnCancelButton_Click);
			//
			// _progressLabel
			//
			this._progressLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this._progressLabel.AutoEllipsis = true;
			this._progressLabel.AutoSize = true;
			this._progressLabel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
			this._progressLabel.Location = new System.Drawing.Point(0, 90);
			this._progressLabel.Margin = new System.Windows.Forms.Padding(0, 5, 0, 0);
			this._progressLabel.Name = "_progressLabel";
			this._progressLabel.Size = new System.Drawing.Size(272, 13);
			this._progressLabel.TabIndex = 9;
			this._progressLabel.Text = "#";
			this._progressLabel.UseMnemonic = false;
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
			this._overviewLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this._overviewLabel.AutoSize = true;
			this._overviewLabel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
			this.tableLayout.SetColumnSpan(this._overviewLabel, 2);
			this._overviewLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._overviewLabel.Location = new System.Drawing.Point(0, 0);
			this._overviewLabel.Margin = new System.Windows.Forms.Padding(0, 0, 0, 20);
			this._overviewLabel.Name = "_overviewLabel";
			this._overviewLabel.Size = new System.Drawing.Size(355, 15);
			this._overviewLabel.TabIndex = 8;
			this._overviewLabel.Text = "#";
			this._overviewLabel.UseMnemonic = false;
			//
			// tableLayout
			//
			this.tableLayout.AutoSize = true;
			this.tableLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.tableLayout.BackColor = System.Drawing.Color.Transparent;
			this.tableLayout.ColumnCount = 2;
			this.tableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayout.Controls.Add(this._cancelButton, 1, 3);
			this.tableLayout.Controls.Add(this._overviewLabel, 0, 0);
			this.tableLayout.Controls.Add(this._progressLabel, 0, 3);
			this.tableLayout.Controls.Add(this._progressBar, 0, 2);
			this.tableLayout.Controls.Add(this._statusLabel, 0, 1);
			this.tableLayout.Dock = System.Windows.Forms.DockStyle.Top;
			this.tableLayout.Location = new System.Drawing.Point(12, 12);
			this.tableLayout.Name = "tableLayout";
			this.tableLayout.RowCount = 4;
			this.tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayout.Size = new System.Drawing.Size(355, 108);
			this.tableLayout.TabIndex = 13;
			this.tableLayout.SizeChanged += new System.EventHandler(this.HandleTableLayoutSizeChanged);
			//
			// ProgressDialog
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.AutoSize = true;
			this.ClientSize = new System.Drawing.Size(379, 150);
			this.ControlBox = false;
			this.Controls.Add(this.tableLayout);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ProgressDialog";
			this.Padding = new System.Windows.Forms.Padding(12);
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Palaso";
			this.Load += new System.EventHandler(this.ProgressDialog_Load);
			this.Shown += new System.EventHandler(this.ProgressDialog_Shown);
			this.tableLayout.ResumeLayout(false);
			this.tableLayout.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion


		private void OnTakingLongTimeTimerClick(object sender, EventArgs e)
		{
			// Show the window now the timer has elapsed, and stop the timer
			_showWindowIfTakingLongTimeTimer.Stop();
			if (!this.Visible)
			{
				Show();
			}
		}

		private void OnCancelButton_Click(object sender, EventArgs e)
		{
			_showWindowIfTakingLongTimeTimer.Stop();
			if(_isClosing)
				return;

			Debug.WriteLine("Dialog:OnCancelButton_Click");

			// Prevent further cancellation
			_cancelButton.Enabled = false;
			_progressTimer.Stop();
			_progressLabel.Text =  "Canceling...";
			// Tell people we're canceling
			OnCancelled( e );
			if (_backgroundWorker != null && _backgroundWorker.WorkerSupportsCancellation)
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
			if (_startTime != default(DateTime))
			{
				TimeSpan elapsed = DateTime.Now - _startTime;
				double estimatedSeconds = (elapsed.TotalSeconds * range) / _progressBar.Value;
				TimeSpan estimatedToGo = new TimeSpan(0, 0, 0, (int)(estimatedSeconds - elapsed.TotalSeconds), 0);
				//_progressLabel.Text = String.Format(
				//    System.Globalization.CultureInfo.CurrentUICulture,
				//    "Elapsed: {0} Remaining: {1}",
				//    GetStringFor(elapsed),
				//    GetStringFor(estimatedToGo));
				_progressLabel.Text = String.Format(
					CultureInfo.CurrentUICulture,
					"{0}",
					//GetStringFor(elapsed),
					GetStringFor(estimatedToGo));
			}
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
			if (InvokeRequired)
			{
				Invoke(new ProgressCallback(UpdateTotal), new object[] { ((ProgressState)sender).TotalNumberOfSteps });
			}
			else
			{
				UpdateTotal(((ProgressState) sender).TotalNumberOfSteps);
			}
		}

		private void UpdateTotal(int steps)
		{
			_startTime = DateTime.Now;
			ProgressRangeMaximum = steps;
			Refresh();
		}

		public void OnStatusLabelChanged(object sender, EventArgs e)
		{
			StatusText = ((ProgressState)sender).StatusLabel;
			Refresh();
		}

		private void OnStartWorker(object sender, EventArgs e)
		{
			_workerStarted = true;
			Debug.WriteLine("Dialog:StartWorker");

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

		//this is here, in addition to the OnShown handler, because of a weird bug were a certain,
		//completely unrelated test (which doesn't use this class at all) can cause tests using this to
		//fail because the OnShown event is never fired.
		//I don't know why  the orginal code we copied this from was using onshown instead of onload,
		//but it may have something to do with its "delay show" feature (which I couldn't get to work,
		//but which would be a terrific thing to have)
		private void ProgressDialog_Load(object sender, EventArgs e)
		{
			if(!_workerStarted)
			{
				OnStartWorker(this, null);
			}
		}

		private void ProgressDialog_Shown(object sender, EventArgs e)
		{
			if(!_workerStarted)
			{
				OnStartWorker(this, null);
			}
		}
	}
}
