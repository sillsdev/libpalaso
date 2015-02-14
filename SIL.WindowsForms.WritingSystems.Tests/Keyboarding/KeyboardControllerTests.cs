// Copyright (c) 2013, SIL International.
// Distributable under the terms of the MIT license (http://opensource.org/licenses/MIT).

#if __MonoCS__
using SIL.WritingSystems.WindowsForms.Keyboarding.Linux;
#endif
using System;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using NUnit.Framework;
using SIL.WindowsForms.WritingSystems.Keyboarding;
using SIL.WritingSystems;

namespace SIL.WindowsForms.WritingSystems.Tests.Keyboarding
{
	[TestFixture]
	public class KeyboardControllerTests
	{
		[SetUp]
		public void Setup()
		{
			KeyboardController.Initialize();
		}

		[TearDown]
		public void TearDown()
		{
			KeyboardController.Shutdown();
		}

		[Test]
		public void CreateKeyboardDefinition_ExistingKeyboard_ReturnsReference()
		{
			var expectedKeyboard = new KeyboardDescription("en-US_foo", "foo - English (US)", "foo", "en-US", true,
				KeyboardController.Instance.Adaptors[0]);
			KeyboardController.Instance.Keyboards.Add(expectedKeyboard);
			IKeyboardDefinition keyboard = Keyboard.Controller.CreateKeyboardDefinition("en-US_foo", KeyboardFormat.Unknown, null);
			Assert.That(keyboard, Is.SameAs(expectedKeyboard));
			Assert.That(keyboard, Is.TypeOf<KeyboardDescription>());
			Assert.That(keyboard.Name, Is.EqualTo("foo - English (US)"));
		}

		[Test]
		public void GetKeyboard_FromInputLanguage_ExistingKeyboardReturnsKeyboard()
		{
			IKeyboardDefinition keyboard = Keyboard.Controller.CreateKeyboardDefinition("en-US_foo", KeyboardFormat.Unknown, null);

			var inputLanguage = new InputLanguageWrapper(new CultureInfo("en-US"), IntPtr.Zero, "foo");
			Assert.That(Keyboard.Controller.GetKeyboard(inputLanguage), Is.EqualTo(keyboard));
		}

		[Test]
		public void GetKeyboard_FromNewPalasoId_ExistingKeyboardReturnsKeyboard()
		{
			IKeyboardDefinition keyboard = Keyboard.Controller.CreateKeyboardDefinition("en-US_foo", KeyboardFormat.Unknown, null);

			Assert.That(Keyboard.Controller.GetKeyboard("en-US_foo"), Is.EqualTo(keyboard));
		}

		[Test]
		public void GetKeyboard_FromNewPalasoId_NonExistingKeyboard()
		{
			Assert.That(Keyboard.Controller.GetKeyboard("en-US_glop"), Is.EqualTo(KeyboardController.NullKeyboard));
		}

		[Test]
		public void GetKeyboard_FromOldParatextId_ExistingKeyboardReturnsKeyboard()
		{
			// This is the case of a keyboard that the old Palaso system was incapable of supporting.
			IKeyboardDefinition keyboardFooBoo = Keyboard.Controller.CreateKeyboardDefinition("az-Latn-AZ_foo", KeyboardFormat.Unknown, null);
			IKeyboardDefinition keyboard = Keyboard.Controller.CreateKeyboardDefinition("en-US_foo", KeyboardFormat.Unknown, null);

			Assert.That(Keyboard.Controller.GetKeyboard("foo|en-US"), Is.EqualTo(keyboard));
			Assert.That(Keyboard.Controller.GetKeyboard("foo|az-Latn-AZ"), Is.EqualTo(keyboardFooBoo));
		}

		[Test]
		public void GetKeyboard_FromOldParatextId_NonExistingKeyboard()
		{
			Assert.That(Keyboard.Controller.GetKeyboard("glop|en-US"), Is.EqualTo(KeyboardController.NullKeyboard));
		}

		[Test]
		public void GetKeyboard_FromOldPalasoId_ExistingKeyboardReturnsKeyboard()
		{
			// This demonstrates the case of a keyboard that the old Palaso system was incapable of supporting.
			// There's no way to reference this keyboard, or is there???
			IKeyboardDefinition keyboardFooBoo = Keyboard.Controller.CreateKeyboardDefinition("az-Latn-AZ_foo", KeyboardFormat.Unknown, null);
			IKeyboardDefinition keyboardFooF = Keyboard.Controller.CreateKeyboardDefinition("en-US_foo-az", KeyboardFormat.Unknown, null);
			IKeyboardDefinition keyboardFoo = Keyboard.Controller.CreateKeyboardDefinition("en-US_foo", KeyboardFormat.Unknown, null);

			Assert.That(Keyboard.Controller.GetKeyboard("foo-az-en-US"), Is.EqualTo(keyboardFooF));
			Assert.That(Keyboard.Controller.GetKeyboard("foo-en-US"), Is.EqualTo(keyboardFoo));
			Assert.That(Keyboard.Controller.GetKeyboard("foo-az-Latn-AZ"), Is.EqualTo(keyboardFooBoo));
		}

		[Test]
		public void GetKeyboard_FromOldPalasoId_NonExistingKeyboard()
		{
			Assert.That(Keyboard.Controller.GetKeyboard("glop-en-US"), Is.EqualTo(KeyboardController.NullKeyboard));
		}

		[Test]
		public void GetKeyboard_FromInputLanguage_NonExistingKeyboard()
		{
			var inputLanguage = new InputLanguageWrapper(new CultureInfo("en-US"), IntPtr.Zero, "foo");
			Assert.That(Keyboard.Controller.GetKeyboard(inputLanguage), Is.EqualTo(KeyboardController.NullKeyboard));
		}

		[Test]
		public void DefaultForWritingSystem_NullInput_ReturnsSystemDefault()
		{
			Assert.That(Keyboard.Controller.DefaultForWritingSystem(null),
				Is.EqualTo(KeyboardController.Instance.Adaptors[0].DefaultKeyboard));
		}

		[Test]
		public void DefaultForWritingSystem_NoLegacyKeyboardSet_ReturnsSystemDefault()
		{
			var ws = new WritingSystemDefinition();
			Assert.That(Keyboard.Controller.DefaultForWritingSystem(ws),
				Is.EqualTo(KeyboardController.Instance.Adaptors[0].DefaultKeyboard));
		}

		[Test]
		public void DefaultForWritingSystem_OldPalasoWinIMEKeyboard()
		{
			var expectedKeyboard = new KeyboardDescription("en-US_foo", "foo - English (US)", "foo", "en-US", true,
				KeyboardController.Instance.Adaptors[0]);
			KeyboardController.Instance.Keyboards.Add(expectedKeyboard);

			// Palaso sets the keyboard property for Windows system keyboards to <layoutname>-<locale>
			var ws = new WritingSystemDefinition {Keyboard = "foo-en-US"};
			Assert.That(Keyboard.Controller.DefaultForWritingSystem(ws), Is.EqualTo(expectedKeyboard));
		}

		[Test]
		public void DefaultForWritingSystem_OldPalasoKeymanKeyboard()
		{
			KeyboardDescription expectedKeyboard;
			if (!KeyboardController.Instance.Keyboards.TryGet("IPA Unicode 1.1.1", out expectedKeyboard))
			{
				expectedKeyboard = new KeyboardDescription("IPA Unicode 1.1.1", "IPA Unicode 1.1.1 - English (US)", "IPA Unicode 1.1.1", string.Empty, true,
					KeyboardController.Instance.Adaptors[0]);
				KeyboardController.Instance.Keyboards.Add(expectedKeyboard);
			}

			// Palaso sets the keyboard property for Keyman keyboards to <layoutname>
			var ws = new WritingSystemDefinition {Keyboard = "IPA Unicode 1.1.1"};
			Assert.That(Keyboard.Controller.DefaultForWritingSystem(ws), Is.EqualTo(expectedKeyboard));
		}

		[Test]
		public void DefaultForWritingSystem_OldPalasoIbusKeyboard()
		{
#if __MonoCS__
			// For this test on Linux we only use the XkbKeyboardAdaptor and simulate an available
			// IBus keyboard. This is necessary because otherwise the test might return an
			// installed Danish IBus keyboard (m17n:da:post) instead of our expected dummy one.
			KeyboardController.Initialize(new XkbKeyboardAdaptor());
#endif
			var expectedKeyboard = new KeyboardDescription("m17n:da:post", "m17n:da:post - English (US)", "m17n:da:post", string.Empty, true,
				KeyboardController.Instance.Adaptors[0]);
			KeyboardController.Instance.Keyboards.Add(expectedKeyboard);

			// Palaso sets the keyboard property for Ibus keyboards to <ibus longname>
			var ws = new WritingSystemDefinition {Keyboard = "m17n:da:post"};
			Assert.That(Keyboard.Controller.DefaultForWritingSystem(ws), Is.EqualTo(expectedKeyboard));
		}

		[Test]
		public void DefaultForWritingSystem_OldFwSystemKeyboard()
		{
			// 0x001C is Albanian (see http://msdn.microsoft.com/en-us/goglobal/bb896001.aspx).
			// Make sure it's not installed on current system.
			if (InputLanguage.InstalledInputLanguages.Cast<InputLanguage>().Any(lang => lang.Culture.LCID == 0x041C))
				Assert.Ignore("Input language 'Albanian (Albania)' is installed on current system. Can't run this test.");

			var inputLanguage = new InputLanguageWrapper("sq-AL", IntPtr.Zero, "US");
			var expectedKeyboard = new KeyboardDescription("sq-AL_US", "US - Albanian (Albania)", "US", "sq-AL", true,
				KeyboardController.Instance.Adaptors[0]) {InputLanguage = inputLanguage};
			KeyboardController.Instance.Keyboards.Add(expectedKeyboard);

			// FieldWorks sets the WindowsLcid property for System keyboards to <lcid>
			var ws = new WritingSystemDefinition {WindowsLcid = 0x041C.ToString(CultureInfo.InvariantCulture)};
			Assert.That(Keyboard.Controller.DefaultForWritingSystem(ws), Is.EqualTo(expectedKeyboard));
		}

		[Test]
		public void DefaultForWritingSystem_OldNonexistingFwSystemKeyboard()
		{
			// 0x001C is Albanian. Make sure it's not installed on current system.
			if (InputLanguage.InstalledInputLanguages.Cast<InputLanguage>().Any(lang => lang.Culture.LCID == 0x041C))
				Assert.Ignore("Input language 'Albanian (Albania)' is installed on current system. Can't run this test.");

			// FieldWorks sets the WindowsLcid property for System keyboards to <lcid>
			var ws = new WritingSystemDefinition {WindowsLcid = 0x041C.ToString(CultureInfo.InvariantCulture)};
			Assert.That(Keyboard.Controller.DefaultForWritingSystem(ws),
				Is.EqualTo(KeyboardController.Instance.Adaptors[0].DefaultKeyboard));
		}

		[Test]
		public void DefaultForWritingSystem_OldFwKeymanKeyboard()
		{
			var expectedKeyboard = new KeyboardDescription("en-US_IPA Unicode 1.1.1", "IPA Unicode 1.1.1 - English (US)", "IPA Unicode 1.1.1", "en-US", true,
				KeyboardController.Instance.Adaptors[0]);
			KeyboardController.Instance.Keyboards.Add(expectedKeyboard);

			// FieldWorks sets the keyboard property for Keyman keyboards to <layoutname> and WindowsLcid to <lcid>
			var ws = new WritingSystemDefinition {Keyboard = "IPA Unicode 1.1.1", WindowsLcid = 0x409.ToString(CultureInfo.InvariantCulture)};
			Assert.That(Keyboard.Controller.DefaultForWritingSystem(ws), Is.EqualTo(expectedKeyboard));
		}
	}
}
