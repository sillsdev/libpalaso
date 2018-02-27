﻿using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SIL.Acknowledgements;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("SIL.Archiving")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("2edd981c-375e-4461-8412-2f62884ce827")]
[assembly: InternalsVisibleTo("SIL.Archiving.Tests")]

[assembly: Acknowledgement("Ionic.Zip", Copyright = "Dino Chiesa", Url = "http://www.codeplex.com/DotNetZip",
	LicenseUrl = "https://opensource.org/licenses/MS-PL", Location = "./Ionic.Zip.dll",
	Html = "<li><a href='http://www.codeplex.com/DotNetZip'>Ionic.Zip</a> © Dino Chiesa 2006-2011 (<a href='https://opensource.org/licenses/MS-PL'>MS-PL</a>) - a library for handling zip archives (Flavor=Retail)</li>")]
[assembly: Acknowledgement("L10NSharp", Url = "https://github.com/sillsdev/l10nsharp/",
	Copyright = "Copyright © SIL International 2010-2014", LicenseUrl = "https://opensource.org/licenses/MIT",
	Location = "./L10NSharp.dll")]