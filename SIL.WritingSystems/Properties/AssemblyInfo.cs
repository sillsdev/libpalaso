using System;
using System.Runtime.CompilerServices;
using SIL.Acknowledgements;

[assembly: CLSCompliant(true)]

[assembly: InternalsVisibleTo("SIL.Core.Tests, PublicKey=00240000048000009400000006020000002400005253413100040000010001008339b2ae1bf006934f6176dec6ea2a8a7d67383613dcb03d71975e7b05ad546562c84529a4811e94c889e55f2532d1a90baaf20be9bff39ac6f5365bd605d70b90489840b7ba6d1c231b0e550c4abe4f60553856ef142a40a91e53d56e79f69dc79c4e95817de498aac924ee011f03b4e1c1d772d51c4946c1185e3bfb621bc6")]
[assembly: InternalsVisibleTo("SIL.WritingSystems.Tests, PublicKey=00240000048000009400000006020000002400005253413100040000010001008339b2ae1bf006934f6176dec6ea2a8a7d67383613dcb03d71975e7b05ad546562c84529a4811e94c889e55f2532d1a90baaf20be9bff39ac6f5365bd605d70b90489840b7ba6d1c231b0e550c4abe4f60553856ef142a40a91e53d56e79f69dc79c4e95817de498aac924ee011f03b4e1c1d772d51c4946c1185e3bfb621bc6")]
[assembly: InternalsVisibleTo("SIL.TestUtilities, PublicKey=00240000048000009400000006020000002400005253413100040000010001008339b2ae1bf006934f6176dec6ea2a8a7d67383613dcb03d71975e7b05ad546562c84529a4811e94c889e55f2532d1a90baaf20be9bff39ac6f5365bd605d70b90489840b7ba6d1c231b0e550c4abe4f60553856ef142a40a91e53d56e79f69dc79c4e95817de498aac924ee011f03b4e1c1d772d51c4946c1185e3bfb621bc6")]
[assembly: InternalsVisibleTo("LanguageData, PublicKey=00240000048000009400000006020000002400005253413100040000010001008339b2ae1bf006934f6176dec6ea2a8a7d67383613dcb03d71975e7b05ad546562c84529a4811e94c889e55f2532d1a90baaf20be9bff39ac6f5365bd605d70b90489840b7ba6d1c231b0e550c4abe4f60553856ef142a40a91e53d56e79f69dc79c4e95817de498aac924ee011f03b4e1c1d772d51c4946c1185e3bfb621bc6")]

[assembly: Acknowledgement("Newtonsoft.Json", Name = "Json.NET", Url = "https://www.nuget.org/packages/Newtonsoft.Json/",
	LicenseUrl = "https://opensource.org/licenses/MIT", Copyright = "Copyright © James Newton-King 2008",
	Location = "./Newtonsoft.Json.dll")]
[assembly: Acknowledgement("icu.net", Url = "https://github.com/sillsdev/icu-dotnet",
	LicenseUrl = "https://opensource.org/licenses/MIT", Copyright = "Copyright © SIL Global 2007-2024", Location = "./icu.net.dll")]
// The Spart code came originally from a 2003 Code Project article at
// https://www.codeproject.com/Articles/5676/Spart-a-parser-generator-framework-C.  You have to dig
// into the source code files to find the copyright and license information.  SIL programmers have
// modified the code in various ways and SIL owns the copyright on those changes, but not on the
// original code.  Since our github repo links to the Code Project article, it seems like the best
// Url value.
[assembly: Acknowledgement("Spart",
	Copyright = "Copyright © Jonathan de Halleux 2003, portions Copyright © 2018 SIL Global",
	Url = "https://github.com/sillsdev/spart",
	LicenseUrl = "https://opensource.org/licenses/Zlib",
	Location = "./Spart.dll")]
