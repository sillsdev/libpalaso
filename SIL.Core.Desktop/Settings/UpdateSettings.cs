using System;

namespace SIL.Settings
{
	/// <summary>
	/// Settings for checking for, downloading, and installing updates
	/// </summary>
	[Serializable]
	public class UpdateSettings
	{
		public enum Channels
		{
			/// <summary>
			/// Get the latest stable release (recommended for most users)
			/// </summary>
			Stable,

			/// <summary>
			/// Get the latest Beta (recommended for users who want the latest features and don't mind a few bugs)
			/// </summary>
			Beta,

			/// <summary>
			/// Get the latest Alpha (recommended only if the developers invite you)
			/// </summary>
			Alpha,

			/// <summary>
			/// Get the latest nightly builds (not recommended)
			/// </summary>
			/// <remarks>This option should not be made available to users</remarks>
			Nightly
		}

		public enum Behaviors
		{
			/// <summary>
			/// Download and install the latest updates
			/// </summary>
			Install,

			/// <summary>
			/// Download the latest updates and prompt to install
			/// </summary>
			Download,

			/// <summary>
			/// Notify the user when updates are available and prompt to download
			/// </summary>
			Notify,

			/// <summary>
			/// Do not check for updates
			/// </summary>
			DoNotCheck
		}

		/// <summary>
		/// Which channel to check for updates
		/// </summary>
		public Channels Channel { get; set; }

		/// <summary>
		/// What to do when an update is available
		/// </summary>
		public Behaviors Behavior { get; set; }

		/// <summary>
		/// Create a new UpdateSettings with defaults: Channel: Stable; Behavior: Download
		/// </summary>
		public UpdateSettings()
		{
			Channel = Channels.Stable;
			Behavior = Behaviors.Download;
		}
	}
}