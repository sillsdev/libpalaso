using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Palaso.Base32;

namespace Palaso.Tests.Base32Tests
{
	[TestFixture]
	public class Base32Tests
	{
		static private string Encode (string s)
		{
			return Base32Convert.ToBase32String(Encoding.UTF7.GetBytes(s), Base32FormattingOptions.InsertTrailingPadding);
		}

		static private string EncodeOmitPadding(string s)
		{
			return Base32Convert.ToBase32String(Encoding.UTF7.GetBytes(s), Base32FormattingOptions.None);
		}


		static private string Decode(string s)
		{
			byte[] bytes = Base32Convert.FromBase32String(s, Base32FormattingOptions.InsertTrailingPadding);
			return Encoding.UTF7.GetString(bytes);
		}

		static private string DecodeOmitPadding(string s)
		{
			byte[] bytes = Base32Convert.FromBase32String(s, Base32FormattingOptions.None);
			return Encoding.UTF7.GetString(bytes);
		}


		[Test]
		public void Encode_EmptyString_EmptyString()
		{
			Assert.AreEqual(string.Empty, Encode(""));
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void Encode_null_throws()
		{
			Base32Convert.ToBase32String(null, Base32FormattingOptions.InsertTrailingPadding);
		}

		[Test]
		public void Encode_rfc4648()
		{
			Assert.AreEqual("MY======", Encode("f"));
			Assert.AreEqual("MZXQ====", Encode("fo"));
			Assert.AreEqual("MZXW6===", Encode("foo"));
			Assert.AreEqual("MZXW6YQ=", Encode("foob"));
			Assert.AreEqual("MZXW6YTB", Encode("fooba"));
			Assert.AreEqual("MZXW6YTBOI======", Encode("foobar"));
		}


		[Test]
		public void EncodeOmitPadding_EmptyString_EmptyString()
		{
			Assert.AreEqual(string.Empty, EncodeOmitPadding(""));
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void EncodeOmitPadding_null_throws()
		{
			Base32Convert.ToBase32String(null, Base32FormattingOptions.None);
		}

		[Test]
		public void EncodeOmitPadding_rfc4648()
		{
			Assert.AreEqual("MY", EncodeOmitPadding("f"));
			Assert.AreEqual("MZXQ", EncodeOmitPadding("fo"));
			Assert.AreEqual("MZXW6", EncodeOmitPadding("foo"));
			Assert.AreEqual("MZXW6YQ", EncodeOmitPadding("foob"));
			Assert.AreEqual("MZXW6YTB", EncodeOmitPadding("fooba"));
			Assert.AreEqual("MZXW6YTBOI", EncodeOmitPadding("foobar"));
		}


		[Test]
		public void Decode_EmptyString_EmptyString()
		{
			Assert.AreEqual(string.Empty, Decode(""));
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void Decode_null_throws()
		{
			Base32Convert.FromBase32String(null, Base32FormattingOptions.InsertTrailingPadding);
		}

		[Test]
		public void Decode_wholealphabet()
		{
			Assert.AreEqual(36, Decode("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ234567======").Length);
		}

		[Test]
		public void DecodeOmitPadding_wholealphabet()
		{
			Assert.AreEqual(36, DecodeOmitPadding("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ234567").Length);
		}

		[Test]
		public void Decode_rfc4648()
		{
			Assert.AreEqual("f",      Decode("MY======"));
			Assert.AreEqual("fo",     Decode("MZXQ===="));
			Assert.AreEqual("foo",    Decode("MZXW6==="));
			Assert.AreEqual("foob",   Decode("MZXW6YQ="));
			Assert.AreEqual("fooba",  Decode("MZXW6YTB"));
			Assert.AreEqual("foobar", Decode("MZXW6YTBOI======"));
		}

		[Test]
		public void Decode_rfc4648_lowercase()
		{
			Assert.AreEqual("f", Decode("my======"));
			Assert.AreEqual("fo", Decode("mzxq===="));
			Assert.AreEqual("foo", Decode("mzxw6==="));
			Assert.AreEqual("foob", Decode("mzxw6yq="));
			Assert.AreEqual("fooba", Decode("mzxw6ytb"));
			Assert.AreEqual("foobar", Decode("mzxw6ytboi======"));
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
	   public void Decode_CharacterNotInAlphabet_throws()
		{
			Base32Convert.FromBase32String("#y======", Base32FormattingOptions.InsertTrailingPadding);

		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void Decode_MisplacedPaddingCharacter_throws()
		{
			Base32Convert.FromBase32String("m=y======", Base32FormattingOptions.InsertTrailingPadding);

		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void Decode_WrongNumberOfPaddingCharacters_throws()
		{
			Base32Convert.FromBase32String("my=====", Base32FormattingOptions.InsertTrailingPadding);

		}


		[Test]
		public void DecodeOmitPadding_EmptyString_EmptyString()
		{
			Assert.AreEqual(string.Empty, DecodeOmitPadding(""));
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void DecodeOmitPadding_null_throws()
		{
			Base32Convert.FromBase32String(null, Base32FormattingOptions.None);
		}

		[Test]
		public void DecodeOmitPadding_rfc4648()
		{
			Assert.AreEqual("f", DecodeOmitPadding("MY"));
			Assert.AreEqual("fo", DecodeOmitPadding("MZXQ"));
			Assert.AreEqual("foo", DecodeOmitPadding("MZXW6"));
			Assert.AreEqual("foob", DecodeOmitPadding("MZXW6YQ"));
			Assert.AreEqual("fooba", DecodeOmitPadding("MZXW6YTB"));
			Assert.AreEqual("foobar", DecodeOmitPadding("MZXW6YTBOI"));
		}

		[Test]
		public void DecodeOmitPadding_rfc4648_lowercase()
		{
			Assert.AreEqual("f", DecodeOmitPadding("my"));
			Assert.AreEqual("fo", DecodeOmitPadding("mzxq"));
			Assert.AreEqual("foo", DecodeOmitPadding("mzxw6"));
			Assert.AreEqual("foob", DecodeOmitPadding("mzxw6yq"));
			Assert.AreEqual("fooba", DecodeOmitPadding("mzxw6ytb"));
			Assert.AreEqual("foobar", DecodeOmitPadding("mzxw6ytboi"));
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void DecodeOmitPadding_CharacterNotInAlphabet_throws()
		{
			Base32Convert.FromBase32String("#y", Base32FormattingOptions.None);

		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void DecodeOmitPadding_PaddingCharacter_throws()
		{
			Base32Convert.FromBase32String("my======", Base32FormattingOptions.None);

		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void DecodeOmitPadding_WrongNumberOfCharacters_1_throws()
		{
			Base32Convert.FromBase32String("1", Base32FormattingOptions.None);

		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void DecodeOmitPadding_WrongNumberOfCharacters_3_throws()
		{
			Base32Convert.FromBase32String("123", Base32FormattingOptions.None);
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void DecodeOmitPadding_WrongNumberOfCharacters_6_throws()
		{
			Base32Convert.FromBase32String("123456", Base32FormattingOptions.None);
		}


		[Test]
		[Category("Long Running")] // ~7 seconds
		public void EncodeThenDecode_Roundtrip()
		{

			byte[] bytes = new byte[] { 0, 1, 18, 62, 95, 100, 148, 201, 254, 255 };

			Permuter.VisitAll(EncodeThenDecode, bytes);
			Permuter.VisitAll(EncodeThenDecode, bytes, bytes);
			Permuter.VisitAll(EncodeThenDecode, bytes, bytes, bytes);
			Permuter.VisitAll(EncodeThenDecode, bytes, bytes, bytes, bytes);
			Permuter.VisitAll(EncodeThenDecode, bytes, bytes, bytes, bytes, bytes);
			Permuter.VisitAll(EncodeThenDecode, bytes, bytes, bytes, bytes, bytes, bytes);

		}

		[Test]
		[Category("Long Running")] // ~7 seconds
		public void EncodeThenDecodeNoPadding_Roundtrip()
		{

			byte[] bytes = new byte[] { 0, 1, 18, 62, 95, 100, 148, 201, 254, 255 };

			Permuter.VisitAll(EncodeThenDecodeNoPadding, bytes);
			Permuter.VisitAll(EncodeThenDecodeNoPadding, bytes, bytes);
			Permuter.VisitAll(EncodeThenDecodeNoPadding, bytes, bytes, bytes);
			Permuter.VisitAll(EncodeThenDecodeNoPadding, bytes, bytes, bytes, bytes);
			Permuter.VisitAll(EncodeThenDecodeNoPadding, bytes, bytes, bytes, bytes, bytes);
			Permuter.VisitAll(EncodeThenDecodeNoPadding, bytes, bytes, bytes, bytes, bytes, bytes);

		}

		static public void EncodeThenDecode(IEnumerator[] a)
		{
			List<byte> octets = new List<byte>();
			for (int j = 1; j < a.Length; j++)
			{
				octets.Add((byte)a[j].Current);
			}

			byte[] result = Base32Convert.FromBase32String(Base32Convert.ToBase32String(octets, Base32FormattingOptions.InsertTrailingPadding), Base32FormattingOptions.InsertTrailingPadding);
			Assert.IsTrue(SameContent(result, octets, Comparer<byte>.Default));
		}

		static public void EncodeThenDecodeNoPadding(IEnumerator[] a)
		{
			List<byte> octets = new List<byte>();
			for (int j = 1; j < a.Length; j++)
			{
				octets.Add((byte)a[j].Current);
			}

			byte[] result = Base32Convert.FromBase32String(Base32Convert.ToBase32String(octets, Base32FormattingOptions.None), Base32FormattingOptions.None);
			Assert.IsTrue(SameContent(result, octets, Comparer<byte>.Default));
		}

		static public bool SameContent<T>(IEnumerable<T> first, IEnumerable<T> second, Comparer<T> comparer)
		{
			IEnumerator<T> enumerator1 = first.GetEnumerator();
			IEnumerator<T> enumerator2 = second.GetEnumerator();
			while (enumerator1.MoveNext())
			{
				if(!enumerator2.MoveNext())
				{
					return false;
				}
				if(comparer.Compare(enumerator1.Current, enumerator2.Current) != 0)
				{
					return false;
				}
			}

			if (enumerator2.MoveNext())
				return false;
			else
				return true;

		}

	}
}
