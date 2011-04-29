using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Palaso.UI.WindowsForms.WritingSystems.WSTree;
using Palaso.WritingSystems;

namespace Palaso.UI.WindowsForms.WritingSystems
{
	public partial class WritingSystemSetupView : UserControl
	{
		private WritingSystemSetupModel _model;

		public WritingSystemSetupView()
		{
			InitializeComponent();
		}
		public WritingSystemSetupView(WritingSystemSetupModel model)
			: this()
		{
			BindToModel(model);
		}
		public void BindToModel(WritingSystemSetupModel model)
		{
			_model = model;
			_model.MethodToShowUiToBootstrapNewDefinition= ShowCreateNewWritingSystemDialog;

			_buttonBar.BindToModel(_model);
			_propertiesTabControl.BindToModel(_model);

			var treeModel = new WritingSystemTreeModel(_model);
			treeModel.Suggestor = model.WritingSystemSuggestor;
			_treeView.BindToModel(treeModel);
			_model.SelectionChanged += UpdateHeaders;
			_model.CurrentItemUpdated += UpdateHeaders;
			UpdateHeaders(null, null);
		}

		/// <summary>
		/// Use this to set the appropriate kinds of writing systems according to your
		/// application.  For example, is the user of your app likely to want voice? ipa? dialects?
		/// </summary>
		public WritingSystemSuggestor WritingSystemSuggestor
		{
			get { return _model.WritingSystemSuggestor; }
		}

		public int LeftColumnWidth
		{
			get { return splitContainer2.SplitterDistance; }
			set { splitContainer2.SplitterDistance = value; }
		}

		private void UpdateHeaders(object sender, EventArgs e)
		{
			if(_model.CurrentDefinition ==null)
			{
				_rfc4646.Text = "";
				_languageName.Text = "";
			}
			else
			{
				_rfc4646.Text = _model.CurrentDefinition.RFC5646;
				_languageName.Text = _model.CurrentDefinition.ListLabel;
			}
		}

		private static WritingSystemDefinition ShowCreateNewWritingSystemDialog()
		{
			var dlg= new LookupISOCodeDialog();
			dlg.ShowDialog();
			if(dlg.DialogResult!=DialogResult.OK)
				return null;
			return new WritingSystemDefinition(dlg.ISOCode, string.Empty,string.Empty,string.Empty, dlg.ISOCode,false);
		}

		private void _propertiesTabControl_Load(object sender, EventArgs e)
		{

		}
	}
}
