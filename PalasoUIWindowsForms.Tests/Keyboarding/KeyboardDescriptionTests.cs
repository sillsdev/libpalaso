// Copyright (c) 2013-2015, SIL International.
// Distributable under the terms of the MIT license (http://opensource.org/licenses/MIT).
using System;
using System.Collections.Generic;
using NUnit.Framework;
using Palaso.UI.WindowsForms.Keyboarding;
using Palaso.UI.WindowsForms.Keyboarding.Types;
using Palaso.WritingSystems;
using Palaso.Tests.Code;

namespace PalasoUIWindowsForms.Tests.Keyboarding
{
	[TestFixture]
	public class KeyboardDescriptionIClonableGenericTests : IClonableGenericTests<KeyboardDescription, IKeyboardDefinition>
	{
		public override KeyboardDescription CreateNewClonable()
		{
			return new KeyboardDescription("foo", "foo", "en-US", null, null);
		}

		public override string ExceptionList
		{
			get { return "|Engine|InputLanguage|"; }
		}

		public override string EqualsExceptionList
		{
			get { return "|Type|Name|OperatingSystem|IsAvailable|InternalName|"; }
		}

		protected override List<ValuesToSet> DefaultValuesForTypes
		{
			get
			{
				return new List<ValuesToSet> {
					new ValuesToSet(true, false),
					new ValuesToSet("to be", "!(to be)"),
					new ValuesToSet(PlatformID.Win32NT, PlatformID.Unix),
					new ValuesToSet(KeyboardType.OtherIm, KeyboardType.System)
				};
			}
		}

		/// <summary>
		/// This test covers the subtle possible problem that Clone() doesn't make the right class of object
		/// when applied to a variable of the interface base class. This fails, for example, if Clone()
		/// is written as a new method rather than an override.
		/// </summary>
		[Test]
		public void DefaultCloneReturnsKeyboardDescription()
		{
			IKeyboardDefinition input = new KeyboardDescription("foo", "foo", "en-US", null, null);
			var test = input.Clone();
			Assert.That(test, Is.InstanceOf<KeyboardDescription>());
		}
	}
}
