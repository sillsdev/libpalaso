// Copyright (c) 2015 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace Palaso.UI.WindowsForms.Widgets
{
	/// <summary>
	/// TextInputBox is a wrapper around either a TextBox or a GeckoBox
	/// (implemented in PalasoUiWindowsForms.GeckoFxWebBrowserAdapter), which must be used by clients
	/// that are using GeckoFx.
	/// </summary>
	public class TextInputBox : UserControl
	{
		private ITextInputBox _inputBox;

		public static bool UseWebTextBox { get; set; }

		public TextInputBox()
		{
			_inputBox = CreateTextBox();
			this.Controls.Add(_inputBox.TheControl);
		}

		private ITextInputBox CreateTextBox()
		{
			if (UseWebTextBox)
			{
				var path = Path.Combine(Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath),
					"PalasoUIWindowsForms.GeckoBrowserAdapter.dll");
				if (File.Exists(path))
				{
					var assembly = Assembly.LoadFile(path);
					if (assembly != null)
					{
						var box = assembly.GetType("Palaso.UI.WindowsForms.GeckoBasedControls.GeckoBox");
						if (box != null)
						{
							try
							{
								return (ITextInputBox)Activator.CreateInstance(box);
							}
							catch (Exception e)
							{
#if DEBUG
								throw new Exception("Could not create gecko-based text box", e);
#endif
								//Eat exceptions creating the GeckoBox control
							}
						}
					}
				}
				Debug.Fail("could not create gecko-based text box");
			}
			// If we can't make a gecko one for any reason, the default one is only slightly imperfect.
			return new StdTextInputBox();
		}

		public ITextInputBox TextBox { get { return _inputBox; } }

		public override string Text
		{
			get { return _inputBox.Text; }
			set { _inputBox.Text = value; }
		}

		/// <summary>
		/// We can't call OnKeyDown directly from _inputBox.OnKeyDown, so here's an indirect path.
		/// </summary>
		public void OnChildKeyDown(KeyEventArgs e)
		{
			OnKeyDown(e);
		}
	}
}
