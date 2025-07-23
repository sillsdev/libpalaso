// Copyright (c) 2025 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using SIL.Acknowledgements;
using static System.String;

namespace SIL.ExtractCopyright
{
	/// <summary>
	/// This class is used to manipulate Debian copyright files.
	/// See https://www.debian.org/doc/packaging-manuals/copyright-format/1.0/.
	/// </summary>
	public class CopyrightFile : DebianControl
	{
		public enum ExitValue {
			Okay = 0,
			NoDebianFolder,
			NoAssemblyFolder,
			NoPrefixFolder
		}

		public CopyrightFile()
		{
		}

		public CopyrightFile(string filename) : base(filename)
		{
		}

		/// <summary>
		/// Create or update the copyright file in the given debian folder.
		/// </summary>
		/// <param name="debianFolder">Debian folder.</param>
		/// <param name="prefixFolder">Prefix folder.</param>
		public static int CreateOrUpdateCopyrightFile(string debianFolder, string prefixFolder)
		{
			CopyrightFile copyrights;
			if (!File.Exists(Path.Combine(debianFolder, "copyright")))
			{
				string programName = null;
				string contactEmail = null;
				string sourceUrl = null;
				// First try to read the primary information from a "Source" paragraph in the control file.
				var controlFile = new DebianControl(Path.Combine(debianFolder, "control"));
				ParseControlContentForValues(controlFile, ref programName, ref contactEmail, ref sourceUrl);
				// If necessary, try to read some primary information from the changelog file.
				if (IsNullOrEmpty(programName) || IsNullOrEmpty(contactEmail))
				{
					var lines = File.ReadAllLines(Path.Combine(debianFolder, "changelog"), Encoding.UTF8);
					ParseChangelogContentForValues(lines, ref programName, ref contactEmail);
				}
				// If we can't derive any information, flag it as unknown.
				if (IsNullOrEmpty(programName))
					programName = "UNKNOWN";
				if (IsNullOrEmpty(contactEmail))
					contactEmail = "UNKNOWN";
				if (IsNullOrEmpty(sourceUrl))
					sourceUrl = "UNKNOWN";
				copyrights = CreateNewCopyrightFile(programName, contactEmail, sourceUrl);
				copyrights._filepath = Path.Combine(debianFolder, "copyright");
				Console.WriteLine("ExtractCopyright: creating new file at \"{0}\"", copyrights._filepath);
			}
			else
			{
				// Initialize from an existing copyright file.
				copyrights = new CopyrightFile(Path.Combine(debianFolder, "copyright"));
				Console.WriteLine("ExtractCopyright: updating existing file at \"{0}\"", copyrights._filepath);
			}

			var ackDict = AcknowledgementsProvider.CollectAcknowledgements();
			foreach (var key in ackDict.Keys)
				copyrights.AddOrUpdateParagraphFromAcknowledgement(ackDict[key], prefixFolder);

			copyrights.WriteDebianControlFile();

			return (int)ExitValue.Okay;
		}

		internal static void ParseControlContentForValues(DebianControl controlFile, ref string programName, ref string contactEmail, ref string sourceUrl)
		{
			DebianParagraph sourcePara = null;
			foreach (var para in controlFile.Paragraphs)
			{
				if (para.Fields.Count > 0 && para.Fields[0].Tag == "Source")
				{
					sourcePara = para;
					break;
				}
			}
			if (sourcePara != null)
			{
				var sourceField = sourcePara.FindField("Source");
				programName = sourceField.Value;
				var maintainerField = sourcePara.FindField("Maintainer");
				if (maintainerField != null)
					contactEmail = maintainerField.Value;
				// Find the "source URL" from
				// 1) Vcs-Browser (url to see source code repository in the browser), or if that isn't provided
				// 2) HomePage (url to see some sort of project home page in the browser), or if that isn't provided
				// 3) Vcs-Git (url to clone git repository on local machine)
				var urlField = sourcePara.FindField("Vcs-Browser") ??
					sourcePara.FindField("HomePage") ??
					sourcePara.FindField("Vcs-Git");
				if (urlField != null)
					sourceUrl = urlField.Value;
			}
		}

		internal static void ParseChangelogContentForValues(string[] lines, ref string programName, ref string contactEmail)
		{
			// The changelog format is specified at https://www.debian.org/doc/debian-policy/ch-source.html#s-dpkgchangelog
			for (int i = 0; i < lines.Length; ++i)
			{
				// The first line of an entry in the changelog looks like this:
				// "bloom-desktop-alpha (3.9.0) stable; urgency=medium"
				if (IsNullOrEmpty(programName) && lines[i].Contains(" urgency=") && lines[i].Contains(";"))
				{
					var headerPieces = lines[i].Trim().Split(' ');
					programName = headerPieces[i];
				}
				// The closing line of an entry in the changelog looks like this:
				// " -- Stephen McConnel <stephen_mcconnel@sil.org>  Wed, 18 Jan 2017 11:38:31 -0600"
				// the two spaces following the <email@address> are significant
				else if (IsNullOrEmpty(contactEmail) && lines[i].StartsWith(" -- ") && lines[i].Contains("@"))
				{
					var line = lines[i].Substring(4).Trim();
					var idx = line.IndexOf("  ", StringComparison.Ordinal);
					if (idx > 0)
						contactEmail = line.Substring(0, idx);
				}
				if (!IsNullOrEmpty(programName) && !IsNullOrEmpty(contactEmail))
					return;
			}
		}

		/// <summary>
		/// Creates the data structure for a new copyright file (in memory only).
		/// </summary>
		/// <returns>A new CopyrightFile object</returns>
		/// <remarks>
		/// The format of the copyright file is defined at https://www.debian.org/doc/packaging-manuals/copyright-format/1.0/.
		/// </remarks>
		public static CopyrightFile CreateNewCopyrightFile(string programName, string contactEmail, string sourceUrl)
		{
			var copyrights = new CopyrightFile();
			var para = new DebianParagraph();
			copyrights.Paragraphs.Add(para);
			para.Fields.Add(new DebianField("Format", "http://www.debian.org/doc/packaging-manuals/copyright-format/1.0/"));
			para.Fields.Add(new DebianField("Upstream-Name", programName));
			para.Fields.Add(new DebianField("Upstream-Contact", contactEmail));
			para.Fields.Add(new DebianField("Source", sourceUrl));

			// REVIEW: can we assume these values?
			var programCopyright = Format("{0} SIL Global", DateTime.Now.Year);
			var programLicense = "MIT";

			para = new DebianParagraph();
			copyrights.Paragraphs.Add(para);
			para.Fields.Add(new DebianField("Files", "*"));
			para.Fields.Add(new DebianField("Copyright", programCopyright));
			para.Fields.Add(new DebianField("License", programLicense));
			if (programLicense == "MIT")
				copyrights.AddLicenseParagraphIfNeeded(programLicense, StandardMITLicense);

			return copyrights;
		}

		internal void AddOrUpdateParagraphFromAcknowledgement(AcknowledgementAttribute ack, string prefix)
		{
			string fileSpec;
			if (!IsNullOrEmpty(ack.Location))
			{
				fileSpec = ack.Location;
				if (!fileSpec.StartsWith("/"))
					fileSpec = Path.GetFileName(fileSpec);
			}
			else
			{
				fileSpec = ack.Key;
			}
			if (!IsNullOrEmpty(prefix))
				fileSpec = Path.Combine(prefix, fileSpec);

			if (IsWindowsSpecific(Path.GetFileName(fileSpec)))
				return;

			foreach (var p in Paragraphs)
			{
				if (p.Fields.Count >= 3 && p.Fields[0].Tag == "Files")
				{
					if (p.Fields[0].Value == fileSpec ||
						Path.GetFileName(p.Fields[0].Value) == fileSpec ||
						Path.GetFileName(fileSpec) == p.Fields[0].Value)
					{
						UpdateParagraphFromAcknowledgement(p, ack);
						return;
					}
				}
			}

			var para = new DebianParagraph();
			Paragraphs.Add(para);
			para.Fields.Add(new DebianField("Files", fileSpec));

			ExtractCopyrightInformation(ack.Copyright, out var person, out var year);
			var copyright = year + " " + person;
			para.Fields.Add(new DebianField("Copyright", copyright));

			ExtractLicenseInformation(ack.LicenseUrl, out var shortLicense, out var longLicense);
			para.Fields.Add(new DebianField("License", shortLicense));

			if (!IsNullOrEmpty(ack.Url))
				para.Fields.Add(new DebianField("Comment", "URL = " + ack.Url));

			if (shortLicense != "????" && longLicense.Count > 0)
			{
				AddLicenseParagraphIfNeeded(shortLicense, longLicense);
			}
		}

		private static bool IsWindowsSpecific(string name)
		{
			switch (name)
			{
			// Windows specific assemblies
			case "irrKlang.NET4.dll":
			case "NAudio.dll":
			case "KeymanLink.dll":
			case "Commons.Xml.Relaxng.dll":
			case "gtk-sharp.dll":
				return true;
			default:
				return false;
			}
		}

		internal void AddLicenseParagraphIfNeeded(string license, IEnumerable<string> detailLines)
		{
			// Check whether we need a license paragraph, and add it if needed.
			foreach (var p in Paragraphs)
			{
				if (p.Fields.Count == 1 && p.Fields[0].Tag == "License" && p.Fields[0].Value == license)
					return;		// license already written out
			}
			var para = new DebianParagraph();
			Paragraphs.Add(para);
			var field = new DebianField("License", license);
			para.Fields.Add(field);
			foreach (var line in detailLines)
				field.FullDescription.Add(line);

		}

		private void UpdateParagraphFromAcknowledgement(DebianParagraph para,
			AcknowledgementAttribute ack)
		{
			ExtractCopyrightInformation(ack.Copyright, out var person, out var year);
			if (year == "????")
				return;			// we don't know if this information is newer or not
			var copyrightField = para.FindField("Copyright");
			if (copyrightField != null)
			{
				ExtractCopyrightInformation(copyrightField.Value, out var prevYear, out _);
				if (prevYear == "????" || prevYear.CompareTo(year) < 0)
				{
					copyrightField.Value = year + " " + person;
				}
				else
				{
					return;		// our current information is as new or newer.
				}
			}

			ExtractLicenseInformation(ack.LicenseUrl, out var shortLicense, out var longLicense);
			if (shortLicense != "????")
			{
				var licenseField = para.FindField("License");
				if (licenseField != null)
					licenseField.Value = shortLicense;
				if (longLicense.Count > 0)
					AddLicenseParagraphIfNeeded(shortLicense, longLicense);
			}

			if (!IsNullOrEmpty(ack.Url))
			{
				var commentField = para.FindField("Comment");
				if (commentField == null)
					para.Fields.Add(new DebianField("Comment", "URL = " + ack.Url));
				else
					commentField.Value = "URL = " + ack.Url;
			}
		}

		private void ExtractCopyrightInformation(string original, out string person, out string year)
		{
			person = "????";
			year = "????";
			if (original == null)
				return;
			// remove explicit copyright marking
			var copyright = original.Replace("Copyright", "");
			copyright = copyright.Replace("(C)", "");
			copyright = copyright.Replace("(c)", "");
			copyright = copyright.Replace("©", "");
			// extract year and person if present
			var match = Regex.Match(copyright, "([0-9][0-9][0-9][0-9][-0-9]*)");
			if (match.Success)
			{
				year = copyright.Substring(match.Index, match.Length);
				person = copyright.Remove(match.Index, match.Length);
			}
			else
			{
				person = copyright;
			}
			person = person.Trim();
			if (IsNullOrEmpty(person))
				person = "????";
		}

		private void ExtractLicenseInformation(string original, out string shortLicense, out List<string> longLicense)
		{
			shortLicense = "????";
			longLicense = new List<string>();
			if (!IsNullOrEmpty(original))
			{
				shortLicense = original;
				var idx = shortLicense.LastIndexOf('/');
				if (idx >= 0)
					shortLicense = shortLicense.Substring(idx + 1);
				if (File.Exists(Path.Combine("/usr/share/common-licenses", shortLicense)))
				{
					longLicense.Add(Path.Combine("/usr/share/common-licenses", shortLicense));
				}
				else if (shortLicense == "MIT")
				{
					for (int i = 0; i < StandardMITLicense.Length; ++i)
						longLicense.Add(StandardMITLicense[i]);
				}
				else if (shortLicense != original)
				{
					longLicense.Add(original);
				}
			}
		}

		/// <summary>
		/// The standard MIT license used for SIL software.
		/// </summary>
		public static string[] StandardMITLicense = {
			"Permission is hereby granted, free of charge, to any person obtaining a",
			"copy of this software and associated documentation files (the \"Software\"),",
			"to deal in the Software without restriction, including without limitation",
			"the rights to use, copy, modify, merge, publish, distribute, sublicense,",
			"and/or sell copies of the Software, and to permit persons to whom the",
			"Software is furnished to do so, subject to the following conditions:",
			"",
			"The above copyright notice and this permission notice shall be included",
			"in all copies or substantial portions of the Software.",
			"",
			"THE SOFTWARE IS PROVIDED \"AS IS\", WITHOUT WARRANTY OF ANY KIND, EXPRESS",
			"OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF",
			"MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.",
			"IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY",
			"CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,",
			"TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE",
			"SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE."
		};
	}
}

