using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using SIL.Extensions;
using SIL.PlatformUtilities;
using SIL.Progress;
using SIL.Reporting;
using SIL.Windows.Forms.Extensions;

namespace SIL.Windows.Forms.Progress
{
	/// <summary>
	/// A full-featured log ui control, which supports colored and styled text, user-controlled
	/// verbosity, error reporting, and copy to clipboard. Protects itself, thread-wise.
	/// Implements IProgress, so that processes can send
	/// text to it without knowing anything about UI.
	/// </summary>
	public partial class LogBox : UserControl, IProgress
	{
		public event EventHandler ReportErrorLinkClicked;

		private Action<IProgress> _getDiagnosticsMethod;
		private readonly ToolStripLabel _reportLink;
		private SynchronizationContext _synchronizationContext;

		public LogBox()
		{
			InitializeComponent();

			_reportLink = new ToolStripLabel("Report this problem to the developers");
			_reportLink.LinkColor = Color.Red;
			_reportLink.IsLink = true;
			_reportLink.Visible = false;
			menuStrip1.Items.Add(_reportLink);
			_reportLink.Click += delegate
			{
				if (!ReportError(_verboseBox.Text))
				{
					try
					{
						if (!string.IsNullOrEmpty(_verboseBox.Text))
						{
							Clipboard.SetText(_verboseBox.Text);
							MessageBox.Show(
								"Information on what happened has been copied to your clipboard. Please email it to the developers of the program you are using.");
						}
					}
					catch (Exception)
					{
						MessageBox.Show(
							   "Unable to copy the message to the clipboard. You might need to restart the application or your computer");
					}
				}

				if (ReportErrorLinkClicked != null)
					ReportErrorLinkClicked(this, EventArgs.Empty);
			};

			_verboseBox.ForeColor = _box.ForeColor = SystemColors.WindowText;
			_verboseBox.BackColor = _box.BackColor = SystemColors.Window;
			menuStrip1.BackColor = Color.White;
			BackColor = VisualStyleInformation.TextControlBorder;
			_tableLayout.BackColor = SystemColors.Window;

			//On some machines (winXP?) we get in trouble if we don't make sure these boxes are visible before
			//they get invoked() to. It's not clear that the following actually works... in addition to this
			//effort, we also catch exceptions when in trying to invoke on them.
			_box.CreateControl();
			_verboseBox.Visible = true;
			_verboseBox.CreateControl();
			_verboseBox.Visible = false;

			SetFont();

			_tableLayout.Size = new Size(ClientSize.Width - (_tableLayout.Left + 1),
				ClientSize.Height - (_tableLayout.Top + 1));

			_box.Dock = DockStyle.Fill;
			_box.LinkClicked += _box_LinkClicked;
			_verboseBox.LinkClicked += _box_LinkClicked;
			_synchronizationContext = SynchronizationContext.Current;
		}

		void _box_LinkClicked(object sender, LinkClickedEventArgs e)
		{
			if (LinkClicked != null)
				LinkClicked(this, e);
		}

		public event EventHandler<LinkClickedEventArgs> LinkClicked;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public SynchronizationContext SyncContext
		{
			get { return _synchronizationContext; }
			set //not private becuase the IProgress requires it be public
			{
				if (value == null || value == _synchronizationContext)
					return;
				throw new NotImplementedException(
					"There's no good reason for clients to be trying to set the SyncContext of a LogBox.");
			}
		}

		public void AddMenuItem(string label, Image image, EventHandler onClick)
		{
			_menu.DropDownItems.Add(label, image, onClick);
		}

		public bool ShowMenu
		{
			get { return _menu.Visible; }
			set
			{
				_menu.Visible = value;
				menuStrip1.Visible = (_menu.Visible || _reportLink.Visible);
				Invalidate();
			}
		}

		public bool ShowDetailsMenuItem
		{
			get { return _showDetailsMenu.Visible; }
			set { _showDetailsMenu.Visible = value; }
		}

		public bool ShowDiagnosticsMenuItem
		{
			get { return _runDiagnostics.Visible; }
			set { _runDiagnostics.Visible = value; }
		}

		public bool ShowFontMenuItem
		{
			get { return _chooseFontMenuItem.Visible; }
			set { _chooseFontMenuItem.Visible = value; }
		}

		public bool ShowCopyToClipboardMenuItem
		{
			get { return _copyToClipboardMenuItem.Visible; }
			set { _copyToClipboardMenuItem.Visible = value; }
		}

		private void SetFont()
		{
			var fnt = SystemFonts.MessageBoxFont;
			var name = LogBoxSettings.Default.FontName;
			var size = LogBoxSettings.Default.FontSize;

			if (!string.IsNullOrEmpty(name) && size > 0f)
				fnt = new Font(name, size, LogBoxSettings.Default.FontStyle);

			Font = fnt;
		}

		public override Font Font
		{
			get { return base.Font; }
			set { base.Font = _verboseBox.Font = _box.Font = value; }
		}

		public override string Text
		{
			get {
				// The Text property get called during ctor so return an
				// empty string in that case.  This works around a crash
				// in WeSay.
				if (_box == null || _verboseBox == null) return String.Empty;
				return "Box:" + _box.Text + "Verbose:" + _verboseBox.Text;
			}
		}

		public string Rtf
		{
			get { return "Box:" + _box.Rtf + "Verbose:" + _verboseBox.Rtf; }
		}

		public void ScrollToTop()
		{
			foreach (var rtfBox in new[] { _box, _verboseBox })
			{
				SafeInvoke(rtfBox, (() =>
				{
					rtfBox.SelectionStart = rtfBox.SelectionLength = 0;
					rtfBox.ScrollToCaret();
				}));
			}
		}

		public void WriteStatus(string message, params object[] args)
		{
			WriteMessage(message, args);
		}

		public void WriteMessageWithColor(Color color, string message, params object[] args)
		{
			Write(color, _box.Font.Style, message, args);
		}

		public void WriteMessageWithColor(string colorName, string message, params object[] args)
		{
			Write(Color.FromName(colorName), _box.Font.Style, message, args);
		}

		public void WriteMessageWithFontStyle(FontStyle style, string message, params object[] args)
		{
			Write(_box.ForeColor, style, message, args);
		}

		public void WriteMessageWithColorAndFontStyle(Color color, FontStyle style, string message, params object[] args)
		{
			Write(color, style, message, args);
		}

		public void WriteMessage(string message, params object[] args)
		{
			Write(_box.ForeColor, _box.Font.Style, message, args);
		}

		/// <summary>
		/// This is an attempt to avoid a mysterious crash (B. Waters) where the invoke was
		/// happening before the window's handle had been created
		/// </summary>
		private void SafeInvoke(Control box, Action action)
		{
			box.SafeInvoke(action, errorHandling:ControlExtensions.ErrorHandlingAction.IgnoreAll);
		}

		private void Write(Color color, FontStyle style, string msg, params object[] args)
		{
#if !DEBUG
			try
			{
#endif
			foreach (var rtfBox in new[] {_box, _verboseBox})
			{
				var rtfBoxForDelegate = rtfBox; //no really, this assignment is needed. Took hours to track down this bug.
				var styleForDelegate = style;
				SafeInvoke(rtfBox, (() =>
				{
					if (Platform.IsWindows)
					{
						if (!rtfBoxForDelegate.Font.FontFamily.IsStyleAvailable(styleForDelegate))
							style = rtfBoxForDelegate.Font.Style;

						using (var fnt = new Font(rtfBoxForDelegate.Font, styleForDelegate))
						{
							rtfBoxForDelegate.SelectionStart = rtfBoxForDelegate.Text.Length;
							rtfBoxForDelegate.SelectionColor = color;
							rtfBoxForDelegate.SelectionFont = fnt;
							rtfBoxForDelegate.AppendText(msg.FormatWithErrorStringInsteadOfException(args) +
								Environment.NewLine);
							rtfBoxForDelegate.SelectionStart = rtfBoxForDelegate.Text.Length;
							rtfBoxForDelegate.ScrollToCaret();
						}
					}
					else
					{
						// changing the text colour throws exceptions with mono 2011-12-09
						// so just append plain text
						rtfBoxForDelegate.AppendText(msg.FormatWithErrorStringInsteadOfException(args) +
							Environment.NewLine);
						rtfBoxForDelegate.SelectionStart = rtfBoxForDelegate.Text.Length;
						rtfBoxForDelegate.ScrollToCaret();
					}
				}));
			}
#if !DEBUG
			}
			catch (Exception)
			{
				//swallow. If the dreaded XP "Invoke or BeginInvoke cannot be called on a control until the window handle has been created" happens, it's really not worth crashing over
				//we  shouldn't be getting that, given the SafeInvoke thing, but I did get a crash report here (but it was confusing, as if the
				//stack trace didn't actually go into this method, but the build date was after I wrote this.  So this exception may never actually happen.
			}
#endif
		}

		public void WriteWarning(string message, params object[] args)
		{
			Write(Color.Blue, _box.Font.Style, "Warning: " + message, args);
		}

		/// <summary>
		/// This is a callback the client can set to soemthing which will then generate
		/// Write() calls.  If it is set, the user sees a "Run diagnostics" menu item.
		/// </summary>
		public Action<IProgress> GetDiagnosticsMethod
		{
			get { return _getDiagnosticsMethod; }
			set
			{
				_getDiagnosticsMethod = value;
				_runDiagnostics.Visible = (_getDiagnosticsMethod != null);
			}
		}

		public void WriteException(Exception error)
		{
			Write(Color.Red, _box.Font.Style, "Exception: " + error.Message);
			WriteVerbose(error.StackTrace);
			if (error.InnerException != null)
			{
				WriteError("Inner--> ");
				WriteException(error.InnerException);
			}
		}

		public void WriteError(string message, params object[] args)
		{
			Write(Color.Red, _box.Font.Style, Environment.NewLine + "Error: " + message, args);

			// There is no Invoke method on ToolStripItems (which the _reportLink is)
			// and setting it to visible from another thread seems to work okay.
			_reportLink.Visible = true;

			menuStrip1.Invoke(new Action(() =>
			{
				menuStrip1.Visible = true;
			}));

			ErrorEncountered = true;
		}

		public bool ErrorEncountered
		{
			get;
			set;
		}

		public IProgressIndicator ProgressIndicator { get; set; } // this isn't implemented for a LogBox

		public void WriteVerbose(string message, params object[] args)
		{
			if (!Platform.IsWindows)
			{
				_verboseBox.AppendText(SafeFormat(message + Environment.NewLine, args));
			}
			else
			{
				SafeInvoke(_verboseBox, (() =>
				{
					_verboseBox.SelectionStart = _verboseBox.Text.Length;
					_verboseBox.SelectionColor = Color.DarkGray;
					_verboseBox.AppendText(SafeFormat(message + Environment.NewLine, args));
				}));
			}
		}

		public static string SafeFormat(string format, params object[] args)
		{
			if (args == null && args.Length == 0)
				return format;      //in many cases, we can avoid the format entirely.  This gets us past the "hg log -template {node}" error.

			return format.FormatWithErrorStringInsteadOfException(args);
		}

		public bool ShowVerbose
		{
			set { _showDetailsMenu.Checked = value; }
		}

		public bool CancelRequested { get; set; }

		private void _showDetails_CheckedChanged(object sender, EventArgs e)
		{
			_verboseBox.Visible = _showDetailsMenu.Checked;
			_box.Visible = !_showDetailsMenu.Checked;

			if (_showDetailsMenu.Checked)
			{
				_box.Dock = DockStyle.None;
				_verboseBox.Dock = DockStyle.Fill;
			}
			else
			{
				_verboseBox.Dock = DockStyle.None;
				_box.Dock = DockStyle.Fill;
			}
		}

		private void _copyToClipboardLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			var text = _verboseBox.Text;
			if (String.IsNullOrEmpty(text))
				return;		// Clipboard and DataObject both strongly dislike both null and String.Empty
			if (!Platform.IsWindows)
			{
				//at least on Xubuntu, getting some rtf on the clipboard would mean that when you pasted, you'd see rtf
				Clipboard.SetText(text);
			}
			else
			{
				var data = new DataObject();
				var rtfText = _verboseBox.Rtf;
				if (!String.IsNullOrEmpty(rtfText))
					data.SetText(rtfText, TextDataFormat.Rtf);
				data.SetText(text, TextDataFormat.UnicodeText);
				Clipboard.SetDataObject(data);
			}
		}

		public void Clear()
		{
			_box.Text = "";
			_verboseBox.Text = "";
		}

		private void copyToClipboardToolStripMenuItem_Click(object sender, EventArgs e)
		{
			_copyToClipboardLink_LinkClicked(sender, null);
		}

		private void LogBox_BackColorChanged(object sender, EventArgs e)
		{
			//_menu.BackColor =  BackColor;
		}

		private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
		{

		}

		//private void _reportProblemLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		//{
		//    if (!ReportError(_verboseBox.Text))
		//    {
		//        Clipboard.SetText(_verboseBox.Text);
		//        MessageBox.Show(
		//            "Information on what happened has been copied to your clipboard. Please email it to the developers of the program you are using.");
		//    }
		//}

		private static bool ReportError(string msg)
		{
			try
			{
				ErrorReport.ReportNonFatalMessageWithStackTrace(msg);
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		private void OnRunDiagnosticsClick(object sender, EventArgs e)
		{
			if (GetDiagnosticsMethod != null)
			{
				ShowVerbose = true;
				GetDiagnosticsMethod(this);
			}
		}

		//this is important because we may be showing characters which aren't in the standard font
		private void OnChooseFontClick(object sender, EventArgs e)
		{
			using(var dlg = new FontDialog())
			{
				dlg.ShowColor = false;
				dlg.ShowEffects = false;
				dlg.ShowApply = false;
				dlg.ShowHelp = false;
				dlg.Font = _box.Font;

				if (DialogResult.OK == dlg.ShowDialog())
				{
					LogBoxSettings.Default.FontName = dlg.Font.Name;
					LogBoxSettings.Default.FontSize = dlg.Font.Size;
					LogBoxSettings.Default.FontStyle = dlg.Font.Style;
					LogBoxSettings.Default.Save();
					SetFont();
				}
			}
		}

		private void HandleTableLayoutPaint(object sender, PaintEventArgs e)
		{
			if (ShowMenu || _reportLink.Visible)
			{
				var pt = menuStrip1.Location;
				using (var pen = new Pen(VisualStyleInformation.TextControlBorder))
					e.Graphics.DrawLine(pen, pt.X, pt.Y - 1, pt.X + menuStrip1.Width, pt.Y - 1);
			}
		}

		private void LogBox_Load(object sender, EventArgs e)
		{

		}
	}
}
