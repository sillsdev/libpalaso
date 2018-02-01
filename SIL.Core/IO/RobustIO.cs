using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using SIL.Code;

namespace SIL.IO
{
	/// <summary>
	/// Provides a more robust version of various IO methods.
	/// The original intent of this class is to attempt to mitigate issues
	/// where we attempt IO but the file is locked by another application.
	/// Our theory is that some anti-virus software locks files while it scans them.
	/// </summary>
	public static class RobustIO
	{
		public static void DeleteDirectory(string path)
		{
			// This is not the same as DirectoryUtilities.DeleteDirectoryRobust(path)
			// as this version requires the directory to be empty.
			RetryUtility.Retry(() => Directory.Delete(path));
		}

		public static void DeleteDirectory(string path, bool recursive)
		{
			if(recursive)
			{
				var succeeded = DirectoryUtilities.DeleteDirectoryRobust(path);
				if(!succeeded)
					throw new IOException("Could not delete directory "+path);
			}
			else
				RetryUtility.Retry(() => Directory.Delete(path, false));
		}

		public static void MoveDirectory(string sourceDirName, string destDirName)
		{
			RetryUtility.Retry(() => Directory.Move(sourceDirName, destDirName));
		}

		public static XElement LoadXElement(string uri)
		{
			// Previously used RetryUtility on XElement.Load(uri). However, we had problems with this
			// in Bloom using some non-roman collection names...specifically, one involving the Northern Pashti
			// localization of 'books' (کتابونه)...see BL-5416. It seems that somewhere in the
			// implementation of Linq.XElement.Load() the path is converted to a URL and then back
			// to a path and something changes in that process so that a valid path passed to Load()
			// raises an invalid path exception. Reading the file directly and then parsing the string
			// works around this problem.
			var content = RobustFile.ReadAllText(uri, Encoding.UTF8);
			return XElement.Parse(content);
		}

		public static void SaveXElement(XElement xElement, string fileName)
		{
			RetryUtility.Retry(() => xElement.Save(fileName));
		}

		/// <summary>
		/// Save an Xml document. This should be equivalent to doc.Save(path) except for extra robustness (and slowness).
		/// </summary>
		/// <param name="doc"></param>
		/// <param name="path"></param>
		public static void SaveXml(XmlDocument doc, string path)
		{
			RetryUtility.Retry(() =>
			{
				using (var stream = RobustFile.Create(path))
				{
					doc.Save(stream);
					stream.Close();
				}
			});
		}
	}
}
