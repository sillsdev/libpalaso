using System.Runtime.CompilerServices;
using SIL.Acknowledgements;

[assembly: InternalsVisibleTo("SIL.Windows.Forms.WritingSystems.Tests, " +
	"PublicKey=00240000048000009400000006020000002400005253413100040000010001008339b2ae1bf006" +
	"934f6176dec6ea2a8a7d67383613dcb03d71975e7b05ad546562c84529a4811e94c889e55f2532" +
	"d1a90baaf20be9bff39ac6f5365bd605d70b90489840b7ba6d1c231b0e550c4abe4f60553856ef" +
	"142a40a91e53d56e79f69dc79c4e95817de498aac924ee011f03b4e1c1d772d51c4946c1185e3bfb621bc6")]

[assembly: Acknowledgement("L10NSharp", Url = "https://github.com/sillsdev/l10nsharp/",
	Copyright = "Copyright © SIL Global 2010-2024", LicenseUrl = "https://opensource.org/licenses/MIT",
	Location = "./L10NSharp.dll")]

// LGPL-2.1 license is actually for Enchant, not for Enchant.Net
[assembly: Acknowledgement("Enchant.Net", Url = "https://github.com/AbiWord/enchantdotnet",
	Copyright = "Copyright © 2007-2008 Eric Scott Albright",
	Location = "./Enchant.Net.dll", LicenseUrl = "https://opensource.org/licenses/LGPL-2.1")]
