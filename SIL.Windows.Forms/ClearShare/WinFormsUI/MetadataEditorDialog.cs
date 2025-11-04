using System;
using System.Windows.Forms;
using L10NSharp;
using SIL.Core.ClearShare;

namespace SIL.Windows.Forms.ClearShare.WinFormsUI
{
	public partial class MetadataEditorDialog : SIL.Windows.Forms.Miscellaneous.FormForUsingPortableClipboard
	{
		private readonly Metadata _originalMetaData;
		private Metadata _returnMetaData;

		public MetadataEditorDialog(Metadata originalMetaData)
		{
			_originalMetaData = originalMetaData;
			InitializeComponent();
			_metadataEditorControl.Metadata = _returnMetaData = originalMetaData.DeepCopy();
			ShowCreator = true;
		}

		/// <summary>
		/// Set this to false if you don't want to collect info on who created it (e.g. you're just getting copyright/license)
		/// </summary>
		public bool ShowCreator {
			get { return _metadataEditorControl.ShowCreator; }
			set
			{
				_metadataEditorControl.ShowCreator = value;
				Text = value ? LocalizationManager.GetString("MetadataEditor.TitleWithCredit", "Credit, Copyright, and License") :  LocalizationManager.GetString("MetadataEditor.TitleNoCredit", "Copyright and License");
			}
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

			//we can't have a custom license without some description of it
			var customLicense = _returnMetaData.License as CustomLicense;
			if(customLicense!=null && string.IsNullOrEmpty(customLicense.RightsStatement))
				_returnMetaData.License = new NullLicense();

			Close();
		}

		// allow subclass to access these methods/properties.

		[CLSCompliant (false)]
		protected virtual void _minimallyCompleteCheckTimer_Tick(object sender, EventArgs e)
		{
			_okButton.Enabled = _metadataEditorControl.Metadata.IsMinimallyComplete;
		}

		protected MetadataEditorControl MetadataControl
		{
			get { return _metadataEditorControl; }
		}

		protected Button OkButton
		{
			get { return _okButton; }
		}
	}
}
