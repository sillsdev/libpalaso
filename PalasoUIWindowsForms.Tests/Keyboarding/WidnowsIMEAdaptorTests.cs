using NUnit.Framework;
using Palaso.UI.WindowsForms.Keyboarding;

namespace PalasoUIWindowsForms.Tests.Keyboarding
{
	[TestFixture]
	public class WidnowsIMEAdaptorTests
	{
		[Test]
		[Category("Windows IME")]
		public void GetLayoutAndLocaleNames_NullName_ReturnsNull()
		{
			string layoutName, localeName;
			WindowsIMEAdaptor.GetLayoutAndLocaleNames(null, out layoutName, out localeName);
			Assert.IsNull(layoutName);
			Assert.IsNull(localeName);
		}

		[Test]
		[Category("Windows IME")]
		public void GetLayoutAndLocaleNames_EmptyName_ReturnsEmpty()
		{
			string layoutName, localeName;
			WindowsIMEAdaptor.GetLayoutAndLocaleNames("", out layoutName, out localeName);
			Assert.AreEqual("", layoutName);
			Assert.AreEqual("", localeName);
		}

		[Test]
		[Category("Windows IME")]
		public void GetLayoutAndLocaleNames_SinglePart_ReturnsAsOnlyLayoutName()
		{
			string layoutName, localeName;
			WindowsIMEAdaptor.GetLayoutAndLocaleNames("keyboard", out layoutName, out localeName);
			Assert.AreEqual("keyboard", layoutName);
			Assert.AreEqual("", localeName);
		}

		[Test]
		[Category("Windows IME")]
		public void GetLayoutAndLocaleNames_OldPalasoIDWith1Dash_SplitsAtDash()
		{
			string layoutName, localeName;
			WindowsIMEAdaptor.GetLayoutAndLocaleNames("English-en", out layoutName, out localeName);
			Assert.AreEqual("English", layoutName);
			Assert.AreEqual("en", localeName);
		}

		[Test]
		[Category("Windows IME")]
		public void GetLayoutAndLocaleNames_OldPalasoIDWith2Dashes_SplitsAtFirstDash()
		{
			string layoutName, localeName;
			WindowsIMEAdaptor.GetLayoutAndLocaleNames("Latin-az-Latn", out layoutName, out localeName);
			Assert.AreEqual("Latin", layoutName);
			Assert.AreEqual("az-Latn", localeName);
		}

		[Test]
		[Category("Windows IME")]
		public void GetLayoutAndLocaleNames_OldPalasoIDWith3Dashes_SplitsAtSecondDash()
		{
			string layoutName, localeName;
			WindowsIMEAdaptor.GetLayoutAndLocaleNames("Latin-az-Latn-AZ", out layoutName, out localeName);
			// The following is not what we really want, but it's what the lame implementation of Palaso used to do.
			Assert.AreEqual("Latin-az", layoutName);
			Assert.AreEqual("Latn-AZ", localeName);
		}

		[Test]
		[Category("Windows IME")]
		public void GetLayoutAndLocaleNames_OldPalasoID_GetsCorrectValues()
		{
			string layoutName, localeName;
			WindowsIMEAdaptor.GetLayoutAndLocaleNames("Latin-az-Latn-AZ", out layoutName, out localeName);
			// The following is not what we really want, but it's what the lame implementation of Palaso used to do.
			Assert.AreEqual("Latin-az", layoutName);
			Assert.AreEqual("Latn-AZ", localeName);
		}

		[Test]
		[Category("Windows IME")]
		public void GetLayoutAndLocaleNames_ParatextIDFix_GetsCorrectValues()
		{
			string layoutName, localeName;
			WindowsIMEAdaptor.GetLayoutAndLocaleNames("Latin|az-Latn-AZ", out layoutName, out localeName);
			Assert.AreEqual("Latin", layoutName);
			Assert.AreEqual("az-Latn-AZ", localeName);
		}

		[Test]
		[Category("Windows IME")]
		public void GetLayoutAndLocaleNames_NewPalasoKeyboardingID_GetsCorrectValues()
		{
			string layoutName, localeName;
			WindowsIMEAdaptor.GetLayoutAndLocaleNames("az-Latn-AZ_Latin", out layoutName, out localeName);
			Assert.AreEqual("Latin", layoutName);
			Assert.AreEqual("az-Latn-AZ", localeName);
		}
	}
}
