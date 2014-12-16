using System;
using System.Collections.Generic;

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
		private readonly DefaultKeyboardDefinition _trivialKeyboard = new DefaultKeyboardDefinition(KeyboardType.System, "English", "en-US");

		public IKeyboardDefinition GetKeyboard(string layoutName)
		{
			return _trivialKeyboard;
		}

		public IKeyboardDefinition GetKeyboard(string layoutName, string locale)
		{
			return _trivialKeyboard;
		}

		public IKeyboardDefinition GetKeyboard(WritingSystemDefinition writingSystem)
		{
			return _trivialKeyboard;
		}

		public IKeyboardDefinition GetKeyboard(IInputLanguage language)
		{
			return _trivialKeyboard;
		}

		public void SetKeyboard(IKeyboardDefinition keyboard)
		{
		}

		public void SetKeyboard(string layoutName)
		{
		}

		public void SetKeyboard(string layoutName, string locale)
		{
		}

		public void SetKeyboard(WritingSystemDefinition writingSystem)
		{
		}

		public void SetKeyboard(IInputLanguage language)
		{
		}

		/// <summary>
		/// Activates the keyboard of the default input language
		/// </summary>
		public void ActivateDefaultKeyboard()
		{
		}

		public IEnumerable<IKeyboardDefinition> AllAvailableKeyboards
		{
			get {return new IKeyboardDefinition[0];}
		}

		public void UpdateAvailableKeyboards()
		{
		}

		public virtual IKeyboardDefinition DefaultForWritingSystem(WritingSystemDefinition ws)
		{
			return _trivialKeyboard;
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
		public virtual IKeyboardDefinition CreateKeyboardDefinition(string layout, string locale)
		{
			return new DefaultKeyboardDefinition(KeyboardType.System, layout, locale);
		}

		/// <summary>
		/// Gets the currently active keyboard
		/// </summary>
		public IKeyboardDefinition ActiveKeyboard { get; set; }

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
