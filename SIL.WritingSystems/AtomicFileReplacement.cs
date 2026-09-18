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
		/// Gets a path beside <paramref name="targetPath"/> to build a replacement in. The name is
		/// unique to the call, so writers sharing the directory cannot collide on it even where the
		/// mutex guarding the store has been reduced to a local-only lock. A process ID alone would
		/// not do: it is unique only within its namespace, and containers with their own are exactly
		/// what that reduced lock is for, so two sharing a store can hold the same ID. The process ID
		/// is still there to say which process left a file behind.
		/// </summary>
		/// <param name="extension">
		/// Extension for the temporary file, without a leading dot. It must be one that nothing
		/// searching the directory looks for, so that a leftover is ignored rather than read.
		/// </param>
		internal static string GetTempPath(string targetPath, string extension)
		{
			return $"{targetPath}.{s_processId}.{Guid.NewGuid():N}.{extension}";
		}

		/// <summary>
		/// Puts a freshly written file in place of the existing one, or in place of nothing if the
		/// target does not exist yet. Falls back to a copy when the file system cannot replace one file
		/// with another, which gives up atomicity but keeps the write working. The file left in place
		/// carries the permissions of the replacement, whatever the file system does with them.
		/// </summary>
		internal static void SwapIntoPlace(string tempPath, string targetPath)
		{
			// Replacing a file can carry the permissions of either the replacement or the file it
			// displaces, and which one varies by file system. The replacement was created under
			// whatever mask the caller intended, so its permissions are the ones to keep. Read them
			// before the swap consumes it, and put them back afterwards.
			bool replacementMode = UnixFilePermissions.TryGetMode(tempPath, out uint mode);

			if (File.Exists(targetPath))
			{
				// Replacing is atomic only where the file system can do it. Where it cannot, the
				// target has to be copied over, and copying truncates it before the first byte
				// arrives. The copy is therefore done here rather than delegated, so that the moment
				// the target stops being intact is known exactly: the displaced contents are put
				// aside first, and go back if anything after that fails.
				string backupPath = GetTempPath(targetPath, "bak");
				bool displacedContentsSaved = false;
				try
				{
					try
					{
						RobustFile.Replace(tempPath, targetPath, backupPath);
					}
					catch (Exception e) when (e is IOException || e is NotSupportedException
						|| e is UnauthorizedAccessException)
					{
						if (File.Exists(targetPath))
						{
							RobustFile.Copy(targetPath, backupPath, true);
							displacedContentsSaved = true;
						}

						RobustFile.Copy(tempPath, targetPath, true);
						RobustFile.Delete(tempPath);
					}
				}
				catch
				{
					// Only once the backup is known to be complete: a copy that failed part way
					// through leaves a partial backup beside a target nothing has touched yet, and
					// restoring from that would destroy the good file.
					if (displacedContentsSaved)
					{
						try
						{
							RobustFile.Move(backupPath, targetPath, overWrite: true);
						}
						catch (Exception e)
						{
							Trace.TraceError(
								$"Could not put the previous contents of \"{targetPath}\" back: {e.Message}");
						}
					}

					throw;
				}

				DeleteIfPresent(backupPath);
			}
			else
			{
				RobustFile.Move(tempPath, targetPath);
			}

			if (replacementMode)
				UnixFilePermissions.TrySetMode(targetPath, mode);
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
