using System;

namespace Palaso.WritingSystems
{
	/// <summary>
	/// A simple record of the properties we track defining a keyboard.
	/// </summary>
	public class KeyboardDefinition : IKeyboardDefinition
	{
		/// <summary>
		/// The Locale of the keyboard. This is mainly significant on Windows, which distinguishes (for example)
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
		public bool IsAvailable
		{
			get { return true; } // Todo EberhardB(JohnT) implement
		}

		/// <summary>
		/// Make this keyboard the active one that will be used for typing.
		/// Review JohnT: Do we need to pass the control or Form in which we want it activated?
		/// </summary>
		public void Activate()
		{
			// Todo EberhardB(JohnT) implement
		}

		public override bool Equals(Object obj)
		{
			if (!(obj is KeyboardDefinition)) return false;
			return Equals((KeyboardDefinition)obj);
		}

		public bool Equals(KeyboardDefinition other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return (Layout == other.Layout && Locale == other.Locale && OperatingSystem == other.OperatingSystem);
		}

		public static bool operator ==(KeyboardDefinition left, KeyboardDefinition right)
		{
			// Check for both being null
			if (ReferenceEquals(null, left))
				return ReferenceEquals(null , right);
			return left.Equals(right);
		}

		public static bool operator !=(KeyboardDefinition left, KeyboardDefinition right)
		{
			return !(left == right);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}
}
