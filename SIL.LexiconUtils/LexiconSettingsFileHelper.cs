using System;
using System.IO;

namespace SIL.LexiconUtils
{
	/// <summary>
	/// Provides utility functions for generating the lexicon settings filenames.
	/// </summary>
	public static class LexiconSettingsFileHelper
	{
		public const string LexiconProjectSettingsExtension = ".lpsx";
		public const string LexiconUserSettingsExtension = ".lusx";
		public const string SharedSettingsFolder = "SharedSettings";
		public const string LexiconProjectSettingsFilename = "LexiconProjectSettings" + LexiconProjectSettingsExtension;

		public static string GetSharedSettingsPath(string basePath)
		{
			return Path.Combine(basePath, SharedSettingsFolder);
		}

		public static string GetLexiconProjectSettingsPath(string basePath)
		{
			return Path.Combine(GetSharedSettingsPath(basePath), LexiconProjectSettingsFilename);
		}

		public static string GetLexiconUserSettingsPath(string basePath)
		{
			return Path.Combine(GetSharedSettingsPath(basePath), Environment.UserName + LexiconUserSettingsExtension);
		}
	}
}
