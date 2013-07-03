using System;

namespace Palaso.WritingSystems
{
	public interface IKeyboardDefinition
	{
		/// <summary>
		/// The Locale of the keyboard. This is mainly significant on Windows, which distinguishes (for example)
		/// a German keyboard used in Germany, Switzerland, and Holland.
		/// </summary>
		string Locale { get; set; }

		/// <summary>
		/// The name identifying the particular keyboard.
		/// </summary>
		string Layout { get; set; }

		/// <summary>
		/// One operating system on which the keyboard is known to work.
		/// Enhance: should we store a list of OS's on which it works?
		/// So far we only support WIN32NT and Linux, and the same keyboard rarely (never?) works on both,
		/// but if we support say Mac, might it share keyboards with Linux?
		/// </summary>
		PlatformID OperatingSystem { get; set; }

		/// <summary>
		/// Answer true if the keyboard is available to use on this system (that is, it can be activated).
		/// </summary>
		bool IsAvailable { get; // Todo EberhardB(JohnT) implement
		}

		/// <summary>
		/// Make this keyboard the active one that will be used for typing.
		/// Review JohnT: Do we need to pass the control or Form in which we want it activated?
		/// </summary>
		void Activate();
	}
}