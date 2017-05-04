using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SIL.Acknowledgements;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("LanguageData")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("LanguageData")]
[assembly: AssemblyCopyright("Copyright ©SIL International  2017")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("12c78d85-bb7f-4f00-bf64-e47557f99039")]

// Allow tests to see internals
#if STRONG_NAME
[assembly: AssemblyKeyFileAttribute("../palaso.snk")]
[assembly: InternalsVisibleTo("LanguageData.Tests, PublicKey=00240000048000009400000006020000002400005253413100040000010001008339b2ae1bf006934f6176dec6ea2a8a7d67383613dcb03d71975e7b05ad546562c84529a4811e94c889e55f2532d1a90baaf20be9bff39ac6f5365bd605d70b90489840b7ba6d1c231b0e550c4abe4f60553856ef142a40a91e53d56e79f69dc79c4e95817de498aac924ee011f03b4e1c1d772d51c4946c1185e3bfb621bc6")]
#else
[assembly: InternalsVisibleTo("LanguageData.Tests")]
#endif


// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

[assembly: Acknowledgement("Newtonsoft.Json", Name = "Json.NET", Url = "https://www.nuget.org/packages/Newtonsoft.Json/",
	LicenseUrl = "https://opensource.org/licenses/MIT", Copyright = "Copyright © James Newton-King 2008",
	Location = "./Newtonsoft.Json.dll")]
[assembly: Acknowledgement("CommandLine", Name = "Command Line Parser Library",
	Url = "https://www.nuget.org/packages/CommandLineParser/", Copyright = "Copyright (c) 2005 - 2013 Giacomo Stelluti Scala",
	LicenseUrl = "https://opensource.org/licenses/MIT", Location = "./CommandLine.dll")]