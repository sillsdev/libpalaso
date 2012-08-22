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
	public partial class ConflateWritingSystemsDialog : Form
	{
		public ConflateWritingSystemsDialog(string wsToConflate, IEnumerable<WritingSystemDefinition> possibleWritingSystemsToConflateWith)
		{
			InitializeComponent();
			_wsLabel.Text = String.Format(_wsLabel.Text, wsToConflate);
			_infoTextLabel.Text = String.Format(_infoTextLabel.Text, wsToConflate);
			_wsSelectionComboBox.Items.AddRange(possibleWritingSystemsToConflateWith.Where(ws => ws.Id != wsToConflate).Select(ws => ws.Id).ToArray());
			_okButton.Click += OnOkClicked;
			_cancelButton.Click += OnCancelClicked;
		}

		private void OnCancelClicked(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
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
