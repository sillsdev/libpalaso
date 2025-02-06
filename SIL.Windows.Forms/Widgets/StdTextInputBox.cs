// Copyright (c) 2025 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using SIL.Keyboarding;
using SIL.PlatformUtilities;
using SIL.Text;
using SIL.Windows.Forms.Spelling;
using SIL.WritingSystems;

namespace SIL.Windows.Forms.Widgets
{
	public partial class StdTextInputBox: TextBox, IControlThatKnowsWritingSystem, ITextInputBox
	{
		private WritingSystemDefinition _writingSystem;
		private string _previousText;

		private bool _multiParagraph;
		private string _nameForLogging;
		private bool _isSpellCheckingEnabled;

		/// <summary>
		/// Don't use this directly, use the Singleton Property TextBoxSpellChecker
		/// </summary>
		private static TextBoxSpellChecker _textBoxSpellChecker;
		public event EventHandler UserLostFocus;
		public event EventHandler UserGotFocus;

		public StdTextInputBox()
		{
			InitializeComponent();
			if (DesignMode)
			{
				return;
			}
			DoubleBuffered = true;
			GotFocus += OnGotFocus;
			LostFocus += OnLostFocus;
			KeyPress += HandleKeyPress;
			TextChanged += OnTextChanged;

			KeyDown += OnKeyDown;

			if (_nameForLogging == null)
			{
				_nameForLogging = "??";
			}
			Name = _nameForLogging;
		}

		public Control TheControl { get { return this; } }

		public void Init(WritingSystemDefinition writingSystem, String name)
		{
			WritingSystem = writingSystem;
			_nameForLogging = name;
			if (_nameForLogging == null)
			{
				_nameForLogging = "??";
			}
			Name = name;
		}


		protected override void OnKeyDown(KeyEventArgs e)
		{
			if (Parent is TextInputBox)
				(Parent as TextInputBox).OnChildKeyDown(e);
			else
				base.OnKeyDown(e);
		}

		private void OnKeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.PageDown)
			{
				e.Handled = false;
			}
		}

		private void OnTextChanged(object sender, EventArgs e)
		{
			LanguageForm.AdjustSpansForTextChange(_previousText, Text, Spans);
			_previousText = Text;
		}

		private void OnLostFocus(object sender, EventArgs e)
		{
			if (UserLostFocus != null)
			{
				UserLostFocus.Invoke(sender, e);
			}
		}

		private void OnGotFocus(object sender, EventArgs e)
		{
			if (UserGotFocus != null)
			{
				UserGotFocus.Invoke(sender, e);
			}
		}

		protected override void OnTextChanged(EventArgs e)
		{
			if (IsDisposed) // are we a zombie still getting events?
			{
				throw new ObjectDisposedException(GetType().ToString());
			}
			base.OnTextChanged(e);
			Height = GetPreferredHeight(Width);

			if (!Platform.IsWindows)
			{
				// For some fonts that don't render properly in MONO
				Refresh();
			}
		}

		// we do this in OnLayout instead of OnResize see
		// "Setting the Size/Location of child controls in the Resize event
		// http://blogs.msdn.com/jfoscoding/archive/2005/03/04/385625.aspx
		protected override void OnLayout(LayoutEventArgs levent)
		{
			Height = GetPreferredHeight(Width);
			base.OnLayout(levent);
		}

		// we still need the resize sometimes or ghost fields disappear
		protected override void OnSizeChanged(EventArgs e)
		{
			Height = GetPreferredHeight(Width);
			base.OnSizeChanged(e);
		}

		protected override void OnResize(EventArgs e)
		{
			Height = GetPreferredHeight(Width);
			base.OnResize(e);
		}

		public override Size GetPreferredSize(Size proposedSize)
		{
			Size size = base.GetPreferredSize(proposedSize);
			size.Height = GetPreferredHeight(size.Width);
			return size;
		}

		private int _oldWidth = -1;
		private string _oldText;
		private Font _oldFont;
		private int _oldHeight = -1;
		private int GetPreferredHeight(int width)
		{
			if (width == _oldWidth && Text == _oldText && Font == _oldFont && _oldHeight > 0)
				return _oldHeight;
			using (Graphics g = CreateGraphics())
			{
				TextFormatFlags flags = TextFormatFlags.TextBoxControl | TextFormatFlags.Default |
										TextFormatFlags.NoClipping;
				if (Multiline && WordWrap)
				{
					flags |= TextFormatFlags.WordBreak;
				}
				if (_writingSystem != null && WritingSystem.RightToLeftScript)
				{
					flags |= TextFormatFlags.RightToLeft;
				}
				Size sz = TextRenderer.MeasureText(g,
													Text == String.Empty ? " " : Text + Environment.NewLine,
												   // replace empty string with space, because mono returns zero height for empty string (windows returns one line height)
												   // need extra new line to handle case where ends in new line (since last newline is ignored)
												   Font,
												   new Size(width, int.MaxValue),
												   flags);

				if (!Platform.IsWindows)
				{
					// For Mono, need to make an additional adjustment if more than one line is displayed
					var sz2 = TextRenderer.MeasureText(g,
						" ",
						Font,
						new Size(width, int.MaxValue),
						flags);
					var numberOfLines = sz.Height / sz2.Height;
					if (sz.Height % sz2.Height != 0)
					{
						numberOfLines++;
					}
					if (numberOfLines > 1)
					{
						sz.Height += numberOfLines * 4;
					}
				}
				_oldHeight = sz.Height + 2; // add enough space for spell checking squiggle underneath
				_oldWidth = width;
				_oldText = Text;
				_oldFont = Font;
				return _oldHeight;
			}
		}

		public StdTextInputBox(WritingSystemDefinition ws, string nameForLogging): this()
		{
			_nameForLogging = nameForLogging;
			if (_nameForLogging == null)
			{
				_nameForLogging = "??";
			}
			Name = _nameForLogging;
			WritingSystem = ws;
		}

		[Browsable(false)]
		public override string Text
		{
			set
			{
				_previousText = value;
				base.Text = value;
			}
			get { return base.Text; }
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public WritingSystemDefinition WritingSystem
		{
			get
			{
				return _writingSystem;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException();
				}
				_writingSystem = value;
				Font = value.CreateDefaultFont();

				if (value.RightToLeftScript)
				{
					RightToLeft = RightToLeft.Yes;
				}
				else
				{
					RightToLeft = RightToLeft.No;
				}
			}
		}

		public bool MultiParagraph
		{
			get { return _multiParagraph; }
			set { _multiParagraph = value; }
		}

		public bool IsSpellCheckingEnabled
		{
			get { return _isSpellCheckingEnabled; }
			set
			{
				_isSpellCheckingEnabled = value;
				if (_isSpellCheckingEnabled && _writingSystem != null && _writingSystem.SpellCheckingId != "none")
				{
					OnSpellCheckingEnabled();
				}
				else
				{
					OnSpellCheckingDisabled();
				}
			}
		}

		private void OnSpellCheckingDisabled()
		{
			TextBoxSpellChecker.SetLanguageForSpellChecking(this, null);
		}

		private void OnSpellCheckingEnabled()
		{
			if (_writingSystem != null)
				TextBoxSpellChecker.SetLanguageForSpellChecking(this, _writingSystem.SpellCheckingId);
		}

		private static TextBoxSpellChecker TextBoxSpellChecker
		{
			get
			{
				if (_textBoxSpellChecker == null)
				{
					_textBoxSpellChecker = new TextBoxSpellChecker();
				}
				return _textBoxSpellChecker;
			}
		}

		protected void HandleKeyPress(object sender, KeyPressEventArgs e)
		{
			if (!MultiParagraph && (e.KeyChar == '\r' || e.KeyChar == '\n')) // carriage return
				e.Handled = true;
		}

		protected override void OnEnter(EventArgs e)
		{
			base.OnEnter(e);
			AssignKeyboardFromWritingSystem();
		}

		public void AssignKeyboardFromWritingSystem()
		{
			_writingSystem?.LocalKeyboard?.Activate();
		}

		protected override void OnLeave(EventArgs e)
		{
			base.OnLeave(e);
			ClearKeyboard();
		}

		public void ClearKeyboard()
		{
			if (_writingSystem != null)
				Keyboard.Controller.ActivateDefaultKeyboard();
		}

		/// <summary>
		/// for automated tests
		/// </summary>
		public void PretendLostFocus()
		{
			OnLostFocus(new EventArgs());
		}

		/// <summary>
		/// for automated tests
		/// </summary>
		public void PretendSetFocus()
		{
			Focus();
		}

		public List<LanguageForm.FormatSpan> Spans { get; set; }
	}
}
