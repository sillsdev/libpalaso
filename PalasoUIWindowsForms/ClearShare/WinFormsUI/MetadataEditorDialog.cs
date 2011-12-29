using System;
using System.Reflection;
using System.Windows.Forms;

namespace Palaso.UI.WindowsForms.ClearShare.WinFormsUI
{
	public partial class MetadataEditorDialog : Form
	{
		private readonly Metadata _originalMetaData;
		private Metadata _returnMetaData;

		public MetadataEditorDialog(Metadata originalMetaData)
		{
			_originalMetaData = originalMetaData;
			InitializeComponent();
			_metdataEditorControl.Metadata = _returnMetaData = originalMetaData.DeepCopy();
			ShowCreator = true;
		}

		/// <summary>
		/// Set this to false if you don't want to collect info on who created it (e.g. you're just getting copyright/license)
		/// </summary>
		public bool ShowCreator {
			get { return _metdataEditorControl.ShowCreator; }
			set { _metdataEditorControl.ShowCreator = value; }
		}

		public Metadata Metadata
		{
			get { return _returnMetaData; }
		}

		private void _cancelButton_Click(object sender, EventArgs e)
		{
			_returnMetaData = _originalMetaData;
			DialogResult = DialogResult.Cancel;
			Close();
		}

		private void _okButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
			Close();
		}


		private void _minimallyCompleteCheckTimer_Tick(object sender, EventArgs e)
		{
			_okButton.Enabled = _metdataEditorControl.Metadata.IsMinimallyComplete;
		}
	}
}
