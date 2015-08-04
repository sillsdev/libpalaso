// Copyright (c) 2015 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
#if __MonoCS__
using System;
using System.Collections.Generic;

namespace Palaso.UI.WindowsForms.Keyboarding.Linux
{
	/// <summary>
	/// This class handles initializing the list of keyboards on Ubuntu versions >= 13.10.
	/// Previously there were two separate keyboard icons for XKB and for IBus keyboards.
	/// Starting with 13.10 there is now just one and the list of keyboards gets stored in
	/// a different dconf key (/org/gnome/desktop/input-sources/sources). This retriever reads
	/// the list of keyboards and registers the ibus keyboards.
	/// </summary>
	[CLSCompliant(false)]
	public class UnityIbusKeyboardRetrievingAdaptor: IbusKeyboardRetrievingAdaptor
	{
		private UnityKeyboardRetrievingHelper _helper = new UnityKeyboardRetrievingHelper();

		#region Specific implementations of IKeyboardRetriever

		public override bool IsApplicable
		{
			get { return _helper.IsApplicable; }
		}

		public override void Initialize()
		{
			_adaptor = new UnityIbusKeyboardSwitchingAdaptor(_IBusCommunicator);
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

			var missingLayouts = new List<string>(keyboards.Keys);
			foreach (var ibusKeyboard in GetAllIBusKeyboards())
			{
				if (keyboards.ContainsKey(ibusKeyboard.LongName))
				{
					missingLayouts.Remove(ibusKeyboard.LongName);
					var keyboard = new IbusKeyboardDescription(_adaptor, ibusKeyboard,
						keyboards[ibusKeyboard.LongName]);
					KeyboardController.Manager.RegisterKeyboard(keyboard);
				}
			}
			foreach (var layout in missingLayouts)
				Console.WriteLine("Didn't find " + layout);
		}
	}
}
#endif
