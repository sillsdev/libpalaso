using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Palaso.TestUtilities;
using SIL.WritingSystems.WritingSystems;

namespace SIL.WritingSystems.Tests.WritingSystems
{
	[TestFixture]
	public class DefaultKeyboardDefinitionTests
	{
		[Test]
		public void KeyboardsDontEqualNull()
		{
			var keyboard1 = new DefaultKeyboardDefinition() { Layout = "layout1", Locale = "en-US", OperatingSystem = PlatformID.MacOSX };
			DefaultKeyboardDefinition keyboard2 = null;
			Assert.That(keyboard1 == keyboard2, Is.False);
			Assert.That(keyboard2 == keyboard1, Is.False);
			Assert.That(keyboard1 != keyboard2, Is.True);
			Assert.That(keyboard2 != keyboard1, Is.True);
		}

		[Test]
		public void KeyboardEqualityDependsOnExpectedProperties()
		{
			var keyboard1 = new DefaultKeyboardDefinition() { Layout = "layout1", Locale = "en-US", OperatingSystem = PlatformID.MacOSX };
			var keyboard2 = new DefaultKeyboardDefinition() { Layout = "layout1", Locale = "en-US", OperatingSystem = PlatformID.MacOSX };
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
			Assert.That(keyboard1 == keyboard2, Is.True, "Equality should NOT depend on OS");
			Assert.That(keyboard1.GetHashCode() == keyboard2.GetHashCode(), "Hash should NOT depend on OS");
			Assert.That(keyboard1 != keyboard2, Is.False);
			Assert.That(kbd1.Equals(kbd2), Is.True);
		}

		[Test]
		public void DefaultKeyboardingController_IsExpectedClass()
		{
			Assert.That(Keyboard.Controller, Is.InstanceOf<DefaultKeyboardController>());
		}
	}

	public class DefaultKeyboardDefinitionIClonableGenericTests : IClonableGenericTests<DefaultKeyboardDefinition, IKeyboardDefinition>
	{
		public override DefaultKeyboardDefinition CreateNewClonable()
		{
			return new DefaultKeyboardDefinition();
		}

		public override string ExceptionList
		{
			get { return ""; }
		}

		public override string EqualsExceptionList
		{
			get
			{
				// Additional properties we do NOT want to consider for equality
				return "|Type|OperatingSystem|IsAvailable|";
			}
		}

		protected override List<ValuesToSet> DefaultValuesForTypes
		{
			get
			{
				return new List<ValuesToSet>
					{
						new ValuesToSet(false, true),
						new ValuesToSet("to be", "!(to be)"),
						new ValuesToSet(PlatformID.Win32NT, PlatformID.Unix),
						new ValuesToSet(KeyboardType.System, KeyboardType.OtherIm)
					};
			}
		}
	}
}
