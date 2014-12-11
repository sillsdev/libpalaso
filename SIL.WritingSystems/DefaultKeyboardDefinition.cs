using System;
using Palaso.Code;

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
	public class DefaultKeyboardDefinition : ICloneable<IKeyboardDefinition>, IEquatable<IKeyboardDefinition>, IKeyboardDefinition
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultKeyboardDefinition"/> class.
		/// </summary>
		public DefaultKeyboardDefinition()
		{
			Type = KeyboardType.System;
			Layout = string.Empty;
			Locale = string.Empty;
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultKeyboardDefinition"/> class.
		/// </summary>
		public DefaultKeyboardDefinition(DefaultKeyboardDefinition kd)
		{
			Type = kd.Type;
			Layout = kd.Layout;
			Locale = kd.Locale;
			OperatingSystem = kd.OperatingSystem;
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
		public KeyboardType Type { get; protected set; }

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
		public string Locale { get; set; }

		/// <summary>
		/// The name identifying the particular keyboard.
		/// </summary>
		public string Layout { get; set; }

		/// <summary>
		/// One operating system on which the keyboard is known to work.
		/// Enhance: should we store a list of OS's on which it works?
		/// So far we only support WIN32NT and Linux, and the same keyboard rarely (never?) works on both,
		/// but if we support say Mac, might it share keyboards with Linux?
		/// </summary>
		public PlatformID OperatingSystem { get; set; }

		/// <summary>
		/// Answer true if the keyboard is available to use on this system (that is, it can be activated).
		/// </summary>
		public bool IsAvailable { get; set; }

		/// <summary>
		/// Make this keyboard the active one that will be used for typing. This default class does not do anything
		/// to achieve this.
		/// </summary>
		public virtual void Activate()
		{
		}

		/// <summary>
		/// Clone this keyboard definition
		/// </summary>
		public virtual IKeyboardDefinition Clone()
		{
			return new DefaultKeyboardDefinition(this);
		}

		/// <summary>
		/// This overload is unfortunately required by the compiler to satisfy IEquateable.
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool Equals(DefaultKeyboardDefinition other)
		{
			return Equals((IKeyboardDefinition) other);
		}

		public override bool Equals(Object obj)
		{
			if (!(obj is IKeyboardDefinition)) return false;
			return Equals((IKeyboardDefinition)obj);
		}

		/// <summary>
		/// We are claiming here that all IKeyboardDefinitions should do equality testing this way.
		/// Currently all implementations inherit from this class and therefore do.
		/// Additional implementations should be careful to do the same, otherwise, basic equality
		/// expectations like a.Equals(b) iff b.Equals(a) may be violated. Similarly other implementations
		/// should use the same definition of GetHashCode().
		/// </summary>
		public bool Equals(IKeyboardDefinition other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Layout == other.Layout && Locale == other.Locale;
		}

		/// <summary>
		/// Equality operator.
		/// </summary>
		public static bool operator ==(DefaultKeyboardDefinition left, DefaultKeyboardDefinition right)
		{
			// Check for both being null
			if (ReferenceEquals(null, left))
				return ReferenceEquals(null, right);
			return left.Equals(right);
		}

		/// <summary>
		/// Equality operator.
		/// </summary>
		public static bool operator !=(DefaultKeyboardDefinition left, DefaultKeyboardDefinition right)
		{
			return !(left == right);
		}

		/// <summary>
		/// Note that to be consistent with Equals, any other implementation of IKeyboardDefinition
		/// should use the same definition of GetHashCode as this.
		/// Currently all implementations inherit from this class and therefore do.
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			// Don't crash if either Layout or Locale somehow end up being null.
			if (Layout != null && Locale != null)
			return Layout.GetHashCode() ^ Locale.GetHashCode();
			if (Layout != null)
				return Layout.GetHashCode();
			if (Locale != null)
				return Locale.GetHashCode();
			return 0;
		}
	}
}
