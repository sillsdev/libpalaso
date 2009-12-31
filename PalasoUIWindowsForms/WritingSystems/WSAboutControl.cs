using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Drawing.Design;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Palaso.UI.WindowsForms.WritingSystems
{
	public partial class WSAboutControl : UserControl
	{
		public class ISOPropertyEditor : UITypeEditor
		{
			public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
			{
				return UITypeEditorEditStyle.Modal;
			}

			public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
			{
				IWindowsFormsEditorService edSvc = (IWindowsFormsEditorService)provider.GetService(typeof (IWindowsFormsEditorService));
				if (edSvc == null)
				{
					return null;
				}

				LookupISOCodeDialog dialog = new LookupISOCodeDialog();
				dialog.ISOCode = (string)value;
				if (edSvc.ShowDialog(dialog) == DialogResult.OK)
				{
					return dialog.ISOCode;
				}

				return value;
			}
		}

		private class WSProxy
		{
			private string _abbreviation;

			public string Abbreviation
			{
				get { return _abbreviation; }
				set { _abbreviation = value; }
			}

			private string _name;

			public string Name
			{
				get { return _name; }
				set { _name = value; }
			}

			private string _iso;

			[EditorAttribute(typeof(ISOPropertyEditor), typeof(UITypeEditor)),
			DescriptionAttribute("Enter the ISO code for this writing system.  Click on the ellipsis to look up the code.")]
			public string ISO
			{
				get { return _iso; }
				set { _iso = value; }
			}

			private string _script;

			[DescriptionAttribute("Enter something here if you need to distinguish between writing systems of the same language in different scripts.  For example, Chinese could be written in simplified or traditional script.")]
			public string Script
			{
				get { return _script; }
				set { _script = value; }
			}

			private string _region;

			[DescriptionAttribute("Enter something here if you need to distinguish between writing systems of the same language in different places.  For example, English is spelled differently in the USA and UK.")]
			public string Region
			{
				get { return _region; }
				set { _region = value; }
			}

			private string _variant;

			public string Variant
			{
				get { return _variant; }
				set { _variant = value; }
			}
		}

		private WritingSystemSetupPM _model;
		private WSProxy _proxy;

		public WSAboutControl()
		{
			InitializeComponent();
			_proxy = new WSProxy();
			_pgAbout.SelectedObject = _proxy;
		}

		public void BindToModel(WritingSystemSetupPM model)
		{
			if (_model != null)
			{
				_model.SelectionChanged -= ModelSelectionChanged;
			}
			_model = model;
			if (_model != null)
			{
				_model.SelectionChanged += ModelSelectionChanged;
			}
			UpdateProxyFromModel();
			UpdateTextFromModel();
			this.Disposed += OnDisposed;
		}

		void OnDisposed(object sender, EventArgs e)
		{
			if (_model != null)
				_model.SelectionChanged -= ModelSelectionChanged;
		}

		private void ModelSelectionChanged(object sender, EventArgs e)
		{
			UpdateProxyFromModel();
			UpdateTextFromModel();
		}

		private void UpdateTextFromModel()
		{
			if (_model.HasCurrentSelection)
			{
				Text = "About " + _model.CurrentLanguageName;
			}
		}

		private void UpdateProxyFromModel()
		{
			_proxy = new WSProxy();
			if (_model.HasCurrentSelection)
			{
				_proxy.Name = _model.CurrentLanguageName;
				_proxy.Abbreviation = _model.CurrentAbbreviation;
				_proxy.ISO = _model.CurrentISO;
				_proxy.Script = _model.CurrentScriptCode;
				_proxy.Region = _model.CurrentRegion;
				_proxy.Variant = _model.CurrentVariant;
				_pgAbout.Enabled = true;
			}
			else
			{
				_pgAbout.Enabled = false;
			}
			_pgAbout.SelectedObject = _proxy;
		}

		private void UpdateModelFromProxy()
		{
			if (_model.HasCurrentSelection)
			{
				_model.CurrentLanguageName = _proxy.Name;
				_model.CurrentAbbreviation = _proxy.Abbreviation;
				_model.CurrentISO = _proxy.ISO;
				_model.CurrentScriptCode = _proxy.Script;
				_model.CurrentRegion = _proxy.Region;
				_model.CurrentVariant = _proxy.Variant;
			}
		}

		private void _pgAbout_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
		{
			UpdateModelFromProxy();
			UpdateTextFromModel();
		}
	}
}
