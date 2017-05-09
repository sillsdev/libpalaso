using System;
using System.Collections.Generic;
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
				});
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
				});
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
				});
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
				});
		}
	}
}
