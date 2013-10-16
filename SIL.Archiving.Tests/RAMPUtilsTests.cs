using System.IO;
using NUnit.Framework;

namespace SIL.Archiving.Tests
{
	/// <summary>
	/// These tests require RAMP to be installed locally.
	/// </summary>
	[TestFixture]
	[Category("SkipOnTeamCity")]
	class RAMPUtilsTests
	{
		/// ------------------------------------------------------------------------------------
		[Test]
		public void GetRAMPFileLocation_RAMPInstalled_ReturnsFileLocation()
		{
			var fileName = RAMPUtils.GetExeFileLocation();
			Assert.IsTrue(File.Exists(fileName), "RAMP executable file not found.");
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void GetLanguageFileLocation_RAMPInstalled_ReturnsFileLocation()
		{
			var fileName = RAMPUtils.GetLanguageFileLocation();
			Assert.IsTrue(File.Exists(fileName), "RAMP language file not found.");
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void GetLanguageList_RAMPInstalled_ReturnsDictionary()
		{
			var langs = RAMPUtils.GetLanguageList();
			Assert.NotNull(langs);
			Assert.Greater(langs.Count, 7000, "There should be over 7000 language entries.");
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void GetLanguageName_English_ReturnsEnglish()
		{
			var langName = RAMPUtils.GetLanguageName("eng");
			Assert.AreEqual(langName, "English");
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void GetLanguageName_Gibberish_ReturnsNull()
		{
			var langName = RAMPUtils.GetLanguageName("z23");
			Assert.IsNull(langName);
		}
	}
}
