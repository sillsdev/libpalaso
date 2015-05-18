using System;
using System.IO;

namespace SIL.Lexicon
{
	/// <summary>
	/// Provides utility functions for generating the lexicon settings filenames.
	/// </summary>
	public static class LexiconSettingsFileHelper
	{
		public const string ProjectLexiconSettingsExtension = ".plsx";
		public const string UserLexiconSettingsExtension = ".ulsx";
		public const string SharedSettingsFolder = "SharedSettings";
		public const string ProjectLexiconSettingsFilename = "LexiconSettings" + ProjectLexiconSettingsExtension;

		public static string GetSharedSettingsPath(string basePath)
		{
			return Path.Combine(basePath, SharedSettingsFolder);
		}

		public static string GetProjectLexiconSettingsPath(string basePath)
		{
			return Path.Combine(GetSharedSettingsPath(basePath), ProjectLexiconSettingsFilename);
		}

		public static string GetUserLexiconSettingsPath(string basePath)
		{
			return Path.Combine(GetSharedSettingsPath(basePath), Environment.UserName + UserLexiconSettingsExtension);
		}
	}
}
