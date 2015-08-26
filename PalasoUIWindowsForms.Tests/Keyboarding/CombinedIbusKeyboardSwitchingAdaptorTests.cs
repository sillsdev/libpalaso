// Copyright (c) 2015 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
#if __MonoCS__
using System;
using NUnit.Framework;
using Palaso.UI.WindowsForms.Keyboarding;
using Palaso.UI.WindowsForms.Keyboarding.Linux;
using Moq;
using IBusDotNet;
using Palaso.UI.WindowsForms.Keyboarding.InternalInterfaces;

namespace PalasoUIWindowsForms.Tests.Keyboarding
{
	[TestFixture]
	public class CombinedIbusKeyboardSwitchingAdaptorTests
	{
		class CombinedIbusKeyboardRetrievingAdaptorDouble: CombinedIbusKeyboardRetrievingAdaptor
		{
			public CombinedIbusKeyboardRetrievingAdaptorDouble() :
				base(new IbusKeyboardAdaptorTests.DoNothingIbusCommunicator())
			{
			}

			// No matter what we want this to be active
			public override bool IsApplicable
			{
				get { return true; }
			}

			protected override void InitKeyboards()
			{
			}

			protected override IBusEngineDesc[] GetIBusKeyboards()
			{
				return new IBusEngineDesc[0];
			}
		}

		private IKeyboardSwitchingAdaptor Adaptor;

		[SetUp]
		public void Setup()
		{
			var keyboardRetrievingAdaptorDouble = new CombinedIbusKeyboardRetrievingAdaptorDouble();
			KeyboardController.Manager.SetKeyboardRetrievers(new[] { keyboardRetrievingAdaptorDouble });
			Adaptor = keyboardRetrievingAdaptorDouble.Adaptor;
		}

		[TearDown]
		public void TearDown()
		{
		}

		private IbusKeyboardDescription CreateKeyboardDescription(string name, string longname,
			string language, uint systemIndex)
		{
			var engineDescMock = new Mock<IBusEngineDesc>();
			engineDescMock.Setup(x => x.Name).Returns(name);
			engineDescMock.Setup(x => x.LongName).Returns(longname);
			engineDescMock.Setup(x => x.Language).Returns(language);

			return new IbusKeyboardDescription(Adaptor, engineDescMock.Object, systemIndex);
		}

		[TestCase("us", "xkb:us::eng", Result="English - English (US)",
			TestName="NoVariant")]
		[TestCase("fr", "xkb:us::eng", Result="Danish - post (m17n)",
			TestName="FallbackToFirstKbd")]
		[TestCase("us", "us:bla", Result="English - English (US)",
			TestName="RegexLayoutAtBeginningOfKeyboardId")] // don't know if this can happen in real life
		[TestCase("us", "foo:us:bla", Result="English - English (US)",
			TestName="RegexLayoutInMiddleOfKeyboardId")] // don't know if this can happen in real life
		[TestCase("us", "foo:us", Result="English - English (US)",
			TestName="RegexLayoutAtEndOfKeyboardId")] // don't know if this can happen in real life
		[TestCase("us", "us", Result="English - English (US)",
			TestName="UnusualKeyboardId")] // don't know if this can happen in real life
		public string DefaultKeyboard(string defaultLayout, string keyboardId)
		{
			KeyboardController.Manager.RegisterKeyboard(CreateKeyboardDescription(
				"post (m17n)", "m17n:da:post", "da", 0));
			KeyboardController.Manager.RegisterKeyboard(CreateKeyboardDescription(
				"German", "xkb:de::ger", "ger", 1));
			KeyboardController.Manager.RegisterKeyboard(CreateKeyboardDescription(
				"IPA Unicode 6.2 (ver 1.4) KMN", "/usr/share/kmfl/IPA14.kmn", "", 2));
			KeyboardController.Manager.RegisterKeyboard(CreateKeyboardDescription(
				"English (US)", keyboardId, "eng", 3));

			CombinedIbusKeyboardSwitchingAdaptor.DefaultLayout = defaultLayout;

			return Adaptor.DefaultKeyboard.Name;
		}

		[Test]
		public void DefaultKeyboard_WithVariant()
		{
			KeyboardController.Manager.RegisterKeyboard(CreateKeyboardDescription(
				"post (m17n)", "m17n:da:post", "da", 0));
			KeyboardController.Manager.RegisterKeyboard(CreateKeyboardDescription(
				"English (international AltGr dead keys)", "xkb:us:altgr-intl:eng", "eng", 1));
			KeyboardController.Manager.RegisterKeyboard(CreateKeyboardDescription(
				"German", "xkb:de::ger", "ger", 2));
			KeyboardController.Manager.RegisterKeyboard(CreateKeyboardDescription(
				"IPA Unicode 6.2 (ver 1.4) KMN", "/usr/share/kmfl/IPA14.kmn", "", 3));

			CombinedIbusKeyboardSwitchingAdaptor.DefaultLayout = "us";
			CombinedIbusKeyboardSwitchingAdaptor.DefaultVariant = "altgr-intl";

			Assert.That(Adaptor.DefaultKeyboard.Name, Is.EqualTo("English - English (international AltGr dead keys)"));
		}

	}
}
#endif
