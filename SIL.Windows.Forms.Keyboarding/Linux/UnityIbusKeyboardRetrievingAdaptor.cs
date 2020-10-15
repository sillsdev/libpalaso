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
		internal readonly UnityKeyboardRetrievingHelper _helper = new UnityKeyboardRetrievingHelper();

		#region Specific implementations of IKeyboardRetriever

		public override bool IsApplicable => _helper.IsApplicable && !Platform.IsGnomeShell && !Platform.IsCinnamon;

		public override void Initialize()
		{
			SwitchingAdaptor = CreateSwitchingAdaptor();
			KeyboardRetrievingHelper.AddIbusVersionAsErrorReportProperty();
			InitKeyboards();
		}

		protected virtual IKeyboardSwitchingAdaptor CreateSwitchingAdaptor()
		{
			return new UnityIbusKeyboardSwitchingAdaptor(IbusCommunicator);
		}

		protected override string GetKeyboardSetupApplication(out string arguments)
		{
			var program = UnityKeyboardRetrievingHelper.GetKeyboardSetupApplication(out arguments);
			return string.IsNullOrEmpty(program) ? base.GetKeyboardSetupApplication(out arguments) : program;
		}
		#endregion

		protected override void InitKeyboards()
		{
			_helper.InitKeyboards(type => type != "xkb", RegisterKeyboards);
		}

		private void RegisterKeyboards(IDictionary<string, uint> keyboards)
		{
			if (keyboards.Count <= 0)
				return;

			Dictionary<string, IbusKeyboardDescription> curKeyboards = KeyboardController.Instance.Keyboards.OfType<IbusKeyboardDescription>().ToDictionary(kd => kd.Id);
			var missingLayouts = new List<string>(keyboards.Keys);
			foreach (var ibusKeyboard in GetAllIBusKeyboards())
			{
				if (keyboards.ContainsKey(ibusKeyboard.LongName))
				{
					missingLayouts.Remove(ibusKeyboard.LongName);
					string id = string.Format("{0}_{1}", ibusKeyboard.Language, ibusKeyboard.LongName);
					IbusKeyboardDescription keyboard;
					if (curKeyboards.TryGetValue(id, out keyboard))
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
			}

			foreach (IbusKeyboardDescription existingKeyboard in curKeyboards.Values)
				existingKeyboard.SetIsAvailable(false);

			foreach (var layout in missingLayouts)
				Console.WriteLine("{0}: Didn't find {1}", GetType().Name, layout);
		}
	}
}
