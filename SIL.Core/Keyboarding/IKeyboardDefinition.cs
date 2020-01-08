using System.Collections.Generic;

namespace SIL.Keyboarding
{
	public enum KeyboardFormat
	{
		/// <summary>
		/// Unknown format
		/// </summary>
		Unknown,
		/// <summary>
		/// Keyman source file (.kmn)
		/// </summary>
		Keyman,
		/// <summary>
		/// Compiled Keyman file (.kmx)
		/// </summary>
		CompiledKeyman,
		/// <summary>
		/// MSKLC file (.klc)
		/// </summary>
		Msklc,
		/// <summary>
		/// Keyboard layout LDML file
		/// </summary>
		Ldml,
		/// <summary>
		/// Mac keyboard layout file
		/// </summary>
		Keylayout,
		/// <summary>
		/// Keyman package file (zipped resources)
		/// </summary>
		KeymanPackage
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
		/// Gets a human-readable (unlocalized) name of the input language.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Gets a localized human-readable name of the input language.
		/// </summary>
		string LocalizedName { get; }

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
		/// Indicates whether we should pass NFC or NFD data to the keyboard. In general this
		/// will be NFC, but (most?) keyman keyboards need NFD (see LT-16637)
		/// </summary>
		bool UseNfcContext { get; }

		/// <summary>
		/// Answer true if the keyboard is available to use on this system (that is, it can be activated).
		/// </summary>
		bool IsAvailable { get; }

		/// <summary>
		/// Make this keyboard the active one that will be used for typing.
		/// Review JohnT: Do we need to pass the control or Form in which we want it activated?
		/// </summary>
		void Activate();

		/// <summary>
		/// Gets the keyboard source format.
		/// </summary>
		KeyboardFormat Format { get; }

		/// <summary>
		/// Gets the keyboard source URL.
		/// For now these will not be changed
		/// </summary>
		IList<string> Urls { get; }
	}
}