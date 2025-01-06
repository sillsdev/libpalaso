// Copyright (c) 2024, SIL Global.
// Distributable under the terms of the MIT license (http://opensource.org/licenses/MIT).

using System;
using System.Windows.Forms;
using IBusDotNet;
using NUnit.Framework;
using SIL.Windows.Forms.Keyboarding.Linux;

namespace SIL.Windows.Forms.Keyboarding.Tests
{
	[TestFixture]
	[Platform(Include="Linux", Reason="Linux specific tests")]
	public class IbusDefaultEventHandlerTests
	{
		private TextBox m_TextBox;
		private IbusDefaultEventHandler m_Handler;

		[SetUp]
		public void SetUp()
		{
			m_TextBox = new TextBox();
			m_TextBox.CreateControl();
			m_TextBox.Focus();
			Application.DoEvents();
			m_Handler = new IbusDefaultEventHandler(m_TextBox);
		}

		/// <summary>Unit tests for the OnUpdatePreeditText method. We test this separately from
		/// CommitText since we expect a slightly different behavior, e.g. the range selection
		/// should remain.</summary>
		[TestCase("",  0, 0, /* Input: */ "e", 1, /* expected: */ "e",  1, 0, TestName="UpdatePreedit_EmptyTextbox_AddsText")]
		[TestCase("b", 1, 0, /* Input: */ "e", 1, /* expected: */ "be", 2, 0, TestName="UpdatePreedit_ExistingText_AddsText")]
		[TestCase("b", 0, 0, /* Input: */ "e", 1, /* expected: */ "eb", 1, 0, TestName="UpdatePreedit_ExistingText_InsertInFront")]

		[TestCase("abc", 0, 1, /* Input: */ "e", 1,/* expected: */ "aebc", 0, 1, TestName="UpdatePreedit_ExistingText_RangeSelection")]
		[TestCase("abc", 0, 3, /* Input: */ "e", 1,/* expected: */ "abce", 0, 3, TestName="UpdatePreedit_ReplaceAll")]
		[TestCase("b", 1, 0, /* Input: */ "\u4FDD\u989D", 0, /* expected: */ "b\u4FDD\u989D", 1, 0, TestName="UpdatePreedit_CursorPos0")]
		[TestCase("b", 0, 1, /* Input: */ "\u4FDD\u989D", 0, /* expected: */ "b\u4FDD\u989D", 0, 1, TestName="UpdatePreedit_CursorPos0_RangeSelection")]
		public void UpdatePreedit(
			string text, int selectionStart, int selectionLength,
			string composition, int cursorPos,
			string expectedText, int expectedSelectionStart, int expectedSelectionLength)
		{
			// Setup
			m_TextBox.Text = text;
			m_TextBox.SelectionStart = selectionStart;
			m_TextBox.SelectionLength = selectionLength;

			// Exercise
			m_Handler.OnUpdatePreeditText(new IBusText(composition), cursorPos);

			// Verify
			Assert.That(m_TextBox.Text, Is.EqualTo(expectedText));
			Assert.That(m_TextBox.SelectionStart, Is.EqualTo(expectedSelectionStart), "SelectionStart");
			Assert.That(m_TextBox.SelectionLength, Is.EqualTo(expectedSelectionLength), "SelectionLength");
		}

		// This tests the scenario where we get a second OnUpdatePreeditText that should replace
		// the composition of the first one.
		[TestCase("bc", 1, 0, "a", 1, /* Input: */ "e", 1, /* expected: */ "bec", 2, 0, TestName="UpdatePreedit_ExistingText_ReplaceFirstChar")]
		// This test tests the scenario where the textbox has one character, b. The user
		// positions the IP in front of the b and then types a and e with ibus (e.g. Danish keyboard).
		// This test simulates typing the e.
		[TestCase("b", 0, 0, "a", 1, /* Input: */ "\u00E6", 1, /* expected: */ "\u00E6b", 1, 0, TestName="UpdatePreedit_ExistingText_InsertSecondChar")]
		public void UpdatePreedit_SecondUpdatePreedit(
			string text, int selectionStart, int selectionLength,
			string firstComposition, int firstCursorPos,
			string composition, int cursorPos,
			string expectedText, int expectedSelectionStart, int expectedSelectionLength)
		{
			// Setup
			m_TextBox.Text = text;
			m_TextBox.SelectionStart = selectionStart;
			m_TextBox.SelectionLength = selectionLength;
			m_Handler.OnUpdatePreeditText(new IBusText(firstComposition), firstCursorPos);

			// Exercise
			m_Handler.OnUpdatePreeditText(new IBusText(composition), cursorPos);

			// Verify
			Assert.That(m_TextBox.Text, Is.EqualTo(expectedText));
			Assert.That(m_TextBox.SelectionStart, Is.EqualTo(expectedSelectionStart), "SelectionStart");
			Assert.That(m_TextBox.SelectionLength, Is.EqualTo(expectedSelectionLength), "SelectionLength");
		}

		/// <summary>Unit tests for the CommitOrReset method</summary>
		[TestCase(IBusAttrUnderline.None,   /* expected: */ false, "a", 1, 0, TestName="CommitOrReset_Commits")]
		[TestCase(IBusAttrUnderline.Single, /* expected: */ true, "", 0, 0, TestName="CommitOrReset_Resets")]
		public void CommitOrReset(IBusAttrUnderline underline,
			bool expectedRetVal, string expectedText, int expectedSelStart, int expectedSelLenth)
		{
			// Setup
			m_TextBox.Text = string.Empty;
			m_TextBox.SelectionStart = 0;
			m_TextBox.SelectionLength = 0;
			m_Handler.OnUpdatePreeditText(new IBusText("a",
				new [] { new IBusUnderlineAttribute(underline, 0, 1)}), 1);

			// Exercise
			var ret = m_Handler.CommitOrReset();

			// Verify
			Assert.That(ret, Is.EqualTo(expectedRetVal));
			Assert.That(m_TextBox.Text, Is.EqualTo(expectedText));
			Assert.That(m_TextBox.SelectionStart, Is.EqualTo(expectedSelStart), "SelectionStart");
			Assert.That(m_TextBox.SelectionLength, Is.EqualTo(expectedSelLenth), "SelectionLength");
		}

		/// <summary>Unit tests for the OnCommitText method. These tests are very similar to
		/// the tests for UpdatePreedit, but there are some important differences in the behavior,
		/// e.g. range selections should be replaced by the composition string.</summary>
		[TestCase("", 0, 0, "e", 1,  /* Input: */ "e", /* expected: */ "e", 1, 0, TestName="Commit_EmptyTextbox_AddsText")]
		[TestCase("b", 1, 0, "e", 1, /* Input: */ "e", /* expected: */ "be", 2, 0, TestName="Commit_ExistingText_AddsText")]
		[TestCase("b", 0, 0, "e", 1, /* Input: */ "e", /* expected: */ "eb", 1, 0, TestName="Commit_ExistingText_InsertInFront")]
		// This test tests the scenario where the textbox has one character, b. The user
		// positions the IP in front of the b and then types a and e with ibus (e.g. Danish keyboard).
		// This test simulates the commit after typing the e.
		[TestCase("ab", 0, 1, "\u00E6", 1, /* Input: */ "\u00E6", /* expected: */ "\u00E6b", 1, 0, TestName="Commit_ExistingText_InsertSecondChar")]

		[TestCase("abc", 0, 1, "e", 1,/* Input: */ "e", /* expected: */ "ebc", 1, 0, TestName="Commit_ExistingText_RangeSelection")]
		[TestCase("abc", 0, 3, "e", 1,/* Input: */ "e", /* expected: */ "e",   1, 0, TestName="Commit_ReplaceAll")]
		public void CommitText(
			string text, int selectionStart, int selectionLength,
			string composition, int insertPos,
			string commitText,
			string expectedText, int expectedSelectionStart, int expectedSelectionLength)
		{
			// Setup
			m_TextBox.Text = text;
			m_TextBox.SelectionStart = selectionStart;
			m_TextBox.SelectionLength = selectionLength;
			m_Handler.OnUpdatePreeditText(new IBusText(composition), insertPos);

			// Exercise
			m_Handler.OnCommitText(new IBusText(commitText));

			// Verify
			Assert.That(m_TextBox.Text, Is.EqualTo(expectedText));
			Assert.That(m_TextBox.SelectionStart, Is.EqualTo(expectedSelectionStart), "SelectionStart");
			Assert.That(m_TextBox.SelectionLength, Is.EqualTo(expectedSelectionLength), "SelectionLength");
		}

		/// <summary>
		/// This test simulates a kind of keyboard similar to the IPA ibus keyboard which calls
		/// commit after earch character. This test simulates the first commit call without a
		/// preceding OnUpdatePreeditText.
		/// </summary>
		[Test]
		public void Commit_Ipa()
		{
			m_TextBox.Text = "a";
			m_TextBox.SelectionStart = 1;
			m_TextBox.SelectionLength = 0;

			m_Handler.OnCommitText(new IBusText("\u014B"));

			Assert.That(m_TextBox.Text, Is.EqualTo("a\u014B"));
			Assert.That(m_TextBox.SelectionStart, Is.EqualTo(2));
			Assert.That(m_TextBox.SelectionLength, Is.EqualTo(0));
		}

		/// <summary>
		/// This test simulates a kind of keyboard similar to the IPA ibus keyboard which calls
		/// commit after earch character. This test simulates the callbacks we get from the IPA
		/// keyboard when the user presses 'n' + '>'. The IPA ibus keyboard commits the 'n',
		/// sends us a backspace and then commits the 'ŋ'. This hypothetical keyboard doesn't
		/// use the SurroundingText IBus capability.
		/// </summary>
		[Test]
		public void Commit_IpaTwoCommits_NoSurroundingText()
		{
			const int KeySymBackspace = 65288;
			const int ScanCodeBackspace = 14;
			m_TextBox.Text = "a";
			m_TextBox.SelectionStart = 1;
			m_TextBox.SelectionLength = 0;

			m_Handler.OnCommitText(new IBusText("n"));
			m_Handler.OnIbusKeyPress(KeySymBackspace, ScanCodeBackspace, 0);
			m_Handler.OnCommitText(new IBusText("\u014B"));

			Assert.That(m_TextBox.Text, Is.EqualTo("a\u014B"));
			Assert.That(m_TextBox.SelectionStart, Is.EqualTo(2));
			Assert.That(m_TextBox.SelectionLength, Is.EqualTo(0));
		}

		/// <summary>
		/// This test simulates a kind of keyboard similar to the IPA ibus keyboard which calls
		/// commit after earch character. This test simulates the callbacks we get from the IPA
		/// keyboard when the user presses 'n' + '>'. The IPA ibus keyboard commits the 'n',
		/// sends us a backspace and then commits the 'ŋ'. This keyboard does
		/// use the SurroundingText IBus capability.
		/// </summary>
		[Test]
		public void Commit_IpaTwoCommits_SurroundingText()
		{
			m_TextBox.Text = "a";
			m_TextBox.SelectionStart = 1;
			m_TextBox.SelectionLength = 0;

			m_Handler.OnCommitText(new IBusText("n"));
			m_Handler.OnDeleteSurroundingText(-1, 1);
			m_Handler.OnCommitText(new IBusText("\u014B"));

			Assert.That(m_TextBox.Text, Is.EqualTo("a\u014B"));
			Assert.That(m_TextBox.SelectionStart, Is.EqualTo(2));
			Assert.That(m_TextBox.SelectionLength, Is.EqualTo(0));
		}

		/// <summary>
		/// Unit tests for the OnDeleteSurroundingText method. These tests assume that offset
		/// is 0-based, however the IBus docs don't say and I haven't found a keyboard in the
		/// wild that uses positive offsets.
		/// </summary>
		[TestCase(1, /* Input: */ -1, 1, /* expected: */ "bc", 0, TestName="DeleteSurroundingText_Before")]
		[TestCase(1, /* Input: */  0, 1, /* expected: */ "ac", 1, TestName="DeleteSurroundingText_After")]
		[TestCase(1, /* Input: */ -2, 1, /* expected: */ "abc",1, TestName="DeleteSurroundingText_IllegalBeforeIgnores")]
		[TestCase(1, /* Input: */ -2, 2, /* expected: */ "c",  0, TestName="DeleteSurroundingText_ToManyBefore")]
		[TestCase(2, /* Input: */ -1, 1, /* expected: */ "ac", 1, TestName="DeleteSurroundingText_BeforeUpdatesSelection")]
		[TestCase(2, /* Input: */ -2, 2, /* expected: */ "c",  0, TestName="DeleteSurroundingText_MultipleBefore")]
		[TestCase(1, /* Input: */  1, 1, /* expected: */ "ab", 1, TestName="DeleteSurroundingText_AfterWithOffset")]
		[TestCase(1, /* Input: */  2, 1, /* expected: */ "abc",1, TestName="DeleteSurroundingText_IllegalAfterIgnores")]
		[TestCase(1, /* Input: */  0, 2, /* expected: */ "a",  1, TestName="DeleteSurroundingText_MultipleAfter")]
		[TestCase(1, /* Input: */  0, 3, /* expected: */ "a",  1, TestName="DeleteSurroundingText_ToManyAfterIgnoresRest")]
		[TestCase(1, /* Input: */  0,-1, /* expected: */ "abc",1, TestName="DeleteSurroundingText_IllegalNumberOfChars")]
		[TestCase(1, /* Input: */  0, 0, /* expected: */ "abc",1, TestName="DeleteSurroundingText_ZeroNumberOfChars")]
		public void DeleteSurroundingText(int cursorPos, int offset, int nChars,
			string expectedText, int expectedCursorPos)
		{
			// Setup
			m_TextBox.Text = "abc";
			m_TextBox.SelectionStart = cursorPos;
			m_TextBox.SelectionLength = 0;

			// Exercise
			m_Handler.OnDeleteSurroundingText(offset, nChars);

			// Verify
			Assert.That(m_TextBox.Text, Is.EqualTo(expectedText));
			Assert.That(m_TextBox.SelectionStart, Is.EqualTo(expectedCursorPos));
			Assert.That(m_TextBox.SelectionLength, Is.EqualTo(0));
		}


		[TestCase(1, 0, TestName = "CancelPreedit_IP")]
		[TestCase(0, 1, TestName = "CancelPreedit_RangeSelection")]
		public void CancelPreedit(int selStart, int selLength)
		{
			m_TextBox.Text = "b";
			m_TextBox.SelectionStart = selStart;
			m_TextBox.SelectionLength = selLength;
			m_Handler.OnUpdatePreeditText(new IBusText("\u4FDD\u989D"), 0);

			m_Handler.Reset();

			Assert.That(m_TextBox.Text, Is.EqualTo("b"));
			Assert.That(m_TextBox.SelectionStart, Is.EqualTo(selStart));
			Assert.That(m_TextBox.SelectionLength, Is.EqualTo(selLength));
		}
	}
}
