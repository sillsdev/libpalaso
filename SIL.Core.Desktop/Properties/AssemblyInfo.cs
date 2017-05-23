using System;
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

[assembly: InternalsVisibleTo("SIL.Core.Desktop.Tests")]

[assembly: Acknowledgement("NDesk.DBus", Name = "NDesk.DBus", Url = "https://www.nuget.org/packages/NDesk.DBus/",
	LicenseUrl = "https://opensource.org/licenses/MIT", Copyright = "Copyright © 2006 Alp Toker", Location = "./NDesk.DBus.dll")]
