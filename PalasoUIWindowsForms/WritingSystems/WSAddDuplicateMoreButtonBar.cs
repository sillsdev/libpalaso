using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace Palaso.UI.WindowsForms.WritingSystems
{
	public partial class WSAddDuplicateMoreButtonBar : UserControl
	{
		WritingSystemSetupPM _model;

		public WSAddDuplicateMoreButtonBar()
		{
			InitializeComponent();
			CreateMoreButtonImage();
		}

		public void BindToModel(WritingSystemSetupPM model)
		{
			Debug.Assert(model != null);
			if (_model != null)
			{
				_model.SelectionChanged -= ModelSelectionChanged;
			}
			_model = model;
			SetButtonStatus();
			_model.SelectionChanged += ModelSelectionChanged;

			Disposed += OnDisposed;
		}

		void OnDisposed(object sender, EventArgs e)
		{
			if (_model != null)
				_model.SelectionChanged -= ModelSelectionChanged;
		}

		private void CreateMoreButtonImage()
		{
			Bitmap downArrow = new Bitmap(11, 6);
			Graphics g = Graphics.FromImage(downArrow);
			g.FillRectangle(Brushes.Transparent, 0, 0, 11, 5);
			g.FillPolygon(Brushes.Black, new[] { new Point(0, 0), new Point(10, 0), new Point(5, 5) });
			_moreButton.Image = downArrow;
		}

		private void ModelSelectionChanged(object sender, EventArgs e)
		{
			SetButtonStatus();
		}

		private void SetButtonStatus()
		{
			if (_model == null)
				return;
			bool enabled = _model.HasCurrentSelection;
			_exportMenuItem.Enabled = enabled;
			_duplicateButton.Enabled = enabled;
			_deleteMenuItem.Enabled = enabled;
		}

		private void MoreButtonClick(object sender, EventArgs e)
		{
			// Popup context menu
			_contextMenu.Show(_moreButton, 0, 0);
		}

		private void AddButtonClick(object sender, EventArgs e)
		{
			_model.AddNew();
		}

		private void DuplicateButtonClick(object sender, EventArgs e)
		{
			_model.DuplicateCurrent();
		}

		private void DeleteMenuClick(object sender, EventArgs e)
		{
			_model.DeleteCurrent();
		}

		private void ImportMenuClick(object sender, EventArgs e)
		{
			OpenFileDialog dialog = new OpenFileDialog();
			dialog.Filter = "LDML files (*.ldml;*.xml)|*.ldml;*.xml|All files (*.*)|*.*";
			dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			dialog.RestoreDirectory = true;
			if (dialog.ShowDialog(this) == DialogResult.OK)
			{
				_model.ImportFile(dialog.FileName);
			}
		}

		private void ExportMenuClick(object sender, EventArgs e)
		{
			SaveFileDialog dialog = new SaveFileDialog ();
			dialog.Filter = "LDML files (*.ldml;*.xml)|*.ldml;*.xml|All files (*.*)|*.*";
			dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			dialog.RestoreDirectory = true;
			if (dialog.ShowDialog (this) == DialogResult.OK)
			{
				_model.ExportCurrentWritingSystemAsFile(dialog.FileName);
			}
		}

	}
}
