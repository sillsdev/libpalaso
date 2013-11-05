// --------------------------------------------------------------------------------------------
// <copyright from='2011' to='2011' company='SIL International'>
// 	Copyright (c) 2011, SIL International. All Rights Reserved.
//
// 	Distributable under the terms of either the Common Public License or the
// 	GNU Lesser General Public License, as specified in the LICENSING.txt file.
// </copyright>
// --------------------------------------------------------------------------------------------
#if __MonoCS__
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Palaso.UI.WindowsForms.Keyboarding;
using Palaso.UI.WindowsForms.Keyboarding.Interfaces;
using Palaso.UI.WindowsForms.Keyboarding.InternalInterfaces;
using Palaso.WritingSystems;
using IBusDotNet;

namespace Palaso.UI.WindowsForms.Keyboarding.Linux
{
	/// <summary>
	/// Class for handling ibus keyboards on Linux. Currently just a wrapper for KeyboardSwitcher.
	/// </summary>
	/// <remarks>TODO: Move functionality from KeyboardSwitcher to here.</remarks>
	public class IbusKeyboardAdaptor: IKeyboardAdaptor
	{
		private IBusConnection Connection = IBusConnectionFactory.Create();

		/// <summary>
		/// Initializes a new instance of the
		/// <see cref="T:Palaso.UI.WindowsForms.Keyboard.Linux.IbusKeyboardAdaptor"/> class.
		/// </summary>
		public IbusKeyboardAdaptor()
		{
		}

		private void InitKeyboards()
		{
			foreach (var ibusKeyboard in GetIBusKeyboards())
			{
				var keyboard = new IBusKeyboardDescription(this, ibusKeyboard);
				KeyboardController.Manager.RegisterKeyboard(keyboard);
			}
		}

		private IEnumerable<IBusEngineDesc> GetIBusKeyboards()
		{
			if (Connection == null)
				yield break;

			var ibusWrapper = new IBusDotNet.InputBusWrapper(Connection);
			object[] engines = ibusWrapper.InputBus.ListActiveEngines();
			foreach (var engine in engines)
				yield return IBusEngineDesc.GetEngineDesc(engine);
		}

		private bool SetIMEKeyboard(IBusKeyboardDescription keyboard)
		{
			try
			{
				if (Connection == null || GlobalCachedInputContext.InputContext == null)
					return false;

				// check our cached value
				if (GlobalCachedInputContext.Keyboard == keyboard)
					return true;

				InputContext context = GlobalCachedInputContext.InputContext;

				if (keyboard == null || keyboard.IBusKeyboardEngine == null)
				{
					context.Reset();
					GlobalCachedInputContext.Keyboard = null;
					context.Disable();
					return true;
				}

				context.SetEngine(keyboard.IBusKeyboardEngine.LongName);

				GlobalCachedInputContext.Keyboard = keyboard;
				return true;
			}
			catch (Exception e)
			{
				Debug.WriteLine(string.Format("Changing keyboard failed, is kfml/ibus running? {0}", e));
				return false;
			}
		}

#region IKeyboardAdaptor implementation
		/// <summary>
		/// Initialize the installed keyboards
		/// </summary>
		public void Initialize()
		{
			InitKeyboards();
		}

		public void UpdateAvailableKeyboards()
		{
			InitKeyboards();
		}

		/// <summary/>
		public void Close()
		{
			if (Connection != null)
			{
				Connection.Dispose();
				Connection = null;
			}
		}

		public bool ActivateKeyboard(IKeyboardDefinition keyboard)
		{
			return SetIMEKeyboard(keyboard as IBusKeyboardDescription);
		}

		/// <summary>
		/// Activates the keyboard
		/// </summary>
		public void ActivateKeyboard(IKeyboardDefinition keyboard,
			IKeyboardDefinition systemKeyboard)
		{
			// TODO: Remove once the other overload is implemented
			ActivateKeyboard(keyboard);

			if (systemKeyboard != null)
				systemKeyboard.Activate();
		}

		/// <summary>
		/// Deactivates the specified keyboard.
		/// </summary>
		public void DeactivateKeyboard(IKeyboardDefinition keyboard)
		{
			SetIMEKeyboard(null);
		}

		/// <summary>
		/// List of keyboard layouts that either gave an exception or other error trying to
		/// get more information. We don't have enough information for these keyboard layouts
		/// to include them in the list of installed keyboards.
		/// </summary>
		public List<IKeyboardErrorDescription> ErrorKeyboards
		{
			get
			{
				return new List<IKeyboardErrorDescription>();
			}
		}

		// Currently we expect this to only be useful on Windows.
		public IKeyboardDefinition GetKeyboardForInputLanguage(IInputLanguage inputLanguage)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// The type of keyboards this adaptor handles: system or other (like Keyman, ibus...)
		/// </summary>
		public KeyboardType Type
		{
			get { return KeyboardType.OtherIm; }
		}

		/// <summary>
		/// Implemenation is not required because this is not the primary (Type System) adapter.
		/// </summary>
		public IKeyboardDefinition DefaultKeyboard
		{
			get { throw new NotImplementedException(); }
		}

		/// <summary>
		/// Only the primary (Type=System) adapter is required to implement this method. This one makes keyboards
		/// during Initialize, but is not used to make an unavailable keyboard to match an LDML file.
		/// </summary>
		public IKeyboardDefinition CreateKeyboardDefinition(string layout, string locale)
		{
			throw new NotImplementedException();
		}

#endregion
	}
}
#endif
