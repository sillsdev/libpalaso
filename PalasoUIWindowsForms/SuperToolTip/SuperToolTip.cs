using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;

namespace Elsehemy
{
	[ProvideProperty("SuperStuff", typeof (Control))]
	[ToolboxItemFilter("System.Windows.Forms")]
	[ToolboxBitmap(typeof (SuperToolTip), "SuperToolTip.jpg")]
	public partial class SuperToolTip : Component, IExtenderProvider
	{
		#region Private Members

		private Dictionary<Control, SuperToolTipInfoWrapper> controlTable;

		private SuperToolTipWindow window;
		private SuperToolTipWindowData winData;
		private Timer fadingTimer;

		private enum FadingDirection
		{
			FadeIn,
			FadeOut
		} ;

		private FadingDirection fadingDirection;
		private bool _useFadding;

		#endregion

		#region Constructors

		public SuperToolTip()
		{
			InitializeComponent();

			InternalInitialize();
		}

		public SuperToolTip(IContainer container)
		{
			container.Add(this);

			InitializeComponent();

			InternalInitialize();
		}

		#endregion

		#region Public Properties

		[DefaultValue(true)]
		public bool UseFadding
		{
			get { return _useFadding; }
			set { _useFadding = value; }
		}

		[DefaultValue(50)]
		public int FadingInterval
		{
			get { return fadingTimer.Interval; }
			set
			{
				if (value > 0)
				{
					fadingTimer.Interval = value;
				}
				else
				{
					throw new ArgumentException("Value must be greater than 0.!");
				}
			}
		}

		#endregion

		#region Public Provided Properties

		[DisplayName("SuperStuff")]
		[TypeConverter(typeof (ExpandableObjectConverter))]
		[Editor(typeof (SuperToolTipEditor), typeof (UITypeEditor))]
		public SuperToolTipInfoWrapper GetSuperStuff(Control control)
		{
			return GetControlInfo(control);
		}

		public void SetSuperStuff(Control control, SuperToolTipInfoWrapper info)
		{
			SetControlInfo(control, info);
		}

		public void ResetSuperStuff(Control control)
		{
			SetControlInfo(control, null);
		}

		public bool ShouldSerializeSuperStuff(Control control)
		{
			SuperToolTipInfoWrapper wrapper;
			if (controlTable.TryGetValue(control, out wrapper))
			{
				return wrapper.UseSuperToolTip;
			}
			return false;
		}

		#endregion

		#region IExtenderProvider Members

		public bool CanExtend(object extendee)
		{
			return ((extendee is Control) && !(extendee is SuperToolTip) && !(extendee is Form));
		}

		#endregion

		#region Control Event Handlers

		private void MouseEntered(object sender, EventArgs e)
		{
			Show((Control) sender);
		}

		private void MouseLeft(object sender, EventArgs e)
		{
			if (window.Bounds.Contains(Control.MousePosition))
			{
				return;
			}
			if (!window.HasMouse)
			{
				Close();
			}
		}

		private void OnToolTipMouseLeave(object sender, EventArgs e)
		{
			Close();
		}

		#endregion

		#region Public Methods

		public void Close()
		{
			if (_useFadding)
			{
				FadeOut();
			}
			else
			{
				CloseTooltip();
			}
		}

		public void Show(Control owner)
		{
			if (controlTable[owner].UseSuperToolTip)
			{
				CloseTooltip();
				if (_useFadding)
				{
					FadeIn();
				}

				winData.SuperInfo = controlTable[owner].SuperToolTipInfo;
				window.Size = winData.Size;
				if (winData.SuperInfo.OffsetForWhereToDisplay != default(Point))
				{
					window.Location = owner.PointToScreen(winData.SuperInfo.OffsetForWhereToDisplay);
				}
				else
				{
					window.Location = owner.PointToScreen(new Point(0, owner.Height));
				}
				window.Show(owner);
			}
		}

		private void CloseTooltip()
		{
			fadingTimer.Stop();
			window.MouseLeave -= OnToolTipMouseLeave;
			window.Close();
			window = null;
			CreateTooltipWindows();
		}

		private void CreateTooltipWindows()
		{
			window = new SuperToolTipWindow();
			window.MouseLeave += OnToolTipMouseLeave;
			winData = new SuperToolTipWindowData();
			winData.SizeChanged += OnWindowSizeChanged;
			window.Controls.Add(winData);
		}

		private void OnWindowSizeChanged(object sender, EventArgs e)
		{
			window.Size = winData.Size;
		}

		#endregion

		#region Helpers

		private void InternalInitialize()
		{
			_useFadding = true;

			fadingTimer = new Timer();
			fadingTimer.Interval = 10;
			fadingTimer.Tick += FadeOnTick;

			CreateTooltipWindows();
			controlTable = new Dictionary<Control, SuperToolTipInfoWrapper>();
		}

		private void FadeOnTick(object obj, EventArgs e)
		{
			if (window.Opacity == 1 && fadingDirection == FadingDirection.FadeIn)
			{
				fadingTimer.Stop();
			}
			if (window.Opacity == 0 && fadingDirection == FadingDirection.FadeOut)
			{
				fadingTimer.Stop();
				window.Close();
			}
			window.Opacity = window.Opacity + (fadingDirection == FadingDirection.FadeOut ? -.1 : .1);
		}

		private SuperToolTipInfoWrapper GetControlInfo(Control control)
		{
			if (!controlTable.ContainsKey(control))
			{
				controlTable.Add(control, new SuperToolTipInfoWrapper());
			}
			return controlTable[control];
		}

		private void SetControlInfo(Control control, SuperToolTipInfoWrapper info)
		{
			if (controlTable.ContainsKey(control))
			{
				if (info == null)
						//hook events to our event handlers;
				{
					control.MouseEnter -= new EventHandler(MouseEntered);
					control.MouseLeave -= new EventHandler(MouseLeft);

					controlTable.Remove(control);
					return;
				}
				controlTable[control] = info;
			}
			else
			{
				controlTable.Add(control, info);
				//hook events to our event handlers;
				control.MouseEnter += new EventHandler(MouseEntered);
				control.MouseLeave += new EventHandler(MouseLeft);
			}
		}

		#region Fadding

		private void FadeIn()
		{
			window.Opacity = 0;
			fadingDirection = FadingDirection.FadeIn;
			fadingTimer.Start();
		}

		private void FadeOut()
		{
			if (window.Visible)
			{
				window.Opacity = 1;
				fadingDirection = FadingDirection.FadeOut;
				fadingTimer.Start();
			}
		}

		#endregion

		#endregion

		#region Designer

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
		}

		#endregion

		#endregion
	}

	[ToolboxItem(false)]
	internal class SuperToolTipWindow : Form
	{
		private bool _hasMouse;

		public SuperToolTipWindow()
		{
			FormBorderStyle = FormBorderStyle.FixedToolWindow;
			ShowInTaskbar = false;
			ControlBox = false;

			StartPosition = FormStartPosition.Manual;
		}

		public bool HasMouse
		{
			get { return _hasMouse; }
		}

		protected override void OnMouseEnter(EventArgs e)
		{
			base.OnMouseEnter(e);
			_hasMouse = true;
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);
			_hasMouse = false;
		}
	}
}