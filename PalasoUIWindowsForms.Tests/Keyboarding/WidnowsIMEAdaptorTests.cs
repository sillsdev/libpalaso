using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Palaso.UI.WindowsForms.Keyboarding;

namespace PalasoUIWindowsForms.Tests.Keyboarding
{
	[TestFixture]
	public class WidnowsIMEAdaptorTests
	{
		[Test]
		[Category("Windows IME")]
		public void WindowsIME_GetLocaleAndLayoutNames_NameHasNoDash_ReturnsName()
		{
			const string name = "keyboard";
			Assert.That(WindowsIMEAdaptor.GetLocaleName(name), Is.EqualTo(""));
			Assert.That(WindowsIMEAdaptor.GetLayoutName(name), Is.EqualTo("keyboard"));
		}

		[Test]
		[Category("Windows IME")]
		public void WindowsIME_GetLocaleAndLayoutNames_NameHasOneDash_ReturnsName()
		{
			const string name = "keyboard-US";
			Assert.That(WindowsIMEAdaptor.GetLocaleName(name), Is.EqualTo("US"));
			Assert.That(WindowsIMEAdaptor.GetLayoutName(name), Is.EqualTo("keyboard"));
		}

		[Test]
		[Category("Windows IME")]
		public void WindowsIME_GetLocaleAndLayoutNames_NameHasTwoDashes_ReturnsName()
		{
			const string name = "keyboard-GB-UK";
			Assert.That(WindowsIMEAdaptor.GetLocaleName(name), Is.EqualTo("GB-UK"));
			Assert.That(WindowsIMEAdaptor.GetLayoutName(name), Is.EqualTo("keyboard"));
		}

		[Test]
		[Category("Windows IME")]
		public void WindowsIME_GetLocaleAndLayoutNames_NameHasThreeOrMoreDashes_ReturnsName()
		{
			const string name = "keyboard (test-test)-GB-UK";
			Assert.That(WindowsIMEAdaptor.GetLocaleName(name), Is.EqualTo("GB-UK"));
			Assert.That(WindowsIMEAdaptor.GetLayoutName(name), Is.EqualTo("keyboard (test-test)"));
		}
	}
}
