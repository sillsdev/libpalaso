using System.Reflection;
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

[assembly: Acknowledgement("DotNetZip", Copyright = "Henrik Feldt/Dino Chiesa", Url = "https://github.com/haf/DotNetZip.Semverd",
	LicenseUrl = "https://raw.githubusercontent.com/haf/DotNetZip.Semverd/master/LICENSE", Location = "./DotNetZip.dll",
	Html = "<li><a href='https://github.com/haf/DotNetZip.Semverd'>DotNetZip</a> © Henrik Feldt/Dino Chiesa 2006-2018 (<a href='https://raw.githubusercontent.com/haf/DotNetZip.Semverd/master/LICENSE'>Multiple</a>) - a library for handling zip archives</li>")]
[assembly: Acknowledgement("L10NSharp", Url = "https://github.com/sillsdev/l10nsharp/",
	Copyright = "Copyright © SIL International 2010-2017", LicenseUrl = "https://opensource.org/licenses/MIT",
	Location = "./L10NSharp.dll")]