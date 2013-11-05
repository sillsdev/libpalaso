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
using System.Windows.Forms;
using Palaso.UI.WindowsForms.Keyboarding;
using Palaso.UI.WindowsForms.Keyboarding.Interfaces;
using Palaso.UI.WindowsForms.Keyboarding.InternalInterfaces;
using Palaso.WritingSystems;
using IBusDotNet;
using Icu;

namespace Palaso.UI.WindowsForms.Keyboarding.Linux
{
	/// <summary>
	/// Class for handling ibus keyboards on Linux. Currently just a wrapper for KeyboardSwitcher.
	/// </summary>
	/// <remarks>TODO: Move functionality from KeyboardSwitcher to here.</remarks>
	public class IbusKeyboardAdaptor: IKeyboardAdaptor
	{
		// TODO: Integrate keyboard switcher functionality from FW into this class and delete
		// the KeyboardSwitcher stub here
		private class KeyboardSwitcher
		{
			private IBusConnection Connection = IBusConnectionFactory.Create();

			#region IDisposable implementation
			#if DEBUG
			~KeyboardSwitcher()
			{
				Dispose(false);
			}
			#endif
			public void Dispose()
			{
				Dispose(true);
				GC.SuppressFinalize(this);
			}

			protected virtual void Dispose(bool fDisposing)
			{
				System.Diagnostics.Debug.WriteLineIf(!fDisposing, "****** Missing Dispose() call for " + GetType().Name + ". ****** ");
				if (fDisposing)
				{
					if (Connection != null)
						Connection.Dispose();
				}
				Connection = null;
			}
			#endregion

			#region IIMEKeyboardSwitcher implementation

			/// <summary>
			/// get/set the keyboard of the current focused inputContext
			/// get returns String.Empty if not connected to ibus
			/// </summary>
			public string IMEKeyboard
			{
				get
				{
					if (Connection == null || GlobalCachedInputContext.InputContext == null)
						return String.Empty;

					InputContext context = GlobalCachedInputContext.InputContext;

					object engine = context.GetEngine();
					IBusEngineDesc engineDesc = IBusEngineDesc.GetEngineDesc(engine);
					return engineDesc.LongName;
				}

				set
				{
					SetIMEKeyboard(value);
				}
			}

			internal bool SetIMEKeyboard(string val)
			{
				try
				{
					if (Connection == null || GlobalCachedInputContext.InputContext == null)
						return false;

					// check our cached value
					if (GlobalCachedInputContext.KeyboardName == val)
						return true;

					InputContext context = GlobalCachedInputContext.InputContext;

					if (String.IsNullOrEmpty(val))
					{
						context.Reset();
						GlobalCachedInputContext.KeyboardName = val;
						context.Disable();
						return true;
					}

					var ibusWrapper = new IBusDotNet.InputBusWrapper(Connection);
					object[] engines = ibusWrapper.InputBus.ListActiveEngines();

					foreach (object engine in engines)
					{
						IBusEngineDesc engineDesc = IBusEngineDesc.GetEngineDesc(engine);
						if (val == FormatKeyboardIdentifier(engineDesc))
						{
							context.SetEngine(engineDesc.LongName);
							break;
						}
					}

					GlobalCachedInputContext.KeyboardName = val;
					return true;
				}
				catch (Exception e)
				{
					System.Diagnostics.Debug.WriteLine(String.Format("KeyboardSwitcher changing keyboard failed, is kfml/ibus running? {0}", e));
					return false;
				}
			}

			/// <summary>Get Ibus keyboard at given index</summary>
			public string GetKeyboardName(int index)
			{
				if (Connection == null)
					return String.Empty;
				var ibusWrapper = new IBusDotNet.InputBusWrapper(Connection);
				object[] engines = ibusWrapper.InputBus.ListActiveEngines();
				IBusEngineDesc engineDesc = IBusEngineDesc.GetEngineDesc(engines[index]);

				return FormatKeyboardIdentifier(engineDesc);
			}

			/// <summary>
			/// Produce IBus keyboard identifier which is simular to the actual ibus switcher menu.
			/// </summary>
			internal string FormatKeyboardIdentifier(IBusEngineDesc engineDesc)
			{
				string id = engineDesc.Language;
				string languageName = string.IsNullOrEmpty(id) ? "Other Language" :
					new Locale(id).GetDisplayName(new Locale(Application.CurrentCulture.TwoLetterISOLanguageName));
				return String.Format("{0} - {1}", languageName, engineDesc.Name);
			}

			/// <summary>number of ibus keyboards</summary>
			public int IMEKeyboardsCount
			{
				get
				{
					if (Connection == null)
						return 0;

					var ibusWrapper = new IBusDotNet.InputBusWrapper(Connection);
					object[] engines = ibusWrapper.InputBus.ListActiveEngines();
					return engines.Length;
				}
			}

			/// <summary/>
			public void Close()
			{
				Dispose();
			}
			#endregion
		}

		private KeyboardSwitcher m_KeyboardSwitcher;

		/// <summary>
		/// Initializes a new instance of the
		/// <see cref="T:Palaso.UI.WindowsForms.Keyboard.Linux.IbusKeyboardAdaptor"/> class.
		/// </summary>
		public IbusKeyboardAdaptor()
		{
		}

		private void InitKeyboards()
		{
			var nKeyboards = m_KeyboardSwitcher.IMEKeyboardsCount;
			for (int i = 0; i < nKeyboards; i++)
			{
				var name = m_KeyboardSwitcher.GetKeyboardName(i);
				// REVIEW: what value should we pass as the locale name and for input language?
				var keyboard = new KeyboardDescription(name, name, string.Empty, null, this, KeyboardType.OtherIm);
				KeyboardController.Manager.RegisterKeyboard(keyboard);
			}
		}

#region IKeyboardAdaptor implementation
		/// <summary>
		/// Initialize the installed keyboards
		/// </summary>
		public void Initialize()
		{
			m_KeyboardSwitcher = new KeyboardSwitcher();
			InitKeyboards();
		}

		public void UpdateAvailableKeyboards()
		{
			InitKeyboards();
		}

		/// <summary/>
		public void Close()
		{
			if (m_KeyboardSwitcher == null)
				return;

			m_KeyboardSwitcher.Dispose();
			m_KeyboardSwitcher = null;
		}

		public bool ActivateKeyboard(IKeyboardDefinition keyboard)
		{
			return m_KeyboardSwitcher.SetIMEKeyboard(keyboard.Name);
		}

		/// <summary>
		/// Activates the keyboard
		/// </summary>
		public void ActivateKeyboard(IKeyboardDefinition keyboard,
			IKeyboardDefinition systemKeyboard)
		{
			// TODO: Remove once the other overload is implemented
			m_KeyboardSwitcher.IMEKeyboard = keyboard.Name;

			if (systemKeyboard != null)
				systemKeyboard.Activate();
		}

		/// <summary>
		/// Deactivates the specified keyboard.
		/// </summary>
		public void DeactivateKeyboard(IKeyboardDefinition keyboard)
		{
			m_KeyboardSwitcher.IMEKeyboard = null;
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
