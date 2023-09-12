using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using SIL.Code;

namespace SIL.IO
{
	/// <summary>
	/// Provides a more robust version of various IO methods for saving images.
	/// The original intent of this class is to attempt to mitigate issues
	/// where we attempt IO but the file is locked by another application.
	/// Our theory is that some anti-virus software locks files while it scans them.
	/// </summary>
	public static class RobustImageIO
	{
		public static void SaveImage(Image image, string fileName)
		{
			RetryUtility.Retry(() => image.Save(fileName),
				RetryUtility.kDefaultMaxRetryAttempts,
				RetryUtility.kDefaultRetryDelay,
				new HashSet<Type>
				{
					Type.GetType("System.IO.IOException"),
					Type.GetType("System.Runtime.InteropServices.ExternalException")
				}, memo: $"SaveImage {fileName}");
		}

		public static void SaveImage(Image image, string fileName, ImageFormat format)
		{
			RetryUtility.Retry(() => image.Save(fileName, format),
				RetryUtility.kDefaultMaxRetryAttempts,
				RetryUtility.kDefaultRetryDelay,
				new HashSet<Type>
				{
					Type.GetType("System.IO.IOException"),
					Type.GetType("System.Runtime.InteropServices.ExternalException")
				}, memo: $"SaveImage {fileName}");
		}

		public static void SaveImage(Image image, Stream stream, ImageFormat format)
		{
			RetryUtility.Retry(() => image.Save(stream, format),
				RetryUtility.kDefaultMaxRetryAttempts,
				RetryUtility.kDefaultRetryDelay,
				new HashSet<Type>
				{
					Type.GetType("System.IO.IOException"),
					Type.GetType("System.Runtime.InteropServices.ExternalException")
				}, memo: $"SaveImage <stream>");
		}

		public static void SaveImage(Image image, string fileName, ImageCodecInfo jpgEncoder, EncoderParameters parameters)
		{
			RetryUtility.Retry(() => image.Save(fileName, jpgEncoder, parameters),
				RetryUtility.kDefaultMaxRetryAttempts,
				RetryUtility.kDefaultRetryDelay,
				new HashSet<Type>
				{
					Type.GetType("System.IO.IOException"),
					Type.GetType("System.Runtime.InteropServices.ExternalException")
				}, memo: $"SaveImage {fileName}");
		}

		/// <summary>
		/// Read a bitmap image from a file.  The file must be known to exist before calling this method.
		/// </summary>
		/// <remarks>
		/// Creating an Image from a file (even using a FileStream) locks that file until the image is
		/// disposed of.  Therefore, we copy the image and dispose of the original.  On Windows, using
		/// Image.FromFile leaks file handles, so we load using a FileStream instead.  For details, see
		/// the last answer to https://stackoverflow.com/questions/16055667/graphics-drawimage-out-of-memory-exception
		/// </remarks>
		public static Image GetImageFromFile(string path)
		{
			Debug.Assert(RobustFile.Exists(path), String.Format("{0} does not exist for RobustImageIO.GetImageFromFile()?!", path));
			return RetryUtility.Retry(() =>
				GetImageFromFileInternal(path),
				RetryUtility.kDefaultMaxRetryAttempts,
				RetryUtility.kDefaultRetryDelay,
				new HashSet<Type>
				{
					Type.GetType("System.IO.IOException"),
					Type.GetType("System.Runtime.InteropServices.ExternalException")
				},
				memo: $"GetImageFromFile {path}");
		}

		private static Image GetImageFromFileInternal(string path)
		{
			using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			{
				using (var image = new Bitmap(stream))
				{
					return new Bitmap(image);
				}
			}
		}
	}
}
