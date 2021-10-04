using System;
using System.Configuration;
using System.IO;
using System.Threading;
using NUnit.Framework;
using SIL.Settings;

namespace SIL.TestUtilities
{
	public class TestUtilities
	{
		public static string GetTempTestDirectory()
		{
			DirectoryInfo dirProject = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));
			return  dirProject.FullName;
		}

		public static void DeleteFolderThatMayBeInUse(string folder)
		{
			if (Directory.Exists(folder))
			{
				try
				{
					Directory.Delete(folder, true);
				}
				catch (Exception e)
				{
					try
					{
						Console.WriteLine(e.Message);
						//maybe we can at least clear it out a bit
						string[] files = Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories);
						foreach (string s in files)
						{
							File.SetAttributes(s, FileAttributes.Normal); //get past readonly
							File.Delete(s);
						}
						//sleep and try again (seems to work)
						Thread.Sleep(1000);
						Directory.Delete(folder, true);
					}
					catch (Exception)
					{
					}
				}
			}
		}

		/// <summary>
		/// Verifies that each property has its provider set to <see cref="CrossPlatformSettingsProvider"/> or a subclass
		/// </summary>
		public static void ValidateProperties(ApplicationSettingsBase settings)
		{
			foreach (SettingsProperty property in settings.Properties)
			{
				Assert.That(property.Provider, Is.AssignableTo<CrossPlatformSettingsProvider>(),
					$"Property '{property.Name}' needs the Provider string set to {typeof(CrossPlatformSettingsProvider)}");
			}
		}
	}
}