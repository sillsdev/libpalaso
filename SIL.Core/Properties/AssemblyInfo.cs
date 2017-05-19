// Copyright (c) 2016-2017 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SIL.Acknowledgements;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("SIL.Core")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("caad337c-3857-4c16-8478-cb0ac8d87e32")]

[assembly: CLSCompliant(true)]

#if STRONG_NAME
// find the public key with: sn -tp palasao.snk
[assembly: AssemblyKeyFileAttribute("../palaso.snk")]
[assembly: InternalsVisibleTo("SIL.Core.Tests, PublicKey=00240000048000009400000006020000002400005253413100040000010001008339b2ae1bf006934f6176dec6ea2a8a7d67383613dcb03d71975e7b05ad546562c84529a4811e94c889e55f2532d1a90baaf20be9bff39ac6f5365bd605d70b90489840b7ba6d1c231b0e550c4abe4f60553856ef142a40a91e53d56e79f69dc79c4e95817de498aac924ee011f03b4e1c1d772d51c4946c1185e3bfb621bc6")]
[assembly: InternalsVisibleTo("SIL.WritingSystems.Tests, PublicKey=00240000048000009400000006020000002400005253413100040000010001008339b2ae1bf006934f6176dec6ea2a8a7d67383613dcb03d71975e7b05ad546562c84529a4811e94c889e55f2532d1a90baaf20be9bff39ac6f5365bd605d70b90489840b7ba6d1c231b0e550c4abe4f60553856ef142a40a91e53d56e79f69dc79c4e95817de498aac924ee011f03b4e1c1d772d51c4946c1185e3bfb621bc6")]
#else
[assembly: InternalsVisibleTo("SIL.Core.Tests")]
[assembly: InternalsVisibleTo("SIL.WritingSystems.Tests")]
#endif

[assembly: Acknowledgement("Ionic.Zip", Copyright = "© Dino Chiesa 2006-2011",
	Url = "http://www.codeplex.com/DotNetZip",
	LicenseUrl = "https://opensource.org/licenses/MS-PL", Location = "./Ionic.Zip.dll",
	Html = "<li><a href='http://www.codeplex.com/DotNetZip'>Ionic.Zip</a> © Dino Chiesa 2006-2011 (<a href='https://opensource.org/licenses/MS-PL'>MS-PL</a>) - a library for handling zip archives (Flavor=Retail)</li>")]
[assembly: Acknowledgement("NDesk.DBus", Name = "NDesk.DBus", Url = "https://www.nuget.org/packages/NDesk.DBus/",
	LicenseUrl = "https://opensource.org/licenses/MIT", Copyright = "Copyright © 2006 Alp Toker", Location = "./NDesk.DBus.dll")]
[assembly: Acknowledgement("Newtonsoft.Json", Name = "Json.NET", Url = "https://www.nuget.org/packages/Newtonsoft.Json/",
	LicenseUrl = "https://opensource.org/licenses/MIT", Copyright = "Copyright © James Newton-King 2008",
	Location = "./Newtonsoft.Json.dll")]
[assembly: Acknowledgement("Mono.Posix", Name = "Mono.Posix", Url = "https://www.nuget.org/packages/Mono.Posix/",
	LicenseUrl = "https://opensource.org/licenses/MIT")]
