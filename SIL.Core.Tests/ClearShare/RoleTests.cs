using NUnit.Framework;
using SIL.Core.ClearShare;

namespace SIL.Tests.ClearShare
{
	[TestFixture]
	public class RoleTests
	{
		/// ------------------------------------------------------------------------------------
		[Test]
		public void Clone_ReturnsDifferentNonNullObject()
		{
			var r = new Role("code", "name", "def");
			var clone = r.Clone();
			Assert.IsNotNull(clone);
			Assert.AreNotSame(r, clone);
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void Clone_ReturnsSameContent()
		{
			var r = new Role("code", "name", "def");
			Assert.IsTrue(r.AreContentsEqual(r.Clone()));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void AreContentsEqual_OtherIsNull_ReturnsFalse()
		{
			var r = new Role(null, null, null);
			Assert.IsFalse(r.AreContentsEqual(null));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void AreContentsEqual_DifferentCodes_ReturnsFalse()
		{
			var r1 = new Role("codered", null, null);
			var r2 = new Role("codeblue", null, null);
			Assert.IsFalse(r1.AreContentsEqual(r2));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void AreContentsEqual_DifferentNames_ReturnsFalse()
		{
			var r1 = new Role("codered", "Jaguar", null);
			var r2 = new Role("codered", "Aston Martin", null);
			Assert.IsFalse(r1.AreContentsEqual(r2));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void AreContentsEqual_DifferentDefinitions_ReturnsFalse()
		{
			var r1 = new Role("codered", "Jaguar", "black");
			var r2 = new Role("codered", "Jaguar", "brown");
			Assert.IsFalse(r1.AreContentsEqual(r2));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void AreContentsEqual_AllSame_ReturnsTrue()
		{
			var r1 = new Role("codered", "Jaguar", "black");
			var r2 = new Role("codered", "Jaguar", "black");
			Assert.IsTrue(r1.AreContentsEqual(r2));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void Equals_SameInstance_ReturnsTrue()
		{
			var r = new Role("codered", "Jaguar", "black");
			Assert.IsTrue(r.Equals(r));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void Equals_CompareToNull_ReturnsFalse()
		{
			var r = new Role("codered", "Jaguar", "black");
			Assert.IsFalse(r.Equals(null));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void Equals_CompareToObjOfDifferentType_ReturnsFalse()
		{
			var r = new Role("codered", "Jaguar", "black");
			Assert.IsFalse(r.Equals("junk"));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void Equals_AllSame_ReturnsTrue()
		{
			var r1 = new Role("codered", "Jaguar", "black");
			var r2 = new Role("codered", "Jaguar", "black");
			Assert.IsTrue(r1.Equals(r2));
		}
	}
}
