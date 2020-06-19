// Copyright (c) 2020 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using IBusDotNet;
using SIL.Extensions;
using SIL.PlatformUtilities;

namespace SIL.Windows.Forms.Keyboarding.Linux
{
	/// <summary>
	/// This class handles initializing the list of keyboards on Ubuntu versions >= 18.04.
	/// The keyboard retrieving part is identical to previous versions but switching keyboards
	/// changed with 18.04.
	/// </summary>
	public class GnomeShellIbusKeyboardRetrievingAdaptor: IbusKeyboardRetrievingAdaptor
	{
		private readonly GnomeKeyboardRetrievingHelper _helper = new GnomeKeyboardRetrievingHelper();

		public GnomeShellIbusKeyboardRetrievingAdaptor()
		{
		}

		/// <summary>
		/// Initializes a new instance of the
		/// <see cref="SIL.Windows.Forms.Keyboarding.Linux.GnomeShellIbusKeyboardRetrievingAdaptor"/> class.
		/// Used in unit tests
		/// </summary>
		/// <param name="ibusCommunicator">Ibus communicator.</param>
		public GnomeShellIbusKeyboardRetrievingAdaptor(IIbusCommunicator ibusCommunicator): base(ibusCommunicator)
		{
		}

		public override bool IsApplicable => _helper.IsApplicable && Platform.IsGnomeShell;

		public override KeyboardAdaptorType Type => KeyboardAdaptorType.System | KeyboardAdaptorType.OtherIm;

		public override void Initialize()
		{
			// We come here twice, for both KeyboardType.System and KeyboardType.OtherIm.
			// We want to create a new switching adaptor only the first time otherwise we're
			// getting into trouble
			if (SwitchingAdaptor != null)
				return;

			SwitchingAdaptor = new GnomeShellIbusKeyboardSwitchingAdaptor(IbusCommunicator);
			KeyboardRetrievingHelper.AddIbusVersionAsErrorReportProperty();
			InitKeyboards();
		}

		protected override void InitKeyboards()
		{
			_helper.InitKeyboards(type => true, RegisterKeyboards);
		}

		private void RegisterKeyboards(IDictionary<string, uint> keyboards, (string type, string layout) firstKeyboard)
		{
			if (keyboards.Count <= 0)
				return;

			var curKeyboards =
				KeyboardController.Instance.Keyboards.OfType<IbusKeyboardDescription>().ToDictionary(kd => kd.Id);
			var missingLayouts = new List<string>(keyboards.Keys);
			var addedKeyboards = new List<string>();
			foreach (var ibusKeyboard in GetAllIBusKeyboards())
			{
				var layout = ibusKeyboard.LongName;
				var type = typeof(IbusKeyboardDescription);

				// ENHANCE: Keyman keyboards contain the path, possibly with username. We should
				// make that more fault tolerant so that we find that keyboard in curKeyboards
				// even when running under a different user or installed globally/locally.
				var id = $"{ibusKeyboard.Language}_{ibusKeyboard.LongName}";

				if (ibusKeyboard.LongName.StartsWith("xkb"))
				{
					type = typeof(IbusXkbKeyboardDescription);
					if (string.IsNullOrEmpty(ibusKeyboard.LayoutVariant)
						&& keyboards.ContainsKey(ibusKeyboard.Layout))
						layout = ibusKeyboard.Layout;
					else if (!string.IsNullOrEmpty(ibusKeyboard.LayoutVariant)
						&& keyboards.ContainsKey($"{ibusKeyboard.Layout}+{ibusKeyboard.LayoutVariant}"))
						layout = $"{ibusKeyboard.Layout}+{ibusKeyboard.LayoutVariant}";
					else
						continue;
				}
				else if (!keyboards.ContainsKey(ibusKeyboard.LongName))
					continue;

				if (addedKeyboards.Contains(layout))
					continue;

				missingLayouts.Remove(layout);

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
					keyboard = Activator.CreateInstance(type, id, ibusKeyboard,
						SwitchingAdaptor) as IbusKeyboardDescription;
					KeyboardController.Instance.Keyboards.Add(keyboard);
					addedKeyboards.Add(layout);
				}
				keyboard.SystemIndex = keyboards[layout];

				if (firstKeyboard.layout == layout)
					((GnomeShellIbusKeyboardSwitchingAdaptor)SwitchingAdaptor).SetDefaultKeyboard(keyboard);
			}

			foreach (var existingKeyboard in curKeyboards.Values)
				existingKeyboard.SetIsAvailable(false);

			foreach (var layout in missingLayouts)
				Console.WriteLine($"{GetType().Name}: Didn't find {layout}");
		}

		protected override IBusEngineDesc[] GetAllIBusKeyboards()
		{
			var keyboards = new List<IBusEngineDesc>(base.GetAllIBusKeyboards());

			var gnomeXkbInfo = new GnomeXkbInfo();
			var xkbKeyboards = gnomeXkbInfo.GetAllLayouts();
			foreach (var xkbKeyboard in xkbKeyboards)
			{
				gnomeXkbInfo.GetLayoutInfo(xkbKeyboard, out var displayName, out var shortName,
					out var xkbLayout, out var xkbVariant);
				keyboards.Add(new XkbIbusEngineDesc {
					LongName = $"xkb:{xkbKeyboard}",
					Description = displayName,
					Name = displayName,
					Language = shortName,
					Layout = xkbLayout,
					LayoutVariant = xkbVariant
				});
			}
			return keyboards.ToArray();
		}
	}
}