// Copyright (c) 2026 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Diagnostics;
using JetBrains.Annotations;
using Mono.Unix.Native;
using SIL.PlatformUtilities;

namespace SIL.IO
{
	/// <summary>
	/// Reads and applies the Unix permission bits of a file. Every member is a no-op on a platform
	/// that does not have them, so a caller needs no platform test of its own.
	/// </summary>
	[PublicAPI]
	public static class UnixFilePermissions
	{
		private const FilePermissions AllModeBits =
			FilePermissions.S_IRWXU | FilePermissions.S_IRWXG | FilePermissions.S_IRWXO;

		/// <summary>
		/// Gets the owner, group and other permission bits of a file. Returns false, with
		/// <paramref name="mode"/> set to 0, on a platform that has no such bits or if the file
		/// cannot be read.
		/// </summary>
		[CLSCompliant(false)]
		public static bool TryGetMode(string path, out uint mode)
		{
			mode = 0;
			if (!Platform.IsUnix)
				return false;

			if (Syscall.stat(path, out Stat status) != 0)
			{
				Trace.TraceInformation(
					$"Could not read the permissions of \"{path}\": {Stdlib.GetLastError()}");
				return false;
			}

			mode = (uint) (status.st_mode & AllModeBits);
			return true;
		}

		/// <summary>
		/// Applies owner, group and other permission bits to a file, returning whether they were
		/// applied. Does nothing on a platform that has no such bits.
		/// </summary>
		[CLSCompliant(false)]
		public static bool TrySetMode(string path, uint mode)
		{
			if (!Platform.IsUnix)
				return false;

			if (Syscall.chmod(path, (FilePermissions) mode & AllModeBits) == 0)
				return true;

			Trace.TraceInformation(
				$"Could not set the permissions of \"{path}\": {Stdlib.GetLastError()}");
			return false;
		}
	}
}
