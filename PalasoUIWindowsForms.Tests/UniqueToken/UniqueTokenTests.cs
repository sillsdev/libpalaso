using NUnit.Framework;

namespace PalasoUIWindowsForms.Tests.UniqueToken
{
	[TestFixture, RequiresSTA]
	class UniqueTokenTests
	{
		[Test]
		public void AcquireTokenQuietly_SucceedsWhenTokenNotCurrentlyHeld()
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
		public void AcquireTokenQuietlyTest_AcquireWithDifferentIdentifierAfterRelease()
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
		public void ReleaseTokenTest_ReleaseBeforeAcquireDoesNotThrow()
		{
			Assert.DoesNotThrow(Palaso.UI.WindowsForms.UniqueToken.UniqueToken.ReleaseToken);
		}

		[Test]
		public void AcquireToken_SucceedsWhenTokenNotCurrentlyHeld()
		{
			const string uniqueIdentifier = "abc";

			// No wait so test doesn't take a while
			bool tokenAcquired = Palaso.UI.WindowsForms.UniqueToken.UniqueToken.AcquireToken(uniqueIdentifier, null, 0);
			Assert.IsTrue(tokenAcquired);

			using (new Palaso.Reporting.ErrorReport.NonFatalErrorReportExpected())
			{
				tokenAcquired = Palaso.UI.WindowsForms.UniqueToken.UniqueToken.AcquireToken(uniqueIdentifier, null, 0);
				Assert.IsFalse(tokenAcquired);
			}

			Palaso.UI.WindowsForms.UniqueToken.UniqueToken.ReleaseToken();

			tokenAcquired = Palaso.UI.WindowsForms.UniqueToken.UniqueToken.AcquireToken(uniqueIdentifier, null, 0);
			Assert.IsTrue(tokenAcquired);

			Palaso.UI.WindowsForms.UniqueToken.UniqueToken.ReleaseToken();
		}
	}
}
