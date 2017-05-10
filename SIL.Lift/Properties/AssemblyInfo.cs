using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SIL.Acknowledgements;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("SIL.Lift")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("d699013f-e994-4e2f-8782-4822bfeeea6f")]

[assembly: InternalsVisibleTo("SIL.Lift.Tests")]

[assembly: Acknowledgement("Commons.Xml.Relaxng", Name = "Commons.Xml.Relaxng", Location = "./Commons.Xml.Relaxng.dll",
	LicenseUrl = "https://opensource.org/licenses/MIT",
	Url = "http://www.mono-project.com/docs/tools+libraries/libraries/xml/#relax-ng")]