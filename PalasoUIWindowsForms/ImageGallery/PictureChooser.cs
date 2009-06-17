using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using Palaso.UI.WindowsForms.ImageGallery;
using System.Linq;

namespace WeSay.LexicalTools.AddPictures
{
	public partial class PictureChooser : Form
	{
		private readonly IImageCollection _images;

		public PictureChooser(IImageCollection images, string searchWords)
		{
			_images = images;
			InitializeComponent();
			_searchTermsBox.Text = searchWords;
		}

		private void _searchButton_Click(object sender, EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;
			_searchButton.Enabled = false;
			_okButton.Enabled = false;
			try
			{
				_thumbnailViewer.Clear();
				if (!string.IsNullOrEmpty(_searchTermsBox.Text))
				{
					IEnumerable<object> results = _images.GetMatchingPictures(_searchTermsBox.Text);
					if (results.Count() == 0)
					{
						_notFoundLabel.Visible = true;
					}
					else
					{
						_notFoundLabel.Visible = false;
						_thumbnailViewer.LoadItems(_images.GetPathsFromResults(results, true));
					}
				}

			}
			catch (Exception error)
			{

			}
			_searchButton.Enabled = true;
			_okButton.Enabled = false;
			Cursor.Current = Cursors.Default;
		}

		private void PictureChooser_Load(object sender, EventArgs e)
		{
			_thumbnailViewer.CaptionMethod = _images.CaptionMethod;

			if (_searchTermsBox.Text.Length > 0)
				_searchButton_Click(this, null);
		}
		public string ChosenPath { get; set; }
		private void _thumbnailViewer_DoubleClick(object sender, EventArgs e)
		{
			ChosenPath = _thumbnailViewer.SelectedPath;
			if (string.IsNullOrEmpty(ChosenPath))
				return;
			this.DialogResult = DialogResult.OK;
			Close();
		}

		private void _okButton_Click(object sender, EventArgs e)
		{
		   ChosenPath = _thumbnailViewer.SelectedPath;
			if (string.IsNullOrEmpty(ChosenPath))
			{
				this.DialogResult = DialogResult.Cancel;
			}
			else
			{
				this.DialogResult = DialogResult.OK;
			}
			Close();
		}

		private void _thumbnailViewer_SelectedIndexChanged(object sender, EventArgs e)
		{
			_okButton.Enabled = (_thumbnailViewer.SelectedItems.Count > 0);
		}

		private void PictureChooser_FormClosing(object sender, FormClosingEventArgs e)
		{
			_thumbnailViewer.Closing();
		}

	}

}
