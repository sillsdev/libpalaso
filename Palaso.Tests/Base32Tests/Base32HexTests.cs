using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Palaso.Base32;

namespace Palaso.Tests.Base32Tests
{
	[TestFixture]
	public class Base32HexTests
	{
		public class Base32HexComparer: IComparer<string>
		{
			#region IComparer<string> Members

			public int Compare(string x, string y)
			{
				return Comparer<string>.Default.Compare(Encode(x), Encode(y));
			}

			#endregion
		}

		public class ByteComparer: IComparer<string>
		{
			#region IComparer<string> Members

			public int Compare(string x, string y)
			{
				byte[] bytesX = Encoding.UTF7.GetBytes(x);
				byte[] bytesY = Encoding.UTF7.GetBytes(y);

				int maxLength = Math.Min(bytesX.Length, bytesY.Length);
				for (int i = 0;i < maxLength;i++)
				{
					int compare =
							Comparer<byte>.Default.Compare(bytesX[i], bytesY[i]);
					if (compare != 0)
					{
						return compare;
					}
				}
				if (bytesX.Length == bytesY.Length)
				{
					return 0;
				}
				if (bytesX.Length < bytesY.Length)
				{
					return -1;
				}
				return 1;
			}

			#endregion
		}

		private static string Encode(string s)
		{
			return
					Base32Convert.ToBase32HexString(Encoding.UTF7.GetBytes(s),
													Base32FormattingOptions.
															InsertTrailingPadding);
		}

		private static string EncodeOmitPadding(string s)
		{
			return
					Base32Convert.ToBase32HexString(Encoding.UTF7.GetBytes(s),
													Base32FormattingOptions.None);
		}

		private static string Decode(string s)
		{
			byte[] bytes =
					Base32Convert.FromBase32HexString(s,
													  Base32FormattingOptions.
															  InsertTrailingPadding);
			return Encoding.UTF7.GetString(bytes);
		}

		private static string DecodeOmitPadding(string s)
		{
			byte[] bytes =
					Base32Convert.FromBase32HexString(s,
													  Base32FormattingOptions.
															  None);
			return Encoding.UTF7.GetString(bytes);
		}

		public static void EncodeThenDecode(IEnumerator[] a)
		{
			List<byte> octets = new List<byte>();
			for (int j = 1;j < a.Length;j++)
			{
				octets.Add((byte) a[j].Current);
			}

			byte[] result =
					Base32Convert.FromBase32HexString(
							Base32Convert.ToBase32HexString(octets,
															Base32FormattingOptions
																	.
																	InsertTrailingPadding),
							Base32FormattingOptions.InsertTrailingPadding);
			Assert.IsTrue(SameContent(result, octets, Comparer<byte>.Default));
		}

		public static void EncodeThenDecodeNoPadding(IEnumerator[] a)
		{
			List<byte> octets = new List<byte>();
			for (int j = 1;j < a.Length;j++)
			{
				octets.Add((byte) a[j].Current);
			}

			byte[] result =
					Base32Convert.FromBase32HexString(
							Base32Convert.ToBase32HexString(octets,
															 Base32FormattingOptions.None),
							Base32FormattingOptions.None);
			Assert.IsTrue(SameContent(result, octets, Comparer<byte>.Default));
		}

		public static bool SameContent<T>(IEnumerable<T> first,
										  IEnumerable<T> second,
										  Comparer<T> comparer)
		{
			IEnumerator<T> enumerator1 = first.GetEnumerator();
			IEnumerator<T> enumerator2 = second.GetEnumerator();
			while (enumerator1.MoveNext())
			{
				if (!enumerator2.MoveNext())
				{
					return false;
				}
				if (
						comparer.Compare(enumerator1.Current,
										 enumerator2.Current) != 0)
				{
					return false;
				}
			}

			if (enumerator2.MoveNext())
			{
				return false;
			}
			else
			{
				return true;
			}
		}

		[Test]
		[ExpectedException(typeof (ArgumentException))]
		public void Decode_CharacterNotInAlphabet_throws()
		{
			Base32Convert.FromBase32HexString("#y======",
											  Base32FormattingOptions.
													  InsertTrailingPadding);
		}

		[Test]
		public void Decode_EmptyString_EmptyString()
		{
			Assert.AreEqual(string.Empty, Decode(""));
		}

		[Test]
		[ExpectedException(typeof (ArgumentException))]
		public void Decode_MisplacedPaddingCharacter_throws()
		{
			Base32Convert.FromBase32HexString("m=y======",
											  Base32FormattingOptions.
													  InsertTrailingPadding);
		}

		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void Decode_null_throws()
		{
			Base32Convert.FromBase32HexString(null,
											  Base32FormattingOptions.
													  InsertTrailingPadding);
		}

		[Test]
		public void Decode_rfc4648()
		{
			Assert.AreEqual("f", Decode("CO======"));
			Assert.AreEqual("fo", Decode("CPNG===="));
			Assert.AreEqual("foo", Decode("CPNMU==="));
			Assert.AreEqual("foob", Decode("CPNMUOG="));
			Assert.AreEqual("fooba", Decode("CPNMUOJ1"));
			Assert.AreEqual("foobar", Decode("CPNMUOJ1E8======"));
		}

		[Test]
		public void Decode_rfc4648_lowercase()
		{
			Assert.AreEqual("f", Decode("co======"));
			Assert.AreEqual("fo", Decode("cpng===="));
			Assert.AreEqual("foo", Decode("cpnmu==="));
			Assert.AreEqual("foob", Decode("cpnmuog="));
			Assert.AreEqual("fooba", Decode("cpnmuoj1"));
			Assert.AreEqual("foobar", Decode("cpnmuoj1e8======"));
		}

		[Test]
		public void Decode_wholealphabet()
		{
			Assert.AreEqual(34,
							Decode(
									"0123456789abcdefghijklmnopqrstuvABCDEFGHIJKLMNOPQRSTUVv=")
									.Length);
		}

		[Test]
		[ExpectedException(typeof (ArgumentException))]
		public void DecodeOmitPadding_WrongNumberOfCharacters_1_throws()
		{
			Base32Convert.FromBase32HexString("1", Base32FormattingOptions.None);
		}

		[Test]
		[ExpectedException(typeof (ArgumentException))]
		public void DecodeOmitPadding_WrongNumberOfCharacters_3_throws()
		{
			Base32Convert.FromBase32HexString("123",
											  Base32FormattingOptions.None);
		}

		[Test]
		[ExpectedException(typeof (ArgumentException))]
		public void DecodeOmitPadding_WrongNumberOfCharacters_6_throws()
		{
			Base32Convert.FromBase32HexString("123456",
											  Base32FormattingOptions.None);
		}

		[Test]
		[ExpectedException(typeof (ArgumentException))]
		public void Decode_WrongNumberOfPaddingCharacters_throws()
		{
			Base32Convert.FromBase32HexString("my=====",
											  Base32FormattingOptions.
													  InsertTrailingPadding);
		}

		[Test]
		[ExpectedException(typeof (ArgumentException))]
		public void DecodeOmitPadding_CharacterNotInAlphabet_throws()
		{
			Base32Convert.FromBase32HexString("#y", Base32FormattingOptions.None);
		}

		[Test]
		public void DecodeOmitPadding_EmptyString_EmptyString()
		{
			Assert.AreEqual(string.Empty, DecodeOmitPadding(""));
		}

		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void DecodeOmitPadding_null_throws()
		{
			Base32Convert.FromBase32HexString(null, Base32FormattingOptions.None);
		}

		[Test]
		[ExpectedException(typeof (ArgumentException))]
		public void DecodeOmitPadding_PaddingCharacter_throws()
		{
			Base32Convert.FromBase32HexString("my======",
											  Base32FormattingOptions.None);
		}

		[Test]
		public void DecodeOmitPadding_rfc4648()
		{
			Assert.AreEqual("f", DecodeOmitPadding("CO"));
			Assert.AreEqual("fo", DecodeOmitPadding("CPNG"));
			Assert.AreEqual("foo", DecodeOmitPadding("CPNMU"));
			Assert.AreEqual("foob", DecodeOmitPadding("CPNMUOG"));
			Assert.AreEqual("fooba", DecodeOmitPadding("CPNMUOJ1"));
			Assert.AreEqual("foobar", DecodeOmitPadding("CPNMUOJ1E8"));
		}

		[Test]
		public void DecodeOmitPadding_rfc4648_lowercase()
		{
			Assert.AreEqual("f", DecodeOmitPadding("co"));
			Assert.AreEqual("fo", DecodeOmitPadding("cpng"));
			Assert.AreEqual("foo", DecodeOmitPadding("cpnmu"));
			Assert.AreEqual("foob", DecodeOmitPadding("cpnmuog"));
			Assert.AreEqual("fooba", DecodeOmitPadding("cpnmuoj1"));
			Assert.AreEqual("foobar", DecodeOmitPadding("cpnmuoj1e8"));
		}

		[Test]
		public void DecodeOmitPadding_wholealphabet()
		{
			Assert.AreEqual(34,
							DecodeOmitPadding(
									"0123456789abcdefghijklmnopqrstuvABCDEFGHIJKLMNOPQRSTUVv")
									.Length);
		}

		[Test]
		public void Encode_EmptyString_EmptyString()
		{
			Assert.AreEqual(string.Empty, Encode(""));
		}

		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void Encode_null_throws()
		{
			Base32Convert.ToBase32HexString(null,
											Base32FormattingOptions.
													InsertTrailingPadding);
		}

		[Test]
		public void Encode_rfc4648()
		{
			Assert.AreEqual("CO======", Encode("f"));
			Assert.AreEqual("CPNG====", Encode("fo"));
			Assert.AreEqual("CPNMU===", Encode("foo"));
			Assert.AreEqual("CPNMUOG=", Encode("foob"));
			Assert.AreEqual("CPNMUOJ1", Encode("fooba"));
			Assert.AreEqual("CPNMUOJ1E8======", Encode("foobar"));
		}

		[Test]
		public void EncodeOmitPadding_EmptyString_EmptyString()
		{
			Assert.AreEqual(string.Empty, EncodeOmitPadding(""));
		}

		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void EncodeOmitPadding_null_throws()
		{
			Base32Convert.ToBase32HexString(null, Base32FormattingOptions.None);
		}

		[Test]
		public void EncodeOmitPadding_rfc4648()
		{
			Assert.AreEqual("CO", EncodeOmitPadding("f"));
			Assert.AreEqual("CPNG", EncodeOmitPadding("fo"));
			Assert.AreEqual("CPNMU", EncodeOmitPadding("foo"));
			Assert.AreEqual("CPNMUOG", EncodeOmitPadding("foob"));
			Assert.AreEqual("CPNMUOJ1", EncodeOmitPadding("fooba"));
			Assert.AreEqual("CPNMUOJ1E8", EncodeOmitPadding("foobar"));
		}

		[Test]
		[Ignore("Long Running (7 seconds)")]
		public void EncodeThenDecode_Roundtrip()
		{
			byte[] bytes =
					new byte[] {0, 1, 18, 62, 95, 100, 148, 201, 254, 255};

			Permuter.VisitAll(EncodeThenDecode, bytes);
			Permuter.VisitAll(EncodeThenDecode, bytes, bytes);
			Permuter.VisitAll(EncodeThenDecode, bytes, bytes, bytes);
			Permuter.VisitAll(EncodeThenDecode, bytes, bytes, bytes, bytes);
			Permuter.VisitAll(EncodeThenDecode,
							  bytes,
							  bytes,
							  bytes,
							  bytes,
							  bytes);
			Permuter.VisitAll(EncodeThenDecode,
							  bytes,
							  bytes,
							  bytes,
							  bytes,
							  bytes,
							  bytes);
		}

		[Test]
		[Ignore("Long Running (7 seconds)")]
		public void EncodeThenDecodeNoPadding_Roundtrip()
		{
			byte[] bytes =
					new byte[] {0, 1, 18, 62, 95, 100, 148, 201, 254, 255};

			Permuter.VisitAll(EncodeThenDecodeNoPadding, bytes);
			Permuter.VisitAll(EncodeThenDecodeNoPadding, bytes, bytes);
			Permuter.VisitAll(EncodeThenDecodeNoPadding, bytes, bytes, bytes);
			Permuter.VisitAll(EncodeThenDecodeNoPadding,
							  bytes,
							  bytes,
							  bytes,
							  bytes);
			Permuter.VisitAll(EncodeThenDecodeNoPadding,
							  bytes,
							  bytes,
							  bytes,
							  bytes,
							  bytes);
			Permuter.VisitAll(EncodeThenDecodeNoPadding,
							  bytes,
							  bytes,
							  bytes,
							  bytes,
							  bytes,
							  bytes);
		}

		[Test]
		public void SortsCorrectly()
		{
			string[] stringsToSort = {
											 "hello", "hell", "world", "h", "H",
											 "special"
									 };
			Array.Sort(stringsToSort, new Base32HexComparer());

			string previous = string.Empty;
			ByteComparer byteComparer = new ByteComparer();
			foreach (string s in stringsToSort)
			{
				Assert.IsTrue(byteComparer.Compare(previous, s) <= 0,
							  string.Format("{0} should sort before {1}",
											previous,
											s));
				previous = s;
			}
		}
	}
}