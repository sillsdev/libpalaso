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

		#region Setup methods
		/// ------------------------------------------------------------------------------------
		[SetUp]
		public void TestSetup()
		{
			m_ctrlOwner = new Form();
			m_verseCtrl = new VerseControl();
			m_ctrlOwner.Controls.Add(m_verseCtrl);
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

		[TestCase("uiBook_KeyDown")]
		[TestCase("uiChapter_KeyDown")]
		[TestCase("uiVerse_KeyDown")]
		public void PastedStandardReferenceInTextField(string methodName)
		{
			PortableClipboard.SetText("JER 31:32");
			m_verseCtrl.GotoBookField();
			var keyEvent = new KeyEventArgs(Keys.V | Keys.Control);
			m_verseCtrl.CallKeyDown(methodName, keyEvent);
			Assert.AreEqual("JER", m_verseCtrl.VerseRef.Book);
			Assert.AreEqual(31, m_verseCtrl.VerseRef.ChapterNum);
			Assert.AreEqual(32, m_verseCtrl.VerseRef.VerseNum);
			Assert.IsTrue(keyEvent.SuppressKeyPress);
		}

		[TestCase("2 Corinthians 3:18", "2CO 3:18", true)]
		[TestCase("2 Corinthians3:18", "2CO 3:18", true)]
		[TestCase("2 Corinthians3*18", "2CO 3:18", true)]
		[TestCase("PSA 119:176", "PSA 119:176", true)]
		[TestCase("MRK 1:0", "MRK 1:0", true)]
		[TestCase("LUK 2.3", "LUK 2:3", true)]
		[TestCase("jhn 4:5", "JHN 4:5", true)]
		[TestCase("ACT 99:888", "ACT 28:31", true)]
		[TestCase("2 Cor 3:18", "2CO 3:18", true)]
		[TestCase("2 C 3:18", "MAT 1:1", false)]
		[TestCase("2CO 3:18", "2CO 3:18", true)]
		[TestCase("2CO3:18", "2CO 3:18", true)]
		[TestCase("2CO 3.18", "2CO 3:18", true)]
		public void PastedTextGetsExpectedResult(string text, string expectedResult, bool isValid)
		{
			m_verseCtrl.VerseRef = new VerseRef("MAT", "1", "1", ScrVers.English);
			PortableClipboard.SetText(text);
			m_verseCtrl.GotoBookField();
			var keyEvent = new KeyEventArgs(Keys.V | Keys.Control);
			m_verseCtrl.CallKeyDown("uiBook_KeyDown", keyEvent);
			Assert.AreEqual(isValid, keyEvent.SuppressKeyPress);
			Assert.AreEqual(expectedResult, m_verseCtrl.VerseRef.ToString());
		}

		[Test]
		[Explicit("Test is just intended to be an easy way to see the verse control and test it manually")]
		public void ShowVerseControl()
		{
			m_ctrlOwner.ShowDialog();
		}
	}

	internal static class VerseControlTestHelperExt
	{
		internal static void CallKeyDown(this VerseControl verseControl, string methodName, KeyEventArgs eventArgs)
		{
			ReflectionHelper.CallMethod(verseControl, methodName, null, eventArgs);
		}
	}
}
