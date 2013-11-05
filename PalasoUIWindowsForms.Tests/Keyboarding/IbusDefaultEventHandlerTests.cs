// Copyright (c) 2013, SIL International.
// Distributable under the terms of the MIT license (http://opensource.org/licenses/MIT).
#if __MonoCS__
using System;
using System.Windows.Forms;
using NUnit.Framework;
using Palaso.UI.WindowsForms.Keyboarding.Linux;

namespace PalasoUIWindowsForms.Tests.Keyboarding
{
	[TestFixture]
	public class IbusDefaultEventHandlerTests
	{
		private TextBox m_TextBox;
		private IbusDefaultEventHandler m_Handler;

		[SetUp]
		public void SetUp()
		{
			m_TextBox = new TextBox();
			m_Handler = new IbusDefaultEventHandler(m_TextBox);
		}

		/// <summary>Unit tests for the OnUpdatePreeditText method. We test this separately from
		/// CommitText since we expect a slightly different behavior, e.g. the range selection
		/// should remain.</summary>
		[Test]
		[TestCase("",  0, 0, /* Input: */ "e", 1, /* expected: */ "e",  1, 0, TestName="UpdatePreedit_EmptyTextbox_AddsText")]
		[TestCase("b", 1, 0, /* Input: */ "e", 1, /* expected: */ "be", 2, 0, TestName="UpdatePreedit_ExistingText_AddsText")]
		[TestCase("b", 0, 0, /* Input: */ "e", 1, /* expected: */ "eb", 1, 0, TestName="UpdatePreedit_ExistingText_InsertInFront")]

		[TestCase("abc", 0, 1, /* Input: */ "e", 1,/* expected: */ "aebc", 0, 1, TestName="UpdatePreedit_ExistingText_RangeSelection")]
		[TestCase("abc", 0, 3, /* Input: */ "e", 1,/* expected: */ "abce", 0, 3, TestName="UpdatePreedit_ReplaceAll")]

		// Chinese Pinyin ibus keyboard for some reason uses a 0-based index
		[TestCase("b", 1, 0, /* Input: */ "保额", 0, /* expected: */ "b保额", 3, 0, TestName="UpdatePreedit_CursorPos0")]
		[TestCase("b", 0, 1, /* Input: */ "保额", 0, /* expected: */ "b保额", 0, 1, TestName="UpdatePreedit_CursorPos0_RangeSelection")]
		public void UpdatePreedit(
			string text, int selectionStart, int selectionLength,
			string composition, int insertPos,
			string expectedText, int expectedSelectionStart, int expectedSelectionLength)
		{
			// Setup
			m_TextBox.Text = text;
			m_TextBox.SelectionStart = selectionStart;
			m_TextBox.SelectionLength = selectionLength;

			// Exercise
			m_Handler.OnUpdatePreeditText(composition, insertPos);

			// Verify
			Assert.That(m_TextBox.Text, Is.EqualTo(expectedText));
			Assert.That(m_TextBox.SelectionStart, Is.EqualTo(expectedSelectionStart), "SelectionStart");
			Assert.That(m_TextBox.SelectionLength, Is.EqualTo(expectedSelectionLength), "SelectionLength");
		}

		[Test]
		// This tests the scenario where we get a second OnUpdatePreeditText that should replace
		// the composition of the first one.
		[TestCase("bc", 1, 0, "a", 1, /* Input: */ "e", 1, /* expected: */ "bec", 2, 0, TestName="UpdatePreedit_ExistingText_ReplaceFirstChar")]
		// This test tests the scenario where the textbox has one character, b. The user
		// positions the IP in front of the b and then types a and e with ibus (e.g. Danish keyboard).
		// This test simulates typing the e.
		[TestCase("b", 0, 0, "a", 1, /* Input: */ "e", 2, /* expected: */ "aeb", 2, 0, TestName="UpdatePreedit_ExistingText_InsertSecondChar")]
		public void UpdatePreedit_SecondUpdatePreedit(
			string text, int selectionStart, int selectionLength,
			string firstComposition, int firstInsertPos,
			string composition, int insertPos,
			string expectedText, int expectedSelectionStart, int expectedSelectionLength)
		{
			// Setup
			m_TextBox.Text = text;
			m_TextBox.SelectionStart = selectionStart;
			m_TextBox.SelectionLength = selectionLength;
			m_Handler.OnUpdatePreeditText(firstComposition, firstInsertPos);

			// Exercise
			m_Handler.OnUpdatePreeditText(composition, insertPos);

			// Verify
			Assert.That(m_TextBox.Text, Is.EqualTo(expectedText));
			Assert.That(m_TextBox.SelectionStart, Is.EqualTo(expectedSelectionStart), "SelectionStart");
			Assert.That(m_TextBox.SelectionLength, Is.EqualTo(expectedSelectionLength), "SelectionLength");
		}

		/// <summary>Unit tests for the OnCommitText method. These tests are very similar to
		/// the tests for UpdatePreedit, but there are some important differences in the behavior,
		/// e.g. range selections should be replaced by the composition string.</summary>
		[Test]
		[TestCase("", 0, 0, "e", 1,  /* Input: */ "e", /* expected: */ "e", 1, 0, TestName="Commit_EmptyTextbox_AddsText")]
		[TestCase("b", 1, 0, "e", 1, /* Input: */ "e", /* expected: */ "be", 2, 0, TestName="Commit_ExistingText_AddsText")]
		[TestCase("b", 0, 0, "e", 1, /* Input: */ "e", /* expected: */ "eb", 1, 0, TestName="Commit_ExistingText_InsertInFront")]
		// This test tests the scenario where the textbox has one character, b. The user
		// positions the IP in front of the b and then types a and e with ibus (e.g. Danish keyboard).
		// This test simulates the commit after typing the e.
		[TestCase("ab", 0, 0, "e", 2, /* Input: */ "æ", /* expected: */ "æb", 1, 0, TestName="Commit_ExistingText_InsertSecondChar")]

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
			m_Handler.OnUpdatePreeditText(composition, insertPos);

			// Exercise
			m_Handler.OnCommitText(commitText);

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

			m_Handler.OnCommitText("ŋ");

			Assert.That(m_TextBox.Text, Is.EqualTo("aŋ"));
			Assert.That(m_TextBox.SelectionStart, Is.EqualTo(2));
			Assert.That(m_TextBox.SelectionLength, Is.EqualTo(0));
		}

		/// <summary>
		/// This test simulates a kind of keyboard similar to the IPA ibus keyboard which calls
		/// commit after earch character. This test simulates the callbacks we get from the IPA
		/// keyboard when the user presses 'n' + '>'. The IPA ibus keyboard commits the 'n',
		/// sends us a backspace and then commits the 'ŋ'.
		/// </summary>
		[Test]
		public void Commit_IpaTwoCommits()
		{
			const int KeySymBackspace = 65288;
			const int ScanCodeBackspace = 14;
			m_TextBox.Text = "a";
			m_TextBox.SelectionStart = 1;
			m_TextBox.SelectionLength = 0;

			m_Handler.OnCommitText("n");
			m_Handler.OnIbusKeyPress(KeySymBackspace, ScanCodeBackspace, 0);
			m_Handler.OnCommitText("ŋ");

			Assert.That(m_TextBox.Text, Is.EqualTo("aŋ"));
			Assert.That(m_TextBox.SelectionStart, Is.EqualTo(2));
			Assert.That(m_TextBox.SelectionLength, Is.EqualTo(0));
		}

		[Test]
		public void CancelPreedit()
		{
			m_TextBox.Text = "b";
			m_TextBox.SelectionStart = 1;
			m_TextBox.SelectionLength = 0;
			m_Handler.OnUpdatePreeditText("保额", 0);

			m_Handler.Reset();

			Assert.That(m_TextBox.Text, Is.EqualTo("b"));
			Assert.That(m_TextBox.SelectionStart, Is.EqualTo(1));
			Assert.That(m_TextBox.SelectionLength, Is.EqualTo(0));
		}

		[Test]
		public void CancelPreedit_RangeSelection()
		{
			m_TextBox.Text = "b";
			m_TextBox.SelectionStart = 0;
			m_TextBox.SelectionLength = 1;
			m_Handler.OnUpdatePreeditText("保额", 0);

			m_Handler.Reset();

			Assert.That(m_TextBox.Text, Is.EqualTo("b"));
			Assert.That(m_TextBox.SelectionStart, Is.EqualTo(0));
			Assert.That(m_TextBox.SelectionLength, Is.EqualTo(1));
		}
	}
}
#endif
