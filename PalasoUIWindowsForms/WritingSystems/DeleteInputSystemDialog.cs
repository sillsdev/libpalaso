using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Palaso.WritingSystems;

namespace Palaso.UI.WindowsForms.WritingSystems
{
	public partial class DeleteInputSystemDialog : Form
	{
		public enum Choices
		{
			Cancel,
			Merge,
			Delete
		}

		public event EventHandler HelpWithDeletingWritingSystemsButtonClickedEvent;

		public DeleteInputSystemDialog(WritingSystemDefinition wsToDelete,
									   IEnumerable<WritingSystemDefinition> possibleWritingSystemsToConflateWith, bool showHelpButton)
		{
			InitializeComponent();
			if (!showHelpButton)
			{
				_helpButton.Hide();
			}
			_deleteRadioButton.Text = String.Format(_deleteRadioButton.Text, DisplayName(wsToDelete));
			_mergeRadioButton.Text = String.Format(_mergeRadioButton.Text, DisplayName(wsToDelete));
			_wsSelectionComboBox.Items.AddRange(
				possibleWritingSystemsToConflateWith.Where(ws => ws != wsToDelete).Select(ws=>new WritingSystemDisplayAdaptor(ws)).ToArray());
			Choice = Choices.Delete;
			if (_wsSelectionComboBox.Items.Count > 0)
			{
				_wsSelectionComboBox.SelectedIndex = 0;
			}
			_wsSelectionComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
			_okButton.Click += OnOkClicked;
			_cancelButton.Click += OnCancelClicked;
			_deleteRadioButton.CheckedChanged += OnDeleteRadioButtonCheckedChanged;
			_mergeRadioButton.CheckedChanged += OnMergeRadioButtonCheckedChanged;
			_helpButton.Click += OnCustomHelpButtonClicked;
			_deleteRadioButton.Checked = true;
		}

		private static string DisplayName(WritingSystemDefinition ws)
		{
			return String.Format("\"{0}\" ({1})", ws.ListLabel, ws.Id);
		}

		private void OnCustomHelpButtonClicked(object sender, EventArgs e)
		{
			if (HelpWithDeletingWritingSystemsButtonClickedEvent != null)
			{
				HelpWithDeletingWritingSystemsButtonClickedEvent(this, e);
			}
		}

		private void OnMergeRadioButtonCheckedChanged(object sender, EventArgs e)
		{
			if(_mergeRadioButton.Checked)
			{
				Choice = Choices.Merge;
				_wsSelectionComboBox.Enabled = true;
				_okButton.Text = "&Merge";
			}
		}

		private void OnDeleteRadioButtonCheckedChanged(object sender, EventArgs e)
		{
			if(_deleteRadioButton.Checked)
			{
				Choice = Choices.Delete;
				_wsSelectionComboBox.Enabled = false;
				_okButton.Text = "&Delete";
			}
		}

		public Choices Choice { get; private set; }


		private void OnCancelClicked(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Choice = Choices.Cancel;
			Close();
		}

		private void OnOkClicked(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
			Close();
		}

		public WritingSystemDefinition WritingSystemToConflateWith
		{
			get { return ((WritingSystemDisplayAdaptor) _wsSelectionComboBox.SelectedItem).AdaptedWs; }
		}

		private class WritingSystemDisplayAdaptor
		{
			private WritingSystemDefinition _wsToAdapt;

			public WritingSystemDisplayAdaptor(WritingSystemDefinition wsToAdapt)
			{
				_wsToAdapt = wsToAdapt;
			}

			public override string ToString()
			{
				return DisplayName(_wsToAdapt);
			}

			public WritingSystemDefinition AdaptedWs
			{
				get { return _wsToAdapt; }
			}
		}
	}
}
