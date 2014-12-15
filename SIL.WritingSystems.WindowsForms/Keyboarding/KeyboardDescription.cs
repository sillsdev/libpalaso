// Copyright (c) 2011-2013 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using SIL.WritingSystems.WindowsForms.Keyboarding.InternalInterfaces;

namespace SIL.WritingSystems.WindowsForms.Keyboarding
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
		/// <summary>
		/// The null keyboard description
		/// </summary>
		public static readonly IKeyboardDefinition Zero = new KeyboardDescriptionNull();

		private readonly IKeyboardAdaptor _engine;
		private readonly string _name;
		private readonly IInputLanguage _inputLanguage;

		/// <summary>
		/// Initializes a new instance of the
		/// <see cref="T:Palaso.UI.WindowsForms.Keyboard.KeyboardDescription"/> class.
		/// </summary>
		internal KeyboardDescription(string name, string layout, string locale,
			IInputLanguage language, IKeyboardAdaptor engine)
			: this(name, layout, locale, language, engine, KeyboardType.System, true)
		{
		}

		/// <summary>
		/// Initializes a new instance of the
		/// <see cref="T:Palaso.UI.WindowsForms.Keyboard.KeyboardDescription"/> class.
		/// </summary>
		internal KeyboardDescription(string name, string layout, string locale,
			IInputLanguage language, IKeyboardAdaptor engine, KeyboardType type, bool isAvailable)
			: base(type, layout, locale, isAvailable)
		{
			_name = name;
			_engine = engine;
			_inputLanguage = language;
		}

		/// <summary>
		/// Gets the keyboard adaptor that handles this keyboard.
		/// </summary>
		internal IKeyboardAdaptor Engine
		{
			get { return _engine; }
		}

		/// <summary>
		/// Deactivate this keyboard.
		/// </summary>
		internal void Deactivate()
		{
			if (Engine != null)
				Engine.DeactivateKeyboard(this);
		}

		internal IInputLanguage InputLanguage
		{
			get { return _inputLanguage; }
		}

		protected static string GetDisplayName(string cultureName, string layoutName)
		{
			return string.Format("{1} - {0}", cultureName, layoutName);
		}

		protected virtual bool DeactivatePreviousKeyboard(IKeyboardDefinition keyboardToActivate)
		{
			return true;
		}

		#region IKeyboardDefinition Members

		/// <summary>
		/// Gets a human-readable name of the input method/language combination.
		/// </summary>
		public override string Name
		{
			get { return _name; }
		}

		/// <summary>
		/// Activate this keyboard.
		/// </summary>
		public override void Activate()
		{
			var activeKeyboard = Keyboard.Controller.ActiveKeyboard as KeyboardDescription;
			if (activeKeyboard != null && activeKeyboard.DeactivatePreviousKeyboard(this))
				activeKeyboard.Deactivate();

			Keyboard.Controller.ActiveKeyboard = Zero;
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
	    #endregion
	}
}
