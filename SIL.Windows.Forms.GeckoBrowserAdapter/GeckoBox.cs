// Copyright (c) 2024, SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SIL.Text;
using SIL.Windows.Forms.Widgets;
using SIL.WritingSystems;

namespace SIL.Windows.Forms.GeckoBrowserAdapter
{
	public partial class GeckoBox : GeckoBase, ITextInputBox, IControlThatKnowsWritingSystem
	{
		private string _pendingHtmlLoad;
		protected bool _keyPressed;
		private EventHandler _textChangedHandler;
		private string _previousText;

		public GeckoBox()
		{
			InitializeComponent();

			_keyPressed = false;

			var designMode = (LicenseManager.UsageMode == LicenseUsageMode.Designtime);
			if (designMode)
				return;

			Debug.WriteLine("New GeckoBox");
			ReadOnly = false;
			Multiline = false;

			_textChangedHandler = new EventHandler(OnTextChanged);
			this.TextChanged += _textChangedHandler;

			_browser.DomMouseDown += (object sender, EventArgs ea) =>
			{
				//DomMouseEventArgs e = new DomMouseEventArgs(e);
				if (!_browserDocumentLoaded)
				{
					return;
				}

				EnsureFocusedGeckoControlHasInputFocus();

				if (ContainsFocus)
				{
					return;
				}

				_browser.Focus();
			};
		}

		public GeckoBox(WritingSystemDefinition ws, string nameForLogging)
			: this()
		{
			_nameForLogging = nameForLogging;
			if (_nameForLogging == null)
			{
				_nameForLogging = "??";
			}
			Name = _nameForLogging;
			WritingSystem = ws;
		}

		protected override void Closing()
		{
			this.TextChanged -= _textChangedHandler;
			_textChangedHandler = null;
			base.Closing();
		}

		/// <summary>
		/// called when the client changes our Control.Text... we need to them move that into the html
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected virtual void OnTextChanged(object sender, EventArgs e)
		{
			SetText(Text);
			AdjustHeight();
			LanguageForm.AdjustSpansForTextChange(_previousText, Text, Spans);
			_previousText = Text;
		}

		public Control TheControl { get { return this; } }
		
		public void Select(int start, int length)
		{
			base.Select();
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			if (Parent is TextInputBox)
				(Parent as TextInputBox).OnChildKeyDown (e);
			else
				base.OnKeyDown(e);
		}

		protected override void OnDomKeyUp(object sender, EventArgs ea)
		{
			DomKeyEventArgs e = new DomKeyEventArgs(ea);
			var content = _browser.Document.GetElementById("main");
			if (!(e.KeyCode == (uint)Keys.Enter)) // carriage return
			{
				_keyPressed = true;
				Text = content.TextContent;
			}
		}

		protected override void OnGeckoBox_Load(object sender, EventArgs e)
		{
			_browserIsReadyToNavigate = true;
			if (_pendingHtmlLoad != null)
			{
				_browser.LoadHtml(_pendingHtmlLoad);
				_pendingHtmlLoad = null;
			}
			else
			{
				SetText(""); //make an empty, editable box
			}
			this.Focus();
		}

		protected override void OnResize(EventArgs e)
		{
			_browser.Width = Width;
			base.OnResize(e);
		}

		protected void RefreshDisplay()
		{
			if (_writingSystem != null)
			{
				_keyPressed = false;
				SetText(Text);
			}
		}

		private void SetText(string s)
		{
			s = System.Security.SecurityElement.Escape(s);
			String justification = "left";
			String multiLineStyle = "";
			String background = "#FFFFFF";

			if (_writingSystem != null && WritingSystem.RightToLeftScript)
			{
				justification = "right";
			}

			String editable = "true";
			if (ReadOnly)
			{
				editable = "false";
				background = System.Drawing.ColorTranslator.ToHtml(BackColor);
			}

			if (Multiline)
			{
				multiLineStyle = "word-wrap:break-word; ";
			}
			else
			{
				multiLineStyle = "white-space:nowrap; ";
			}
			Font font;
			if (_writingSystem != null)
				font = _writingSystem.CreateDefaultFont();
			else
				font = this.Parent.Font;
			var html = new StringBuilder("<html>");
			html.AppendLine("<head>");
			html.AppendLine("<meta charset=\"UTF-8\">");
			html.AppendLine("<style>");
			html.AppendLine("@font-face {");
			html.AppendFormat("    font-family: \"{0}\";\n", font.Name);
			html.AppendFormat("    src: local(\"{0}\");\n", font.Name);
			html.AppendLine("}");
			html.AppendLine("</style>");
			html.AppendLine("</head>");
			html.AppendFormat("<body style='background:{1}' id='mainbody' {0}>", GetLanguageHtml(_writingSystem), background);
			html.AppendFormat("<div style='min-height:15px; font-family:{0}; font-size:{1}pt; text-align:{3} background:{5}; color:{6}; {7}' id='main' name='textArea' contentEditable='{4}'>{2}</div>",
					font.Name, font.Size.ToString(), s, justification, editable, System.Drawing.ColorTranslator.ToHtml(BackColor), System.Drawing.ColorTranslator.ToHtml(ForeColor), multiLineStyle);
			html.AppendLine("</body>");
			html.AppendLine("</html>");
			if (!_browserIsReadyToNavigate)
			{
				_pendingHtmlLoad = html.ToString();
			}
			else
			{
				if (!_keyPressed)
				{
					_browser.LoadHtml(html.ToString());
				}
				_keyPressed = false;
			}
		}

		public void SetHtml(string html)
		{
			if (!_browserIsReadyToNavigate)
			{
				_pendingHtmlLoad = html;
			}
			else
			{
				_browser.LoadHtml(html);
			}

		}


		public bool IsSpellCheckingEnabled { get; set; }


		protected override void OnLeave(EventArgs e)
		{
			base.OnLeave(e);

			ClearKeyboard();
		}
		protected override void OnEnter(EventArgs e)
		{
			base.OnEnter(e);

			AssignKeyboardFromWritingSystem();
		}

		public List<LanguageForm.FormatSpan> Spans { get; set; }

		public override string Text
		{
			set
			{
				_previousText = value;
				base.Text = value;
			}
			get { return base.Text; }
		}
	}

}
