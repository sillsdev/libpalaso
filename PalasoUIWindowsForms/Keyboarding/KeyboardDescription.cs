// Copyright (c) 2011-2013 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Diagnostics;
using Palaso.UI.WindowsForms.Keyboarding.InternalInterfaces;
using Palaso.WritingSystems;

namespace Palaso.UI.WindowsForms.Keyboarding
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
		public static IKeyboardDefinition Zero = new KeyboardDescriptionNull();

		/// <summary>
		/// Initializes a new instance of the
		/// <see cref="T:Palaso.UI.WindowsForms.Keyboard.KeyboardDescription"/> class.
		/// </summary>
		internal KeyboardDescription(string name, string layout, string locale,
			IInputLanguage language, IKeyboardAdaptor engine)
			: this(name, layout, locale, language, engine, KeyboardType.System)
		{
		}

		/// <summary>
		/// Initializes a new instance of the
		/// <see cref="T:Palaso.UI.WindowsForms.Keyboard.KeyboardDescription"/> class.
		/// </summary>
		internal KeyboardDescription(string name, string layout, string locale,
			IInputLanguage language, IKeyboardAdaptor engine, KeyboardType type)
		{
			InternalName = name;
			Layout = layout;
			Locale = locale;
			Engine = engine;
			Type = type;
			IsAvailable = true;
			OperatingSystem = Environment.OSVersion.Platform;
			InputLanguage = language;
		}

		internal KeyboardDescription(IKeyboardAdaptor engine, KeyboardType type)
		{
			Engine = engine;
			Type = type;
			IsAvailable = true;
			OperatingSystem = Environment.OSVersion.Platform;
		}

		internal KeyboardDescription(KeyboardDescription other)
			:base(other)
		{
			InternalName = other.Name;
			Engine = other.Engine;
			IsAvailable = other.IsAvailable;
			InputLanguage = other.InputLanguage;
		}

		/// <summary>
		/// Gets the keyboard adaptor that handles this keyboard.
		/// </summary>
		internal IKeyboardAdaptor Engine { get; private set; }

		/// <summary>
		/// Deactivate this keyboard.
		/// </summary>
		internal void Deactivate()
		{
			if (Engine != null)
				Engine.DeactivateKeyboard(this);
		}

		/// <summary>
		/// Deepclone
		/// </summary>
		/// <returns></returns>
		public override IKeyboardDefinition Clone()
		{
			Debug.Assert(GetType().Name == typeof(KeyboardDescription).Name,
				"Derived class doesn't implement Clone()");
			return new KeyboardDescription(this);
		}

		/// <summary>
		/// overload unfortunately required by IEquatable
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool Equals(KeyboardDescription other)
		{
			return Equals((IKeyboardDefinition) other);
		}

		/// <summary>
		/// Returns a <see cref="T:System.String"/> that represents the current
		/// <see cref="T:Palaso.UI.WindowsForms.Keyboard.KeyboardDescription"/>.
		/// </summary>
		public override string ToString()
		{
			return Name;
		}

		internal IInputLanguage InputLanguage { get; private set; }

		protected string InternalName { get; set; }

		protected void SetInputLanguage(IInputLanguage inputLanguage)
		{
			InputLanguage = inputLanguage;
		}

		protected static string GetDisplayName(string cultureName, string layoutName)
		{
			return string.Format("{1} - {0}", cultureName, layoutName);
		}

		#region IKeyboardDefinition Members

		/// <summary>
		/// Gets a human-readable name of the input method/language combination.
		/// </summary>
		public override string Name { get { return InternalName; } }

		/// <summary>
		/// Activate this keyboard.
		/// </summary>
		public override void Activate()
		{
			var oldActiveKeyboard = Keyboard.Controller.ActiveKeyboard;
			if (oldActiveKeyboard != null && oldActiveKeyboard.Id == this.Id)
				return;

			var activeKeyboard = oldActiveKeyboard as KeyboardDescription;
			if (activeKeyboard != null)
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
