// Copyright (c) 2025 SIL Global
// Distributable under the terms of the MIT license (http://opensource.org/licenses/MIT).
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using IBusDotNet;

namespace SIL.Windows.Forms.Keyboarding.Linux
{
	/// <summary>
	/// Default event handler for IBUS events that works with a WinForms text box.
	/// </summary>
	public class IbusDefaultEventHandler : IIbusEventHandler
	{
		private TextBox m_TextBox;
		/// <summary>
		/// The start of the selection in the textbox before we insert a temporary composition string.
		/// </summary>
		private int m_SelectionStart;
		/// <summary>
		/// The initial length of the selection in the textbox.
		/// </summary>
		private int m_SelectionLength;
		/// <summary>
		/// Number of characters that got inserted temporarily as composition string.
		/// </summary>
		private int m_PreeditLength;
		/// <summary>
		/// <c>true</c> while we're inside of Reset(bool).
		/// </summary>
		private bool m_InReset;

		public IbusDefaultEventHandler(TextBox textBox)
		{
			m_TextBox = textBox;
			m_SelectionStart = -1;
			m_SelectionLength = -1;
		}

		/// <summary>
		/// Reset the selection and optionally cancel any open compositions.
		/// </summary>
		/// <param name="cancel">Set to <c>true</c> to also cancel the open composition.</param>
		/// <returns><c>true</c> if there was an open composition that we cancelled, otherwise
		/// <c>false</c>.</returns>
		private bool Reset(bool cancel)
		{
			if (m_InReset)
				return false;

			var retVal = false;
			m_InReset = true;
			if (cancel && m_SelectionStart > -1)
			{
				var preeditStart = m_SelectionStart + m_SelectionLength;
				m_TextBox.Text = RemoveChars(m_TextBox.Text, preeditStart, m_PreeditLength);
				m_TextBox.SelectionStart = m_SelectionStart;
				m_TextBox.SelectionLength = m_SelectionLength;
				retVal = true;
			}

			m_SelectionStart = -1;
			m_SelectionLength = -1;
			m_PreeditLength = 0;
			m_InReset = false;
			return retVal;
		}

		/// <summary>
		/// Removes the characters from the preedit part of <paramref name="text"/>.
		/// </summary>
		/// <returns>The new string.</returns>
		/// <param name="text">Text.</param>
		/// <param name="removePos">Position inside of <paramref name="text"/> where the
		/// temporary preedit characters start and where we should start to remove the
		/// characters.</param>
		/// <param name="preeditTextLen">Length of the preedit text part.</param>
		private static string RemoveChars(string text, int removePos, int preeditTextLen)
		{
			var toRemove = preeditTextLen;
			if (removePos + toRemove > text.Length)
				toRemove = Math.Max(text.Length - removePos, 0);
			if (toRemove > 0)
				return text.Remove(removePos, toRemove);
			return text;
		}

		/// <summary>
		/// Trim beginning backspaces, if any. Return number trimmed.
		/// </summary>
		private static int TrimBeginningBackspaces(ref string text)
		{
			const char backSpace = '\b'; // 0x0008

			if (!text.StartsWith(backSpace.ToString()))
				return 0;

			var count = text.Length - text.TrimStart(backSpace).Length;
			text = text.TrimStart(backSpace);
			return count;
		}

		private void OnCommitText(string text)
		{
			// Replace 'toRemove' characters starting at 'insertPos' with 'text'.
			int insertPos, toRemove;
			if (m_SelectionStart > -1)
			{
				insertPos = m_SelectionStart;
				toRemove = m_SelectionLength + m_PreeditLength;
			}
			else
			{
				// IPA Unicode 6.2 ibus keyboard doesn't call OnUpdatePreeditText,
				// only OnCommitText, so we don't have a m_SelectionStart stored.
				insertPos = m_TextBox.SelectionStart;
				toRemove = m_TextBox.SelectionLength;
			}
			var countBackspace = TrimBeginningBackspaces(ref text);
			insertPos -= countBackspace;
			if (insertPos < 0)
				insertPos = 0;
			toRemove += countBackspace;
			var boxText = RemoveChars(m_TextBox.Text, insertPos, toRemove);

			m_TextBox.Text = boxText.Insert(insertPos, text);
			m_TextBox.SelectionStart = insertPos + text.Length;
			m_TextBox.SelectionLength = 0;
			Reset(false);
		}

		// When the application loses focus the user expects different behavior for different
		// ibus keyboards: for some keyboards (those that do the editing in-place and don't display
		// a selection window, e.g. "Danish - post (m17n)") the user expects that what he
		// typed remains, i.e. gets committed. Otherwise (e.g. with the Danish keyboard) it's not
		// possible to type an "a" and then click in a different field or switch applications.
		//
		// For other keyboards (e.g. Chinese Pinyin) the commit is made when the user selects
		// one of the possible characters in the pop-up window. If he clicks in a different
		// field while the pop-up window is open the composition should be deleted.
		//
		// There doesn't seem to be a way to ask an IME keyboard if it shows a pop-up window or
		// if we should commit or reset the composition. One indirect way however seems to be to
		// check the attributes: it seems that keyboards where we can/should commit set the
		// underline attribute to IBusAttrUnderline.None.
		private bool IsCommittingKeyboard { get; set; }

		private void CheckAttributesForCommittingKeyboard(IBusText text)
		{
			IsCommittingKeyboard = false;
			foreach (var attribute in text.Attributes)
			{
				var iBusUnderlineAttribute = attribute as IBusUnderlineAttribute;
				if (iBusUnderlineAttribute != null && iBusUnderlineAttribute.Underline == IBusAttrUnderline.None)
					IsCommittingKeyboard = true;
			}
		}

		#region IIbusEventHandler implementation
		/// <summary>
		/// This method gets called when the IBus CommitText event is raised and indicates that
		/// the composition is ending. The temporarily inserted composition string will be
		/// replaced with <paramref name="ibusText"/>.
		/// </summary>
		/// <seealso cref="IbusKeyboardSwitchingAdaptor.HandleKeyPress"/>
		public void OnCommitText(object ibusText)
		{
			// Note: when we try to pass IBusText as ibusText parameter we get a compiler crash
			// in FW.

			if (m_TextBox.InvokeRequired)
			{
				m_TextBox.BeginInvoke((Action) (() => OnCommitText(ibusText)));
				return;
			}

			if (!m_TextBox.Focused)
				return;

			OnCommitText(((IBusText)ibusText).Text);
		}

		/// <summary>
		/// Called when the IBus UpdatePreeditText event is raised to update the composition.
		/// </summary>
		/// <param name="obj">New composition string that will replace the existing
		/// composition string.</param>
		/// <param name="cursorPos">0-based position where the cursor should be put after
		/// updating the composition (pre-edit window). This position is relative to the
		/// composition/preedit text.</param>
		/// <seealso cref="IbusKeyboardSwitchingAdaptor.HandleKeyPress"/>
		public void OnUpdatePreeditText(object obj, int cursorPos)
		{
			if (m_TextBox.InvokeRequired)
			{
				m_TextBox.BeginInvoke((Action) (() => OnUpdatePreeditText(obj, cursorPos)));
				return;
			}

			if (!m_TextBox.Focused)
				return;

			var text = obj as IBusText;
			CheckAttributesForCommittingKeyboard(text);

			var compositionText = text.Text;

			if (m_SelectionStart == -1 || m_SelectionStart + m_SelectionLength > m_TextBox.Text.Length)
			{
				// Remember selection in textbox prior to inserting composition text.
				m_SelectionStart = m_TextBox.SelectionStart;
				m_SelectionLength = m_TextBox.SelectionLength;
			}

			var preeditStart = m_SelectionStart + m_SelectionLength;
			m_TextBox.SelectionStart = preeditStart;
			m_TextBox.SelectionLength = 0;
			// Remove the previous composition text starting at preeditStart
			m_TextBox.Text = RemoveChars(m_TextBox.Text, preeditStart, m_PreeditLength);
			if (preeditStart >= m_TextBox.Text.Length)
				m_TextBox.AppendText(compositionText);
			else
				m_TextBox.Text = m_TextBox.Text.Insert(preeditStart, compositionText);

			m_PreeditLength = compositionText.Length;
			if (m_SelectionLength > 0)
			{
				// We want to keep the range selection. It gets deleted in the CommitTextHandler.
				// This mimics the behavior in gedit.
				m_TextBox.SelectionStart = m_SelectionStart;
				m_TextBox.SelectionLength = m_SelectionLength;
			}
			else
			{
				m_TextBox.SelectionStart = m_SelectionStart + cursorPos;
				m_TextBox.SelectionLength = 0;
			}
		}

		/// <summary>
		/// Called when the IBus DeleteSurroundingText is raised to delete surrounding
		/// characters.
		/// </summary>
		/// <param name="offset">The character offset from the cursor position of the text to be
		/// deleted. A negative value indicates a position before the cursor.</param>
		/// <param name="nChars">The number of characters to be deleted.</param>
		public void OnDeleteSurroundingText(int offset, int nChars)
		{
			if (m_TextBox.InvokeRequired)
			{
				m_TextBox.BeginInvoke((Action) (() => OnDeleteSurroundingText(offset, nChars)));
				return;
			}

			if (!m_TextBox.Focused || nChars <= 0)
				return;

			var selectionStart = m_TextBox.SelectionStart;
			var startIndex = selectionStart + offset;
			if (startIndex + nChars <= 0)
				return;

			startIndex = Math.Max(startIndex, 0);
			startIndex = Math.Min(startIndex, m_TextBox.Text.Length);

			if (startIndex + nChars > m_TextBox.Text.Length)
				nChars = m_TextBox.Text.Length - startIndex;

			if (startIndex < selectionStart)
				selectionStart = Math.Max(selectionStart - nChars, 0);

			m_TextBox.Text = m_TextBox.Text.Remove(startIndex, nChars);
			m_TextBox.SelectionStart = selectionStart;
			m_TextBox.SelectionLength = 0;
		}

		/// <summary>
		/// Called when the IBus HidePreeditText event is raised to cancel/remove the composition.
		/// </summary>
		public void OnHidePreeditText()
		{
			if (m_TextBox.InvokeRequired)
			{
				m_TextBox.BeginInvoke((Action) OnHidePreeditText);
				return;
			}
			if (!m_TextBox.Focused)
				return;

			Reset(true);
		}

		/// <summary>
		/// Called when the IBus ForwardKeyEvent (as in: forwarding key event) is raised.
		/// </summary>
		/// <param name="keySym">Key symbol.</param>
		/// <param name="scanCode">Scan code.</param>
		/// <param name="index">Index into a vector of keycodes associated with a given key
		/// depending on which modifier keys are pressed. 0 is always unmodified, and 1 is with
		/// shift alone.
		/// </param>
		/// <seealso cref="IbusKeyboardSwitchingAdaptor.HandleKeyPress"/>
		public void OnIbusKeyPress(int keySym, int scanCode, int index)
		{
			if (m_TextBox.InvokeRequired)
			{
				m_TextBox.BeginInvoke((Action) (() => OnIbusKeyPress(keySym, scanCode, index)));
				return;
			}
			if (!m_TextBox.Focused)
				return;

			var inChar = (char)(0x00FF & keySym);
			OnCommitText(inChar.ToString());
		}

		/// <summary>
		/// Called by the IBusKeyboardAdapter to cancel any open compositions.
		/// </summary>
		/// <returns><c>true</c> if there was an open composition that got cancelled, otherwise
		/// <c>false</c>.</returns>
		public bool Reset()
		{
			Debug.Assert(!m_TextBox.InvokeRequired);
			return Reset(true);
		}

		/// <summary>
		/// Called by the IbusKeyboardAdapter to commit or cancel any open compositions, e.g.
		/// after the application loses focus.
		/// </summary>
		/// <returns><c>true</c> if there was an open composition that got cancelled, otherwise
		/// <c>false</c>.</returns>
		public bool CommitOrReset()
		{
			// don't check if we have focus - we won't if this gets called from OnLostFocus.
			// However, the IbusKeyboardAdapter calls this method only for the control that just
			// lost focus, so it's ok not to check :-)

			if (IsCommittingKeyboard)
			{
				m_SelectionStart = -1;
				m_SelectionLength = -1;
				m_PreeditLength = 0;
				return false;
			}

			return Reset(true);
		}

		/// <summary>
		/// Called by the IBusKeyboardAdapter to get the position (in pixels) and line height of
		/// the end of the selection. The position is relative to the screen in the
		/// PointToScreen sense, that is (0,0) is the top left of the primary monitor.
		/// IBus will use this information when it opens a pop-up window to present a list of
		/// composition choices.
		/// </summary>
		public Rectangle SelectionLocationAndHeight
		{
			get
			{
				Debug.Assert(!m_TextBox.InvokeRequired);
				var posInTextBox = m_TextBox.GetPositionFromCharIndex(m_TextBox.SelectionStart + m_TextBox.SelectionLength);
				var posScreen = m_TextBox.PointToScreen(posInTextBox);
				return new Rectangle(posScreen, new Size(0, m_TextBox.Font.Height));
			}
		}

		/// <summary>
		/// Called by the IbusKeyboardAdapter to find out if a preedit is active.
		/// </summary>
		public bool IsPreeditActive => m_SelectionStart > -1;

		#endregion
	}
}
