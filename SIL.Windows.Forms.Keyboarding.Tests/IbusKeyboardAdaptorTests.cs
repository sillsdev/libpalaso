// Copyright (c) 2013, SIL International.
// Distributable under the terms of the MIT license (http://opensource.org/licenses/MIT).

using System;
using Moq;
using NUnit.Framework;
using IBusDotNet;
using X11.XKlavier;
using SIL.Windows.Forms.Keyboarding.Linux;

#pragma warning disable 0067

namespace SIL.Windows.Forms.Keyboarding.Tests
{
	[TestFixture]
	[Platform(Include = "Linux", Reason = "Linux specific tests")]
	public class IbusKeyboardAdaptorTests
	{
		public class DoNothingIbusCommunicator: IIbusCommunicator
		{
			public event Action<object> CommitText;

			public event Action<object, int> UpdatePreeditText;

			public event Action<int, int> DeleteSurroundingText;

			public event Action HidePreeditText;

			public event Action<int, int, int> KeyEvent;

			public void FocusIn()
			{
				GlobalCachedInputContext.InputContext = new Mock<IInputContext>().Object;
			}

			public void FocusOut()
			{
				GlobalCachedInputContext.Clear();
			}

			public void SetCursorLocation(int x, int y, int width, int height)
			{
				throw new NotImplementedException();
			}

			public bool ProcessKeyEvent(int keySym, int scanCode, System.Windows.Forms.Keys state)
			{
				throw new NotImplementedException();
			}

			public void Reset()
			{
				throw new NotImplementedException();
			}

			public void CreateInputContext()
			{
			}

			public bool IsDisposed => false;

			public bool Connected => true;

			public void NotifySelectionLocationAndHeight(int x, int y, int height)
			{
				throw new NotImplementedException();
			}

			public IBusConnection Connection => throw new NotImplementedException();

			public void Dispose()
			{
			}
		}

		class XkbKeyboardRetrievingAdaptorDouble : XkbKeyboardRetrievingAdaptor
		{
			public XkbKeyboardRetrievingAdaptorDouble(IXklEngine engine): base(engine)
			{
			}

			public override bool IsApplicable =>
				// No matter what we want this to be active
				true;

			protected override void InitLocales()
			{
			}
		}

		class IbusKeyboardRetrievingAdaptorDouble : IbusKeyboardRetrievingAdaptor
		{
			public IbusKeyboardRetrievingAdaptorDouble(IIbusCommunicator ibusCommunicator): base(ibusCommunicator)
			{
			}

			public override bool IsApplicable =>
				// No matter what we want this to be active
				true;

			protected override void InitKeyboards()
			{
			}

			protected override IBusEngineDesc[] GetIBusKeyboards()
			{
				return new IBusEngineDesc[0];
			}
		}

		private static IbusKeyboardDescription CreateMockIbusKeyboard(IKeyboardSwitchingAdaptor ibusKeyboardAdapter,
			string name, string language, string layout)
		{
			var engineDescMock = new Mock<IBusEngineDesc>();
			engineDescMock.Setup(x => x.Name).Returns(name);
			engineDescMock.Setup(x => x.Language).Returns(language);
			engineDescMock.Setup(x => x.Layout).Returns(layout);
			var keyboard = new IbusKeyboardDescription($"{language}_{name}", engineDescMock.Object, ibusKeyboardAdapter) {SystemIndex = 3};
			KeyboardController.Instance.Keyboards.Add(keyboard);
			return keyboard;
		}

		private static XkbKeyboardDescription CreateMockXkbKeyboard(string name, string layout, string locale,
			string layoutName, int group, IKeyboardSwitchingAdaptor adapter)
		{
			var keyboard = new XkbKeyboardDescription($"{layout}_{locale}", name, layout, locale, true,
				new InputLanguageWrapper(locale, IntPtr.Zero, layoutName), adapter, group);
			KeyboardController.Instance.Keyboards.Add(keyboard);
			return keyboard;
		}

		[TearDown]
		public void TearDown()
		{
			KeyboardController.Shutdown();
			GlobalCachedInputContext.Keyboard = null;
			GlobalCachedInputContext.Clear();
		}

		[TestCase("Pinyin", "zh", "us", TestName="Pinyin")]
		[TestCase("IPA Unicode 6.2 (ver 1.3) KMN", "x040F", "en", TestName="IPA")]
		[TestCase("Some hypothetical ibus keyboard", "de", "fr", TestName="FrenchParentLayout")]
		public void ActivatingIbusKeyboardAlsoActivatesXkbKeyboard(
			string name, string language, string layout)
		{
			const int DeKeyboardGroup = 1;
			const int EnKeyboardGroup = 2;
			const int FrKeyboardGroup = 3;

			// Setup
			var ibusKeyboardAdapter = new IbusKeyboardRetrievingAdaptorDouble(new DoNothingIbusCommunicator());
			var xklEngineMock = new Mock<IXklEngine>();
			var xkbKeyboardAdapter = new XkbKeyboardRetrievingAdaptorDouble(xklEngineMock.Object);
			KeyboardController.Initialize(xkbKeyboardAdapter, ibusKeyboardAdapter);

			var ibusKeyboard = CreateMockIbusKeyboard(ibusKeyboardAdapter.SwitchingAdaptor, name, language, layout);
			var deKeyboard = CreateMockXkbKeyboard("German - German (Germany)", "de", "de-DE", "German", DeKeyboardGroup, xkbKeyboardAdapter.SwitchingAdaptor);
			CreateMockXkbKeyboard("English (US) - English (United States)", "us", "en-US", "English", EnKeyboardGroup, xkbKeyboardAdapter.SwitchingAdaptor);
			CreateMockXkbKeyboard("French - French (France)", "fr", "fr-FR", "French", FrKeyboardGroup, xkbKeyboardAdapter.SwitchingAdaptor);

			deKeyboard.Activate();

			// Exercise
			ibusKeyboard.Activate();

			// Verify
			xklEngineMock.Verify(x => x.SetGroup(layout == "fr" ? FrKeyboardGroup : EnKeyboardGroup),
				$"Switching to the ibus keyboard should activate the {(layout == "fr" ? "French" : "English")} xkb keyboard.");
		}
	}
}
#pragma warning restore 0067
