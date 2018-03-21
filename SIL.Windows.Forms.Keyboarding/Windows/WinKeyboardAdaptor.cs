// Copyright (c) 2013-2018 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using SIL.Keyboarding;

namespace SIL.Windows.Forms.Keyboarding.Windows
{
	/// <summary>
	/// Class for handling Windows system keyboards
	/// </summary>
	internal class WinKeyboardAdaptor : IKeyboardRetrievingAdaptor
	{
		public bool IsApplicable => true;

		public IKeyboardSwitchingAdaptor SwitchingAdaptor { get; private set; }

		public WinKeyboardAdaptor()
		{
			SwitchingAdaptor = new WindowsKeyboardSwitchingAdapter();
		}
		
		private static string GetDisplayName(string layout, string locale)
		{
			return $"{layout} - {locale}";
		}

		public void UpdateAvailableKeyboards()
		{
			var curKeyboards = KeyboardController.Instance.Keyboards.OfType<WinKeyboardDescription>().ToDictionary(kd => kd.Id);

			foreach (InputLanguage inputLanguage in InputLanguage.InstalledInputLanguages)
			{
				var keyboardId = $"{inputLanguage.Culture.Name}_{inputLanguage.LayoutName}";
				var localizedKeyboardName = GetDisplayName(inputLanguage.LayoutName, inputLanguage.Culture.DisplayName);
				WinKeyboardDescription existingKeyboard;
				if (curKeyboards.TryGetValue(keyboardId, out existingKeyboard))
				{
					if (!existingKeyboard.IsAvailable)
					{
						existingKeyboard.SetIsAvailable(true);
						existingKeyboard.SetLocalizedName(localizedKeyboardName);
					}
					curKeyboards.Remove(keyboardId);
				}
				else
				{
					// Prevent a keyboard with this id from being registered again.
					// Potentially, id's are duplicated. e.g. A Keyman keyboard linked to a windows one.
					// For now we simply ignore this second registration.
					// A future enhancement would be to include knowledge of the driver in the Keyboard definition so
					// we could choose the best one to register.
					KeyboardDescription keyboard;
					if (!KeyboardController.Instance.Keyboards.TryGet(keyboardId, out keyboard))
					{
						KeyboardController.Instance.Keyboards.Add(
							new WinKeyboardDescription(keyboardId, localizedKeyboardName, inputLanguage.LayoutName, inputLanguage.Culture.Name, true,
								new InputLanguageWrapper(inputLanguage), this));
					}
				}
			}

			// Set each unhanandled keyboard to unavailable
			foreach (var existingKeyboard in curKeyboards.Values)
			{
				existingKeyboard.SetIsAvailable(false);
			}
		}

		#region IKeyboardRetrievingAdaptor Members

		/// <summary>
		/// The type of keyboards this adaptor handles: system or other (like Keyman, ibus...)
		/// </summary>
		public KeyboardAdaptorType Type => KeyboardAdaptorType.System;

		public void Initialize()
		{
			UpdateAvailableKeyboards();
		}

		/// <summary>
		/// Creates and returns a keyboard definition object based on the ID.
		/// Note that this method is used when we do NOT have a matching available keyboard.
		/// Therefore we can presume that the created one is NOT available.
		/// </summary>
		public KeyboardDescription CreateKeyboardDefinition(string id)
		{
			string layout, locale;
			KeyboardController.GetLayoutAndLocaleFromLanguageId(id, out layout, out locale);

			string cultureName;
			var inputLanguage = WinKeyboardUtils.GetInputLanguage(locale, layout, out cultureName);

			return new WinKeyboardDescription(id, GetDisplayName(layout, cultureName), layout, locale, false, inputLanguage, this,
				new TfInputProcessorProfile());
		}

		public bool CanHandleFormat(KeyboardFormat format)
		{
			switch (format)
			{
				case KeyboardFormat.Msklc:
				case KeyboardFormat.Unknown:
					return true;
			}
			return false;
		}

		public Action GetKeyboardSetupAction()
		{
			return () =>
			{
				using (Process.Start(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "control.exe"),
					"input.dll")) {}
			};
		}

		public bool IsSecondaryKeyboardSetupApplication => false;
		#endregion

		#region IDisposable & Co. implementation
		// Region last reviewed: never

		/// <summary>
		/// See if the object has been disposed.
		/// </summary>
		public bool IsDisposed { get; private set; }

		/// <summary>
		/// Finalizer, in case client doesn't dispose it.
		/// Force Dispose(false) if not already called (i.e. m_isDisposed is true)
		/// </summary>
		/// <remarks>
		/// In case some clients forget to dispose it directly.
		/// </remarks>
		~WinKeyboardAdaptor()
		{
			Dispose(false);
			// The base class finalizer is called automatically.
		}

		/// <summary>
		///
		/// </summary>
		/// <remarks>Must not be virtual.</remarks>
		public void Dispose()
		{
			Dispose(true);
			// This object will be cleaned up by the Dispose method.
			// Therefore, you should call GC.SupressFinalize to
			// take this object off the finalization queue
			// and prevent finalization code for this object
			// from executing a second time.
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Executes in two distinct scenarios.
		///
		/// 1. If disposing is true, the method has been called directly
		/// or indirectly by a user's code via the Dispose method.
		/// Both managed and unmanaged resources can be disposed.
		///
		/// 2. If disposing is false, the method has been called by the
		/// runtime from inside the finalizer and you should not reference (access)
		/// other managed objects, as they already have been garbage collected.
		/// Only unmanaged resources can be disposed.
		/// </summary>
		/// <param name="disposing"></param>
		/// <remarks>
		/// If any exceptions are thrown, that is fine.
		/// If the method is being done in a finalizer, it will be ignored.
		/// If it is thrown by client code calling Dispose,
		/// it needs to be handled by fixing the bug.
		///
		/// If subclasses override this method, they should call the base implementation.
		/// </remarks>
		protected virtual void Dispose(bool disposing)
		{
			Debug.WriteLineIf(!disposing, "****************** " + GetType().Name + " 'disposing' is false. ******************");
			// Must not be run more than once.
			(SwitchingAdaptor as IDisposable)?.Dispose();

			// Dispose unmanaged resources here, whether disposing is true or false.

			IsDisposed = true;
		}

		#endregion IDisposable & Co. implementation
	}
}
