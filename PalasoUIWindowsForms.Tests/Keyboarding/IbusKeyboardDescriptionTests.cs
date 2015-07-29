// Copyright (c) 2015 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
#if __MonoCS__
using System;
using System.Collections.Generic;
using NUnit.Framework;
using IBusDotNet;
using Palaso.UI.WindowsForms.Keyboarding.Linux;
using Palaso.WritingSystems;
using Palaso.Tests.Code;

namespace PalasoUIWindowsForms.Tests.Keyboarding
{
	class IbusKeyboardDescriptionIClonableGenericTests :
		IClonableGenericTests<IbusKeyboardDescription, IKeyboardDefinition>
	{
		#region IBusEngineDescImpl
		private class IBusEngineDescImpl: IBusEngineDesc
		{
			public string Name { get; set; }
			public string LongName { get; set; }
			public string Description { get; set; }
			public string Language { get; set; }
			public string License { get; set; }
			public string Author { get; set; }
			public string Icon { get; set; }
			public string Layout { get; set; }
			public string Hotkeys { get; set; }
			public uint   Rank { get; set; }
			public string Symbol { get; set; }
			public string Setup { get; set; }
			public string LayoutVariant { get; set; }
			public string LayoutOption { get; set; }
			public string Version { get; set; }
			public string TextDomain { get; set; }
		}
		#endregion

		public override IbusKeyboardDescription CreateNewClonable()
		{
			return new IbusKeyboardDescription(null, new IBusEngineDescImpl {
				Name = "foo", LongName = "long foo", Language = "se"
			}, 6);
		}

		public override string ExceptionList
		{
			get { return "|Engine|InputLanguage|IBusKeyboardEngine|"; }
		}

		public override string EqualsExceptionList
		{
			get { return "|Type|Name|OperatingSystem|IsAvailable|InternalName|SystemIndex|"; }
		}

		protected override List<ValuesToSet> DefaultValuesForTypes
		{
			get
			{
				return new List<ValuesToSet>
				{
					new ValuesToSet(true, false),
					new ValuesToSet("to be", "!(to be)"),
					new ValuesToSet(PlatformID.Win32NT, PlatformID.Unix),
					new ValuesToSet(KeyboardType.OtherIm, KeyboardType.System),
					new ValuesToSet(5, 4)
				};
			}
		}
	}
}
#endif
