using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Design;
using System.Windows.Forms.Design;
using System.Drawing.Drawing2D;

namespace Elsehemy
{
	partial class dlgSuperToolTipEditor : Form
	{

		class ColorNameCouple
		{
			private Color cBegin;
			public Color ColorBegin
			{
				get { return cBegin; }
				set { cBegin = value; }
			}

			private Color cMiddle;
			public Color ColorMiddle
			{
				get { return cMiddle; }
				set { cMiddle = value; }
			}

			private Color cEnd;
			public Color ColorEnd
			{
				get { return cEnd; }
				set { cEnd = value; }
			}

			private string name;
			public string Name
			{
				get { return name; }
				set { name = value; }
			}

		}
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			LoadPredefinedColors();
		}
		private void LoadPredefinedColors()
		{
			cmbPredefined.DisplayMember = "Name";

			ColorNameCouple cnc;
			#region Office2007
			cnc = new ColorNameCouple();
			cnc.Name = "Office 2007";
			cnc.ColorBegin = Color.FromArgb(255,255,255);
			cnc.ColorMiddle = Color.FromArgb(242,246,251);
			cnc.ColorEnd = Color.FromArgb(202,218,239);
			cmbPredefined.Items.Add(cnc);
			#endregion

			#region Pinkish
			cnc = new ColorNameCouple();
			cnc.Name = "Pinkish";
			cnc.ColorBegin = Color.FromArgb(26, 6, 12); cnc.ColorMiddle = Color.FromArgb(148, 39, 72); cnc.ColorEnd = Color.FromArgb(238, 176, 193);
			cmbPredefined.Items.Add(cnc);
			#endregion

			#region Grayish
			cnc = new ColorNameCouple();
			cnc.Name = "Grayish";
			cnc.ColorBegin = Color.FromArgb(0, 0, 0); cnc.ColorMiddle = Color.FromArgb(112, 112, 112); cnc.ColorEnd = Color.FromArgb(245, 245, 245);
			cmbPredefined.Items.Add(cnc);
			#endregion

			#region Blueish
			cnc = new ColorNameCouple();
			cnc.Name = "Blueish";
			cnc.ColorBegin = Color.FromArgb(12, 35, 148); cnc.ColorMiddle = Color.FromArgb(63, 94, 239); cnc.ColorEnd = Color.FromArgb(245, 245, 245);
			cmbPredefined.Items.Add(cnc);
			#endregion

			#region Greenish
			cnc = new ColorNameCouple();
			cnc.Name = "Greenish";
			cnc.ColorBegin = Color.FromArgb(85, 138, 26); cnc.ColorMiddle = Color.FromArgb(205, 215, 21); cnc.ColorEnd = Color.FromArgb(68, 71, 7);
			cmbPredefined.Items.Add(cnc);
			#endregion

			#region Dark Green
			cnc = new ColorNameCouple();
			cnc.Name = "Dark Green";
			cnc.ColorBegin = Color.FromArgb(28, 116, 12); cnc.ColorMiddle = Color.FromArgb(10, 36, 4); cnc.ColorEnd = Color.FromArgb(28, 116, 12);
			cmbPredefined.Items.Add(cnc);
			#endregion

			#region Fuchia
			cnc = new ColorNameCouple();
			cnc.Name = "Fuchia";
			cnc.ColorBegin = Color.FromArgb(202, 23, 166); cnc.ColorMiddle = Color.FromArgb(0, 0, 0); cnc.ColorEnd = Color.FromArgb(183, 43, 131);
			cmbPredefined.Items.Add(cnc);
			#endregion




		}

		private SuperToolTipInfoWrapper _sttinfo;

		public SuperToolTipInfoWrapper SuperToolTipInfoWrapper
		{
			get { return _sttinfo; }
			set { _sttinfo = value; }
		}

		public dlgSuperToolTipEditor(SuperToolTipInfoWrapper sttiw)
			: this()
		{
			this.superToolTip1.SetSuperStuff(btnPreview, sttiw);
			this._sttinfo = sttiw;
			this.chkShowHeaderSeparator.Checked = this._sttinfo.SuperToolTipInfo.ShowHeaderSeparator;
			this.chkShowFooterSeparator.Checked = this._sttinfo.SuperToolTipInfo.ShowFooterSeparator;

			this.picBodyImage.Image = this._sttinfo.SuperToolTipInfo.BodyImage;
			this.txtBody.Text = this._sttinfo.SuperToolTipInfo.BodyText;
			this.txtBody.ForeColor = this._sttinfo.SuperToolTipInfo.BodyForeColor;
			this.txtBody.Font = this._sttinfo.SuperToolTipInfo.BodyFont;

			this.picFooterImage.Image = this._sttinfo.SuperToolTipInfo.FooterImage;
			this.txtFooter.Text = this._sttinfo.SuperToolTipInfo.FooterText;
			this.txtFooter.ForeColor = this._sttinfo.SuperToolTipInfo.FooterForeColor;
			this.txtFooter.Font = this._sttinfo.SuperToolTipInfo.FooterFont;
			this.chkFooter.Checked = this._sttinfo.SuperToolTipInfo.ShowFooter;

			this.txtHeader.Text = this._sttinfo.SuperToolTipInfo.HeaderText;
			this.txtHeader.ForeColor = this._sttinfo.SuperToolTipInfo.HeaderForeColor;
			this.txtHeader.Font = this._sttinfo.SuperToolTipInfo.HeaderFont;
			this.chkHeader.Checked = this._sttinfo.SuperToolTipInfo.ShowHeader;

			this.btnBegin.BackColor = this._sttinfo.SuperToolTipInfo.BackgroundGradientBegin;
			this.btnMiddle.BackColor = this._sttinfo.SuperToolTipInfo.BackgroundGradientMiddle;
			this.btnEnd.BackColor = this._sttinfo.SuperToolTipInfo.BackgroundGradientEnd;

			this.grpHeader.Enabled = this._sttinfo.SuperToolTipInfo.ShowHeader;
			this.grpFooter.Enabled = this._sttinfo.SuperToolTipInfo.ShowFooter;
		}
		public dlgSuperToolTipEditor()
		{
			InitializeComponent();

			btnHeaderFont.Tag = txtHeader;
			btnBodyFont.Tag = txtBody;
			btnFooterFont.Tag = txtFooter;
		}

		private void GetColor(object sender, EventArgs e)
		{
			dlgColorPicker.Color = ((Button)sender).BackColor;
			if (dlgColorPicker.ShowDialog() == DialogResult.OK)
			{
				((Button)sender).BackColor = dlgColorPicker.Color;
				pnlBackColorPreview.Invalidate();

				switch (((Button)sender).Name)
				{
					case "btnBegin":
						this._sttinfo.SuperToolTipInfo.BackgroundGradientBegin = dlgColorPicker.Color;
						break;
					case "btnMiddle":
						this._sttinfo.SuperToolTipInfo.BackgroundGradientMiddle = dlgColorPicker.Color;
						break;
					case "btnEnd":
						this._sttinfo.SuperToolTipInfo.BackgroundGradientEnd = dlgColorPicker.Color;
						break;
				}

			}
		}

		private void GetFont(object sender, EventArgs e)
		{
			RichTextBox txt = ((Button)sender).Tag as RichTextBox;
			if (dlgFontPicker.ShowDialog() == DialogResult.OK)
			{
				txt.ForeColor = dlgFontPicker.Color;
				txt.Font = dlgFontPicker.Font;
				OnTextSettingsChanged(txt, EventArgs.Empty);
			}
		}

		private void pnlBackColorPreview_Paint(object sender, PaintEventArgs e)
		{
			MakeColorPreview(e.Graphics);
		}

		private void MakeColorPreview(Graphics graphics)
		{
			Rectangle rect1 = new Rectangle(0, pnlBackColorPreview.Height / 2, pnlBackColorPreview.Width, pnlBackColorPreview.Height / 2);
			Rectangle rect2 = new Rectangle(0, 0, pnlBackColorPreview.Width, pnlBackColorPreview.Height / 2);
			using (LinearGradientBrush b2 = new LinearGradientBrush(new Rectangle(0, pnlBackColorPreview.Height / 2 - 1, pnlBackColorPreview.Width, pnlBackColorPreview.Height / 2), _sttinfo.SuperToolTipInfo.BackgroundGradientMiddle, _sttinfo.SuperToolTipInfo.BackgroundGradientEnd, LinearGradientMode.Vertical))
			using (LinearGradientBrush b1 = new LinearGradientBrush(new Rectangle(0, 0, pnlBackColorPreview.Width, pnlBackColorPreview.Height / 2), _sttinfo.SuperToolTipInfo.BackgroundGradientBegin, _sttinfo.SuperToolTipInfo.BackgroundGradientMiddle, LinearGradientMode.Vertical))
			{
				graphics.FillRectangle(b2, rect1);
				graphics.FillRectangle(b1, rect2);
			}
		}

		private void OnBackgroundColorSettingChanged(object sender, EventArgs e)
		{
			grpCustom.Enabled = rdbtnCustom.Checked;
			grpPredefined.Enabled = rdbtnPredefined.Checked;
		}

		private void btnClearBodyImage_Click(object sender, EventArgs e)
		{
			picBodyImage.Image = null;
			_sttinfo.SuperToolTipInfo.BodyImage = null;
		}

		private void btnBrowseBodyImage_Click(object sender, EventArgs e)
		{
			if (dlgImagePicker.ShowDialog() == DialogResult.OK)
			{
				picBodyImage.Image = Bitmap.FromFile(dlgImagePicker.FileName);
				_sttinfo.SuperToolTipInfo.BodyImage = picBodyImage.Image as Bitmap;
			}

		}

		private void OnTextSettingsChanged(object sender, EventArgs e)
		{
			RichTextBox txt = (RichTextBox)sender;
			switch (txt.Name)
			{
				case "txtHeader":
					_sttinfo.SuperToolTipInfo.HeaderFont = txt.Font;
					_sttinfo.SuperToolTipInfo.HeaderForeColor = txt.ForeColor;
					_sttinfo.SuperToolTipInfo.HeaderText = txt.Text;
					break;
				case "txtBody":
					_sttinfo.SuperToolTipInfo.BodyFont = txtBody.Font;
					_sttinfo.SuperToolTipInfo.BodyForeColor = txtBody.ForeColor;
					_sttinfo.SuperToolTipInfo.BodyText = txt.Text;
					break;

				case "txtFooter":
					_sttinfo.SuperToolTipInfo.FooterFont = txt.Font;
					_sttinfo.SuperToolTipInfo.FooterForeColor = txt.ForeColor;
					_sttinfo.SuperToolTipInfo.FooterText = txt.Text;
					break;

			}

		}

		private void cmbPredefined_SelectedIndexChanged(object sender, EventArgs e)
		{
			ColorNameCouple cn = cmbPredefined.SelectedItem as ColorNameCouple;
			_sttinfo.SuperToolTipInfo.BackgroundGradientBegin = cn.ColorBegin;
			_sttinfo.SuperToolTipInfo.BackgroundGradientMiddle = cn.ColorMiddle;
			_sttinfo.SuperToolTipInfo.BackgroundGradientEnd = cn.ColorEnd;

			btnBegin.BackColor = cn.ColorBegin;
			btnMiddle.BackColor = cn.ColorMiddle;
			btnEnd.BackColor = cn.ColorEnd;
			pnlBackColorPreview.Invalidate();
		}

		private void btnBrowseFooterImage_Click(object sender, EventArgs e)
		{
			if (dlgImagePicker.ShowDialog() == DialogResult.OK)
			{
				picFooterImage.Image = Bitmap.FromFile(dlgImagePicker.FileName);
				_sttinfo.SuperToolTipInfo.FooterImage = picFooterImage.Image as Bitmap;
			}
		}

		private void btnClearFooterImage_Click(object sender, EventArgs e)
		{
			picFooterImage.Image = null;
			_sttinfo.SuperToolTipInfo.BodyImage = null;
		}

		private void chkShowFooterSeparator_CheckedChanged(object sender, EventArgs e)
		{
			_sttinfo.SuperToolTipInfo.ShowFooterSeparator = chkShowFooterSeparator.Checked;
		}

		private void chkShowHeaderSeparator_CheckedChanged(object sender, EventArgs e)
		{
			_sttinfo.SuperToolTipInfo.ShowHeaderSeparator = chkShowHeaderSeparator.Checked;
		}

		private void chkFooter_CheckedChanged(object sender, EventArgs e)
		{
			_sttinfo.SuperToolTipInfo.ShowFooter = chkFooter.Checked;
			grpFooter.Enabled = chkFooter.Checked;
		}

		private void chkHeader_CheckedChanged(object sender, EventArgs e)
		{
			_sttinfo.SuperToolTipInfo.ShowHeader = chkHeader.Checked;
			grpHeader.Enabled = chkHeader.Checked;
		}
	}
	public class SuperToolTipEditor : UITypeEditor
	{
		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
		{
			return UITypeEditorEditStyle.Modal;
		}

		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			if (provider == null)
			{
				return null;
			}
			IWindowsFormsEditorService iwefs = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
			SuperToolTipInfoWrapper info = (SuperToolTipInfoWrapper)value;
			dlgSuperToolTipEditor dlg = new dlgSuperToolTipEditor(info);
			if (iwefs.ShowDialog(dlg) == DialogResult.OK)
			{
				return dlg.SuperToolTipInfoWrapper;
			}
			return value;
		}
	}

}
