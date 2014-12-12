using System;
using System.Collections.Generic;
using NUnit.Framework;
using Palaso.TestUtilities;

namespace SIL.WritingSystems.Tests
{
	[TestFixture]
	public class DefaultKeyboardDefinitionTests
	{
		[Test]
		public void KeyboardsDontEqualNull()
		{
			var keyboard1 = new DefaultKeyboardDefinition(KeyboardType.System, "layout1", "en-US");
			Assert.That(keyboard1.Equals(null), Is.False);
		}

		[Test]
		public void KeyboardEqualityDependsOnExpectedProperties()
		{
			var keyboard1 = new DefaultKeyboardDefinition(KeyboardType.System, "layout1", "en-US");
			var keyboard2 = new DefaultKeyboardDefinition(KeyboardType.System, "layout1", "en-US");
			Assert.That(keyboard1.Equals(keyboard2), Is.True);
			Assert.That(keyboard1.GetHashCode() == keyboard2.GetHashCode());

			keyboard2 = new DefaultKeyboardDefinition(KeyboardType.System, "layout2", "en-US");
			Assert.That(keyboard1.Equals(keyboard2), Is.False);
			Assert.That(keyboard1.GetHashCode() != keyboard2.GetHashCode());

			keyboard2 = new DefaultKeyboardDefinition(KeyboardType.System, "layout1", "en-US");
			Assert.That(keyboard1.Equals(keyboard2), Is.True);
			keyboard2 = new DefaultKeyboardDefinition(KeyboardType.System, "layout1", "en-GB");
			Assert.That(keyboard1.Equals(keyboard2), Is.False);
			Assert.That(keyboard1.GetHashCode() != keyboard2.GetHashCode());

			keyboard2 = new DefaultKeyboardDefinition(KeyboardType.System, "layout1", "en-US");
			Assert.That(keyboard1.Equals(keyboard2), Is.True);
		}

		[Test]
		public void DefaultKeyboardingController_IsExpectedClass()
		{
			Assert.That(Keyboard.Controller, Is.InstanceOf<DefaultKeyboardController>());
		}
	}

	public class DefaultKeyboardDefinitionCloneableTests : CloneableTests<DefaultKeyboardDefinition, IKeyboardDefinition>
	{
		public override DefaultKeyboardDefinition CreateNewClonable()
		{
			return new DefaultKeyboardDefinition(KeyboardType.System, string.Empty, string.Empty);
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
				return "|_isAvailable|_type|";
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
