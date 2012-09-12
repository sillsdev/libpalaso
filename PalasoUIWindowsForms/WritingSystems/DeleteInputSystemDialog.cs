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

		public DeleteInputSystemDialog(string wsToDelete,
									   IEnumerable<WritingSystemDefinition> possibleWritingSystemsToConflateWith)
		{
			InitializeComponent();

			_deleteRadioButton.Text = String.Format(_deleteRadioButton.Text, wsToDelete);
			_mergeRadioButton.Text = String.Format(_mergeRadioButton.Text, wsToDelete);
			_wsSelectionComboBox.Items.AddRange(
				possibleWritingSystemsToConflateWith.Where(ws => ws.Id != wsToDelete).Select(ws => ws.Id).ToArray());
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
			_deleteRadioButton.Checked = true;
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

		public string WritingSystemToConflateWith
		{
			get { return (string) _wsSelectionComboBox.SelectedItem; }
		}

	}
}
