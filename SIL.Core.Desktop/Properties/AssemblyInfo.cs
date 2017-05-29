﻿using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SIL.Acknowledgements;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("SIL.Core.Desktop")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("c48de000-2086-4ceb-a5a1-3171c272725e")]

[assembly: CLSCompliant(true)]

#if STRONG_NAME
[assembly: InternalsVisibleTo("SIL.Core.Desktop.Tests, " +
	"PublicKey=00240000048000009400000006020000002400005253413100040000010001008339b2ae1bf006" +
	"934f6176dec6ea2a8a7d67383613dcb03d71975e7b05ad546562c84529a4811e94c889e55f2532" +
	"d1a90baaf20be9bff39ac6f5365bd605d70b90489840b7ba6d1c231b0e550c4abe4f60553856ef" +
	"142a40a91e53d56e79f69dc79c4e95817de498aac924ee011f03b4e1c1d772d51c4946c1185e3bfb621bc6")]
#else
[assembly: InternalsVisibleTo("SIL.Core.Desktop.Tests")]
#endif

[assembly: Acknowledgement("NDesk.DBus", Name = "NDesk.DBus", Url = "https://www.nuget.org/packages/NDesk.DBus/",
	LicenseUrl = "https://opensource.org/licenses/MIT", Copyright = "Copyright © 2006 Alp Toker", Location = "./NDesk.DBus.dll")]
