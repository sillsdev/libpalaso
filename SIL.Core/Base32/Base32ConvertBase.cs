using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace SIL.Base32
{
	public static partial class Base32Convert
	{
		private const byte InvalidCharacter = 0xff;

		private static byte[] CreateDecodeMap(string base32alphabet,
											 bool isCaseSensitive)
		{
			byte[] decode = new byte[128];
			for (int i = 0;i < decode.Length;i++)
			{
				decode[i] = InvalidCharacter;
			}

			for (int i = 0;i < base32alphabet.Length;i++)
			{
				decode[base32alphabet[i]] = (byte) i;
				if (!isCaseSensitive)
				{
					decode[char.ToLower(base32alphabet[i])] = (byte) i;
					decode[char.ToUpper(base32alphabet[i])] = (byte) i;
				}
			}

			return decode;
		}

		private static string ToBase32String(IEnumerable<byte> octets,
									string base32alphabet,
									Base32FormattingOptions options)
		{
			if (octets == null)
			{
				throw new ArgumentNullException();
			}
			bool shouldPadResult = (options &
									Base32FormattingOptions.
											InsertTrailingPadding) ==
								   Base32FormattingOptions.InsertTrailingPadding;
			StringBuilder result = new StringBuilder();

			int paddingCharactersRequired = 0;
			// for every block of five octets, there are 8 characters
			IEnumerator<byte> octetEnumerator = octets.GetEnumerator();
			while (octetEnumerator.MoveNext())
			{
				//01234567 01234567 01234567 01234567 01234567
				//01234      01234      0123 4     01 234
				//     012 34     0 1234      01234      01234

				// byte 0
				byte b = octetEnumerator.Current;
				result.Append(base32alphabet[b >> 3]);
				int remainingBits = b << 2;
				if (!octetEnumerator.MoveNext())
				{
					result.Append(base32alphabet[0x1f & remainingBits]);
					paddingCharactersRequired = 6;
					break;
				}

				// byte 1
				b = octetEnumerator.Current;
				result.Append(base32alphabet[0x1f & (remainingBits | (b >> 6))]);
				result.Append(base32alphabet[0x1f & b >> 1]);
				remainingBits = b << 4;
				if (!octetEnumerator.MoveNext())
				{
					result.Append(base32alphabet[0x1f & remainingBits]);
					paddingCharactersRequired = 4;
					break;
				}

				// byte 2
				b = octetEnumerator.Current;
				result.Append(base32alphabet[0x1f & (remainingBits | (b >> 4))]);
				remainingBits = b << 1;
				if (!octetEnumerator.MoveNext())
				{
					result.Append(base32alphabet[0x1f & remainingBits]);
					paddingCharactersRequired = 3;
					break;
				}

				// byte 3
				b = octetEnumerator.Current;
				result.Append(base32alphabet[0x1f & (remainingBits | (b >> 7))]);
				result.Append(base32alphabet[0x1f & (b >> 2)]);
				remainingBits = b << 3;
				if (!octetEnumerator.MoveNext())
				{
					result.Append(base32alphabet[0x1f & remainingBits]);
					paddingCharactersRequired = 1;
					break;
				}

				// byte 4
				b = octetEnumerator.Current;
				result.Append(base32alphabet[0x1f & (remainingBits | (b >> 5))]);
				result.Append(base32alphabet[0x1f & b]);
			}
			if (shouldPadResult)
			{
				result.Append('=', paddingCharactersRequired);
				Debug.Assert(result.Length % 8 == 0);
			}
			return result.ToString();
		}

		private static byte[] FromBase32String(string base32,
									byte[] decodeMap,
									Base32FormattingOptions options)
		{
			if (base32 == null)
			{
				throw new ArgumentNullException("base32");
			}
			if (decodeMap == null)
			{
				throw new ArgumentNullException("decodeMap");
			}
			if (decodeMap.Length != 0x080)
			{
				throw new ArgumentException("decodeMap must have 128 slots");
			}
			bool shouldBePadded = (options &
								   Base32FormattingOptions.InsertTrailingPadding) ==
								  Base32FormattingOptions.InsertTrailingPadding;
			if (!shouldBePadded)
			{
				int i = base32.IndexOf('=');
				if (i != -1)
				{
					throw new ArgumentException(string.Format(
														"Invalid character at index {0}: \"=\"",
														i));
				}
			}
			int remainder = base32.Length % 8;
			if (remainder > 0)
			{
				if (shouldBePadded || remainder == 1 || remainder == 3 ||
					remainder == 6)
				{
					throw new ArgumentException("base32 not proper length");
				}
				base32 = base32 + new string('=', 8 - remainder);
			}

			byte[] result = new byte[(base32.Length / 8) * 5];
			byte[] x = new byte[8];

			int pad = 0;
			for (int i = 0, k = 0;
				 i < base32.Length;
				 i += 8)
			{
				int j;
				for (j = 0;j < 8;++j)
				{
					char a = base32[i + j];
					if (a == '=')
					{
						x[j] = 0;
						pad++;
						continue;
					}
					else if (pad != 0)
					{
						throw new ArgumentException(
								string.Format(
										"Invalid character at index {0}: \"=\" (padding found in the middle of the input)",
										i + j - 1));
					}

					if (a >= 0x80 || (x[j] = decodeMap[a]) == 0xff)
					{
						throw new ArgumentException(string.Format(
															"Invalid character at index {0}: \"{1}\"",
															i + j,
															a));
					}
				}

				result[k++] = (byte) ((x[0] << 3) | (x[1] >> 2));
				result[k++] = (byte) ((x[1] << 6) | (x[2] << 1) | (x[3] >> 4));
				result[k++] = (byte) ((x[3] << 4) | (x[4] >> 1));
				result[k++] = (byte) ((x[4] << 7) | (x[5] << 2) | (x[6] >> 3));
				result[k++] = (byte) ((x[6] << 5) | x[7]);
			}

			if (pad != 0)
			{
				if (pad == 1)
				{
					Array.Resize(ref result, result.Length - 1);
				}
				else if (pad == 3)
				{
					Array.Resize(ref result, result.Length - 2);
				}
				else if (pad == 4)
				{
					Array.Resize(ref result, result.Length - 3);
				}
				else if (pad == 6)
				{
					Array.Resize(ref result, result.Length - 4);
				}
				else
				{
					throw new ArgumentException(
							string.Format("Invalid padding of length {0}", pad));
				}
			}

			return result;
		}
	}
}