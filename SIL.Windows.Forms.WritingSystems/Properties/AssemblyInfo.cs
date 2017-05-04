using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SIL.Acknowledgements;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("SIL.Windows.Forms.WritingSystems")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("fc1e6fc7-4a4c-4c83-a5fa-aafc550647e5")]

[assembly: InternalsVisibleTo("SIL.Windows.Forms.WritingSystems.Tests")]

[assembly: Acknowledgement("L10NSharp", Url = "https://github.com/sillsdev/l10nsharp/",
	Copyright = "Copyright © SIL International 2010-2014", LicenseUrl = "https://opensource.org/licenses/MIT",
	Location = "./L10NSharp.dll")]

// LGPL-2.1 license is actually for Enchant, not for Enchant.Net
[assembly: Acknowledgement("Enchant.Net", Url = "https://github.com/AbiWord/enchantdotnet",
	Copyright = "Copyright © 2007-2008 Eric Scott Albright",
	Location = "./Enchant.Net.dll", LicenseUrl = "https://opensource.org/licenses/LGPL-2.1")]
