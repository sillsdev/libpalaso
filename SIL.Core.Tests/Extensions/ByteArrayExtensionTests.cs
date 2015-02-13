using System;
using System.Text;
using NUnit.Framework;
using SIL.Extensions;

namespace SIL.Tests.Extensions
{
	[TestFixture]
	public class ByteArrayExtensionTests
	{
		[Test]
		public void SameIdenticalByteArrayAreEqual()
		{
			var data = Encoding.UTF8.GetBytes(@"<element guid='c1ee3115-e382-11de-8a39-0800200c9a66' />");
			Assert.IsTrue(data.AreByteArraysEqual(data));
		}

		[Test]
		public void NullTargetIsNeverEqualWithExistingSource()
		{
			var data = Encoding.UTF8.GetBytes(@"<element guid='c1ee3115-e382-11de-8a39-0800200c9a66' />");
			Assert.IsFalse(data.AreByteArraysEqual(null));
		}

		[Test]
		public void DifferentLengthArraysWithCommonContentToAPointAreNotEqual()
		{
			// Note: this test has common bytes, up to a point.
			var data1 = Encoding.UTF8.GetBytes(@"<element val='True' />");
			var data2 = Encoding.UTF8.GetBytes(@"<element val='False' />");
			Assert.IsFalse(data1.AreByteArraysEqual(data2));
		}

		[Test]
		public void DifferentLengthArraysWithDifferentContentAreNotEqual()
		{
			// Note: this test has very few common bytes, beyond the opening "<" and normal attr stuff like "=" and "'", and the closing " />".
			var data1 = Encoding.UTF8.GetBytes(@"<element val='True' />");
			var data2 = Encoding.UTF8.GetBytes(@"<foo nameoffoo='mystuff' />");
			Assert.IsFalse(data1.AreByteArraysEqual(data2));
		}

		[Test]
		public void SameLengthButSomewhatDifferentDifferentContentArraysAreNotEqual()
		{
			var data1 = Encoding.UTF8.GetBytes(@"<element val='sourcestuff' />");
			var data2 = Encoding.UTF8.GetBytes(@"<element val='targetstuff' />");
			Assert.IsFalse(data1.AreByteArraysEqual(data2));
		}

		[Test]
		public void SameLengthButVeryDifferentDifferentContentArraysAreNotEqual()
		{
			var data1 = Encoding.UTF8.GetBytes(@"<element val='sourcestuff' />");
			var data2 = Encoding.UTF8.GetBytes(@"<tnemele lav='ffutsecruos' />"); // Same workd, but in reversal order of the letters
			Assert.IsFalse(data1.AreByteArraysEqual(data2));
		}

		[Test]
		public void SameContentByteArrayAreEqual()
		{
			var data1 = Encoding.UTF8.GetBytes(@"<element guid='c1ee3115-e382-11de-8a39-0800200c9a66' />");
			var data2 = Encoding.UTF8.GetBytes(@"<element guid='c1ee3115-e382-11de-8a39-0800200c9a66' />");
			Assert.IsTrue(data1.AreByteArraysEqual(data2));
		}

		[Test]
		public void NullSourceThrows()
		{
			var data = Encoding.UTF8.GetBytes(@"<element guid='c1ee3115-e382-11de-8a39-0800200c9a66' />");
			Assert.Throws<NullReferenceException>(() => ((byte[])null).AreByteArraysEqual(data));
		}
	}
}