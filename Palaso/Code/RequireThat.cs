using System;
using System.Collections.Generic;
using System.Text;

namespace Palaso.Code
{
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
	}
}
