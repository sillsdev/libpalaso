using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SIL.Keyboarding
{
	/// <summary>
	/// A trivial implementation of the interface.
	/// </summary>
	public class DefaultKeyboardController : IKeyboardController
	{
		private readonly DefaultKeyboardDefinition _defaultKeyboard = new DefaultKeyboardDefinition("en-US_English", "English", "English", "en-US", true);
		private readonly DefaultKeyboardDefinition _nullKeyboard = new DefaultKeyboardDefinition(string.Empty, "(default)");
		private readonly Dictionary<string, DefaultKeyboardDefinition> _keyboards = new Dictionary<string, DefaultKeyboardDefinition>();

		public DefaultKeyboardController()
		{
			_keyboards[_defaultKeyboard.Id] = _defaultKeyboard;
		}

		public IKeyboardDefinition DefaultKeyboard
		{
			get { return _defaultKeyboard; }
		}

		public IKeyboardDefinition GetKeyboard(string id)
		{
			IKeyboardDefinition keyboard;
			if (TryGetKeyboard(id, out keyboard))
				return keyboard;
			return _nullKeyboard;
		}

		public bool TryGetKeyboard(string id, out IKeyboardDefinition keyboard)
		{
			DefaultKeyboardDefinition kd;
			if (_keyboards.TryGetValue(id, out kd))
			{
				keyboard = kd;
				return true;
			}
			keyboard = null;
			return false;
		}

		public IKeyboardDefinition GetKeyboard(string layoutName, string locale)
		{
			IKeyboardDefinition keyboard;
			if (TryGetKeyboard(layoutName, locale, out keyboard))
				return keyboard;
			return _nullKeyboard;
		}

		public bool TryGetKeyboard(string layoutName, string locale, out IKeyboardDefinition keyboard)
		{
			keyboard = _keyboards.Values.FirstOrDefault(k => k.Layout == layoutName && k.Locale == locale);
			return keyboard != null;
		}

		public IKeyboardDefinition GetKeyboard(IInputLanguage language)
		{
			IKeyboardDefinition keyboard;
			if (TryGetKeyboard(language, out keyboard))
				return keyboard;
			return _nullKeyboard;
		}

		public bool TryGetKeyboard(IInputLanguage language, out IKeyboardDefinition keyboard)
		{
			return TryGetKeyboard(language.LayoutName, language.Culture.Name, out keyboard);
		}

		public IKeyboardDefinition GetKeyboard(int windowsLcid)
		{
			IKeyboardDefinition keyboard;
			if (TryGetKeyboard(windowsLcid, out keyboard))
				return keyboard;
			return _nullKeyboard;
		}

		public bool TryGetKeyboard(int windowsLcid, out IKeyboardDefinition keyboard)
		{
			try
			{
				CultureInfo ci = CultureInfo.GetCultureInfo(windowsLcid);
				keyboard = _keyboards.Values.FirstOrDefault(k => k.Locale == ci.Name);
			}
			catch (CultureNotFoundException)
			{
				keyboard = null;
			}
			return keyboard != null;
		}

		/// <summary>
		/// Activates the keyboard of the default input language
		/// </summary>
		public void ActivateDefaultKeyboard()
		{
			_defaultKeyboard.Activate();
		}

		public IEnumerable<IKeyboardDefinition> AvailableKeyboards
		{
			get { return _keyboards.Values; }
		}

		public void UpdateAvailableKeyboards()
		{
		}

		/// <summary>
		/// Creates and returns a keyboard definition object based on the layout and locale.
		/// </summary>
		/// <remarks>The keyboard controller implementing this method will have to check the
		/// availability of the keyboard and what engine provides it.</remarks>
		public virtual IKeyboardDefinition CreateKeyboard(string id, KeyboardFormat format, IEnumerable<string> urls)
		{
			DefaultKeyboardDefinition keyboard;
			if (!_keyboards.TryGetValue(id, out keyboard))
			{
				string[] parts = id.Split('_');
				string layout, locale = null;
				if (parts.Length == 1)
				{
					layout = parts[0];
				}
				else
				{
					locale = parts[0];
					layout = parts[1];
				}
				keyboard = new DefaultKeyboardDefinition(id, layout, layout, locale, true);
				_keyboards[id] = keyboard;
			}

			keyboard.Format = format;

			// Clear any existing URL list
			keyboard.Urls.Clear();
			foreach (string url in urls)
				keyboard.Urls.Add(url);
			return keyboard;
		}

		/// <summary>
		/// Gets the currently active keyboard
		/// </summary>
		public IKeyboardDefinition ActiveKeyboard { get; set; }

		public void Reset()
		{
			_keyboards.Clear();
			_keyboards[_defaultKeyboard.Id] = _defaultKeyboard;
		}

		#region Implementation of IDisposable
		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			GC.SuppressFinalize(this);
			// nothing else to do
		}
		#endregion
	}
}
