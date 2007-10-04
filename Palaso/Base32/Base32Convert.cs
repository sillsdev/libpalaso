using System.Collections.Generic;

namespace Palaso.Base32
{
	/// <summary>
	///  base32 encoding and decoding as defined by RFC4648
	///  http://rfc.net/rfc4648.html
	/// </summary>
	public partial class Base32Convert
	{
		private const string base32Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

		public static string ToBase32String(IEnumerable<byte> octets, Base32FormattingOptions options)
		{
			return ToBase32String(octets, base32Alphabet, options);
		}

		public static byte[] FromBase32String(string base32, Base32FormattingOptions options)
		{
			byte[] decodeMap = CreateDecodeMap(base32Alphabet, false);
			return FromBase32String(base32, decodeMap, options);
		}

	}
}