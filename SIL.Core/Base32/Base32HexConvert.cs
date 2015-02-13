using System.Collections.Generic;

namespace SIL.Base32
{
	  public partial class Base32Convert
	{
		private const string base32hexAlphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUV";

		/// <summary>
		/// base32hex encoding as defined by RFC4648
		/// http://rfc.net/rfc4648.html
		/// One property with this alphabet, which the base64 and base32
		/// alphabets lack, is that encoded data maintains its sort order when
		/// the encoded data is compared bit-wise.
		/// </summary>
		/// <remarks>
		/// This is suitable for encoding sort keys in xml documents such
		/// that xslt 1 can produce a proper ordering.
		/// </remarks>
		public static string ToBase32HexString(IEnumerable<byte> octets, Base32FormattingOptions options)
		{
			return ToBase32String(octets, base32hexAlphabet, options);
		}

		/// <summary>
		/// base32hex decoding as defined by RFC4648
		/// http://rfc.net/rfc4648.html
		/// One property with this alphabet, which the base64 and base32
		/// alphabets lack, is that encoded data maintains its sort order when
		/// the encoded data is compared bit-wise.
		/// </summary>
		/// <remarks>
		/// This is suitable for encoding sort keys in xml documents such
		/// that xslt 1 can produce a proper ordering.
		/// </remarks>
		public static byte[] FromBase32HexString(string base32, Base32FormattingOptions options)
		{
			byte[] decodeMap = CreateDecodeMap(base32hexAlphabet, false);
			return FromBase32String(base32, decodeMap, options);
		}
	}
}