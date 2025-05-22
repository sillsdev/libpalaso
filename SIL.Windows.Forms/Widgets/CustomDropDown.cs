using System;
using System.Windows.Forms;

namespace SIL.Windows.Forms.Widgets
{
	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// This class extends ToolStripDropDown by providing a means to easily host custom
	/// controls on a drop-down, whether the custom control is dropped down from a toolbar
	/// button, menu, or just by calling the Show() method.
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	public class CustomDropDown : ToolStripDropDown
	{
		private Timer _tmrMouseMonitor;
		private Timer _tmrVisibilityTimeout;
		private bool _mouseOver;
		private bool _autoCloseWhenMouseLeaves = true;
		private Control _hostedControl;

		/// ------------------------------------------------------------------------------------
		public CustomDropDown()
		{
			Padding = Padding.Empty;
			AutoSize = false;
			LayoutStyle = ToolStripLayoutStyle.Table;
			DropShadowEnabled = true;
			DoubleBuffered = true;
		}

		/// ------------------------------------------------------------------------------------
		protected override void Dispose(bool disposing)
		{
			if (_hostedControl != null)
				_hostedControl.Resize -= HandleHostedControlResize;

			base.Dispose(disposing);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets a value indicating whether or not the drop down will automatically
		/// close several seconds after the mouse is no longer over the drop-down.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public bool AutoCloseWhenMouseLeaves
		{
			get { return _autoCloseWhenMouseLeaves; }
			set
			{
				_autoCloseWhenMouseLeaves = value;
				if (!value && _tmrMouseMonitor != null)
				{
					_tmrMouseMonitor.Stop();
					_tmrMouseMonitor.Dispose();
					_tmrMouseMonitor = null;
				}
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Adds a host to the drop-down.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public void AddHost(ToolStripControlHost host)
		{
			_hostedControl = host.Control;
			Size = _hostedControl.Size;
			_hostedControl.Dock = DockStyle.Fill;

			host.AutoSize = false;
			host.Dock = DockStyle.Fill;
			host.Padding = Padding.Empty;
			host.Margin = Padding.Empty;
			host.Size = _hostedControl.Size;
			Items.Add(host);

			_hostedControl.Resize += HandleHostedControlResize;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Adjust the size of the popup as the size of the hosted control changes. This
		/// is necessary in case the hosted control can be resized by the user at runtime.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		void HandleHostedControlResize(object sender, EventArgs e)
		{
			Size = _hostedControl.Size;
		}

		/// ------------------------------------------------------------------------------------
		public void AddControl(Control ctrl)
		{
			if (ctrl != null)
			{
				Items.Clear();
				AddHost(new ToolStripControlHost(ctrl));
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Start and stop the timer when the owning drop-down's visibility changes.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);

			if (Visible && _autoCloseWhenMouseLeaves && _hostedControl != null)
			{
				_hostedControl.Invalidate();
				InitializeMouseMonitorTimer();
			}
			else if (!Visible && _tmrMouseMonitor != null)
			{
				_tmrMouseMonitor.Stop();
				_tmrMouseMonitor.Dispose();
				_tmrMouseMonitor = null;
			}
		}

		/// ------------------------------------------------------------------------------------
		private void InitializeMouseMonitorTimer()
		{
			_tmrMouseMonitor = new Timer();
			_tmrMouseMonitor.Interval = 1;
			_tmrMouseMonitor.Tick += HandleTimerMouseMonitorTick;
			_tmrMouseMonitor.Start();
		}

		/// ------------------------------------------------------------------------------------
		private void HandleTimerMouseMonitorTick(object sender, EventArgs e)
		{
			bool prevMouseOverValue = _mouseOver;
			var pt = _hostedControl.PointToClient(MousePosition);
			_mouseOver = _hostedControl.ClientRectangle.Contains(pt);

			if (!_mouseOver && prevMouseOverValue)
			{
				// The mouse has left the popup so setup a timer to make it disappear
				// in 2 seconds if the mouse doesn't come back over it sooner.
				InitializeVisibilityTimeoutTimer();
			}
			else if (_mouseOver && !prevMouseOverValue && _tmrVisibilityTimeout != null)
			{
				// The mouse has come back over the popup so
				// terminate the visibility timeout timer.
				_tmrVisibilityTimeout.Stop();
				_tmrVisibilityTimeout = null;
			}
		}

		/// ------------------------------------------------------------------------------------
		private void InitializeVisibilityTimeoutTimer()
		{
			_tmrVisibilityTimeout = new Timer();
			_tmrVisibilityTimeout.Interval = 2000;
			_tmrVisibilityTimeout.Tick += HandleTimerVisibilityTimeoutTick;
			_tmrVisibilityTimeout.Start();
		}

		/// ------------------------------------------------------------------------------------
		void HandleTimerVisibilityTimeoutTick(object sender, EventArgs e)
		{
			_tmrVisibilityTimeout.Stop();
			Hide();
		}

		/// ------------------------------------------------------------------------------------
		protected override void OnOpening(System.ComponentModel.CancelEventArgs e)
		{
			Opacity = 0;

			base.OnOpening(e);

			if (e.Cancel)
				return;

			var timer = new Timer();
			timer.Interval = 1;
			timer.Tick += delegate
			{
				Opacity += 0.1;
				if (Opacity >= .99)
					timer.Stop();
			};

			timer.Start();
		}
	}
}
