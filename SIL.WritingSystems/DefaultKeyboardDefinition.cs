using System.Collections.Generic;

namespace SIL.WritingSystems
{
	/// <summary>
	/// A simple record of the properties we track in writing systems defining a keyboard and implementing the keyboard-related
	/// writing system methods and properties.
	/// </summary>
	/// <remarks>This is a not-fully-functional base class. Apps that make use of keyboard switching functionality will use
	/// the implementations of IKeyboardDefinition from SIL.WritingSystems.WindowsForms or some similar library.
	/// In particular while this class can store various data it does nothing about actually activating a keyboard.
	/// Review: possibly that method and this class should be made abstract?</remarks>
	public class DefaultKeyboardDefinition : IKeyboardDefinition
	{
		private readonly string _locale;
		private readonly string _layout;
		private readonly string _id;
		private readonly List<string> _urls = new List<string>(); 

		public DefaultKeyboardDefinition(string id, string name)
			: this(id, name, string.Empty, string.Empty, false)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultKeyboardDefinition"/> class.
		/// </summary>
		public DefaultKeyboardDefinition(string id, string name, string layout, string locale, bool isAvailable)
		{
			_layout = layout;
			_locale = locale;
			_id = id;
			Name = name;
			IsAvailable = isAvailable;
		}

		/// <summary>
		/// Gets an identifier of the language/keyboard layout
		/// </summary>
		public string Id
		{
			get { return _id; }
		}

		/// <summary>
		/// Gets a human-readable name of the input language.
		/// </summary>
		public string Name { get; protected set; }

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
		public bool IsAvailable { get; protected set; }

		/// <summary>
		/// Make this keyboard the active one that will be used for typing. This default class does not do anything
		/// to achieve this.
		/// </summary>
		public virtual void Activate()
		{
		}

		/// <summary>
		/// Gets or sets the keyboard source format.
		/// </summary>
		public KeyboardFormat Format { get; set; }

		/// <summary>
		/// Gets the keyboard source URLs.
		/// </summary>
		public IList<string> Urls { get { return _urls; } }

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
