using System;
using System.Collections.Generic;
using System.Linq;

namespace SIL.WritingSystems
{
	/// <summary>
	/// A trivial implementation of the interface.
	/// </summary>
	public class DefaultKeyboardController : IKeyboardController
	{
		/// <summary>
		/// A common definition of the trivial thing all the Get methods currently return.
		/// </summary>
		private readonly DefaultKeyboardDefinition _defaultKeyboard = new DefaultKeyboardDefinition("en-US_English", "English");
		private readonly Dictionary<string, DefaultKeyboardDefinition> _keyboards = new Dictionary<string, DefaultKeyboardDefinition>();

		public DefaultKeyboardController()
		{
			_keyboards[_defaultKeyboard.Id] = _defaultKeyboard;
		}

		public IKeyboardDefinition GetKeyboard(string id)
		{
			DefaultKeyboardDefinition keyboard;
			if (_keyboards.TryGetValue(id, out keyboard))
				return keyboard;
			return null;
		}

		public IKeyboardDefinition GetKeyboard(string layoutName, string locale)
		{
			return _keyboards.Values.FirstOrDefault(k => k.Layout == layoutName && k.Locale == locale);
		}

		public IKeyboardDefinition GetKeyboard(WritingSystemDefinition writingSystem)
		{
			return writingSystem.LocalKeyboard;
		}

		public IKeyboardDefinition GetKeyboard(IInputLanguage language)
		{
			return GetKeyboard(language.LayoutName, language.Culture.Name);
		}

		public void SetKeyboard(IKeyboardDefinition keyboard)
		{
			ActiveKeyboard = keyboard;
		}

		public void SetKeyboard(string id)
		{
			ActiveKeyboard = GetKeyboard(id);
		}

		public void SetKeyboard(string layoutName, string locale)
		{
			ActiveKeyboard = GetKeyboard(layoutName, locale);
		}

		public void SetKeyboard(WritingSystemDefinition writingSystem)
		{
			ActiveKeyboard = GetKeyboard(writingSystem);
		}

		public void SetKeyboard(IInputLanguage language)
		{
			ActiveKeyboard = GetKeyboard(language);
		}

		/// <summary>
		/// Activates the keyboard of the default input language
		/// </summary>
		public void ActivateDefaultKeyboard()
		{
			ActiveKeyboard = _defaultKeyboard;
		}

		public IEnumerable<IKeyboardDefinition> AllAvailableKeyboards
		{
			get { return _keyboards.Values; }
		}

		public void UpdateAvailableKeyboards()
		{
		}

		public virtual IKeyboardDefinition DefaultForWritingSystem(WritingSystemDefinition ws)
		{
			return LegacyForWritingSystem(ws) ?? _defaultKeyboard;
		}

		public IKeyboardDefinition LegacyForWritingSystem(WritingSystemDefinition ws)
		{
			return null;
		}

		/// <summary>
		/// Creates and returns a keyboard definition object based on the layout and locale.
		/// </summary>
		/// <remarks>The keyboard controller implementing this method will have to check the
		/// availability of the keyboard and what engine provides it.</remarks>
		public virtual IKeyboardDefinition CreateKeyboardDefinition(string id, KeyboardFormat format, IEnumerable<string> urls)
		{
			DefaultKeyboardDefinition keyboard;
			if (!_keyboards.TryGetValue(id, out keyboard))
			{
				string[] parts = id.Split('_');
				string locale = parts[0];
				string layout = parts.Length > 1 ? parts[1] : null;
				keyboard = new DefaultKeyboardDefinition(id, layout, layout, locale, false);
				_keyboards[id] = keyboard;
			}

			keyboard.Format = format;
			
			// Clear any exisiting URL list
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
