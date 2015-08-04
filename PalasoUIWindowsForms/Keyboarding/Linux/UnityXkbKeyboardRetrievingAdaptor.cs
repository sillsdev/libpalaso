// Copyright (c) 2015 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
#if __MonoCS__
using System;
using System.Collections.Generic;
using X11.XKlavier;

namespace Palaso.UI.WindowsForms.Keyboarding.Linux
{
	/// <summary>
	/// This class handles initializing the list of keyboards on Ubuntu versions >= 13.10.
	/// Previously there were two separate keyboard icons for XKB and for IBus keyboards.
	/// Starting with 13.10 there is now just one and the list of keyboards gets stored in
	/// a different dconf key (/org/gnome/desktop/input-sources/sources). This retriever reads
	/// the list of keyboards and registers the xkb keyboards.
	/// </summary>
	[CLSCompliant(false)]
	public class UnityXkbKeyboardRetrievingAdaptor: XkbKeyboardRetrievingAdaptor
	{
		private UnityKeyboardRetrievingHelper _helper = new UnityKeyboardRetrievingHelper();

		#region Specific implementations of IKeyboardRetriever

		public override bool IsApplicable
		{
			get { return _helper.IsApplicable; }
		}

		public override void Initialize()
		{
			_adaptor = new UnityXkbKeyboardSwitchingAdaptor(_engine);
		}
		#endregion

		protected override void InitLocales()
		{
			_helper.InitKeyboards(type => type == "xkb", RegisterKeyboards);
		}

		protected override void ReinitLocales()
		{
			InitLocales();
		}

		private void RegisterKeyboards(IDictionary<string, uint> keyboards)
		{
			if (keyboards.Count <= 0)
				return;

			var configRegistry = XklConfigRegistry.Create(_engine);
			var layouts = configRegistry.Layouts;
			foreach (var kvp in layouts)
			{
				foreach (var layout in kvp.Value)
				{
					uint index;
					// Custom keyboards may omit defining a country code.  Try to survive such cases.
					string codeToMatch;
					if (layout.CountryCode == null)
						codeToMatch = layout.LanguageCode.ToLowerInvariant();
					else
						codeToMatch = layout.CountryCode.ToLowerInvariant();
					if ((keyboards.TryGetValue(layout.LayoutId, out index) && (layout.LayoutId == codeToMatch)) ||
						keyboards.TryGetValue(string.Format("{0}+{1}", codeToMatch, layout.LayoutId), out index))
					{
						AddKeyboardForLayout(layout, index, _adaptor);
					}
				}
			}
		}
	}
}
#endif
