using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using L10NSharp;
using Microsoft.Win32;
using SIL.WritingSystems.WindowsForms.Keyboarding;
using SIL.WritingSystems.WindowsForms.Keyboarding.Interfaces;
using SIL.WritingSystems;

namespace SIL.WritingSystems.WindowsForms
{
	public partial class WSKeyboardControl : UserControl
	{

		private class KeyboardDefinitionAdapter
		{
			private IKeyboardDefinition _descriptor;

			public KeyboardDefinitionAdapter(IKeyboardDefinition descriptor)
			{
				_descriptor = descriptor;
			}

			public string Id
			{
				get { return _descriptor.Id; }
			}

			public string Layout
			{
				get { return _descriptor.Layout; }
			}

			public string Locale
			{
				get { return _descriptor.Locale; }
			}

			public override string ToString()
			{
				return _descriptor.LocalizedName;
			}
		}

		private WritingSystemSetupModel _model;
		private string _defaultFontName;
		private float _defaultFontSize;

		public WSKeyboardControl()
		{
			InitializeComponent();
			if (KeyboardController.IsInitialized)
				KeyboardController.Register(_testArea);
			_defaultFontSize = _testArea.Font.SizeInPoints;
			_defaultFontName = _testArea.Font.Name;
			_possibleKeyboardsList.ShowItemToolTips = true;
#if MONO
	// Keyman is not supported, setup link should not say "Windows".
			_keymanConfigurationLink.Visible = false;
			_keyboardSettingsLink.Text = L10NSharp.LocalizationManager.GetString("WSKeyboardControl.SetupKeyboards", "Set up keyboards");

			// The sequence of Events in Mono dictate using GotFocus instead of Enter as the point
			// when we want to assign keyboard and font to this textbox.  (For some reason, using
			// Enter works fine for the WSFontControl._testArea textbox control.)
			this._testArea.Enter -= new System.EventHandler(this._testArea_Enter);
			this._testArea.GotFocus += new System.EventHandler(this._testArea_Enter);
#endif
		}

		private bool _hookedToForm;

		/// <summary>
		/// This seems to be the best available hook to connect this control to the form's Activated event.
		/// The control can't be visible until it is on a form!
		/// It is tempting to unhook it when it is no longer visible, but unfortunately OnVisibleChanged
		/// apparently doesn't work like that. According to http://memprofiler.com/articles/thecontrolvisiblechangedevent.aspx,
		/// it is fired when visibility is gained because parent visibility changed, but not when visibility
		/// is LOST because parent visibility changed.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);
			if (_hookedToForm || !(TopLevelControl is Form))
				return;
			_hookedToForm = true;
			((Form) TopLevelControl).Activated += WSKeyboardControl_Activated;
		}

		// When the top-level form we are part of is activated, update our keyboard list,
		// in case the user changed things using the control panel.
		private void WSKeyboardControl_Activated(object sender, EventArgs e)
		{
			Keyboard.Controller.UpdateAvailableKeyboards(); // Enhance JohnT: would it be cleaner to have a Model method to do this?
			PopulateKeyboardList();
			UpdateFromModel(); // to restore selection.
		}

		public void BindToModel(WritingSystemSetupModel model)
		{
			if (_model != null)
			{
				_model.SelectionChanged -= ModelSelectionChanged;
				_model.CurrentItemUpdated -= ModelCurrentItemUpdated;
			}
			_model = model;
			Enabled = false;
			if (_model != null)
			{
				_model.SelectionChanged += ModelSelectionChanged;
				_model.CurrentItemUpdated += ModelCurrentItemUpdated;
				PopulateKeyboardList();
				UpdateFromModel();
			}
			this.Disposed += OnDisposed;
		}

		private void OnDisposed(object sender, EventArgs e)
		{
			if (_model != null)
				_model.SelectionChanged -= ModelSelectionChanged;
		}

		protected override void OnSizeChanged(EventArgs e)
		{
			base.OnSizeChanged(e);
			// single column occupies whole list, leaving room for vertical scroll so we don't get a spurious
			// horizontal one.
			_keyboards.Width = _possibleKeyboardsList.ClientSize.Width - SystemInformation.VerticalScrollBarWidth;
		}

		private Color _unavailableColor = Color.FromKnownColor(KnownColor.DimGray);

		private Color _labelColor = Color.LightCyan;

		private ListViewItem MakeLabelItem(string text)
		{
			var label1 = new ListViewItem(text);
			label1.BackColor = _labelColor;
			label1.Font = new Font(_possibleKeyboardsList.Font, FontStyle.Bold);
			return label1;
		}

		private bool IsItemLabel(ListViewItem item)
		{
			return item.BackColor == _labelColor;
		}

		private void PopulateKeyboardList()
		{
			if (_model == null || !_model.HasCurrentSelection)
			{
				_possibleKeyboardsList.Items.Clear();
				return;
			}
			_possibleKeyboardsList.BeginUpdate();
			Rectangle originalBounds = _possibleKeyboardsList.Bounds;
			_possibleKeyboardsList.Items.Clear();
			_possibleKeyboardsList.Items.Add(MakeLabelItem(
				LocalizationManager.GetString("WSKeyboardControl.KeyboardsPreviouslyUsed", "Previously used keyboards")));
			var unavailableFont = new Font(_possibleKeyboardsList.Font, FontStyle.Italic);
			foreach (var keyboard in _model.KnownKeyboards)
			{
				var adapter = new KeyboardDefinitionAdapter(keyboard);
				var item = new ListViewItem(adapter.ToString());
				item.Tag = adapter;
				if (!keyboard.IsAvailable)
				{
					item.Font = unavailableFont;
					item.ForeColor = _unavailableColor;
				}
				item.ToolTipText = adapter.ToString();
				_possibleKeyboardsList.Items.Add(item);
				if (keyboard == _model.CurrentKeyboard)
					item.Selected = true;
			}
			_possibleKeyboardsList.Items.Add(MakeLabelItem(
				LocalizationManager.GetString("WSKeyboardControl.KeyboardsAvailable", "Available keyboards")));
			foreach (var keyboard in _model.OtherAvailableKeyboards)
			{
				var adapter = new KeyboardDefinitionAdapter(keyboard);
				var item = new ListViewItem(adapter.ToString());
				item.Tag = adapter;
				item.ToolTipText = adapter.ToString();
				_possibleKeyboardsList.Items.Add(item);
			}

			_possibleKeyboardsList.Bounds = originalBounds;
			_possibleKeyboardsList.EndUpdate();
		}

		private void ModelCurrentItemUpdated(object sender, EventArgs e)
		{
			UpdateFromModel();
		}

		private void ModelSelectionChanged(object sender, EventArgs e)
		{
			PopulateKeyboardList(); // different writing system may have different current ones.
			UpdateFromModel();
		}

		private string _selectKeyboardPattern;

		private void UpdateFromModel()
		{
			if (_model == null || !_model.HasCurrentSelection)
			{
				Enabled = false;
				_selectKeyboardLabel.Visible = false;
				return;
			}
			Enabled = true;
			_selectKeyboardLabel.Visible = true;
			if (_selectKeyboardPattern == null)
				_selectKeyboardPattern = _selectKeyboardLabel.Text;
			_selectKeyboardLabel.Text = string.Format(_selectKeyboardPattern, _model.CurrentDefinition.ListLabel);
			KeyboardDefinitionAdapter currentKeyboardDefinition = null;
			if (_possibleKeyboardsList.SelectedItems.Count > 0)
				currentKeyboardDefinition = _possibleKeyboardsList.SelectedItems[0].Tag as KeyboardDefinitionAdapter;

			if (_model.CurrentKeyboard != null &&
				(currentKeyboardDefinition == null || _model.CurrentKeyboard.Layout != currentKeyboardDefinition.Layout
				|| _model.CurrentKeyboard.Locale != currentKeyboardDefinition.Locale))
			{
				foreach (ListViewItem item in _possibleKeyboardsList.Items)
				{
					var keyboard = item.Tag as KeyboardDefinitionAdapter;
					if (keyboard != null && keyboard.Layout == _model.CurrentKeyboard.Layout
						&& keyboard.Locale == _model.CurrentKeyboard.Locale)
					{
						//_possibleKeyboardsList.SelectedItems.Clear();
						// We're updating the display from the model, so we don't need to run the code that
						// updates the model from the display.
						_possibleKeyboardsList.SelectedIndexChanged -= _possibleKeyboardsList_SelectedIndexChanged;
						item.Selected = true; // single select is true, so should clear any old one.
						_possibleKeyboardsList.SelectedIndexChanged += _possibleKeyboardsList_SelectedIndexChanged;
						break;
					}
				}
			}
			SetTestAreaFont();
		}

		private void SetTestAreaFont()
		{
			float fontSize = _model.CurrentDefaultFontSize;
			if (fontSize <= 0 || float.IsNaN(fontSize) || float.IsInfinity(fontSize))
			{
				fontSize = _defaultFontSize;
			}
			string fontName = _model.CurrentDefaultFontName;
			if (string.IsNullOrEmpty(fontName))
			{
				fontName = _defaultFontName;
			}
			_testArea.Font = new Font(fontName, fontSize);
			_testArea.RightToLeft = _model.CurrentRightToLeftScript ? RightToLeft.Yes : RightToLeft.No;
		}

		protected override void OnGotFocus(EventArgs e)
		{
			base.OnGotFocus(e);
			_possibleKeyboardsList.Focus(); // allows the user to use arrows to select, also makes selection more conspicuous
		}

		private string _previousKeyboard;
		private int _previousTime;

		private void _possibleKeyboardsList_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (_possibleKeyboardsList.SelectedItems.Count != 0)
			{
				var item = _possibleKeyboardsList.SelectedItems[0];
				if (IsItemLabel(item) || item.ForeColor == _unavailableColor)
				{
					item.Selected = false;
					UpdateFromModel(); // back to whatever the model has
					return;
				}
			}
			if (Environment.TickCount - _previousTime < 1000 && _possibleKeyboardsList.SelectedItems.Count == 0)
			{
				_previousTime = Environment.TickCount;
			}
			_previousTime = Environment.TickCount;
			if (_model == null)
			{
				return;
			}
			if (_possibleKeyboardsList.SelectedItems.Count != 0)
			{
				var currentKeyboard = _possibleKeyboardsList.SelectedItems[0].Tag as KeyboardDefinitionAdapter;
				if (_model.CurrentKeyboard == null ||
					_model.CurrentKeyboard.Layout != currentKeyboard.Layout ||
					_model.CurrentKeyboard.Locale != currentKeyboard.Locale)
				{
					_model.CurrentKeyboard = Keyboard.Controller.CreateKeyboardDefinition(currentKeyboard.Layout,
						currentKeyboard.Locale);
				}
			}

		}

		private void _testArea_Enter(object sender, EventArgs e)
		{
			if (_model == null)
			{
				return;
			}
			_model.ActivateCurrentKeyboard();
			SetTestAreaFont();
		}

		private void _testArea_Leave(object sender, EventArgs e)
		{
			if (_model == null)
			{
				return;
			}
			Keyboard.Controller.ActivateDefaultKeyboard();
		}

		private void _windowsKeyboardSettingsLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			string program = null;
			string arguments = null;

#if MONO
			// Try for the most likely keyboard setup programs.  If none found,
			// inform the user.
			if (KeyboardController.CombinedKeyboardHandling)
			{
				if (File.Exists("/usr/bin/unity-control-center"))
				{
					program = "/usr/bin/unity-control-center";
					arguments = "region layouts";
				}
				else if (File.Exists("/usr/bin/gnome-control-center"))
				{
					program = "/usr/bin/gnome-control-center";
					arguments = "region layouts";
				}
			}
			else
			{
				if (File.Exists("/usr/bin/ibus-setup"))
					program = "/usr/bin/ibus-setup";
			}
			if (String.IsNullOrEmpty(program))
			{
				MessageBox.Show("Cannot open keyboard setup program", "Information");
				return;
			}
#else
			program = Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.System), @"control.exe");
			arguments = @"input.dll";
#endif
			var processInfo = new ProcessStartInfo(program, arguments);
			using (Process.Start(processInfo))
			{
			}
		}

		private void _keymanConfigurationLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			int version = 0;
			string keymanPath = GetKeymanRegistryValue(@"root path", ref version);
			if (keymanPath != null)
			{
				string keyman = Path.Combine(keymanPath, @"kmshell.exe");
				if (File.Exists(keyman))
				{
					// From Marc Durdin (7/16/09):
					// Re LT-9902, in Keyman 6, you could launch the configuration dialog reliably by running kmshell.exe.
					// However, Keyman 7 works slightly differently.  The recommended approach is to use the COM API:
					// http://www.tavultesoft.com/keymandev/documentation/70/comapi_interface_IKeymanProduct_OpenConfiguration.html
					// Sample code:
					//	dim kmcom, product
					//	Set kmcom = CreateObject("kmcomapi.TavultesoftKeyman")
					//	rem  Pro = ProductID 1; Light = ProductID 8
					//	rem  Following line will raise exception if product is not installed, so try/catch it
					//	Set product = kmcom.Products.ItemsByProductID(1)
					//	Product.OpenConfiguration
					// But if that is not going to be workable for you, then use the parameter  "-c" to start configuration.
					// Without a parameter, the action is to start Keyman Desktop itself; v7.0 would fire configuration if restarted,
					// v7.1 just flags to the user that Keyman is running and where to find it.  This change was due to feedback that
					// users would repeatedly try to start Keyman when it was already running, and get confused when they got the
					// Configuration dialog.  Sorry for the unannounced change... 9
					// The -c parameter will not work with Keyman 6, so you would need to test for the specific version.  For what it's worth, the
					// COM API is static and should not change, while the command line parameters are not guaranteed to change from version to version.
					string param = @"";
					if (version > 6)
						param = @"-c";
					using (Process.Start(keyman, param))
					{
						return;
					}
				}
			}
			MessageBox.Show(LocalizationManager.GetString("WSKeyboardControl.KeymanNotInstalled", "Keyman 5.0 or later is not Installed."));
		}

		/// <summary>
		/// This method returns the path to Keyman Configuration if it is installed. Otherwise it returns null.
		/// It also sets the version of Keyman that it found.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="version">The version.</param>
		/// <returns></returns>
		private static string GetKeymanRegistryValue(string key, ref int version)
		{
			using (var rkKeyman = Registry.LocalMachine.OpenSubKey(@"Software\Tavultesoft\Keyman", false))
			{
				if (rkKeyman == null)
					return null;

				//May 2008 version 7.0 is the lastest version. The others are here for
				//future versions.
				int[] versions = {10, 9, 8, 7, 6, 5};
				foreach (int vers in versions)
				{
					using (var rkApplication = rkKeyman.OpenSubKey(vers + @".0", false))
					{
						if (rkApplication != null)
						{
							foreach (string sKey in rkApplication.GetValueNames())
							{
								if (sKey == key)
								{
									version = vers;
									return (string) rkApplication.GetValue(sKey);
								}
							}
						}
					}
				}
			}

			return null;
		}
	}
}
