using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Xsl;
using SIL.Lift.Parsing;
using SIL.Lift.Validation;

namespace SIL.Lift.Migration
{
	///<summary>
	/// This class implements migrating from an older version of LIFT to the current version of
	/// LIFT.
	///</summary>
	public class Migrator
	{
		///<summary>
		/// Check whether the given file needs to be migrated.
		///</summary>
		public static bool IsMigrationNeeded(string pathToLift)
		{
			return (Validator.GetLiftVersion(pathToLift) != Validator.LiftVersion);
		}

		/// <summary>
		/// Creates a new file migrated to the current version
		/// </summary>
		/// <returns>the path to the  migrated one, in the same directory</returns>
		public static string MigrateToLatestVersion(string pathToOriginalLift)
		{
			if (!IsMigrationNeeded(pathToOriginalLift))
			{
				throw new ArgumentException("This file is already the most current version. Use Migrator.IsMigrationNeeded() first to determine if migration is needed.");
			}

			string sourceVersion = Validator.GetLiftVersion(pathToOriginalLift);

			string migrationSourcePath = pathToOriginalLift;
			while (sourceVersion != Validator.LiftVersion)
			{
				string xslName = GetNameOfXsltWhichConvertsFromVersion(sourceVersion);
				string targetVersion = xslName.Split(new[] { '-' })[2];
				targetVersion = targetVersion.Remove(targetVersion.LastIndexOf('.'));
				string migrationTargetPath = String.Format("{0}-{1}", pathToOriginalLift, targetVersion);
				DoOneMigrationStep(xslName, migrationSourcePath, migrationTargetPath);
				if (migrationSourcePath != pathToOriginalLift)
					File.Delete(migrationSourcePath);
				migrationSourcePath = migrationTargetPath;
				sourceVersion = targetVersion;
			}
			return migrationSourcePath;
		}



		private static void DoOneMigrationStep(string xslName, string migrationSourcePath, string migrationTargetPath)
		{
			Stream xslstream = Assembly.GetExecutingAssembly().GetManifestResourceStream(xslName);
			if (xslstream != null)
			{
				XslCompiledTransform xsl = new XslCompiledTransform();
				xsl.Load(new XmlTextReader(xslstream));
				xsl.Transform(migrationSourcePath, migrationTargetPath);
			}
		}

		///<summary>
		/// Do a reverse data migration from LIFT 0.13 to LIFT 0.12 as output by FieldWorks
		/// Language Explorer.
		///</summary>
		public static void ReverseMigrateFrom13ToFLEx12(string fromPath, string toPath)
		{
			DoOneMigrationStep("SIL.Lift.Migration.ReverseLIFT-0.13-0.12.xsl", fromPath, toPath);

			//now strip the actual text off our our semantic domains
//            string pattern = @"(semantic_domain'\s*value\s*=\s*'(\d\.)*\d)(\w|\s|,)*(')";
			string pattern = @"(semantic_domain'\s*value\s*=\s*'(\d\.)*\d)[change]*(')";
			char qt = '"';
			pattern = pattern.Replace("'", "[" + qt + ",']");
			pattern = pattern.Replace("change", "^'\"");
			GrepFile(toPath, pattern, "$1$3");

		}

		private static void GrepFile(string inputPath, string pattern, string replaceWith)
		{
			Regex regex = new Regex(pattern, RegexOptions.Compiled);
//
//                        var m = regex.Match(" <trait name='semantic_domain' value='2.5 something or other'/>");
//                        char qt = '"';
//                        string pattern2 = @"(semantic_domain'\s*value\s*=\s*'(\d\.)*\d)(\w|\s)*(')";
//                        pattern2 = pattern2.Replace("'", "[" + qt + ",']");
//                        Regex regex2 = new Regex(pattern2, RegexOptions.Multiline);
//
//                        var input = @" <trait name='semantic_domain' value='2.5 something'/> <trait name='semantic_domain' value='3.3 other'/>";
//                        input = input.Replace('\'', '"');
//                        var m2 = regex2.Match(input);
//                        var z = regex2.Replace(input, "$1$4");
//
			string tempPath = inputPath + ".tmp";

			using (StreamReader reader = File.OpenText(inputPath))
			{
				using (StreamWriter writer = new StreamWriter(tempPath))
				{
					while (!reader.EndOfStream)
					{
						// ReSharper disable AssignNullToNotNullAttribute
						writer.WriteLine(regex.Replace(reader.ReadLine(), replaceWith));
						// ReSharper restore AssignNullToNotNullAttribute
					}
					writer.Close();
				}
				reader.Close();
			}
			//string backupPath = GetUniqueFileName(inputPath);
			string backupPath = inputPath + ".bak";

			File.Replace(tempPath, inputPath, backupPath);

		}



		private static string GetNameOfXsltWhichConvertsFromVersion(string sourceVersion)
		{
			string[] resources = typeof(LiftMultiText).Assembly.GetManifestResourceNames();
			string xslName = null;
			foreach (string name in resources)
			{
				if (name.EndsWith(".xsl") && name.StartsWith("SIL.Lift.Migration.LIFT-" + sourceVersion + "-"))
				{
					xslName = name;
					break;
				}
			}
			if (xslName == null)
				throw new LiftFormatException(string.Format("This program is not able to convert from the version of this lift file, {0}, to the version this program uses, {1}", sourceVersion, Validator.LiftVersion));
			return xslName;
		}
	}
}