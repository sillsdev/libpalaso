using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Palaso.Tests.Code;
using Palaso.WritingSystems;

namespace Palaso.Tests.WritingSystems
{
	[TestFixture]
	public class KeyboardDefinitionTests
	{
		[Test]
		public void KeyboardsDontEqualNull()
		{
			var keyboard1 = new KeyboardDefinition() { Layout = "layout1", Locale = "en-US", OperatingSystem = PlatformID.MacOSX };
			KeyboardDefinition keyboard2 = null;
			Assert.That(keyboard1 == keyboard2, Is.False);
			Assert.That(keyboard2 == keyboard1, Is.False);
			Assert.That(keyboard1 != keyboard2, Is.True);
			Assert.That(keyboard2 != keyboard1, Is.True);
		}

		[Test]
		public void KeyboardEqualityDependsOnExpectedProperties()
		{
			var keyboard1 = new KeyboardDefinition() { Layout = "layout1", Locale = "en-US", OperatingSystem = PlatformID.MacOSX };
			var keyboard2 = new KeyboardDefinition() { Layout = "layout1", Locale = "en-US", OperatingSystem = PlatformID.MacOSX };
			Assert.That(keyboard1 == keyboard2, Is.True);
			Assert.That(keyboard1.GetHashCode() == keyboard2.GetHashCode());
			Assert.That(keyboard1 != keyboard2, Is.False);
			IKeyboardDefinition kbd1 = keyboard1;
			IKeyboardDefinition kbd2 = keyboard2;
			Assert.That(kbd1.Equals(kbd2), Is.True);

			keyboard2.Layout = "layout2";
			Assert.That(keyboard1 == keyboard2, Is.False);
			Assert.That(keyboard1.GetHashCode() != keyboard2.GetHashCode());
			Assert.That(keyboard1 != keyboard2, Is.True);
			Assert.That(kbd1.Equals(kbd2), Is.False);

			keyboard2.Layout = "layout1";
			Assert.That(keyboard1 == keyboard2, Is.True);
			keyboard2.Locale = "en-GB";
			Assert.That(keyboard1 == keyboard2, Is.False);
			Assert.That(keyboard1.GetHashCode() != keyboard2.GetHashCode());
			Assert.That(keyboard1 != keyboard2, Is.True);
			Assert.That(kbd1.Equals(kbd2), Is.False);

			keyboard2.Locale = "en-US";
			Assert.That(keyboard1 == keyboard2, Is.True);
			keyboard2.OperatingSystem = PlatformID.Unix;
			Assert.That(keyboard1 == keyboard2, Is.False);
			Assert.That(keyboard1.GetHashCode() != keyboard2.GetHashCode());
			Assert.That(keyboard1 != keyboard2, Is.True);
			Assert.That(kbd1.Equals(kbd2), Is.False);
		}

		[Test]
		public void DefaultKeyboardingController_IsExpectedClass()
		{
			Assert.That(Keyboarding.Controller, Is.InstanceOf<DefaultKeyboardController>());
		}

		[Test]
		public void Activate_CallsControllerActivate()
		{
			var keyboard1 = new KeyboardDefinition() { Layout = "layout1", Locale = "en-US", OperatingSystem = PlatformID.MacOSX };
			var controller = new MockController();
			Keyboarding.Controller = controller;
			keyboard1.Activate();
			Keyboarding.Controller = null;
			Assert.That(controller.ActivatedKeyboard, Is.EqualTo(keyboard1));
		}

		[Test]
		public void IsAvailable_ChecksControllerList()
		{
			var keyboard1 = new KeyboardDefinition() { Layout = "layout1", Locale = "en-US", OperatingSystem = PlatformID.MacOSX };
			var keyboard2 = new KeyboardDefinition() { Layout = "layout2", Locale = "en-US", OperatingSystem = PlatformID.MacOSX };
			var controller = new MockController();
			var availableKeyboards = new List<IKeyboardDefinition>();
			availableKeyboards.Add(keyboard1);
			controller.AllAvailableKeyboards = availableKeyboards;
			Keyboarding.Controller = controller;
			Assert.That(keyboard1.IsAvailable, Is.True);
			Assert.That(keyboard2.IsAvailable, Is.False);
			Keyboarding.Controller = null;
		}

		class MockController : IKeyboardController
		{
			public IKeyboardDefinition ActivatedKeyboard;
			public void Activate(IKeyboardDefinition keyboard)
			{
				ActivatedKeyboard = keyboard;
			}

			public IEnumerable<IKeyboardDefinition> AllAvailableKeyboards { get; set; }
			public IKeyboardDefinition DefaultForWritingSystem(IWritingSystemDefinition ws)
			{
				throw new NotImplementedException();
			}
		}
	}

	public class KeyboardDefinitionIClonableGenericTests : IClonableGenericTests<KeyboardDefinition>
	{
		public override KeyboardDefinition CreateNewClonable()
		{
			return new KeyboardDefinition();
		}

		public override string ExceptionList
		{
			get { return ""; }
		}

		protected override List<ValuesToSet> DefaultValuesForTypes
		{
			get
			{
				return new List<ValuesToSet>
					{
						new ValuesToSet(false, true),
						new ValuesToSet("to be", "!(to be)"),
						new ValuesToSet(PlatformID.Win32NT, PlatformID.Unix)
					};
			}
		}
	}
}
