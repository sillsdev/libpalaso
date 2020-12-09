// Copyright (c) 2020 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.Linq;
using IBusDotNet;
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

		/// <summary>
		/// Initializes a new instance of the
		/// <see cref="SIL.Windows.Forms.Keyboarding.Linux.GnomeShellIbusKeyboardRetrievingAdaptor"/> class.
		/// </summary>
		public GnomeShellIbusKeyboardRetrievingAdaptor()
		{
		}

		/// <summary>
		/// Initializes a new instance of the
		/// <see cref="SIL.Windows.Forms.Keyboarding.Linux.GnomeShellIbusKeyboardRetrievingAdaptor"/> class.
		/// </summary>
		/// <param name="ibusCommunicator">Ibus communicator.</param>
		/// <remarks>This overload is used in unit tests</remarks>
		protected GnomeShellIbusKeyboardRetrievingAdaptor(IIbusCommunicator ibusCommunicator): base(ibusCommunicator)
		{
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing && !IsDisposed)
			{
				// dispose managed and unmanaged objects
				Unmanaged.LibGnomeDesktopCleanup();
			}
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

		private void RegisterKeyboards(IDictionary<string, uint> installedKeyboards,
			(string type, string layout) firstKeyboard)
		{
			if (installedKeyboards.Count <= 0)
				return;

			// The list of keyboards we knew before registering the new keyboards
			var priorKeyboardsList =
				KeyboardController.Instance.Keyboards.OfType<IbusKeyboardDescription>().ToDictionary(kd => kd.Id);
			// The list of keyboards that are installed on this system but we haven't encountered yet
			var missingLayouts = new List<string>(installedKeyboards.Keys);

			// The list of keyboards we registered
			var addedKeyboards = new List<string>();

			foreach (var ibusKeyboard in GetAllIBusKeyboards())
			{
				Type type;
				string layout;

				// ENHANCE: Keyman keyboards contain the path, possibly with username. We should
				// make that more fault tolerant so that we find that keyboard in curKeyboards
				// even when running under a different user or installed globally/locally.
				var id = $"{ibusKeyboard.Language}_{ibusKeyboard.LongName}";

				if (ibusKeyboard.LongName.StartsWith("xkb"))
				{
					type = typeof(IbusXkbKeyboardDescription);
					layout = string.IsNullOrEmpty(ibusKeyboard.LayoutVariant)
							? ibusKeyboard.Layout
							: $"{ibusKeyboard.Layout}+{ibusKeyboard.LayoutVariant}";
				}
				else
				{
					type = typeof(IbusKeyboardDescription);
					layout = ibusKeyboard.LongName;
				}

				// Don't add a keyboard that is not installed locally
				if (!IsKeyboardAvailable(installedKeyboards, layout))
					continue;

				// Don't add same keyboard twice
				if (addedKeyboards.Contains(layout))
					continue;

				// we found this keyboard, so remove it from list
				missingLayouts.Remove(layout);

				if (priorKeyboardsList.TryGetValue(id, out var keyboard))
				{
					if (!keyboard.IsAvailable)
					{
						keyboard.SetIsAvailable(true);
						keyboard.IBusKeyboardEngine = ibusKeyboard;
					}

					// Remove this keyboard from list so that we don't set it inactive
					priorKeyboardsList.Remove(id);
				}
				else
				{
					// Add keyboard to keyboards list
					keyboard = Activator.CreateInstance(type, id, ibusKeyboard,
						SwitchingAdaptor) as IbusKeyboardDescription;
					KeyboardController.Instance.Keyboards.Add(keyboard);
					addedKeyboards.Add(layout);
				}

				keyboard.SystemIndex = installedKeyboards[layout];

				if (firstKeyboard.layout == layout)
					((GnomeShellIbusKeyboardSwitchingAdaptor) SwitchingAdaptor).SetDefaultKeyboard(
						keyboard);
			}

			// All keyboards that we knew before (e.g. from writing systems) and didn't encounter
			// are not available on this system
			foreach (var existingKeyboard in priorKeyboardsList.Values)
				existingKeyboard.SetIsAvailable(false);

			// output a warning for all keyboards that were in installedKeyboards but that we couldn't
			// process because they didn't have the properties we expected.
			foreach (var layout in missingLayouts)
				Console.WriteLine($"{GetType().Name}: Didn't find {layout}");
		}

		private static bool IsKeyboardAvailable(
			IDictionary<string, uint> availableKeyboards, string layout)
		{
			return !string.IsNullOrEmpty(layout) && availableKeyboards.ContainsKey(layout);
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

		protected override string GetKeyboardSetupApplication(out string arguments)
		{
			return KeyboardRetrievingHelper.GetKeyboardSetupApplication(out arguments);
		}
	}
}
