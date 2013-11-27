using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Palaso.Code
{
	/// <summary>
	/// Allows statements like:
	/// 	RequireThat.Directory(dir).Exists();
	///		RequireThat.Directory(dir).DoesNotExist();
	///		RequireThat.File(path).DoesNotExist();
	///		RequireThat.File(path).Directory.Exists();
	/// </summary>
	public static class RequireThat
	{
		public static DirectoryChecker Directory(string directory)
		{
			return new DirectoryChecker(directory);
		}

		public class DirectoryChecker
		{
			private readonly string _directory;

			public DirectoryChecker(string directory)
			{
				_directory = directory;
			}

			public void Exists()
			{
				if (!System.IO.Directory.Exists(_directory))
					throw new ArgumentException("The path '" + _directory + "' does not exist.");
			}
			public void DoesNotExist()
			{
				if (System.IO.Directory.Exists(_directory))
					throw new ArgumentException("The path '" + _directory + "' must not already exist.");
			}

			public DirectoryChecker Parent()
			{
				return new DirectoryChecker(System.IO.Directory.GetParent(_directory).FullName);
			}
		}

		public static FileChecker File(string path)
		{
			return new FileChecker(path);
		}

		public class FileChecker
		{
			private readonly string _path;

			public FileChecker(string path)
			{
				_path = path;
			}

			public void Exists()
			{
				if (!System.IO.File.Exists(_path))
					throw new ArgumentException("The file '" + _path + "' does not exist.");
			}
			public void DoesNotExist()
			{
				if (System.IO.File.Exists(_path))
					throw new ArgumentException("The file '" + _path + "' must not already exist.");
			}

			/// <summary>
			/// Allows us to say RequireThat.File("foo/blah.txt").Directory.Exists();
			/// </summary>
			/// <returns></returns>
			public DirectoryChecker Directory
			{
				get
				{
					return new DirectoryChecker(Path.GetDirectoryName(_path));
				}
			}
		}
	}
}
