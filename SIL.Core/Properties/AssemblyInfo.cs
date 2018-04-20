// Copyright (c) 2016-2017 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Runtime.CompilerServices;
using SIL.Acknowledgements;

[assembly: CLSCompliant(true)]

#if STRONG_NAME
[assembly: InternalsVisibleTo("SIL.Core.Tests, PublicKey=00240000048000009400000006020000002400005253413100040000010001008339b2ae1bf006934f6176dec6ea2a8a7d67383613dcb03d71975e7b05ad546562c84529a4811e94c889e55f2532d1a90baaf20be9bff39ac6f5365bd605d70b90489840b7ba6d1c231b0e550c4abe4f60553856ef142a40a91e53d56e79f69dc79c4e95817de498aac924ee011f03b4e1c1d772d51c4946c1185e3bfb621bc6")]
[assembly: InternalsVisibleTo("SIL.WritingSystems.Tests, PublicKey=00240000048000009400000006020000002400005253413100040000010001008339b2ae1bf006934f6176dec6ea2a8a7d67383613dcb03d71975e7b05ad546562c84529a4811e94c889e55f2532d1a90baaf20be9bff39ac6f5365bd605d70b90489840b7ba6d1c231b0e550c4abe4f60553856ef142a40a91e53d56e79f69dc79c4e95817de498aac924ee011f03b4e1c1d772d51c4946c1185e3bfb621bc6")]
#else
[assembly: InternalsVisibleTo("SIL.Core.Tests")]
[assembly: InternalsVisibleTo("SIL.WritingSystems.Tests")]
#endif

[assembly: Acknowledgement("Newtonsoft.Json", Name = "Json.NET", Url = "https://www.nuget.org/packages/Newtonsoft.Json/",
	LicenseUrl = "https://opensource.org/licenses/MIT", Copyright = "Copyright © James Newton-King 2008",
	Location = "./Newtonsoft.Json.dll")]
[assembly: Acknowledgement("Mono.Posix", Name = "Mono.Posix", Url = "https://www.nuget.org/packages/Mono.Posix/",
	LicenseUrl = "https://opensource.org/licenses/MIT")]
