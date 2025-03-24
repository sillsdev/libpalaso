using System.Diagnostics;
using NUnit.Framework;
using SIL.Network;

namespace SIL.Tests.Network
{
	[TestFixture]
	public class RobustNetworkOperationTests
	{
		[Test]
		public void DoHttpGetAndGetProxyInfo_404_ReturnsFalse()
		{
			var gotProxy = RobustNetworkOperation.DoHttpGetAndGetProxyInfo(
				"http://hg.palaso.org/", out _, out _, out _, s => Debug.WriteLine(s));
			Assert.That(gotProxy, Is.False);
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
