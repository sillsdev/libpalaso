using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Palaso.UI.Widgets;
using Palaso.UI.WindowsForms.Keyboarding;
using Palaso.WritingSystems;

namespace Palaso.UI.WindowsForms.WritingSystems
{
	[Obsolete]
	internal partial class WSListItem : UserControl, ControlListBox.ISelectableControl
	{
		private readonly WritingSystemDefinition _writingSystemDefinition;
		private bool _isSelected = false;
		private bool _doingLoading=false;
		public event EventHandler Selecting;
		public event EventHandler DeleteRequested;
		public event EventHandler DuplicateRequested;


		public WSListItem(WritingSystemDefinition writingSystemDefinition)
		{
			_writingSystemDefinition = writingSystemDefinition;
			InitializeComponent();
			SetHeight(false);

			//I've had these get lost from the designer, so I set them here now.
			superToolTip1.GetSuperStuff(_variant).SuperToolTipInfo.BodyText = "Variant subtags are values used to indicate dialects or script variations not already covered by combinations of language, script and region subtag.";
			superToolTip1.GetSuperStuff(_iso).SuperToolTipInfo.BodyText = "A code defined by iso 639-3 (Ethnologue code)";

			superToolTip1.GetSuperStuff(_regionBox).SuperToolTipInfo.HeaderText = "Region";
			superToolTip1.GetSuperStuff(_regionBox).SuperToolTipInfo.BodyText = "Enter something here if you need to distinguish between writing systems of the same language in different places.  For example, English is spelled differently in the USA and UK.";
		}

		public bool PushToWritingSystemDefinition()
		{
			bool wasChanged = false;
			Definition.LanguageName = SaveToProperty(_language, Definition.LanguageName, ref wasChanged);
			Definition.ISO = SaveToProperty(_iso, Definition.ISO, ref wasChanged);
			Definition.Region = SaveToProperty(_regionBox, Definition.Region, ref wasChanged);
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
				_fontAndKeboardLink.Focus();
				_language.Focus();
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


		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
//            if (_markedForDeletion)
//            {
//                e.Graphics.DrawLine(Pens.Red, ClientRectangle.Left + 15, 15, ClientRectangle.Right - 15, 15);
//            }
			if (this.Selected)
			{
				Rectangle baseRect = base.ClientRectangle;
				baseRect.Inflate(-2, -2);
				float radius = 14;

				System.Drawing.Drawing2D.GraphicsPath gp = new System.Drawing.Drawing2D.GraphicsPath();
				gp.StartFigure();
				gp.AddArc(baseRect.X, baseRect.Y, radius, radius, 180, 90);
				gp.AddArc(baseRect.X + baseRect.Width - radius, baseRect.Y, radius, radius, 270, 90);
				gp.AddArc(baseRect.X + baseRect.Width - radius, baseRect.Y + baseRect.Height - radius, radius, radius, 0,
						  90);
				gp.AddArc(baseRect.X, baseRect.Y + baseRect.Height - radius, radius, radius, 90, 90);
				gp.CloseFigure();

				e.Graphics.DrawPath(new Pen(Color.RoyalBlue), gp);
				gp.Dispose();
			}
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
			_doingLoading = true;
			LoadScriptBox();

			_language.Text = Definition.LanguageName;
			_iso.Text = Definition.ISO;
			_variant.Text = Definition.Variant;
			_abbreviation.Text = Definition.Abbreviation;
			_regionBox.Text = Definition.Region;

			SelectCorrectScriptComboItem();
			UpdateDisplay();
			_doingLoading = false;
		}

		private void SelectCorrectScriptComboItem()
		{
			_scriptBox.SelectedItem = Definition.CurrentScriptOption;
			if (_scriptBox.SelectedItem == null)
			{
				_scriptBox.Text = Definition.Script; //not an expected script
			}
		}

		private void UpdateDisplay()
		{
			//SetEnabledStates(Definition.MarkedForDeletion);
			//_labelSummary.Visible = !Definition.MarkedForDeletion;

			_writingSystemLabel.Text = Definition.DisplayLabel;
			_labelSummary.Text = Definition.VerboseDescription;
		}

//        private void SetEnabledStates(bool markedForDeletion)
//        {
//            foreach (Control control in this.Controls)
//            {
//                control.Enabled = !markedForDeletion;
//            }
//        }

		private string CurrentScriptCode
		{
			get
			{
				WritingSystemDefinition.ScriptOption script = _scriptBox.SelectedItem as WritingSystemDefinition.ScriptOption;
				if (script == null)
				{
					return string.Empty;
				}
				return script.Code;
			}
		}




		private void LoadScriptBox()
		{
			_scriptBox.Items.Clear();
			foreach (WritingSystemDefinition.ScriptOption option in Definition.ScriptOptions)
			{
				_scriptBox.Items.Add(option);
				if (option.Code == "latn")
				{
					_scriptBox.SelectedIndex = _scriptBox.Items.Count - 1;
				}
			}
		}

		private void OnSomethingChanged(object sender, EventArgs e)
		{
			if(_doingLoading)
				return;
			PushToWritingSystemDefinition();
			UpdateDisplay();
		}

		private void OnFontAndKeyboardLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			FontDialog dialog = new FontDialog();
			dialog.FontFamily = _writingSystemDefinition.DefaultFontName;
			dialog.Keyboard = _writingSystemDefinition.Keyboard;
			dialog.RightToLeftScript = _writingSystemDefinition.RightToLeftScript;

			//if we know one native word, that'd make a better sample text
			if (!String.IsNullOrEmpty(_writingSystemDefinition.NativeName))
			{
				dialog.SampleText = _writingSystemDefinition.NativeName;
			}
			if (dialog.ShowDialog() == DialogResult.OK)
			{
				_writingSystemDefinition.DefaultFontName = dialog.FontFamily;
				_writingSystemDefinition.Keyboard = dialog.Keyboard;
				_writingSystemDefinition.RightToLeftScript = dialog.RightToLeftScript;
			}
		}

		private void OnDeleteButton(object sender, EventArgs e)
		{
			if (DeleteRequested != null)
			{
				DeleteRequested.Invoke(this, null);
			}

//            Definition.MarkedForDeletion = true;
//           // Invalidate();
//            UpdateDisplay();
		}


//        private void OnUndoDeletion(object sender, LinkLabelLinkClickedEventArgs e)
//        {
//            Definition.MarkedForDeletion = false;
//            //Invalidate();
//            UpdateDisplay();
//
//        }

		private void _duplicateButton_Click(object sender, EventArgs e)
		{
			if(DuplicateRequested!=null)
			{
				DuplicateRequested.Invoke(this, null);
			}
		}

		private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{

		}

		private void OnScriptChanged(object sender, EventArgs e)
		{
			if (string.IsNullOrEmpty(_writingSystemDefinition.Keyboard))
			{
				TryToGuessCorrectKeyboard();
			}
			OnSomethingChanged(sender,e);
		}

		private void TryToGuessCorrectKeyboard()
		{
			if (_writingSystemDefinition.Script == "Zipa")
			{
				_writingSystemDefinition.Keyboard = SearchForKeyboard("IPA");
			}
			else
			{
				if (!String.IsNullOrEmpty(_writingSystemDefinition.Script))
				{
					 _writingSystemDefinition.Keyboard = SearchForKeyboard(_writingSystemDefinition.Script);
				}
			}
		}

		private string SearchForKeyboard(string s)
		{
			List<KeyboardController.KeyboardDescriptor> keyboards = KeyboardController.GetAvailableKeyboards(KeyboardController.Engines.All);
			foreach (KeyboardController.KeyboardDescriptor keyboard in keyboards)
			{
				if(keyboard.Name.Contains(s))
				{
					return keyboard.Name;
				}
			}
			return string.Empty;
		}

		private void _findISOCodeLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			LookupISOCodeDialog dialog = new LookupISOCodeDialog();
			dialog.ISOCode = _iso.Text;
			Cursor.Current = Cursors.WaitCursor;
			if (dialog.ShowDialog() == DialogResult.OK)
			{
				if (!String.IsNullOrEmpty(dialog.ISOCode))
				{
					_iso.Text = dialog.ISOCode;
				}
			}
		}

	}
}