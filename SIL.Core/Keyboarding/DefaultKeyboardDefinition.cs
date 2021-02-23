using System.Collections.Generic;

namespace SIL.Keyboarding
{
	/// <summary>
	/// A simple record of the properties we track in writing systems defining a keyboard and implementing the keyboard-related
	/// writing system methods and properties.
	/// </summary>
	/// <remarks>This is a not-fully-functional base class. Apps that make use of keyboard switching functionality will use
	/// the implementations of IKeyboardDefinition from SIL.Windows.Forms.WritingSystems or some similar library.
	/// In particular while this class can store various data it does nothing about actually activating a keyboard.
	/// Review: possibly that method and this class should be made abstract?</remarks>
	public class DefaultKeyboardDefinition : IKeyboardDefinition
	{
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
			Layout = layout;
			Locale = locale;
			Id = id;
			Name = name;
			IsAvailable = isAvailable;
		}

		/// <summary>
		/// Gets an identifier of the language/keyboard layout
		/// </summary>
		public string Id { get; }

		/// <summary>
		/// Gets a human-readable name of the input language.
		/// </summary>
		// ReSharper disable once MemberCanBePrivate.Global
		public string Name { get; protected set; }

		/// <summary>
		/// Gets a localized human-readable name of the input language.
		/// </summary>
		public virtual string LocalizedName => Name;

		/// <summary>
		/// The Locale of the keyboard in the format languagecode2-country/regioncode2.
		/// Review JohnT: Possibly the alternate format languagecode2-country-region might be better?
		/// http://msdn.microsoft.com/en-us/library/windows/desktop/dd373814%28v=vs.85%29.aspx says that is the standard if region is included.
		/// This is mainly significant on Windows, which distinguishes (for example)
		/// a German keyboard used in Germany, Switzerland, and Holland.
		/// </summary>
		public string Locale { get; }

		/// <summary>
		/// The name identifying the particular keyboard.
		/// </summary>
		public string Layout { get; }

		/// <summary>
		/// Indicates whether we should pass NFC or NFD data to the keyboard. This implementation
		/// always returns <c>true</c>.
		/// </summary>
		public virtual bool UseNfcContext => true;

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
			Keyboard.Controller.ActiveKeyboard = this;
		}

		/// <summary>
		/// Gets or sets the keyboard source format.
		/// </summary>
		public KeyboardFormat Format { get; set; }

		/// <summary>
		/// Gets the keyboard source URLs.
		/// </summary>
		public IList<string> Urls => _urls;

		/// <summary>
		/// Returns a <see cref="T:System.String"/> that represents the current
		/// <see cref="T:SIL.Windows.Forms.Keyboarding.KeyboardDescription"/>.
		/// </summary>
		public override string ToString()
		{
			return Name;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is DefaultKeyboardDefinition other))
				return false;

			return Id == other.Id &&  Name == other.Name && Locale == other.Locale &&
				Layout == other.Layout && UseNfcContext == other.UseNfcContext &&
				IsAvailable == other.IsAvailable && Format == other.Format;
		}

		public override int GetHashCode()
		{
			// https://stackoverflow.com/a/263416/487503
			unchecked // Overflow is fine, just wrap
			{
				var hash = 31;
				hash *= 71 + Id?.GetHashCode() ?? 0;
				hash *= 71 + Name?.GetHashCode() ?? 0;
				hash *= 71 + LocalizedName?.GetHashCode() ?? 0;
				hash *= 71 + Locale?.GetHashCode() ?? 0;
				hash *= 71 + Layout?.GetHashCode() ?? 0;
				hash *= 71 + UseNfcContext.GetHashCode();
				hash *= 71 + IsAvailable.GetHashCode();
				hash *= 71 + Urls?.GetHashCode() ?? 0;
				return hash;
			}
		}
	}
}
