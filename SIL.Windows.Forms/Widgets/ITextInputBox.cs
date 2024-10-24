// Copyright (c) 2024, SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using SIL.Text;
using SIL.WritingSystems;

namespace SIL.Windows.Forms.Widgets
{
	public interface ITextInputBox
	{
		Size GetPreferredSize(Size proposedSize);

		[Browsable(false)]
		string Text { set; get; }

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		WritingSystemDefinition WritingSystem { get; set; }

		bool MultiParagraph { get; set; }
		bool IsSpellCheckingEnabled { get; set; }
		int SelectionStart { get; set; }
		int SelectionLength { get; set; }
		void Select(int start, int length);
		void AssignKeyboardFromWritingSystem();
		void ClearKeyboard();
		void Init(WritingSystemDefinition writingSystem, String name);
		Font Font { get; set; }

		Object Tag { get; set; }
		bool Multiline { get; set; }
		bool WordWrap { get; set; }
		bool ReadOnly { get; set; }
		bool Focus();
		void Show();
		Color ForeColor { get; set; }
		bool TabStop { get; set; }
		bool IsDisposed { get; }
		event EventHandler UserLostFocus;
		event EventHandler UserGotFocus;

		/// <summary>
		/// for automated tests
		/// </summary>
		void PretendLostFocus();

		/// <summary>
		/// for automated tests
		/// </summary>
		void PretendSetFocus();

		/// <summary>
		/// Formatting information that we need to preserve.
		/// </summary>
		List<LanguageForm.FormatSpan> Spans { get; set; }

		Control TheControl { get; }
	}
}
