using System;
using System.ComponentModel;
using System.Drawing;

using System.Windows.Forms;

namespace Palaso.UI.WindowsForms.WritingSystems
{
	public partial class FontControl : UserControl
	{
	  //  private Font _font;

		public FontControl()
		{
			InitializeComponent();
		}

		public string FontFamilyName
		{
			get { return this._fontFamilyCombo.Text; }
			set
			{
				this._fontFamilyCombo.Text = value;
				UpdateFontDisplay();
			}
		}

//        private void _btnFont_Click(object sender, EventArgs e)
//        {
//            _fontDialog.Font = _font;
//            _fontDialog.ShowColor = false;
//            _fontDialog.ShowEffects = false;
//            if (DialogResult.OK != _fontDialog.ShowDialog())
//            {
//                return;
//            }
//            _font = _fontDialog.Font;
//            UpdateFontDisplay();
//        }

		private void UpdateFontDisplay()
		{

			try
			{
				_sampleTextBox.Font = new Font(_fontFamilyCombo.Text, 12);
				_sampleTextBox.ForeColor = Color.Black;
				if (_sampleTextBox.Font.FontFamily.Name != _fontFamilyCombo.Text.Trim())
				{
				_sampleTextBox.ForeColor=Color.Gray;
				}
			}
			catch(Exception)
			{
				_sampleTextBox.ForeColor=Color.Gray;
			}

			this.Invalidate();
		}

		private void _fontFamilyCombo_SelectedIndexChanged(object sender, EventArgs e)
		{
			UpdateFontDisplay();
		}

		private void FontControl_Load(object sender, EventArgs e)
		{
			_fontFamilyCombo.Items.Clear();
			foreach (FontFamily family in System.Drawing.FontFamily.Families)
			{
				_fontFamilyCombo.Items.Add(family.Name);
			}
			_sampleTextBox.Text = "sample";
			UpdateFontDisplay();
		}

		private void _fontFamilyCombo_TextChanged(object sender, EventArgs e)
		{
			UpdateFontDisplay();
		}
	}
}