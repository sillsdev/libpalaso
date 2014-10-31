using NUnit.Framework;

namespace PalasoUIWindowsForms.Tests.UniqueToken
{
	[TestFixture, RequiresSTA]
	class UniqueTokenTests
	{
		[Test]
		public void AcquireTokenQuietlyTest()
		{
			const string uniqueIdentifier = "abc";

			bool tokenAcquired = Palaso.UI.WindowsForms.UniqueToken.UniqueToken.AcquireTokenQuietly(uniqueIdentifier);
			Assert.IsTrue(tokenAcquired);

			tokenAcquired = Palaso.UI.WindowsForms.UniqueToken.UniqueToken.AcquireTokenQuietly(uniqueIdentifier);
			Assert.IsFalse(tokenAcquired);

			Palaso.UI.WindowsForms.UniqueToken.UniqueToken.ReleaseToken();

			tokenAcquired = Palaso.UI.WindowsForms.UniqueToken.UniqueToken.AcquireTokenQuietly(uniqueIdentifier);
			Assert.IsTrue(tokenAcquired);

			Palaso.UI.WindowsForms.UniqueToken.UniqueToken.ReleaseToken();
		}

		[Test]
		public void AcquireTokenQuietlyTest_AcquireReleaseAcquireRelease()
		{
			const string uniqueIdentifier = "abc";
			const string uniqueIdentifier2 = "def";

			bool tokenAcquired = Palaso.UI.WindowsForms.UniqueToken.UniqueToken.AcquireTokenQuietly(uniqueIdentifier);
			Assert.IsTrue(tokenAcquired);

			Palaso.UI.WindowsForms.UniqueToken.UniqueToken.ReleaseToken();

			tokenAcquired = Palaso.UI.WindowsForms.UniqueToken.UniqueToken.AcquireTokenQuietly(uniqueIdentifier2);
			Assert.IsTrue(tokenAcquired);

			Palaso.UI.WindowsForms.UniqueToken.UniqueToken.ReleaseToken();
		}

		[Test]
		public void AcquireTokenQuietlyTest_AcquireTwiceNotAllowed()
		{
			const string uniqueIdentifier = "abc";
			const string uniqueIdentifier2 = "def";

			bool tokenAcquired = Palaso.UI.WindowsForms.UniqueToken.UniqueToken.AcquireTokenQuietly(uniqueIdentifier);
			Assert.IsTrue(tokenAcquired);

			tokenAcquired = Palaso.UI.WindowsForms.UniqueToken.UniqueToken.AcquireTokenQuietly(uniqueIdentifier2);
			Assert.IsFalse(tokenAcquired);

			Palaso.UI.WindowsForms.UniqueToken.UniqueToken.ReleaseToken();
		}

		[Test]
		public void ReleaseTokenTest_ReleaseBeforeAcquireOk()
		{
			Palaso.UI.WindowsForms.UniqueToken.UniqueToken.ReleaseToken();
		}

		[Test]
		public void AcquireTokenTest()
		{
			const string uniqueIdentifier = "abc";

			bool tokenAcquired = Palaso.UI.WindowsForms.UniqueToken.UniqueToken.AcquireToken(uniqueIdentifier);
			Assert.IsTrue(tokenAcquired);

			Palaso.UI.WindowsForms.UniqueToken.UniqueToken.ReleaseToken();
		}
	}
}
