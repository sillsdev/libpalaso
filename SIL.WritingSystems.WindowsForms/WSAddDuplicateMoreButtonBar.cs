using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using SIL.Reporting;

namespace SIL.WritingSystems.WindowsForms
{
	public partial class WSAddDuplicateMoreButtonBar : UserControl
	{
		WritingSystemSetupModel _model;

		public WSAddDuplicateMoreButtonBar()
		{
			InitializeComponent();
			CreateMoreButtonImage();
		}

		public void BindToModel(WritingSystemSetupModel model)
		{
			Debug.Assert(model != null);
			if (_model != null)
			{
				_model.SelectionChanged -= ModelSelectionChanged;
				_model.CurrentItemUpdated -= OnCurrentItemUpdated;
			}
			_model = model;
			SetButtonStatus();
			_model.SelectionChanged += ModelSelectionChanged;
			_model.CurrentItemUpdated += OnCurrentItemUpdated;

			Disposed += OnDisposed;
		}

		private void OnCurrentItemUpdated(object sender, EventArgs e)
		{
			SetButtonStatus();
		}

		void OnDisposed(object sender, EventArgs e)
		{
			if (_model != null)
			{
				_model.SelectionChanged -= ModelSelectionChanged;
				_model.CurrentItemUpdated -= OnCurrentItemUpdated;
			}
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
			_duplicateMenuItem.Enabled = enabled;
			if(enabled)
			{
				var label = _model.CurrentDefinition == null ? "" : _model.CurrentDefinition.ListLabel;
				_duplicateMenuItem.Text = string.Format("Add New Language by Copying {0}", label);
				_deleteMenuItem.Text = string.Format("Delete {0}...", label);
				_exportMenuItem.Text = string.Format("Save a Copy of the {0} LDML file...", label);
			}
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
				// There's no reason for the program to crash if the import fails.
				// See https://jira.sil.org/browse/WS-112 for details.
				try
				{
					_model.ImportFile(dialog.FileName);
				}
				catch (ApplicationException ex)
				{
					ErrorReport.NotifyUserOfProblem(ex, "There was a problem reading the LDML (language/orthography definition) file.");
				}
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
