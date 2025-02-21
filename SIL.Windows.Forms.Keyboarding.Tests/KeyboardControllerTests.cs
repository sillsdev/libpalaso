// Copyright (c) 2025 SIL Global
// Distributable under the terms of the MIT license (http://opensource.org/licenses/MIT).

using System;
using System.Globalization;
using NUnit.Framework;
using SIL.Keyboarding;
using Moq;

namespace SIL.Windows.Forms.Keyboarding.Tests
{
	[TestFixture]
	public class KeyboardControllerTests
	{
		private Mock<IKeyboardRetrievingAdaptor> _mockKeyboardAdaptor = new Mock<IKeyboardRetrievingAdaptor>();

		[SetUp]
		public void Setup()
		{
			// To avoid OS specific behavior in these tests we mock up our own keyboard adaptor returning appropriate values to enable these tests
			_mockKeyboardAdaptor.Setup(k => k.IsApplicable).Returns(true);
			_mockKeyboardAdaptor.Setup(k => k.Type).Returns(KeyboardAdaptorType.System);
			_mockKeyboardAdaptor.Setup(k => k.CanHandleFormat(KeyboardFormat.Unknown)).Returns(true);
			_mockKeyboardAdaptor.Setup(k => k.CreateKeyboardDefinition(It.IsAny<string>())).Returns<string>(
				id =>
				{
					var idParts = id.Split('_');
					var mockKd = new MockKeyboardDescription(id, idParts[0], idParts[1]);
					return mockKd;
				});
			KeyboardController.Initialize(_mockKeyboardAdaptor.Object);
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
				KeyboardController.Instance.Adaptors[KeyboardAdaptorType.System].SwitchingAdaptor);
			KeyboardController.Instance.Keyboards.Add(expectedKeyboard);
			IKeyboardDefinition keyboard = Keyboard.Controller.CreateKeyboard("en-US_foo", KeyboardFormat.Unknown, null);
			Assert.That(keyboard, Is.SameAs(expectedKeyboard));
			Assert.That(keyboard, Is.TypeOf<KeyboardDescription>());
			Assert.That(keyboard.Name, Is.EqualTo("foo - English (US)"));
		}

		[Test]
		public void GetKeyboard_FromInputLanguage_ExistingKeyboardReturnsKeyboard()
		{
			IKeyboardDefinition keyboard = Keyboard.Controller.CreateKeyboard("en-US_foo", KeyboardFormat.Unknown, null);

			var inputLanguage = new InputLanguageWrapper(new CultureInfo("en-US"), IntPtr.Zero, "foo");
			Assert.That(Keyboard.Controller.GetKeyboard(inputLanguage), Is.EqualTo(keyboard));
		}

		[Test]
		public void GetKeyboard_FromNewPalasoId_ExistingKeyboardReturnsKeyboard()
		{
			IKeyboardDefinition keyboard = Keyboard.Controller.CreateKeyboard("en-US_foo", KeyboardFormat.Unknown, null);

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
			IKeyboardDefinition keyboardFooBoo = Keyboard.Controller.CreateKeyboard("az-Latn-AZ_foo", KeyboardFormat.Unknown, null);
			IKeyboardDefinition keyboard = Keyboard.Controller.CreateKeyboard("en-US_foo", KeyboardFormat.Unknown, null);

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
			IKeyboardDefinition keyboardFooBoo = Keyboard.Controller.CreateKeyboard("az-Latn-AZ_foo", KeyboardFormat.Unknown, null);
			IKeyboardDefinition keyboardFooF = Keyboard.Controller.CreateKeyboard("en-US_foo-az", KeyboardFormat.Unknown, null);
			IKeyboardDefinition keyboardFoo = Keyboard.Controller.CreateKeyboard("en-US_foo", KeyboardFormat.Unknown, null);

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

		/// <summary>
		/// This mock provides sufficient IKeyboardDefinition for testing the KeyboardController
		/// </summary>
		private class MockKeyboardDescription : KeyboardDescription
		{
			public MockKeyboardDescription(string id, string locale, string layout) : base(id, id + "mockKbDescription", layout, locale, true, null)
			{
				InputLanguage = new InputLanguageWrapper(locale, IntPtr.Zero, layout);
			}
		}
	}
}
