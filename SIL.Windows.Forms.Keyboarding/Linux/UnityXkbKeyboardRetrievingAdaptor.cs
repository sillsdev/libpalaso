// Copyright (c) 2015 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;
using System.Linq;
using X11.XKlavier;

namespace SIL.Windows.Forms.Keyboarding.Linux
{
	/// <summary>
	/// This class handles initializing the list of keyboards on Ubuntu versions >= 13.10.
	/// Previously there were two separate keyboard icons for XKB and for IBus keyboards.
	/// Starting with 13.10 there is now just one and the list of keyboards gets stored in
	/// a different dconf key (/org/gnome/desktop/input-sources/sources). This retriever reads
	/// the list of keyboards and registers the xkb keyboards.
	/// </summary>
	public class UnityXkbKeyboardRetrievingAdaptor : XkbKeyboardRetrievingAdaptor
	{
		private readonly UnityKeyboardRetrievingHelper _helper = new UnityKeyboardRetrievingHelper();

		#region Specific implementations of IKeyboardRetriever

		public override bool IsApplicable => _helper.IsApplicable;

		public override void Initialize()
		{
			SwitchingAdaptor = new UnityXkbKeyboardSwitchingAdaptor(XklEngine);
			InitLocales();
		}

		protected override string GetKeyboardSetupApplication(out string arguments)
		{
			var program = _helper.GetKeyboardSetupApplication(out arguments);
			return string.IsNullOrEmpty(program) ? base.GetKeyboardSetupApplication(out arguments) : program;
		}
		#endregion

		protected override void InitLocales()
		{
			_helper.InitKeyboards(type => type == "xkb", RegisterKeyboards);
		}

		private void RegisterKeyboards(IDictionary<string, uint> keyboards)
		{
			if (keyboards.Count <= 0)
				return;

			var configRegistry = XklConfigRegistry.Create(XklEngine);
			var layouts = configRegistry.Layouts;
			var curKeyboards = KeyboardController.Instance.Keyboards.OfType<XkbKeyboardDescription>().ToDictionary(kd => kd.Id);
			foreach (var kvp in layouts)
			{
				foreach (var layout in kvp.Value)
				{
					// Custom keyboards may omit defining a country code.  Try to survive such cases.
					var codeToMatch = layout.CountryCode == null ? layout.LanguageCode.ToLowerInvariant() : layout.CountryCode.ToLowerInvariant();
					if ((keyboards.TryGetValue(layout.LayoutId, out var index) && (layout.LayoutId == codeToMatch)) ||
						keyboards.TryGetValue($"{codeToMatch}+{layout.LayoutId}", out index))
					{
						AddKeyboardForLayout(curKeyboards, layout, index, SwitchingAdaptor);
					}
				}
			}

			foreach (var existingKeyboard in curKeyboards.Values)
				existingKeyboard.SetIsAvailable(false);
		}
	}
}
