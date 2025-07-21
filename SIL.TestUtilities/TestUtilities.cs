using JetBrains.Annotations;
using NUnit.Framework;
using System;
using System.IO;
using System.Threading;

namespace SIL.TestUtilities
{
	public class TestUtilities
	{
		[PublicAPI]
		public static string GetTempTestDirectory()
		{
			var dirProject = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));
			return dirProject.FullName;
		}

		public static void DeleteFolderThatMayBeInUse(string folder)
		{
			if (!Directory.Exists(folder))
				return;
			
			try
			{
				Directory.Delete(folder, true);
			}
			catch (Exception e)
			{
				try
				{
					Console.WriteLine(e.Message);
					// Maybe we can at least clear it out a bit.
					var files = Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories);
					foreach (var s in files)
					{
						File.SetAttributes(s, FileAttributes.Normal); // Get past readonly
						File.Delete(s);
					}
					// Sleep and try again (seems to work)
					Thread.Sleep(1000);
					Directory.Delete(folder, true);
				}
				catch (Exception e2)
				{
					Console.WriteLine(e2);
				}
			}
		}
	}
}