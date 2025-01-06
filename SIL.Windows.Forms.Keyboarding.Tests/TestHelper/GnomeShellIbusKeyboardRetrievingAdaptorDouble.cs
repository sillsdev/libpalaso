// Copyright (c) 2024, SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using IBusDotNet;
using SIL.Reflection;
using SIL.Windows.Forms.Keyboarding.Linux;

namespace SIL.Windows.Forms.Keyboarding.Tests.TestHelper
{
	internal class GnomeShellIbusKeyboardRetrievingAdaptorDouble : GnomeShellIbusKeyboardRetrievingAdaptor
	{
		private readonly IBusEngineDesc[] _ibusKeyboards;

		public GnomeShellIbusKeyboardRetrievingAdaptorDouble(string[] keyboards,
			IBusEngineDesc[] ibusKeyboards)
			: base(new IbusKeyboardAdaptorTests.DoNothingIbusCommunicator())
		{
			_ibusKeyboards = ibusKeyboards;
			ReflectionHelper.SetField(this, "_helper",
				new GnomeKeyboardRetrievingHelperDouble(keyboards));
		}

		// No matter what we want this to be active
		public override bool IsApplicable => true;

		protected override IBusEngineDesc[] GetAllIBusKeyboards() => _ibusKeyboards;
	}
}