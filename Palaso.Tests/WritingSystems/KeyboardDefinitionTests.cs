using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
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
			Assert.That(keyboard1 != keyboard2, Is.False);
			IKeyboardDefinition kbd1 = keyboard1;
			IKeyboardDefinition kbd2 = keyboard2;
			Assert.That(kbd1.Equals(kbd2), Is.True);

			keyboard2.Layout = "layout2";
			Assert.That(keyboard1 == keyboard2, Is.False);
			Assert.That(keyboard1 != keyboard2, Is.True);
			Assert.That(kbd1.Equals(kbd2), Is.False);

			keyboard2.Layout = "layout1";
			Assert.That(keyboard1 == keyboard2, Is.True);
			keyboard2.Locale = "en-GB";
			Assert.That(keyboard1 == keyboard2, Is.False);
			Assert.That(keyboard1 != keyboard2, Is.True);
			Assert.That(kbd1.Equals(kbd2), Is.False);

			keyboard2.Locale = "en-US";
			Assert.That(keyboard1 == keyboard2, Is.True);
			keyboard2.OperatingSystem = PlatformID.Unix;
			Assert.That(keyboard1 == keyboard2, Is.False);
			Assert.That(keyboard1 != keyboard2, Is.True);
			Assert.That(kbd1.Equals(kbd2), Is.False);

		}
	}
}
