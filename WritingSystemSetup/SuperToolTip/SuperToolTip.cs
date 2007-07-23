using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Design;
using System.Drawing;


namespace Elsehemy
{
	[ProvideProperty("SuperStuff", typeof(Control))]
	[ToolboxItemFilter("System.Windows.Forms")]
	[ToolboxBitmap(typeof(SuperToolTip), "SuperToolTip.jpg")]
	public partial class SuperToolTip : Component, IExtenderProvider
	{
		#region Private Members

		private Dictionary<Control, SuperToolTipInfoWrapper> controlTable;

		private SuperToolTipWindow window;
		private SuperToolTipControlHost host;
		private SuperToolTipWindowData winData;
		private Timer timer;

		private int fadingDirection;
		private bool isFadding;
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
			get { return timer.Interval; }
			set
			{
				if (value > 0) timer.Interval = value;
				else throw new ArgumentException("Value must be greater than 0.!");
			}

		}



		#endregion

		#region Public Provided Properties

		[DisplayName("SuperStuff")]
		[TypeConverter(typeof(ExpandableObjectConverter))]
		[Editor(typeof(Elsehemy.SuperToolTipEditor), typeof(UITypeEditor))]
		public SuperToolTipInfoWrapper GetSuperStuff(Control control)
		{
			return GetControlInfo(control);
		}

		public void SetSuperStuff(Control control, SuperToolTipInfoWrapper info)
		{
			SetContolInfo(control, info);
		}

		public void ResetSuperStuff(Control control)
		{
			controlTable[control].UseSuperToolTip = false;
			controlTable[control].SuperToolTipInfo = null;
		}

		public bool ShouldSerializeSuperStuff(Control control)
		{
			return controlTable[control].UseSuperToolTip;
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
		{ Show((Control)sender); }

		private void MouseLeft(object sender, EventArgs e)
		{
			Close();
		}

		#endregion

		#region Public Methods

		public void Close()
		{
			if (_useFadding)
			{
				isFadding = true;
				FadeOut();
			}
			else
			{
				window.Close();
			}
		}

		public void Show(Control owner)
		{
			if (controlTable[owner].UseSuperToolTip)
			{

				winData.SuperInfo = controlTable[owner].SuperToolTipInfo;
				if (isFadding)
				{
					window.Close();
				}
				if (_useFadding)
				{
					isFadding = true;
					FadeIn();
				}

				window.Show(owner, new Point(0, owner.Height), ToolStripDropDownDirection.BelowRight);
			}
		}

		#endregion

		#region Helpers

		private void InternalInitialize()
		{
			timer = new Timer();
			timer.Interval = 50;
			_useFadding = true;
			fadingDirection = 1;
			timer.Tick += delegate
			{
				if (window.Opacity == 1 && fadingDirection == 1)
				{
					timer.Enabled = false;
					isFadding = false;
				}
				if (window.Opacity == 0 && fadingDirection == -1)
				{
					timer.Enabled = false;
					isFadding = false;
					window.Close();
				}
				isFadding = true;
				window.Opacity = window.Opacity + 0.5 * fadingDirection;
			};

			winData = new SuperToolTipWindowData();
			window = new SuperToolTipWindow();
			host = new SuperToolTipControlHost(winData, "windowHost");
			controlTable = new Dictionary<Control, SuperToolTipInfoWrapper>();
			host.Margin = window.Margin = host.Padding = window.Padding = new Padding(0);
			window.Items.Add(host);
		}

		private SuperToolTipInfoWrapper GetControlInfo(Control control)
		{
			if (!controlTable.ContainsKey(control))
				controlTable.Add(control, new SuperToolTipInfoWrapper());
			return controlTable[control];
		}

		private void SetContolInfo(Control control, SuperToolTipInfoWrapper info)
		{
			if (controlTable.ContainsKey(control))
			{
				if (info == null)
				//hook events to our event handlers;
				{
					control.MouseEnter -= new EventHandler(this.MouseEntered);
					control.MouseLeave -= new EventHandler(this.MouseLeft);
					controlTable.Remove(control);
					return;
				}
				controlTable[control] = info;
			}
			else
			{
				controlTable.Add(control, info);
				//hook events to our event handlers;
				control.MouseEnter += new EventHandler(this.MouseEntered);
				control.MouseLeave += new EventHandler(this.MouseLeft);
			}
		}

		#region Fadding

		private void FadeIn()
		{
			window.Opacity = 0;
			fadingDirection = 1;
			timer.Enabled = true;
		}

		private void FadeOut()
		{
			window.Opacity = 1;
			fadingDirection = -1;
			timer.Enabled = true;
		}

		#endregion

		#endregion

		#region Designer
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

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
	class SuperToolTipWindow : ToolStripDropDown
	{ }

	[ToolboxItem(false)]
	class SuperToolTipControlHost : ToolStripControlHost
	{
		public SuperToolTipControlHost(Control c)
			: base(c)
		{

		}
		public SuperToolTipControlHost(Control c, string name)
			: base(c, name)
		{

		}
	}
}
