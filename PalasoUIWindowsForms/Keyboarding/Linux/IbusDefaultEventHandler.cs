// Copyright (c) 2013, SIL International.
// Distributable under the terms of the MIT license (http://opensource.org/licenses/MIT).
#if __MonoCS__
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using IBusDotNet;
using Palaso.UI.WindowsForms.Extensions;
using Palaso.UI.WindowsForms.Keyboarding.Interfaces;

namespace Palaso.UI.WindowsForms.Keyboarding.Linux
{
	/// <summary>
	/// Default event handler for IBUS events that works with a WinForms text box.
	/// </summary>
	public class IbusDefaultEventHandler: IIbusEventHandler
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

			bool retVal = false;
			m_InReset = true;
			if (cancel && m_SelectionStart > -1)
			{
				var preeditStart = m_SelectionStart + m_SelectionLength;
				m_TextBox.Text = RemoveChars(m_TextBox.Text, preeditStart, preeditStart, m_PreeditLength);
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
		/// <param name="preeditStartPos">Position inside of <paramref name="text"/> where the
		/// temporary preedit characters start.</param>
		/// <param name="removePos">Position where to start removing characters. This position
		/// is relative to the entire <paramref name="text"/>.</param>
		/// <param name="preeditTextLen">Length of the preedit text part.</param>
		private static string RemoveChars(string text, int preeditStartPos, int removePos, int preeditTextLen)
		{
			var toRemove = preeditTextLen;
			if (removePos - preeditStartPos >= preeditTextLen)
				toRemove = preeditTextLen - removePos + preeditStartPos;
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

			int count = text.Length - text.TrimStart(backSpace).Length;
			text = text.TrimStart(backSpace);
			return count;
		}

		#region IIbusEventHandler implementation
		/// <summary>
		/// This method gets called when the IBus CommitText event is raised and indicates that
		/// the composition is ending. The temporarily inserted composition string will be
		/// replaced with <paramref name="text"/>.
		/// </summary>
		/// <seealso cref="IBusKeyboardAdaptor.HandleKeyPress"/>
		public void OnCommitText(string text)
		{
			if (m_TextBox.InvokeRequired)
			{
				m_TextBox.BeginInvoke(() => OnCommitText(text));
				return;
			}
			if (!m_TextBox.Focused)
				return;

			// Replace 'toRemove' characters starting at 'insertPos' with 'text'.
			int insertPos, toRemove;
			if (m_SelectionStart > -1)
			{
				insertPos = m_SelectionStart;
				toRemove = m_SelectionLength + m_PreeditLength;
			}
			else
			{
				// IPA Unicode 6.2 ibus keyboard doesn't call OnUpdatPreeditText,
				// only OnCommitText, so we don't have a m_SelectionStart stored.
				insertPos = m_TextBox.SelectionStart;
				toRemove = m_TextBox.SelectionLength;
			}
			var countBackspace = TrimBeginningBackspaces(ref text);
			insertPos -= countBackspace;
			if (insertPos < 0)
				insertPos = 0;
			toRemove += countBackspace;
			var boxText = RemoveChars(m_TextBox.Text, insertPos, insertPos, toRemove);

			m_TextBox.Text = boxText.Insert(insertPos, text);
			m_TextBox.SelectionStart = insertPos + text.Length;
			m_TextBox.SelectionLength = 0;
			Reset(false);
		}

		/// <summary>
		/// Called when the IBus UpdatePreeditText event is raised to update the composition.
		/// </summary>
		/// <param name="compositionText">New composition string that will replace the existing
		/// composition (sub-)string.</param>
		/// <param name="cursorPos">1-based index in the composition (pre-edit) window. The
		/// composition string will be replaced with <paramref name="compositionText"/>starting
		/// at this position.</param>
		/// <seealso cref="IBusKeyboardAdaptor.HandleKeyPress"/>
		public void OnUpdatePreeditText(string compositionText, int cursorPos)
		{
			if (m_TextBox.InvokeRequired)
			{
				m_TextBox.BeginInvoke(() => OnUpdatePreeditText(compositionText, cursorPos));
				return;
			}

			if (!m_TextBox.Focused)
				return;

			// Chinese Pinyin keyboard for some reason passes 0 (instead of 1) as the cursorPos
			// of the first character it inserts, i.e. it uses a 0-based cursorPos instead of
			// 1-based which other ibus keyboards seems to use. This would be a problem if it
			// later would try to add additional characters at a higher index. Fortunately it
			// always replaces the entire compositionText starting from the beginning.
			if (cursorPos == 0)
				cursorPos++;

			if (m_SelectionStart == -1 || m_SelectionStart + m_SelectionLength > m_TextBox.Text.Length)
			{
				// Remember selection in textbox prior to inserting composition text.
				m_SelectionStart = m_TextBox.SelectionStart;
				m_SelectionLength = m_TextBox.SelectionLength;
			}

			var insertPos = m_SelectionStart + m_SelectionLength + cursorPos - 1;
			var preeditStart = m_SelectionStart + m_SelectionLength;
			m_TextBox.SelectionStart = insertPos;
			m_TextBox.SelectionLength = 0;
			// Remove the previous composition text starting at insertPos
			m_TextBox.Text = RemoveChars(m_TextBox.Text, preeditStart, insertPos, m_PreeditLength);
			if (insertPos >= m_TextBox.Text.Length)
			{
				insertPos = m_TextBox.Text.Length;
				m_TextBox.AppendText(compositionText);
			}
			else
				m_TextBox.Text = m_TextBox.Text.Insert(insertPos, compositionText);

			m_PreeditLength = cursorPos + compositionText.Length - 1;
			if (m_SelectionLength > 0)
			{
				// We want to keep the range selection. It gets deleted in the CommitTextHandler.
				// This mimics the behavior in gedit.
				m_TextBox.SelectionStart = m_SelectionStart;
				m_TextBox.SelectionLength = m_SelectionLength;
			}
			else
			{
				m_TextBox.SelectionStart = insertPos + compositionText.Length;
				m_TextBox.SelectionLength = 0;
			}
		}

		/// <summary>
		/// Called when the IBus HidePreeditText event is raised to cancel/remove the composition.
		/// </summary>
		public void OnHidePreeditText()
		{
			if (m_TextBox.InvokeRequired)
			{
				m_TextBox.BeginInvoke(OnHidePreeditText);
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
		/// <seealso cref="IBusKeyboardAdaptor.HandleKeyPress"/>
		public void OnIbusKeyPress(int keySym, int scanCode, int index)
		{
			if (m_TextBox.InvokeRequired)
			{
				m_TextBox.BeginInvoke(() => OnIbusKeyPress(keySym, scanCode, index));
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
		#endregion
	}
}
#endif
