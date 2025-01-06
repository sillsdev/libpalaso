using System;
using NUnit.Framework;
using SIL.Core.ClearShare;

namespace SIL.Tests.ClearShare
{
	[TestFixture]
	public class OlacSystemTests
	{
		private OlacSystem _system;

		[SetUp]
		public void TestSetup()
		{
			_system = new OlacSystem();
		}

		[Test]
		public void GetContributorElement_NullRoleCode_ThrowsException()
		{
			Assert.Throws<ArgumentNullException>(() => _system.GetContributorElement(null, "name"));
		}

		[Test]
		public void GetContributorElement_NullName_ThrowsException()
		{
			Assert.Throws<ArgumentNullException>(() => _system.GetContributorElement("code", null));
		}

		[Test]
		public void GetContributorElement_HasGoodRoleCodeAndName_ReturnsElement()
		{
			Assert.AreEqual("<dc:contributor xsi:type='olac:role' olac:code='secret' " +
				"view='Compiler'>willy</dc:contributor>", _system.GetContributorElement("secret", "willy"));
		}

		[Test]
		public void TryGetRoleByCode_TryInvalidCode_ReturnsFalse()
		{
			Role role;
			Assert.IsFalse(_system.TryGetRoleByCode("junk", out role));
		}

		[Test]
		public void TryGetRoleByCode_TryNullCode_ReturnsFalse()
		{
			Role role;
			Assert.IsFalse(_system.TryGetRoleByCode(null, out role));
		}

		[Test]
		public void TryGetRoleByCode_TryGoodCode_ReturnsTrueAndRole()
		{
			Role role;
			Assert.IsTrue(_system.TryGetRoleByCode("developer", out role));
			Assert.AreEqual("developer", role.Code);
		}

		[Test]
		public void TryGetRoleByName_TryInvalidName_ReturnsFalse()
		{
			Role role;
			Assert.IsFalse(_system.TryGetRoleByName("junk", out role));
		}

		[Test]
		public void TryGetRoleByName_TryNullName_ReturnsFalse()
		{
			Role role;
			Assert.IsFalse(_system.TryGetRoleByName(null, out role));
		}

		[Test]
		public void TryGetRoleByName_TryGoodName_ReturnsTrueAndRole()
		{
			Role role;
			Assert.IsTrue(_system.TryGetRoleByName("Developer", out role));
			Assert.AreEqual("Developer", role.Name);
		}

		[Test]
		public void GetRoleByCodeOrThrow_TryInvalidCode_ThrowsException()
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => _system.GetRoleByCodeOrThrow("junk"));
		}

		[Test]
		public void GetRoleByCodeOrThrow_TryNullCode_ThrowsException()
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => _system.GetRoleByCodeOrThrow(null));
		}

		[Test]
		public void GetRoleByCodeOrThrow_TryGoodCode_ReturnsRole()
		{
			Assert.AreEqual("developer", _system.GetRoleByCodeOrThrow("developer").Code);
		}
	}
}
