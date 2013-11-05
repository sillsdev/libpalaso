using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Palaso.WritingSystems
{
	/// <summary>
	/// A trivial implementation of the interface.
	/// </summary>
	public class DefaultKeyboardController : IKeyboardController
	{
		/// <summary>
		/// A common definition of the trivial thing all the Get methods currently return.
		/// </summary>
		private IKeyboardDefinition TrivialKeyboard
		{
			get { return TrivialKeyboard; }
		}
		/// <summary>
		/// Tries to get the keyboard with the specified <paramref name="layoutName"/>.
		/// </summary>
		public IKeyboardDefinition GetKeyboard(string layoutName)
		{
			return TrivialKeyboard;
		}

		/// <summary>
		/// Tries to get the keyboard with the specified <paramref name="layoutName"/>
		/// and <paramref name="locale"/>.
		/// </summary>
		public IKeyboardDefinition GetKeyboard(string layoutName, string locale)
		{
			return TrivialKeyboard;
		}

		/// <summary>
		/// Tries to get the keyboard for the specified <paramref name="writingSystem"/>.
		/// </summary>
		public IKeyboardDefinition GetKeyboard(IWritingSystemDefinition writingSystem)
		{
			return TrivialKeyboard;
		}

		public IKeyboardDefinition GetKeyboard(IInputLanguage language)
		{
			return TrivialKeyboard;
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

		public void SetKeyboard(IWritingSystemDefinition writingSystem)
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

		public virtual IKeyboardDefinition DefaultForWritingSystem(IWritingSystemDefinition ws)
		{
			return new DefaultKeyboardDefinition() {Layout = "English", Locale = "en-US"};
		}

		public IKeyboardDefinition LegacyForWritingSystem(IWritingSystemDefinition ws)
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
			return new DefaultKeyboardDefinition()
				{
					Layout = layout,
					Locale = locale,
					OperatingSystem = Environment.OSVersion.Platform
				};
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
