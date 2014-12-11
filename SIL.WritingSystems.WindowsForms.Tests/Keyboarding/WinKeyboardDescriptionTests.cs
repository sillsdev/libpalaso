// Copyright (c) 2014 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
#if !__MonoCS__
using System;
using System.Collections.Generic;
using Microsoft.Unmanaged.TSF;
using Palaso.TestUtilities;
using SIL.WritingSystems.WindowsForms.Keyboarding.Windows;



namespace SIL.WritingSystems.WindowsForms.Tests.Keyboarding
{
	internal class TestWrapper
	{
		public class WinKeyboardDescriptionIClonableGenericTests :
			IClonableGenericTests<WinKeyboardDescription, IKeyboardDefinition>
		{
			public override WinKeyboardDescription CreateNewClonable()
			{
				return new WinKeyboardDescription("en", "US", new WinKeyboardAdaptor());
			}

			public override string ExceptionList
			{
				get { return "|Engine|InputLanguage|"; }
			}

			public override string EqualsExceptionList
			{
				get
				{
					return "|Type|Name|OperatingSystem|IsAvailable|InternalName|InternalLocalizedName|" +
						"ConversionMode|SentenceMode|WindowHandle|InputProcessorProfile|";
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
						new ValuesToSet(KeyboardType.System, KeyboardType.OtherIm),
						new ValuesToSet(1, 100),
						new ValuesToSet((IntPtr)1, (IntPtr)2),
						new ValuesToSet(new TfInputProcessorProfile { Flags = TfIppFlags.Enabled | TfIppFlags.Active},
							new TfInputProcessorProfile { Flags = TfIppFlags.Enabled })
					};
				}
			}
		}
	}
}
#endif
