using System.Runtime.CompilerServices;
using SIL.Acknowledgements;

[assembly: System.CLSCompliant(true)]
[assembly: InternalsVisibleTo("SIL.Windows.Forms.Tests, " +
	"PublicKey=00240000048000009400000006020000002400005253413100040000010001008339b2ae1bf006" +
	"934f6176dec6ea2a8a7d67383613dcb03d71975e7b05ad546562c84529a4811e94c889e55f2532" +
	"d1a90baaf20be9bff39ac6f5365bd605d70b90489840b7ba6d1c231b0e550c4abe4f60553856ef" +
	"142a40a91e53d56e79f69dc79c4e95817de498aac924ee011f03b4e1c1d772d51c4946c1185e3bfb621bc6")]

[assembly: Acknowledgement("L10NSharp", Url = "https://github.com/sillsdev/l10nsharp/",
	Copyright = "Copyright © SIL International 2010-2014", LicenseUrl = "https://opensource.org/licenses/MIT",
	Location = "./L10NSharp.dll")]
[assembly: Acknowledgement("Markdig", Url = "https://github.com/lunet-io/markdig",
	LicenseUrl = "https://github.com/lunet-io/markdig/blob/master/license.txt", Copyright = "Copyright © 2018-2019, Alexandre Mutel",
	Location = "./Markdig.dll")]

// LGPL-2.1 license is actually for Enchant, not for Enchant.Net
[assembly: Acknowledgement("Enchant.Net", Url = "https://github.com/AbiWord/enchantdotnet",
	Copyright = "Copyright © 2007-2008 Eric Scott Albright",
	Location = "./Enchant.Net.dll", LicenseUrl = "https://opensource.org/licenses/LGPL-2.1")]

[assembly: Acknowledgement("Mono.Posix", Name = "Mono.Posix", Url = "https://www.nuget.org/packages/Mono.Posix/",
	LicenseUrl = "https://opensource.org/licenses/MIT")]
[assembly: Acknowledgement("taglib-sharp", Name = "TagLib#", Url = "https://github.com/mono/taglib-sharp",
	Copyright = "Copyright (C) 2005, 2007 Brian Nickel, Copyright (C) 2006 Novell, Inc., Copyright (C) 2002,2003 Scott Wheeler (Original Implementation)",
	Location = "./taglib-sharp.dll", LicenseUrl = "https://opensource.org/licenses/LGPL-2.1")]
