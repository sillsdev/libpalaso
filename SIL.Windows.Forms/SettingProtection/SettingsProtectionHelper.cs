using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using JetBrains.Annotations;

namespace SIL.Windows.Forms.SettingProtection
{
	/// <summary>
	/// This class takes an Settings-launching control and adds the behaviors needed to conform to the standard SIL
	/// "Rice Farmer" settings protection behavior.
	/// If you use the standard SettingsLauncherButton, you don't have to worry about this class (it uses this).
	/// But if you have a need custom control for look/feel, then add this component to the form and
	/// in the designer, set the SettingsProtection valued for the control(s) you want to hide.
	/// When you control is clicked, have it call LaunchSettingsIfAppropriate()
	/// </summary>
	///

	//NB: I wanted this to be typeof(Component), in the hopes of also netting ToolStripItems,
	//but it just doesn't work. So, for those, use the ManageComponent() to add it at runtime
	[ProvideProperty("SettingsProtection", typeof(Control))]
	public partial class SettingsProtectionHelper : Component, IExtenderProvider
	{
		private readonly HashSet<Component> _componentsUnderSettingsProtection;
		private bool _isDisposed;

		public bool CanExtend(object extendee)
		{
			VerifyNotDisposed();
			return extendee is Control || extendee is ToolStripButton;
		}

		public SettingsProtectionHelper(IContainer container)
		{
			InitializeComponent();

			_componentsUnderSettingsProtection = new HashSet<Component>();

			if (LicenseManager.UsageMode != LicenseUsageMode.Designtime)
			{
				container?.Add(this);
				_checkForCtrlKeyTimer.Enabled = true;
			}
		}

		/// <summary>
		/// The control should call this when the user clicks on it. It will challenge if necessary, and carry out the supplied code if everything is rosy.
		/// </summary>
		/// <param name="settingsLaunchingFunction"></param>
		/// <returns>DialogResult.Cancel if the challenge fails, otherwise whatever the settingsLaunchingFunction returns.</returns>
		public DialogResult LaunchSettingsIfAppropriate(Func<DialogResult> settingsLaunchingFunction)
		{
			if (SettingsProtectionSingleton.Settings.RequirePassword)
			{
				using (var dlg = new SettingsPasswordDialog(SettingsProtectionSingleton.FactoryPassword, SettingsPasswordDialog.Mode.Challenge))
				{
					if (DialogResult.OK != dlg.ShowDialog())
						return DialogResult.Cancel;
				}
			}
			var result = settingsLaunchingFunction();
			UpdateDisplay();
			return result;
		}

		private void UpdateDisplay()
		{
			if (_componentsUnderSettingsProtection == null)//sometimes get a tick before this has been set
				return;

			var keys = Keys.Control | Keys.Shift;

			foreach (var component in _componentsUnderSettingsProtection)
			{
				bool visible = !SettingsProtectionSingleton.Settings.NormallyHidden || ((Control.ModifierKeys & keys) == keys);

				ShowOrHideComponent(component, visible);
			}
		}

		private static void ShowOrHideComponent(Component component, bool visible)
		{
			if (component is Control control)
				control.Visible = visible;
			else if (component is ToolStripItem item)
				item.Visible = visible;
			else
				throw new InvalidCastException(
					"Only components which are Controls or ToolStripItems can be under settings protection.");
		}

		private void _checkForCtrlKeyTimer_Tick(object sender, EventArgs e)
		{
			UpdateDisplay();
		}

		#region IExtenderProvider Members
		[PublicAPI]
		[DefaultValue(false)]
		public bool GetSettingsProtection(Control c)
		{
			if (c == null)
				throw new ArgumentNullException();

			return _componentsUnderSettingsProtection.Contains(c);
		}

		[PublicAPI]
		public void SetSettingsProtection(Control c, bool isProtected)
		{
			if (c == null)
				throw new ArgumentNullException();

			if (isProtected)
				_componentsUnderSettingsProtection.Add(c);
			else
			{
				_componentsUnderSettingsProtection.Remove(c);
				ShowOrHideComponent(c, true);
			}
		}
		#endregion

		#region IComponent Members

		public override ISite Site
		{
			get
			{
				VerifyNotDisposed();
				return base.Site;
			}
			set
			{
				VerifyNotDisposed();
				base.Site = value;
			}
		}

		private void VerifyNotDisposed()
		{
			if (_isDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
		}
		#endregion

		/// <summary>
		/// Allows you to dynamically add a control or ToolStripItem, rather than having to use the winforms Designer
		/// </summary>
		/// <remarks>
		/// Equivalent to calling SetSettingsProtection with isProtected true.
		/// </remarks>
		/// <exception cref="ArgumentNullException">controlOrToolStripItem was null</exception>
		/// <exception cref="ArgumentException">Although this method's signature seems to imply
		/// that it can take any component, it actually only supports Controls and ToolStripItems.
		/// For another type of component to be supported, it would have to have a Visible property
		/// (or some other property or method that could be used to hide or show it) and explicit
		/// code would need to be added to allow for it.</exception>
		[PublicAPI]
		public void ManageComponent(Component controlOrToolStripItem)
		{
			if (controlOrToolStripItem is Control ctrl)
				SetSettingsProtection(ctrl, true);
			else if (controlOrToolStripItem is ToolStripItem item)
				SetSettingsProtection(item, true);
			else if (controlOrToolStripItem == null)
				throw new ArgumentNullException(nameof(controlOrToolStripItem));
			else
				throw new ArgumentException("Only components which are Controls or ToolStripItems can be managed.",
					nameof(controlOrToolStripItem));
		}

		/// <summary>
		/// Allows you to dynamically make a ToolStripItem protected (i.e., managed) or not,
		/// rather than having to use the winforms Designer
		/// </summary>
		/// <exception cref="ArgumentNullException">controlOrToolStripItem was null</exception>
		[PublicAPI]
		public void SetSettingsProtection(ToolStripItem c, bool isProtected)
		{
			if (c == null)
				throw new ArgumentNullException();

			if (isProtected)
				_componentsUnderSettingsProtection.Add(c);
			else
			{
				_componentsUnderSettingsProtection.Remove(c);
				ShowOrHideComponent(c, true);
			}
		}
	}
}
