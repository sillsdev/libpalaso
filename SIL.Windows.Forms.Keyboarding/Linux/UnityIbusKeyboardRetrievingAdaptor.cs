// Copyright (c) 2015 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;
using System.Linq;
using SIL.PlatformUtilities;

namespace SIL.Windows.Forms.Keyboarding.Linux
{
	/// <summary>
	/// This class handles initializing the list of keyboards on Ubuntu versions >= 13.10.
	/// Previously there were two separate keyboard icons for XKB and for IBus keyboards.
	/// Starting with 13.10 there is now just one and the list of keyboards gets stored in
	/// a different dconf key (/org/gnome/desktop/input-sources/sources). This retriever reads
	/// the list of keyboards and registers the ibus keyboards.
	/// </summary>
	public class UnityIbusKeyboardRetrievingAdaptor : IbusKeyboardRetrievingAdaptor
	{
		private readonly GnomeKeyboardRetrievingHelper _helper = new GnomeKeyboardRetrievingHelper();

		#region Specific implementations of IKeyboardRetriever

		public override bool IsApplicable => _helper.IsApplicable && !Platform.IsGnomeShell;

		public override void Initialize()
		{
			SwitchingAdaptor = new UnityIbusKeyboardSwitchingAdaptor(IbusCommunicator);
			KeyboardRetrievingHelper.AddIbusVersionAsErrorReportProperty();
			InitKeyboards();
		}

		protected override string GetKeyboardSetupApplication(out string arguments)
		{
			var program = GnomeKeyboardRetrievingHelper.GetKeyboardSetupApplication(out arguments);
			return string.IsNullOrEmpty(program) ? base.GetKeyboardSetupApplication(out arguments) : program;
		}
		#endregion

		protected override void InitKeyboards()
		{
			_helper.InitKeyboards(type => type != "xkb", RegisterKeyboards);
		}

		private void RegisterKeyboards(IDictionary<string, uint> keyboards, (string, string) firstKeyboard)
		{
			if (keyboards.Count <= 0)
				return;

			var curKeyboards = KeyboardController.Instance.Keyboards.OfType<IbusKeyboardDescription>().ToDictionary(kd => kd.Id);
			var missingLayouts = new List<string>(keyboards.Keys);
			foreach (var ibusKeyboard in GetAllIBusKeyboards())
			{
				if (!keyboards.ContainsKey(ibusKeyboard.LongName))
					continue;

				missingLayouts.Remove(ibusKeyboard.LongName);
				var id = $"{ibusKeyboard.Language}_{ibusKeyboard.LongName}";
				if (curKeyboards.TryGetValue(id, out var keyboard))
				{
					if (!keyboard.IsAvailable)
					{
						keyboard.SetIsAvailable(true);
						keyboard.IBusKeyboardEngine = ibusKeyboard;
					}
					curKeyboards.Remove(id);
				}
				else
				{
					keyboard = new IbusKeyboardDescription(id, ibusKeyboard, SwitchingAdaptor);
					KeyboardController.Instance.Keyboards.Add(keyboard);
				}
				keyboard.SystemIndex = keyboards[ibusKeyboard.LongName];
			}

			foreach (var existingKeyboard in curKeyboards.Values)
				existingKeyboard.SetIsAvailable(false);

			foreach (var layout in missingLayouts)
				Console.WriteLine($"{GetType().Name}: Didn't find {layout}");
		}
	}
}
