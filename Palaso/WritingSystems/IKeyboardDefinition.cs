using System;

namespace Palaso.WritingSystems
{
	/// <summary>
	/// The different keyboard types we're supporting.
	/// </summary>
	public enum KeyboardType
	{
		/// <summary>
		/// System keyboard like Windows API or xkb
		/// </summary>
		System,
		/// <summary>
		/// Other input method like Keyman, InKey or ibus
		/// </summary>
		OtherIm
	}

	/// <summary>
	///  An interface defining the properties and behaviors of keyboards relevant to writing system methods and properties.
	/// </summary>
	public interface IKeyboardDefinition
	{
		/// <summary>
		/// Gets an identifier of the language/keyboard layout
		/// </summary>
		string Id { get; }

		/// <summary>
		/// Gets the type of this keyboard (system or other)
		/// </summary>
		KeyboardType Type { get; }

		/// <summary>
		/// Gets a human-readable name of the input language.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// The Locale of the keyboard. This is mainly significant on Windows, which distinguishes (for example)
		/// a German keyboard used in Germany, Switzerland, and Holland.
		/// </summary>
		string Locale { get; }

		/// <summary>
		/// The name identifying the particular keyboard.
		/// </summary>
		string Layout { get; }

		/// <summary>
		/// One operating system on which the keyboard is known to work.
		/// Enhance: should we store a list of OS's on which it works?
		/// So far we only support WIN32NT and Linux, and the same keyboard rarely (never?) works on both,
		/// but if we support say Mac, might it share keyboards with Linux?
		/// </summary>
		PlatformID OperatingSystem { get; }

		/// <summary>
		/// Answer true if the keyboard is available to use on this system (that is, it can be activated).
		/// </summary>
		bool IsAvailable { get; }

		/// <summary>
		/// Make this keyboard the active one that will be used for typing.
		/// Review JohnT: Do we need to pass the control or Form in which we want it activated?
		/// </summary>
		void Activate();
	}
}