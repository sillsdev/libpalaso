using System;
using System.Threading;
using System.Windows.Forms;
using NUnit.Framework;
using SIL.PlatformUtilities;
using SIL.Reflection;
using SIL.Scripture;
using SIL.Windows.Forms.Miscellaneous;

namespace SIL.Windows.Forms.Scripture.Tests
{
	[TestFixture]
	[Apartment(ApartmentState.STA)]
	public class VerseControlTests
	{
		private Form m_ctrlOwner;
		private VerseControl m_verseCtrl;
		private TextBox m_textBox;

		#region Setup methods
		/// ------------------------------------------------------------------------------------
		[SetUp]
		public void TestSetup()
		{
			m_ctrlOwner = new Form();
			m_verseCtrl = new VerseControl()
				{Anchor = AnchorStyles.Left | AnchorStyles.Right};
			m_ctrlOwner.Controls.Add(m_verseCtrl);
			m_textBox = new TextBox()
				{Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right, Height = 20};
			m_ctrlOwner.Controls.Add(m_textBox);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// End of a test
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[TearDown]
		public void TestTearDown()
		{
			if (Platform.IsWindows)
			{
				// m_dbScp.SimulateDropDownButtonClick(); can cause this form to hang on close on mono.
				m_ctrlOwner.Close();
			}
			m_ctrlOwner.Dispose();
		}
		#endregion

		[Test]
		public void PastedStandardReferenceInTextField()
		{
			PortableClipboard.SetText("JER 31:32");
			m_verseCtrl.GotoBookField();
			m_verseCtrl.CallProcessCmdKeyWithCtrlV();
			Assert.AreEqual("JER", m_verseCtrl.VerseRef.Book);
			Assert.AreEqual(31, m_verseCtrl.VerseRef.ChapterNum);
			Assert.AreEqual(32, m_verseCtrl.VerseRef.VerseNum);
		}

		[TestCase("2 Corinthians 3:18", "2CO 3:18")]
		[TestCase("2 Corinthians3:18", "2CO 3:18")]
		[TestCase("2 Corinthians3*18", "2CO 3:18")]
		[TestCase("2 Corinthians", "2CO 1:1")]
		[TestCase("PSA 119:176", "PSA 119:176")]
		[TestCase("MRK 1:0", "MRK 1:0")]
		[TestCase("MRK", "MRK 1:1")]
		[TestCase("LUK 2.3", "LUK 2:3")]
		[TestCase("jhn 4:5", "JHN 4:5")]
		[TestCase("ACT 99:888", "ACT 28:31")]
		[TestCase("2 Cor 3:18", "2CO 3:18")]
		[TestCase("2 Co\u0301r 3:18", "2CO 3:18")] // verify that diacritic will be skipped
		[TestCase("2 C\u00f3r 3:18", "2CO 3:18")] // verify composed character will work
		[TestCase("2 C 3:18", "2CH 3:17")] // Partial match finds 2CH, but there are only 17 verses in chapter 3
		[TestCase("2CO 3:18", "2CO 3:18")]
		[TestCase("2CO 3\u200f:\u200e18", "2CO 3:18")]
		[TestCase("2CO3:18", "2CO 3:18")]
		[TestCase("2CO 3.18", "2CO 3:18")]
		[TestCase("2CO 3", "2CO 3:1")]
		[TestCase("Songs 3:3", "SNG 3:3")]
		[TestCase("ngs 3:3", "MAT 1:1")] // shouldn't find something that start on a word boundary
		[TestCase("M 6:3", "MIC 6:3")]
		[TestCase("M 28:18", "MAT 28:18")]
		public void PastedTextGetsExpectedResult(string text, string expectedResult)
		{
			m_verseCtrl.VerseRef = new VerseRef("MAT", "1", "1", ScrVers.English);
			PortableClipboard.SetText(text);
			m_verseCtrl.GotoBookField();
			m_verseCtrl.CallProcessCmdKeyWithCtrlV();
			Assert.AreEqual(expectedResult, m_verseCtrl.VerseRef.ToString());
		}

		[Test]
		[Explicit("Test is just intended to be an easy way to see the verse control and test it manually")]
		public void ShowVerseControl()
		{
			// tried to make the default layout show all controls, but you may need to make the form taller
			// you should be able to paste a reference into the verse control and trying to paste into the
			// text box should bring up the message box in OnClick.
			var menuStrip = new MenuStrip() {Anchor = AnchorStyles.Top};
			m_ctrlOwner.Controls.Add(menuStrip);
			var menuItem = new ToolStripMenuItem("Paste", null, OnPasteClick);
			menuItem.ShortcutKeys = Keys.V | Keys.Control;
			menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { menuItem });
			m_ctrlOwner.ShowDialog();
		}

		private void OnPasteClick(object sender, EventArgs e)
		{
			MessageBox.Show("You pasted text", "Event happened");
		}
	}

	internal static class VerseControlTestHelperExt
	{
		internal static void CallProcessCmdKeyWithCtrlV(this VerseControl verseControl)
		{
			Keys keysData = Keys.Control | Keys.V;
			Message msg = Message.Create(IntPtr.Zero, 0x100, IntPtr.Zero, IntPtr.Zero);
			ReflectionHelper.GetBoolResult(verseControl, "ProcessCmdKey", new object[] {msg, keysData});
		}
	}
}
