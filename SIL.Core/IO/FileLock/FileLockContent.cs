namespace SIL.IO.FileLock
{
	/// <summary>
	/// Class which gets serialized into the file lock - responsible for letting conflicting
	/// processes know which process owns this lock, when the lock was acquired, and the process name.
	/// </summary>
	public class FileLockContent
	{
		/// <summary>
		/// The process ID
		/// </summary>
		public long PID { get; set; }

		/// <summary>
		/// The timestamp (DateTime.Now.Ticks)
		/// </summary>
		public long Timestamp { get; set; }

		/// <summary>
		/// The name of the process
		/// </summary>
		public string ProcessName { get; set; }
	}

	public class MissingFileLockContent : FileLockContent
	{
	}

	public class OtherProcessOwnsFileLockContent : FileLockContent
	{
	}
}
