using System;

namespace Palaso.WritingSystems
{
	/// <summary>
	/// A simple record of the properties we track defining a keyboard.
	/// </summary>
	public class KeyboardDefinition
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
	}
}
