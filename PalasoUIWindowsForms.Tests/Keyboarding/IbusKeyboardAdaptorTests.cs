// Copyright (c) 2013, SIL International.
// Distributable under the terms of the MIT license (http://opensource.org/licenses/MIT).
#if __MonoCS__
using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using IBusDotNet;
using X11.XKlavier;
using Palaso.UI.WindowsForms.Keyboarding;
using Palaso.UI.WindowsForms.Keyboarding.Interfaces;
using Palaso.UI.WindowsForms.Keyboarding.InternalInterfaces;
using Palaso.UI.WindowsForms.Keyboarding.Linux;
using Palaso.UI.WindowsForms.Keyboarding.Types;

namespace PalasoUIWindowsForms.Tests.Keyboarding
{
	[TestFixture]
	public class IbusKeyboardAdaptorTests
	{
		class DoNothingIbusCommunicator: IIbusCommunicator
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

			public bool IsDisposed
			{
				get { return false; }
			}

			public bool Connected
			{
				get { return true; }
			}

			public void NotifySelectionLocationAndHeight(int x, int y, int height)
			{
				throw new NotImplementedException();
			}

			public IBusConnection Connection
			{
				get
				{
					throw new NotImplementedException();
				}
			}

			public void Dispose()
			{
			}
		}

		class XkbKeyboardAdaptorDouble: XkbKeyboardAdaptor
		{
			public XkbKeyboardAdaptorDouble(IXklEngine engine): base(engine)
			{
			}

			protected override void InitLocales()
			{
			}
		}

		class IbusKeyboardAdaptorDouble: IbusKeyboardAdaptor
		{
			public IbusKeyboardAdaptorDouble(IIbusCommunicator ibusCommunicator): base(ibusCommunicator)
			{
			}

			protected override void InitKeyboards()
			{
			}
		}

		private static IbusKeyboardDescription CreateMockIbusKeyboard(IbusKeyboardAdaptor ibusKeyboardAdapter,
			string name, string language, string layout)
		{
			var engineDescMock = new Mock<IBusEngineDesc>();
			engineDescMock.Setup(x => x.Name).Returns(name);
			engineDescMock.Setup(x => x.Language).Returns(language);
			engineDescMock.Setup(x => x.Layout).Returns(layout);
			var keyboard = new IbusKeyboardDescription(ibusKeyboardAdapter, engineDescMock.Object);
			KeyboardController.Manager.RegisterKeyboard(keyboard);
			return keyboard;
		}

		private static XkbKeyboardDescription CreateMockXkbKeyboard(string name, string layout, string locale,
			string layoutName, int group, XkbKeyboardAdaptor adapter)
		{
			var keyboard = new XkbKeyboardDescription(name, layout, locale,
				new InputLanguageWrapper(locale, IntPtr.Zero, layoutName), adapter, group);
			KeyboardController.Manager.RegisterKeyboard(keyboard);
			return keyboard;
		}

		[TearDown]
		public void TearDown()
		{
			GlobalCachedInputContext.Keyboard = null;
			GlobalCachedInputContext.Clear();
		}

		[Test]
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
			var ibusKeyboardAdapter = new IbusKeyboardAdaptorDouble(new DoNothingIbusCommunicator());
			var xklEngineMock = new Mock<IXklEngine>();
			var xkbKeyboardAdapter = new XkbKeyboardAdaptorDouble(xklEngineMock.Object);
			KeyboardController.Manager.SetKeyboardAdaptors(new IKeyboardAdaptor[] { xkbKeyboardAdapter, ibusKeyboardAdapter});

			var ibusKeyboard = CreateMockIbusKeyboard(ibusKeyboardAdapter, name, language, layout);
			var deKeyboard = CreateMockXkbKeyboard("German - German (Germany)", "de", "de-DE", "German", DeKeyboardGroup, xkbKeyboardAdapter);
			CreateMockXkbKeyboard("English (US) - English (United States)", "us", "en-US", "English", EnKeyboardGroup, xkbKeyboardAdapter);
			CreateMockXkbKeyboard("French - French (France)", "fr", "fr-FR", "French", FrKeyboardGroup, xkbKeyboardAdapter);

			deKeyboard.Activate();

			// Exercise
			ibusKeyboard.Activate();

			// Verify
			xklEngineMock.Verify(x => x.SetGroup(layout == "fr" ? FrKeyboardGroup : EnKeyboardGroup),
				string.Format("Switching to the ibus keyboard should activate the {0} xkb keyboard.",
					layout == "fr" ? "French" : "English"));
		}
	}
}
#endif
