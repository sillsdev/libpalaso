using System.IO;
using Ionic.Zip;

namespace SIL.IO
{
	public static class ZipUtilities
	{
		public static void ExtractToDirectory(string zipFilePath, string destinationDir)
		{
			using (ZipFile zipFile = ZipFile.Read(zipFilePath))
				foreach (ZipEntry entry in zipFile)
				{
					byte[] data = new byte[entry.UncompressedSize];
					using (var stream = entry.OpenReader())
						stream.Read(data, 0, data.Length);

					string fileName = Path.Combine(destinationDir, entry.FileName);
					string directory = Path.GetDirectoryName(fileName);
					Directory.CreateDirectory(directory);

					using (FileStream output = new FileStream(fileName, FileMode.Create))
						output.Write(data, 0, data.Length);
				}
		}
	}
}
