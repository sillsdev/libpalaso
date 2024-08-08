using System;
using NUnit.Framework;
using SIL.Core.ClearShare;

namespace SIL.Tests.ClearShare
{
	[TestFixture]
	public class ContributionTests
	{
		/// ------------------------------------------------------------------------------------
		[Test]
		public void Clone_ReturnsDifferentNonNullObject()
		{
			var r = new Role("dev", "developer", "def");
			var c = new Contribution("name", r);
			var clone = c.Clone();
			Assert.IsNotNull(clone);
			Assert.AreNotSame(c, clone);
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void Clone_ReturnsSameContent()
		{
			var r = new Role("dev", "developer", "def");
			var c = new Contribution("name", r);
			c.Date = DateTime.Now;
			c.Comments = "stupid note";
			Assert.IsTrue(c.AreContentsEqual(c.Clone() as Contribution));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void Clone_RolesAreDifferentObject()
		{
			var r = new Role("dev", "developer", "def");
			var c = new Contribution("name", r).Clone() as Contribution;
			Assert.AreNotSame(r, c.Role);
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void AreContentsEqual_OtherIsNull_ReturnsFalse()
		{
			var c = new Contribution(null, null);
			Assert.IsFalse(c.AreContentsEqual(null));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void AreContentsEqual_NamesDifferent_ReturnsFalse()
		{
			var c1 = new Contribution("joey", null);
			var c2 = new Contribution("bucky", null);
			Assert.IsFalse(c1.AreContentsEqual(c2));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void AreContentsEqual_RolesDifferent_ReturnsFalse()
		{
			var r1 = new Role("codered", null, null);
			var r2 = new Role("codeblue", null, null);

			var c1 = new Contribution("joey", r1);
			var c2 = new Contribution("joey", r2);
			Assert.IsFalse(c1.AreContentsEqual(c2));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void AreContentsEqual_LicensesDifferent_ReturnsFalse()
		{
			var l1 = License.CreativeCommons_Attribution;
			var l2 = License.CreativeCommons_Attribution_ShareAlike;

			var c1 = new Contribution("joey", null) { ApprovedLicense = l1 };
			var c2 = new Contribution("joey", null) { ApprovedLicense = l2 };
			Assert.IsFalse(c1.AreContentsEqual(c2));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void AreContentsEqual_DatesDifferent_ReturnsFalse()
		{
			var c1 = new Contribution("bucky", null) { Date = DateTime.Now };
			var c2 = new Contribution("bucky", null) { Date = DateTime.Now.AddDays(1) };
			Assert.IsFalse(c1.AreContentsEqual(c2));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void AreContentsEqual_NotesDifferent_ReturnsFalse()
		{
			var c1 = new Contribution("bucky", null) { Comments = "get bread" };
			var c2 = new Contribution("bucky", null) { Comments = "get pickles" };
			Assert.IsFalse(c1.AreContentsEqual(c2));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void AreContentsEqual_AllSame_ReturnsTrue()
		{
			var r1 = new Role("dev", "developer", "def");
			var r2 = new Role("dev", "developer", "def");

			var l1 = License.CreativeCommons_Attribution;
			var l2 = License.CreativeCommons_Attribution;
			var d1 = DateTime.Now;

			var c1 = new Contribution("joey", r1) { Date = d1, Comments = "get bread", ApprovedLicense = l1 };
			var c2 = new Contribution("joey", r2) { Date = d1, Comments = "get bread", ApprovedLicense = l2 };
			Assert.IsTrue(c1.AreContentsEqual(c2));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void Equals_SameInstance_ReturnsTrue()
		{
			var c = new Contribution("joey", null);
			Assert.IsTrue(c.Equals(c));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void Equals_CompareToNull_ReturnsFalse()
		{
			var c = new Contribution("joey", null);
			Assert.IsFalse(c.Equals(null));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void Equals_CompareToObjOfDifferentType_ReturnsFalse()
		{
			var c = new Contribution("joey", null);
			Assert.IsFalse(c.Equals("junk"));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void Equals_DateIsNull_DoesNotThrow()
		{
			var c1 = new Contribution("joey", null) { Comments = "note" };
			var c2 = new Contribution("joey", null) { Comments = "note" };
			Assert.IsTrue(c1.Equals(c2));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void Equals_NoteIsNull_DoesNotThrow()
		{
			var now = DateTime.UtcNow;
			var c1 = new Contribution("joey", null) { Date = now };
			var c2 = new Contribution("joey", null) { Date = now };
			Assert.IsTrue(c1.Equals(c2));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void Equals_AllSame_ReturnsTrue()
		{
			var r1 = new Role("dev", "developer", "def");
			var r2 = new Role("dev", "developer", "def");

			var l1 = License.CreativeCommons_Attribution;
			var l2 = License.CreativeCommons_Attribution;

			var d1 = DateTime.Now;

			var c1 = new Contribution("joey", r1) { Date = d1, Comments = "get bread", ApprovedLicense = l1 };
			var c2 = new Contribution("joey", r2) { Date = d1, Comments = "get bread", ApprovedLicense = l2 };
			Assert.IsTrue(c1.Equals(c2));
		}
	}
}
