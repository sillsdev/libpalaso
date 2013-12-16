// Copyright (c) 2013 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
#if !__MonoCS__
using System.Collections.Generic;
using System.Windows.Forms;
using NUnit.Framework;
using Palaso.UI.WindowsForms.Keyboarding.Windows;

namespace PalasoUIWindowsForms.Tests.Keyboarding
{
	[TestFixture]
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
		public void Languages()
		{
			var adaptor = new WinKeyboardAdaptorDouble();

			var lcids = new List<int>();
			foreach (InputLanguage lang in InputLanguage.InstalledInputLanguages)
			{
				if (!lcids.Contains(lang.Culture.LCID))
					lcids.Add(lang.Culture.LCID);
			}
			Assert.That(adaptor.GetLanguages(), Is.EquivalentTo(lcids));
		}
	}
}
#endif