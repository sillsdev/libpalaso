using System.Text;
using NUnit.Framework;
using Palaso.Extensions;

namespace Palaso.Tests.Extensions
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
		public void VariationsOnANullAreNeverEqual()
		{
			var data = Encoding.UTF8.GetBytes(@"<element guid='c1ee3115-e382-11de-8a39-0800200c9a66' />");
			Assert.IsFalse(data.AreByteArraysEqual(null));
			Assert.IsFalse(((byte[])null).AreByteArraysEqual(data));
			Assert.IsFalse(((byte[])null).AreByteArraysEqual(null));
		}

		[Test]
		public void DifferentLengthArraysAreNotEqual()
		{
			var data1 = Encoding.UTF8.GetBytes(@"<element val='True' />");
			var data2 = Encoding.UTF8.GetBytes(@"<element val='False' />");
			Assert.IsFalse(data1.AreByteArraysEqual(data2));
		}

		[Test]
		public void SameLengthButDifferentContentArraysAreNotEqual()
		{
			var data1 = Encoding.UTF8.GetBytes(@"<element val='True' />");
			var data2 = Encoding.UTF8.GetBytes(@"<element val='true' />");
			Assert.IsFalse(data1.AreByteArraysEqual(data2));
		}

		[Test]
		public void SameContentByteArrayAreEqual()
		{
			var data1 = Encoding.UTF8.GetBytes(@"<element guid='c1ee3115-e382-11de-8a39-0800200c9a66' />");
			var data2 = Encoding.UTF8.GetBytes(@"<element guid='c1ee3115-e382-11de-8a39-0800200c9a66' />");
			Assert.IsTrue(data1.AreByteArraysEqual(data2));
		}
	}
}