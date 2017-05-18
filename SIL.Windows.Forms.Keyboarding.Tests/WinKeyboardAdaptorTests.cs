// Copyright (c) 2013 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.Collections.Generic;
using System.Windows.Forms;
using NUnit.Framework;
using SIL.Windows.Forms.Keyboarding.Windows;

namespace SIL.Windows.Forms.Keyboarding.Tests
{
	[TestFixture]
	[Platform(Exclude = "Linux", Reason = "Windows specific tests")]
	public class WinKeyboardAdaptorTests
	{
		private class WinKeyboardAdaptorDouble : WinKeyboardAdaptor
		{
			public short[] GetLanguages()
			{
				return Languages;
			}
		}

		[Test]
		[Category("SkipOnTeamCity")] // TeamCity build agents don't have TSF enabled
		public void Languages()
		{
			var adaptor = new WinKeyboardAdaptorDouble();

			var lcids = new List<int>();
			foreach (InputLanguage lang in InputLanguage.InstalledInputLanguages)
			{
				if (!lcids.Contains(lang.Culture.LCID))
					lcids.Add(lang.Culture.LCID);
			}
			// This test can fail if a Keyman keyboard is installed but
			// keyman is not running
			Assert.That(adaptor.GetLanguages(), Is.EquivalentTo(lcids),
				"WinKeyboardAdaptor.GetLanguages returned a different set of languages from what " +
				"we get from Windows. This can happen if a Keyman keyboard is installed in the " +
				"system but Keyman is not running.");
		}
	}
}
