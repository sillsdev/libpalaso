// Copyright (c) 2017-2018 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace SIL.Tests.ExtractCopyright
{
	[TestFixture]
	public class ExtractCopyrightTests
	{
		const string _readTestData = @"Format: http://www.debian.org/doc/packaging-manuals/copyright-format/1.0/
Upstream-Name: bloom-desktop-alpha
Upstream-Contact: Steve McConnel <stephen_mcconnel@sil.org>
Source: https://github.com/BloomBooks/BloomDesktop

Files: *
Copyright: 2015 SIL International
License: MIT

License: MIT
 Permission is hereby granted, free of charge, to any person obtaining a
 copy of this software and associated documentation files (the ""Software""),
 to deal in the Software without restriction, including without limitation
 the rights to use, copy, modify, merge, publish, distribute, sublicense,
 and/or sell copies of the Software, and to permit persons to whom the
 Software is furnished to do so, subject to the following conditions:
 .
 The above copyright notice and this permission notice shall be included
 in all copies or substantial portions of the Software.
 .
 THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND, EXPRESS
 OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
 IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
 CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
 TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
 SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.


Files: /usr/lib/bloom-desktop-alpha/Newtonsoft.Json.dll
Copyright: 2007 James Newton-King
License: MIT

Files: /usr/lib/bloom-desktop-alpha/DistFiles/pdf/*
Copyright: 2012 Mozilla Foundation
License: Apache-2.0

License: Apache-2.0
 Apache License
 Version 2.0, January 2004
 http://www.apache.org/licenses/
 .
 TERMS AND CONDITIONS FOR USE, REPRODUCTION, AND DISTRIBUTION
 .
 1. Definitions.
 .
 ""License"" shall mean the terms and conditions for use, reproduction, and
 distribution as defined by Sections 1 through 9 of this document.
 .
 [and more stuff for a couple of hundred lines we don't need for testing]

Files: /usr/lib/bloom-desktop-alpha/Vulcan.Uczniowie.HelpProvider.dll
Copyright: 2007 VULCAN
License: Free
 ""Feel free to use the code without any explicit permission.""
 (http://www.wiktorzychla.com/2007/08/context-help-made-easy-reloaded.html)
";

		[Test]
		public void ReadSimpleCopyrightFileWorks()
		{
			var copyrights = ReadCopyrightFileFromString(_readTestData);
			Assert.AreEqual(7, copyrights.Paragraphs.Count);

			Assert.AreEqual(4, copyrights.Paragraphs[0].Fields.Count);
			Assert.AreEqual("Format", copyrights.Paragraphs[0].Fields[0].Tag);
			Assert.AreEqual("http://www.debian.org/doc/packaging-manuals/copyright-format/1.0/", copyrights.Paragraphs[0].Fields[0].Value);
			Assert.AreEqual(0, copyrights.Paragraphs[0].Fields[0].FullDescription.Count);
			Assert.AreEqual("Upstream-Name", copyrights.Paragraphs[0].Fields[1].Tag);
			Assert.AreEqual("Upstream-Contact", copyrights.Paragraphs[0].Fields[2].Tag);
			Assert.AreEqual("Source", copyrights.Paragraphs[0].Fields[3].Tag);
			Assert.AreEqual("https://github.com/BloomBooks/BloomDesktop", copyrights.Paragraphs[0].Fields[3].Value);

			Assert.AreEqual(3, copyrights.Paragraphs[1].Fields.Count);
			Assert.AreEqual(1, copyrights.Paragraphs[2].Fields.Count);
			Assert.AreEqual(3, copyrights.Paragraphs[3].Fields.Count);
			Assert.AreEqual(3, copyrights.Paragraphs[4].Fields.Count);
			Assert.AreEqual(1, copyrights.Paragraphs[5].Fields.Count);

			Assert.AreEqual(3, copyrights.Paragraphs[6].Fields.Count);
			Assert.AreEqual("License", copyrights.Paragraphs[6].Fields[2].Tag);
			Assert.AreEqual("Free", copyrights.Paragraphs[6].Fields[2].Value);
			Assert.AreEqual(2, copyrights.Paragraphs[6].Fields[2].FullDescription.Count);
			Assert.AreEqual("\"Feel free to use the code without any explicit permission.\"", copyrights.Paragraphs[6].Fields[2].FullDescription[0]);
			Assert.AreEqual("(http://www.wiktorzychla.com/2007/08/context-help-made-easy-reloaded.html)", copyrights.Paragraphs[6].Fields[2].FullDescription[1]);
		}

		const string _writeTestData = @"Format: http://www.debian.org/doc/packaging-manuals/copyright-format/1.0/
Upstream-Name: MyProgram
Upstream-Contact: me@my.com
Source: https://github.com/myopensourcepersona/MyProgram

Files: *
Copyright: 2017 Me Myself
License: MIT
 Permission is hereby granted, free of charge, to any person obtaining a
 copy of this software and associated documentation files (the ""Software""),
 to deal in the Software without restriction, including without limitation
 the rights to use, copy, modify, merge, publish, distribute, sublicense,
 and/or sell copies of the Software, and to permit persons to whom the
 Software is furnished to do so, subject to the following conditions:
 .
 The above copyright notice and this permission notice shall be included
 in all copies or substantial portions of the Software.
 .
 THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND, EXPRESS
 OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 ETC, ETC, ETC.  ENOUGH FOR THIS TEST!
";

		[Test]
		public void WriteSimpleCopyrightFileWorks()
		{
			var copyrights = new SIL.ExtractCopyright.DebianControl();
			var para = new SIL.ExtractCopyright.DebianParagraph();
			copyrights.Paragraphs.Add(para);
			para.Fields.Add(new SIL.ExtractCopyright.DebianField("Format", "http://www.debian.org/doc/packaging-manuals/copyright-format/1.0/"));
			para.Fields.Add(new SIL.ExtractCopyright.DebianField("Upstream-Name", "MyProgram"));
			para.Fields.Add(new SIL.ExtractCopyright.DebianField("Upstream-Contact", "me@my.com"));
			para.Fields.Add(new SIL.ExtractCopyright.DebianField("Source", "https://github.com/myopensourcepersona/MyProgram"));

			para = new SIL.ExtractCopyright.DebianParagraph();
			copyrights.Paragraphs.Add(para);
			para.Fields.Add(new SIL.ExtractCopyright.DebianField("Files", "*"));
			para.Fields.Add(new SIL.ExtractCopyright.DebianField("Copyright", "2017 Me Myself"));
			var field = new SIL.ExtractCopyright.DebianField("License", "MIT");
			para.Fields.Add(field);
			field.FullDescription.Add("Permission is hereby granted, free of charge, to any person obtaining a");
			field.FullDescription.Add("copy of this software and associated documentation files (the \"Software\"),");
			field.FullDescription.Add("to deal in the Software without restriction, including without limitation");
			field.FullDescription.Add("the rights to use, copy, modify, merge, publish, distribute, sublicense,");
			field.FullDescription.Add("and/or sell copies of the Software, and to permit persons to whom the");
			field.FullDescription.Add("Software is furnished to do so, subject to the following conditions:");
			field.FullDescription.Add("");
			field.FullDescription.Add("The above copyright notice and this permission notice shall be included");
			field.FullDescription.Add("in all copies or substantial portions of the Software.");
			field.FullDescription.Add(".");
			field.FullDescription.Add("THE SOFTWARE IS PROVIDED \"AS IS\", WITHOUT WARRANTY OF ANY KIND, EXPRESS");
			field.FullDescription.Add("OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF");
			field.FullDescription.Add("ETC, ETC, ETC.  ENOUGH FOR THIS TEST!");

			var mem = new MemoryStream(4096);
			using (var writer = new StreamWriter(mem))
			{
				copyrights.WriteDebianParagraphs(writer);
			}

			Assert.AreEqual(_writeTestData.Replace("\r", "").Replace("\n", Environment.NewLine),
				Encoding.UTF8.GetString(mem.ToArray()));
		}

		[Test]
		public void CreateCopyrightFileWorks()
		{
			var copyrights = SIL.ExtractCopyright.CopyrightFile.CreateNewCopyrightFile("Program", "steve@wherever.org", "https://github.com/SteveWhomever/Program");

			var ackDict = Acknowledgements.AcknowledgementsProvider.CollectAcknowledgements();
			Assert.IsNotNull(ackDict);
			Assert.IsNotNull(ackDict.Keys);

			foreach (var key in ackDict.Keys)
			{
				copyrights.AddOrUpdateParagraphFromAcknowledgement(ackDict[key], "");
			}

			Assert.LessOrEqual(ackDict.Keys.Count + 3, copyrights.Paragraphs.Count, "We should have the two standard paragraphs, one paragraph per acknowledgement, and at least one license paragraph");

			// Collect the license paragraphs, assert that the MIT license has a paragraph, and test its content.
			var licenseParas = new List<SIL.ExtractCopyright.DebianParagraph>();
			SIL.ExtractCopyright.DebianParagraph mitLicensePara = null;
			foreach (var p in copyrights.Paragraphs)
			{
				if (p.Fields.Count == 1 && p.Fields[0].Tag == "License")
				{
					licenseParas.Add(p);
					if (p.Fields[0].Value == "MIT")
						mitLicensePara = p;
				}
			}
			Assert.LessOrEqual(1, licenseParas.Count);
			Assert.IsNotNull(mitLicensePara);
			Assert.AreEqual(SIL.ExtractCopyright.CopyrightFile.StandardMITLicense.Length, mitLicensePara.Fields[0].FullDescription.Count);
			for (int i = 0; i < SIL.ExtractCopyright.CopyrightFile.StandardMITLicense.Length; ++i)
			{
				Assert.AreEqual(SIL.ExtractCopyright.CopyrightFile.StandardMITLicense[i], mitLicensePara.Fields[0].FullDescription[i]);
			}
		}

		[Test]
		public void UpdateCopyrightFileWorks()
		{
			var copyrights = ReadCopyrightFileFromString(_readTestData);
			Assert.AreEqual(7, copyrights.Paragraphs.Count);
			var ackDict = SIL.Acknowledgements.AcknowledgementsProvider.CollectAcknowledgements();
			Assert.IsNotNull(ackDict);
			Assert.IsNotNull(ackDict.Keys);

			foreach (var key in ackDict.Keys)
			{
				copyrights.AddOrUpdateParagraphFromAcknowledgement(ackDict[key], "");
			}

			// Find all License paragraphs and ensure no duplicates
			var licensesDescribed = new List<string>();
			foreach (var p in copyrights.Paragraphs)
			{
				if (p.Fields.Count == 1 && p.Fields[0].Tag == "License")
				{
					CollectionAssert.DoesNotContain(licensesDescribed, p.Fields[0].Value);
					licensesDescribed.Add(p.Fields[0].Value);
				}
			}

			// Find all Files paragraphs and ensure no duplicates
			var fileParas = new List<string>();
			foreach (var p in copyrights.Paragraphs)
			{
				if (p.Fields.Count >= 3 && p.Fields[0].Tag == "Files")
				{
					CollectionAssert.DoesNotContain(fileParas, p.Fields[0].Value);
					// but we may have a file-only spec that would match a complete pathname,
					// we need to check for that case as well.
					foreach (var file in fileParas)
					{
						StringAssert.DoesNotEndWith("/" + p.Fields[0].Value, file);
					}
					fileParas.Add(p.Fields[0].Value);
				}
			}
		}

		const string _controlFileData =
			@"Source: bloom-desktop-alpha
Section: x11
Priority: extra
Maintainer: Stephen McConnel <stephen_mcconnel@sil.org>
Homepage: http://bloomlibrary.org/
Vcs-Git: git://github.com/BloomBooks/BloomDesktop.git
Vcs-Browser: https://github.com/BloomBooks/BloomDesktop
Standards-Version: 3.9.5
Build-Depends: debhelper (>= 9.0.0), cli-common-dev (>= 0.8),
 wget, unzip, desktop-file-utils,
 optipng, libsndfile1,
 mono4-sil, libgdiplus4-sil,
 libenchant-dev, libxklavier-dev, libdconf-dev,
 libicu-dev,
 libgtk2.0-cil (>= 2.12.10), libgtk2.0-dev (>= 2.14), libasound2-dev,
 icu-devtools | libicu-dev (<< 52)

Package: bloom-desktop-alpha
Architecture: any
Depends: ${shlibs:Depends}, ${cli:Depends}, ${misc:Depends},
 mono4-sil, libgdiplus4-sil, gtklp,
 chmsee, libtidy5,
 fonts-sil-andika-new-basic | fonts-sil-andikanewbasic,
 optipng, libsndfile1,
 wmctrl, lame
Suggests: art-of-reading
Replaces: bloom-desktop, bloom-desktop-beta
Conflicts: bloom-desktop-unstable
Description: Literacy materials development for language communities
 Bloom Desktop is an application that dramatically ""lowers the bar"" for
 language communities who want books in their own languages. Bloom delivers
 a low-training, high-output system where mother tongue speakers and their
 advocates work together to foster both community authorship and access to
 external material in the vernacular.
";

		[Test]
		public void ParseControlContentForValuesWorks()
		{
			var controlFile = ReadCopyrightFileFromString(_controlFileData) as SIL.ExtractCopyright.DebianControl;
			string programName = null;
			string contactEmail = null;
			string sourceUrl = null;
			SIL.ExtractCopyright.CopyrightFile.ParseControlContentForValues(controlFile, ref programName, ref contactEmail, ref sourceUrl);
			Assert.AreEqual("bloom-desktop-alpha", programName);
			Assert.AreEqual("Stephen McConnel <stephen_mcconnel@sil.org>", contactEmail);
			Assert.AreEqual("https://github.com/BloomBooks/BloomDesktop", sourceUrl);
		}

		readonly string[] _changelogLines = new string[] {
			"bloom-desktop-alpha (3.9.0) stable; urgency=medium",
			"",
			"  * Up the version number on the master (unstable/alpha) branch to 3.9.",
			"  * Rename the package to bloom-desktop-alpha.",
			"  * More bug fixes and enhancements: see the source repository log for details.",
			"",
			" -- Stephen McConnel <stephen_mcconnel@sil.org>  Wed, 18 Jan 2017 11:38:31 -0600",
			"",
			"bloom-desktop-unstable (3.8.0) stable; urgency=medium",
			"",
			"  * Up the version number on the master (unstable) branch to 3.8.",
			"  * More bug fixes and enhancements: see the source repository log for details.",
			"",
			" -- Stephen McConnel <stephen_mcconnel@sil.org>  Fri, 27 May 2016 10:39:04 -0500",
			""
		};

		[Test]
		public void ParseChangelogContentForValuesWorks()
		{
			string programName = null;
			string contactEmail = null;
			SIL.ExtractCopyright.CopyrightFile.ParseChangelogContentForValues(_changelogLines, ref programName, ref contactEmail);
			Assert.AreEqual("bloom-desktop-alpha", programName);
			Assert.AreEqual("Stephen McConnel <stephen_mcconnel@sil.org>", contactEmail);

			// ignore both if both are already set
			programName = "bloom-desktop";
			contactEmail = "steve@sil.org";
			SIL.ExtractCopyright.CopyrightFile.ParseChangelogContentForValues(_changelogLines, ref programName, ref contactEmail);
			Assert.AreEqual("bloom-desktop", programName);
			Assert.AreEqual("steve@sil.org", contactEmail);

			// ignore contactEmail if already set, set programName if null
			programName = null;
			contactEmail = "steve@sil.org";
			SIL.ExtractCopyright.CopyrightFile.ParseChangelogContentForValues(_changelogLines, ref programName, ref contactEmail);
			Assert.AreEqual("bloom-desktop-alpha", programName);
			Assert.AreEqual("steve@sil.org", contactEmail);

			// ignore programName if already set, set contactEmail if empty
			programName = "bloom-desktop";
			contactEmail = String.Empty;
			SIL.ExtractCopyright.CopyrightFile.ParseChangelogContentForValues(_changelogLines, ref programName, ref contactEmail);
			Assert.AreEqual("bloom-desktop", programName);
			Assert.AreEqual("Stephen McConnel <stephen_mcconnel@sil.org>", contactEmail);
		}

		SIL.ExtractCopyright.CopyrightFile ReadCopyrightFileFromString(string content)
		{
			var copyrights = new SIL.ExtractCopyright.CopyrightFile();
			var mem = new MemoryStream(Encoding.UTF8.GetBytes(content));
			using (var reader = new StreamReader(mem))
			{
				copyrights.ReadDebianParagraphs(reader);
			}
			return copyrights;
		}
	}
}