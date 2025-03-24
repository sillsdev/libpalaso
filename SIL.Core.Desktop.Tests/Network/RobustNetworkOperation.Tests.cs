using System.Diagnostics;
using NUnit.Framework;
using SIL.Network;

namespace SIL.Tests.Network
{
	[TestFixture]
	public class RobustNetworkOperationTests
	{
		[Test, Ignore("Run by hand")]
		public void DoHttpGetAndGetProxyInfo_404()
		{
			var gotProxy = RobustNetworkOperation.DoHttpGetAndGetProxyInfo(
				"http://hg.palaso.org/", out _, out _, out _, s => Debug.WriteLine(s));
			Assert.That(gotProxy, Is.False); // ... or throws.
		}

		[Test]
		public void DoHttpGetAndGetProxyInfo_NoProxy_ReturnsFalse()
		{
			var gotProxy = RobustNetworkOperation.DoHttpGetAndGetProxyInfo(
				"https://sil.org/", out _, out _, out _, s => Debug.WriteLine(s));
			Assert.That(gotProxy, Is.False);
		}
	}
}
