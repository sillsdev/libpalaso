﻿// Copyright (c) 2011-2015 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.IO;

namespace SIL.Windows.Forms.Pathway
{
	public static class PathwayUtils
	{
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the directory for the Pathway application or string.Empty if the directory name
		/// is not in the registry.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static string PathwayInstallDirectory
		{
			get
			{
				object regObj;
				if (RegistryHelper.RegEntryValueExists(RegistryHelper.CompanyKey, SilSubKey.Pathway,
					"PathwayDir", out regObj))
				{
					return (string)regObj;
				}

				// Some broken Windows machines can have trouble accessing HKLM (LT-15158).
				if (RegistryHelper.CompanyKeyLocalMachine == null)
				{
					string companyName = "SIL";
					string defaultDir = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), companyName), "Pathway7");
					if (Directory.Exists(defaultDir))
						return defaultDir;
					else
						return string.Empty;
				}

				if (RegistryHelper.RegEntryValueExists(RegistryHelper.CompanyKeyLocalMachine, SilSubKey.Pathway,
					"PathwayDir", out regObj))
				{
					return (string) regObj;
				}
				return string.Empty;
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets a value indicating whether SIL Pathway is installed for Scripture.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static bool IsPathwayForScrInstalled
		{
			get
			{
				return CheckPathwayInstallation(true);
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets a value indicating whether SIL Pathway is installed.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static bool IsPathwayInstalled
		{
			get
			{
				return CheckPathwayInstallation(false);
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Checks the Pathway installation for files.
		/// </summary>
		/// <param name="fSupportScripture">if set to <c>true</c> check the installation to contain
		/// files to support export of Scripture to Pathway; <c>false</c> to check for a
		/// standard Pathway installation.</param>
		/// <returns><c>true</c> if Pathway is installed as specified</returns>
		/// ------------------------------------------------------------------------------------
		private static bool CheckPathwayInstallation(bool fSupportScripture)
		{
			string pathwayDirectory = PathwayInstallDirectory;
			if (string.IsNullOrEmpty(pathwayDirectory))
				return false;

			string psExportDllPath = Path.Combine(pathwayDirectory, "PsExport.dll");
			if (!File.Exists(psExportDllPath))
				return false;

			string scrFilePath = Path.Combine(pathwayDirectory, "ScriptureStyleSettings.xml");
			if (fSupportScripture && !File.Exists(scrFilePath))
				return false;

			return true;
		}
	}
}
