using System;

namespace SIL.WritingSystems
{
	/// <summary>
	/// A simple record of the properties we track in writing systems defining a keyboard and implementing the keyboard-related
	/// writing system methods and properties.
	/// </summary>
	/// <remarks>This is a not-fully-functional base class. Apps that make use of keyboard switching functionality will use
	/// the implementations of IKeyboardDefinition from PalasoUIWindowsForms or some similar library.
	/// In particular while this class can store various data it does nothing about actually activating a keyboard.
	/// Review: possibly that method and this class should be made abstract?</remarks>
	public class DefaultKeyboardDefinition : IKeyboardDefinition
	{
		private readonly KeyboardType _type;
		private readonly string _locale;
		private readonly string _layout;
		private readonly bool _isAvailable;

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultKeyboardDefinition"/> class.
		/// </summary>
		public DefaultKeyboardDefinition(KeyboardType type, string layout, string locale, bool isAvailable = false)
		{
			_type = type;
			_layout = layout;
			_locale = locale;
			_isAvailable = isAvailable;
		}

		/// <summary>
		/// Gets the identifier for the keyboard based on the provided locale and layout.
		/// </summary>
		public static string GetId(string locale, string layout)
		{
			return String.Format("{0}_{1}", locale, layout);
		}

		/// <summary>
		/// Gets an identifier of the language/keyboard layout
		/// </summary>
		public string Id
		{
			get { return GetId(Locale, Layout); }
		}

		/// <summary>
		/// Gets the type of this keyboard (system or other)
		/// </summary>
		public KeyboardType Type
		{
			get { return _type; }
		}

		/// <summary>
		/// Gets a human-readable name of the input language.
		/// </summary>
		public virtual string Name
		{
			get { return string.Format("{0} - {1}", Layout, Locale); }
		}

		/// <summary>
		/// Gets a localized human-readable name of the input language.
		/// </summary>
		public virtual string LocalizedName
		{
			get { return Name; }
		}

		/// <summary>
		/// The Locale of the keyboard in the format languagecode2-country/regioncode2.
		/// Review JohnT: Possibly the alternate format languagecode2-country-region might be better?
		/// http://msdn.microsoft.com/en-us/library/windows/desktop/dd373814%28v=vs.85%29.aspx says that is the standard if region is included.
		/// This is mainly significant on Windows, which distinguishes (for example)
		/// a German keyboard used in Germany, Switzerland, and Holland.
		/// </summary>
		public string Locale
		{
			get { return _locale; }
		}

		/// <summary>
		/// The name identifying the particular keyboard.
		/// </summary>
		public string Layout
		{
			get { return _layout; }
		}

		/// <summary>
		/// Answer true if the keyboard is available to use on this system (that is, it can be activated).
		/// </summary>
		public bool IsAvailable
		{
			get { return _isAvailable; }
		}

		/// <summary>
		/// Make this keyboard the active one that will be used for typing. This default class does not do anything
		/// to achieve this.
		/// </summary>
		public virtual void Activate()
		{
		}

		/// <summary>
		/// Returns a <see cref="T:System.String"/> that represents the current
		/// <see cref="T:Palaso.UI.WindowsForms.Keyboard.KeyboardDescription"/>.
		/// </summary>
		public override string ToString()
		{
			return Name;
		}
	}
}
