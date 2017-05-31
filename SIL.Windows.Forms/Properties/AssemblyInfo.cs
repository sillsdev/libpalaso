using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SIL.Acknowledgements;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("SIL.Windows.Forms")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("3030bd1f-35f1-4557-bfa5-ec26e58b5308")]

[assembly: System.CLSCompliant(true)]
[assembly: InternalsVisibleTo("SIL.Windows.Forms.Tests")]

[assembly: Acknowledgement("L10NSharp", Url = "https://github.com/sillsdev/l10nsharp/",
	Copyright = "Copyright © SIL International 2010-2014", LicenseUrl = "https://opensource.org/licenses/MIT",
	Location = "./L10NSharp.dll")]
[assembly: Acknowledgement("MarkdownDeep", Url = "https://www.nuget.org/packages/MarkdownDeep.NET/",
	LicenseUrl = "https://opensource.org/licenses/Apache-2.0", Copyright = "Copyright © 2010-2011 Topten Software",
	Location = "./MarkdownDeep.dll")]

// LGPL-2.1 license is actually for Enchant, not for Enchant.Net
[assembly: Acknowledgement("Enchant.Net", Url = "https://github.com/AbiWord/enchantdotnet",
	Copyright = "Copyright © 2007-2008 Eric Scott Albright",
	Location = "./Enchant.Net.dll", LicenseUrl = "https://opensource.org/licenses/LGPL-2.1")]

[assembly: Acknowledgement("Mono.Posix", Name = "Mono.Posix", Url = "https://www.nuget.org/packages/Mono.Posix/",
	LicenseUrl = "https://opensource.org/licenses/MIT")]
[assembly: Acknowledgement("taglib-sharp", Name = "TagLib#", Url = "https://github.com/mono/taglib-sharp",
	Copyright = "Copyright (C) 2005, 2007 Brian Nickel, Copyright (C) 2006 Novell, Inc., Copyright (C) 2002,2003 Scott Wheeler (Original Implementation)",
	Location = "./taglib-sharp.dll", LicenseUrl = "https://opensource.org/licenses/LGPL-2.1")]
[assembly: Acknowledgement("gtk-sharp", Name = "gtk-sharp", Url = "http://www.mono-project.com/docs/gui/gtksharp/",
	Location = "./gtk-sharp.dll", LicenseUrl = "https://opensource.org/licenses/MIT")]
