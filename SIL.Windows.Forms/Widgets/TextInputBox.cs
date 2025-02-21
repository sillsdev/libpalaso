// Copyright (c) 2025 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using SIL.Reporting;

namespace SIL.Windows.Forms.Widgets
{
	/// <summary>
	/// TextInputBox is a wrapper around either a TextBox or a GeckoBox
	/// (implemented in SIL.Windows.Forms.GeckoBrowserAdapter), which must be used by clients
	/// that are using GeckoFx.
	/// </summary>
	public class TextInputBox : UserControl
	{
		private ITextInputBox _inputBox;

		public static bool UseWebTextBox { get; set; }

		public TextInputBox()
		{
			_inputBox = CreateTextBox();
			// If the inner control is a Gecko-based text box (currently accessed only by
			// reflection), this doesn't seem to be needed.  If the inner control is a TextBox,
			// this is the only way to get the inner text box to be as wide as this control is
			// supposed to be.
			if (_inputBox.TheControl is TextBox)
				_inputBox.TheControl.Dock = DockStyle.Fill;	// Make the real inner box match the size of the outer virtual box.
			this.Controls.Add(_inputBox.TheControl);
		}

		private ITextInputBox CreateTextBox()
		{
			if (UseWebTextBox)
			{
				var path = Path.Combine(Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().Location).LocalPath),
					"SIL.Windows.Forms.GeckoBrowserAdapter.dll");
				if (File.Exists(path))
				{
					var assembly = Assembly.LoadFile(path);
					if (assembly != null)
					{
						var box = assembly.GetType("SIL.Windows.Forms.GeckoBrowserAdapter.GeckoBox");
						if (box != null)
						{
							try
							{
								return (ITextInputBox)Activator.CreateInstance(box);
							}
							catch (Exception e)
							{
								Logger.WriteMinorEvent("Could not create gecko - based text box", e);
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

		protected override void OnSizeChanged(EventArgs e)
		{
			// If the inner control is a Gecko-based text box (currently accessed only by
			// reflection), setting its minimum size like this causes a spurious, useless scrollbar
			// to appear.  If the inner control is a TextBox, this is the only way to get the visual
			// affect of the text box being as high as this control is supposed to be.
			if (_inputBox.TheControl is TextBox)
				_inputBox.TheControl.MinimumSize = Size;
			base.OnSizeChanged(e);
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
