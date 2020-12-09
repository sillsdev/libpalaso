// Copyright (c) 2011-2013 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using SIL.Keyboarding;

namespace SIL.Windows.Forms.Keyboarding
{
	/// <summary>
	/// Default implementation for a keyboard layout/language description. This is a keyboard definition that is
	/// actually useable. In addition to the IKeyboardDefinition properties that are saved in the LDML, this has a
	/// name suitable for use in a chooser, can store an IInputLanguage to positively associate it with a particular
	/// one, and records the engine (IKeyboardAdapter) which is associated with it and can perform such functions as really activating it.
	/// This class is adequate for some engines; others subclass it.
	/// </summary>
	public class KeyboardDescription : DefaultKeyboardDefinition
	{
		private readonly IKeyboardSwitchingAdaptor _engine;

		/// <summary>
		/// Initializes a new instance of the
		/// <see cref="KeyboardDescription"/> class.
		/// </summary>
		public KeyboardDescription(string id, string name, string layout, string locale, bool isAvailable,
			IKeyboardSwitchingAdaptor engine)
			: base(id, name, layout, locale, isAvailable)
		{
			_engine = engine;
		}

		/// <summary>
		/// Gets the keyboard adaptor that handles this keyboard.
		/// </summary>
		public IKeyboardSwitchingAdaptor Engine => _engine;

		/// <summary>
		/// Deactivate this keyboard.
		/// </summary>
		public void Deactivate()
		{
			Engine?.DeactivateKeyboard(this);
		}

		public IInputLanguage InputLanguage { get; protected internal set; }

		protected virtual bool DeactivatePreviousKeyboard(IKeyboardDefinition keyboardToActivate)
		{
			return keyboardToActivate != this;
		}

		/// <summary>
		/// Activate this keyboard.
		/// </summary>
		public override void Activate()
		{
			if (Keyboard.Controller.ActiveKeyboard == this)
			{
				// Don't waste the time, energy, and buggy behavior with IMEs to 'reactivate' ourself.
				return;
			}
			var activeKeyboard = Keyboard.Controller.ActiveKeyboard as KeyboardDescription;
			if (activeKeyboard != null && activeKeyboard.DeactivatePreviousKeyboard(this))
				activeKeyboard.Deactivate();

			Keyboard.Controller.ActiveKeyboard = KeyboardController.NullKeyboard;
			if (Engine == null)
				return;

			// Some engines may not always be able to activate a keyboard.
			// In particular the Ibus one can currently only do so if the application gives it a context.
			// At this time only FieldWorks views knows how to do this.
			// If we have NOT successfully set the keyboard we do not want to record it as active,
			// because that can suppress a subsequent attempt to set the same one after the context
			// is established when it would work. For example, we might try to set it when creating a
			// selection before the control is focused, and fail, but try again when the control gets
			// focus (at which time we first create the context) and succeed.
			if (Engine.ActivateKeyboard(this))
				Keyboard.Controller.ActiveKeyboard = this;
		}
	}
}
