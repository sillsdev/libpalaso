using System;
using System.Diagnostics;
using System.IO;
using SIL.IO;

namespace SIL.WritingSystems
{
	/// <summary>
	/// Builds a file's new contents alongside the old one and swaps the two, for stores guarded by a
	/// machine-wide mutex. Such a mutex can be abandoned by a process that dies inside the critical
	/// section, so a writer that is interrupted part way through must not be able to leave the file
	/// truncated or missing.
	/// </summary>
	internal static class AtomicFileReplacement
	{
		private static readonly int s_processId = GetCurrentProcessId();

		private static int GetCurrentProcessId()
		{
			using (Process process = Process.GetCurrentProcess())
				return process.Id;
		}

		/// <summary>
		/// Gets a path beside <paramref name="targetPath"/> to build a replacement in. The process ID
		/// keeps processes sharing the directory from colliding on it even when the mutex guarding the
		/// store has been reduced to a local-only lock.
		/// </summary>
		/// <param name="extension">
		/// Extension for the temporary file, without a leading dot. It must be one that nothing
		/// searching the directory looks for, so that a leftover is ignored rather than read.
		/// </param>
		internal static string GetTempPath(string targetPath, string extension)
		{
			return $"{targetPath}.{s_processId}.{extension}";
		}

		/// <summary>
		/// Puts a freshly written file in place of the existing one, or in place of nothing if the
		/// target does not exist yet. Falls back to a copy when the file system cannot replace one file
		/// with another, which gives up atomicity but keeps the write working.
		/// </summary>
		internal static void SwapIntoPlace(string tempPath, string targetPath)
		{
			if (!File.Exists(targetPath))
			{
				RobustFile.Move(tempPath, targetPath);
				return;
			}

			try
			{
				RobustFile.Replace(tempPath, targetPath, null);
			}
			catch (Exception e) when (e is IOException || e is NotSupportedException)
			{
				RobustFile.ReplaceByCopyDelete(tempPath, targetPath, null);
			}
		}

		/// <summary>
		/// Removes a replacement that was never put in place. A failure to clean up is ignored: the
		/// caller is handling a more important error, and nothing reads a file at this path.
		/// </summary>
		internal static void DeleteIfPresent(string tempPath)
		{
			try
			{
				if (File.Exists(tempPath))
					RobustFile.Delete(tempPath);
			}
			catch (Exception e)
			{
				Trace.TraceInformation($"Could not remove the temporary file \"{tempPath}\": {e.Message}");
			}
		}
	}
}
