using NUnit.Framework;
using SIL.Core.ClearShare;

namespace SIL.Tests.ClearShare
{
	/// ----------------------------------------------------------------------------------------
	[TestFixture]
	public class LicenseTests
	{
		/// ------------------------------------------------------------------------------------
		[Test]
		public void AreContentsEqual_OtherIsNull_ReturnsFalse()
		{
			var l = License.CreativeCommons_Attribution_ShareAlike;
			Assert.IsFalse(l.AreContentsEqual(null));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void AreContentsEqual_AreDifferent_ReturnsFalse()
		{
			var l1 = License.CreativeCommons_Attribution_ShareAlike;
			var l2 = License.CreativeCommons_Attribution;
			Assert.IsFalse(l1.AreContentsEqual(l2));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void AreContentsEqual_AreSame_ReturnsTrue()
		{
			var l1 = License.CreativeCommons_Attribution_ShareAlike;
			var l2 = License.CreativeCommons_Attribution_ShareAlike;
			Assert.IsTrue(l1.AreContentsEqual(l2));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void Equals_SameInstance_ReturnsTrue()
		{
			var l = License.CreativeCommons_Attribution_ShareAlike;
			Assert.IsTrue(l.Equals(l));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void Equals_CompareToNull_ReturnsFalse()
		{
			var l = License.CreativeCommons_Attribution_ShareAlike;
			Assert.IsFalse(l.Equals(null));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void Equals_CompareToObjOfDifferentType_ReturnsFalse()
		{
			var l = License.CreativeCommons_Attribution_ShareAlike;
			Assert.IsFalse(l.Equals("junk"));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void Equals_AreSame_ReturnsTrue()
		{
			var l1 = License.CreativeCommons_Attribution_ShareAlike;
			var l2 = License.CreativeCommons_Attribution_ShareAlike;
			Assert.IsTrue(l1.Equals(l2));
		}
	}
}
