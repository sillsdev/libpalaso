using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SIL.PlatformUtilities;
using SIL.Windows.Forms.ImageToolbox;
using SIL.Windows.Forms.Miscellaneous;

namespace ClipboardTestApp
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}

		private void OnCopyText(object sender, EventArgs e)
		{
			PortableClipboard.SetText(textBox1.Text);
			UpdateLabels();
		}

		private void OnPasteText(object sender, EventArgs e)
		{
			textBox1.Text = PortableClipboard.GetText();
		}

		private void OnCopyImage(object sender, EventArgs e)
		{
			try
			{
				using var openFileDialog =
					new OpenFileDialogWithViews(OpenFileDialogWithViews.DialogViewTypes.Details);
				if (openFileDialog.ShowDialog(this) != DialogResult.OK)
					return;

				using var image = PalasoImage.FromFile(openFileDialog.FileName);
				PortableClipboard.CopyImageToClipboard(image);
				UpdateLabels();
			}
			catch (NotImplementedException)
			{
				if (Platform.IsWindows)
					throw;
				// ignore on Linux - currently not yet implemented
			}
		}

		private void OnPasteImage(object sender, EventArgs e)
		{
			pictureBox1.Image = PortableClipboard.GetImage();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			UpdateLabels();
		}

		protected override void OnActivated(EventArgs e)
		{
			base.OnActivated(e);
			UpdateLabels();
		}

		private void UpdateLabels()
		{
			labelContainsText.Text = PortableClipboard.ContainsText() ? "true" : "false";
			labelContainsImage.Text = PortableClipboard.ContainsImage() ? "true" : "false";
		}

		private void OnClose(object sender, EventArgs e)
		{
			Close();
		}
	}
}