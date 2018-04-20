using System;
using System.IO.Compression;

namespace SIL.IO
{
	[Obsolete("Use System.IO.Compression.ZipFile class")]
	public static class ZipUtilities
	{
		public static void ExtractToDirectory(string zipFilePath, string destinationDir)
		{
			ZipFile.ExtractToDirectory(zipFilePath, destinationDir);
		}
	}
}
