using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Palaso.UI;
using Palaso.WritingSystems;

namespace Palaso
{
	public partial class WSListItem : UserControl, ControlListBox.ISelectableControl
	{
		private readonly WritingSystemDefinition _writingSystemDefinition;
		private bool _isSelected = false;
		public event EventHandler Selecting;


		public WSListItem(WritingSystemDefinition writingSystemDefinition)
		{
			_writingSystemDefinition = writingSystemDefinition;
			InitializeComponent();
			SetHeight(false);
		}

		public bool SaveToWritingSystemDefinition()
		{
			bool wasChanged = false;
			Definition.LanguageName = SaveToProperty(_language, Definition.LanguageName, ref wasChanged);
			Definition.ISO = SaveToProperty(_iso, Definition.ISO, ref wasChanged);
			Definition.Region = SaveToProperty(_countryBox, Definition.Region, ref wasChanged);
			Definition.Variant = SaveToProperty(_variant, Definition.Variant, ref wasChanged);
			Definition.Script = SaveToProperty(CurrentScriptCode, Definition.Script, ref wasChanged);
			Definition.Abbreviation = SaveToProperty(_abbreviation, Definition.Abbreviation, ref wasChanged);
			return wasChanged;
		}

		private string SaveToProperty(string value, string property, ref bool wasChanged)
		{
			value = value.Trim();
			if (property != value)
			{
				wasChanged = true;
			}
			return value;
		}

		private string SaveToProperty(TextBox box, string property, ref bool wasChanged)
		{
			box.Text = box.Text.Trim();
			if (property != box.Text)
			{
				wasChanged = true;
			}
			return box.Text;
		}

		private string SaveToProperty(ComboBox box, string property, ref bool wasChanged)
		{
			box.Text = box.Text.Trim();
			if (property != box.Text)
			{
				wasChanged = true;
			}
			return box.Text;
		}

		public bool Selected
		{
			get
			{
				return _isSelected;
			}
			set
			{
				if (_isSelected == value)
					return;
			   SetHeight(value);
				if (value && Selecting!=null)
				{
					Selecting.Invoke(this, null);
				}
				_isSelected = value;
				 this.Invalidate();
			}
		}

		public WritingSystemDefinition Definition
		{
			get
			{
				return _writingSystemDefinition;
			}
		}

		private void SetHeight(bool selected)
		{
			if (selected)
			{
				Height = 120;
			}
			else
			{
				Height = 30;
			}
		}


		private void WSListItem_Paint(object sender, PaintEventArgs e)
		{
			if (!this.Selected)
			{
				return;
			}
			Rectangle baseRect = base.ClientRectangle;
			baseRect.Inflate(-2, -2);
			float radius = 14;

			System.Drawing.Drawing2D.GraphicsPath gp = new System.Drawing.Drawing2D.GraphicsPath();
			gp.StartFigure();
			gp.AddArc(baseRect.X, baseRect.Y, radius, radius, 180, 90);
			gp.AddArc(baseRect.X + baseRect.Width - radius, baseRect.Y, radius, radius, 270, 90);
			gp.AddArc(baseRect.X + baseRect.Width - radius, baseRect.Y + baseRect.Height - radius, radius, radius, 0, 90);
			gp.AddArc(baseRect.X, baseRect.Y + baseRect.Height - radius, radius, radius, 90, 90);
			gp.CloseFigure();

			e.Graphics.DrawPath(new Pen(Color.RoyalBlue), gp);
			gp.Dispose();
		}

		private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{

		}

		private void WSListItem_Click(object sender, EventArgs e)
		{
			Selected = true;
		}


		private void OnLoad(object sender, EventArgs e)
		{
			LoadScriptBox();

			_language.Text = Definition.LanguageName;
			_iso.Text = Definition.ISO;
			_variant.Text = Definition.Variant;
			_abbreviation.Text = Definition.Abbreviation;
			_countryBox.Text = Definition.Region;

			SelectCorrectScriptComboItem();
			UpdateDisplay();
		}

		private void SelectCorrectScriptComboItem()
		{
			string script = Definition.Script;
			if (String.IsNullOrEmpty(script))
			{
				script = "latn";
			}
			foreach (ScriptOption option in _scriptBox.Items)
			{
				if(option.Code == script)
				{
					_scriptBox.SelectedItem = option;
					break;
				}
			}
		}

		private void UpdateDisplay()
		{

			_abbreviationLabel.Text = GetBestLabel();

			StringBuilder identifier = new StringBuilder();
			identifier.Append(_iso.Text);
			if (!String.IsNullOrEmpty(CurrentScriptCode))
			{
				identifier.AppendFormat("-{0}", CurrentScriptCode);
			}
			if (!String.IsNullOrEmpty(_countryBox.Text))
			{
				identifier.AppendFormat("-{0}", _countryBox.Text);
			}
			if (!String.IsNullOrEmpty(_variant.Text))
			{
				identifier.AppendFormat("-{0}", _variant.Text);
			}

			StringBuilder summary = new StringBuilder();
			summary.AppendFormat("The");
			if (!String.IsNullOrEmpty(_variant.Text))
			{
				summary.AppendFormat(" {0} variant of ", _variant.Text);
			}

			summary.AppendFormat(" {0}", _language.Text);
			if (!String.IsNullOrEmpty(_countryBox.Text))
			{
				summary.AppendFormat(" in {0}", _countryBox.Text);
			}
			if (!String.IsNullOrEmpty(_scriptBox.Text))
			{
				summary.AppendFormat(" written in {0} script", _scriptBox.Text);
			}

			summary.AppendFormat(". ({0})",identifier.ToString());
			_labelSummary.Text = summary.ToString();
		}

		private string CurrentScriptCode
		{
			get
			{
				ScriptOption script = _scriptBox.SelectedItem as ScriptOption;
				if (script == null)
				{
					return string.Empty;
				}
				return script.Code;
			}
		}

		private string GetBestLabel()
		{
			if (!String.IsNullOrEmpty(_abbreviation.Text))
			{
			   return _abbreviation.Text;
			}
			else
			{
				if (!String.IsNullOrEmpty(_iso.Text))
				{
					return _iso.Text;
				}
				else
				{
					if (!String.IsNullOrEmpty(_language.Text))
					{
						string n = _language.Text;
						return n.Substring(0, n.Length>4? 4: n.Length);
					}
				}
			}
			return "???";
		}

		private void LoadScriptBox()
		{
			_scriptBox.Items.Clear();

			_scriptBox.Items.Add(new ScriptOption("Thai", "Thai"));
			_scriptBox.Items.Add(new ScriptOption("Khmer", "khmr"));
			_scriptBox.Items.Add(new ScriptOption("Korean", "Kore"));
			_scriptBox.Items.Add(new ScriptOption("Lao", "Laoo"));
			_scriptBox.Items.Add(new ScriptOption("Latin", "Latn"));
			_scriptBox.SelectedIndex = _scriptBox.Items.Count - 1;
			_scriptBox.Items.Add(new ScriptOption("Lanna", "Lana"));
			_scriptBox.Items.Add(new ScriptOption("Myanmar (Burmese)", "Mymr"));
		}

		class ScriptOption
		{
			private string _label;
			private string _code;

			public ScriptOption(string label, string code)
			{
				_label = label;
				_code = code;
			}

			public string Code
			{
				get
				{
					return _code;
				}
			}

			public string Label
			{
				get
				{
					return _label;
				}
			}
			public override string ToString()
			{
				return _label;
			}
		}

		private void OnSomethingChanged(object sender, EventArgs e)
		{
			UpdateDisplay();
		}

		private void OnFontLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{

		}

		private void _iso_TextChanged(object sender, EventArgs e)
		{

			UpdateDisplay();
		}
	}
}
