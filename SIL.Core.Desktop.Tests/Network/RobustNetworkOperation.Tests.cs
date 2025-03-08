using System.Diagnostics;
using NUnit.Framework;
using SIL.Network;

namespace SIL.Tests.Network
{
	[TestFixture]
	public class RobustNetworkOperationTests
	{


		[Test, Ignore("Run by hand")]
		public void DoHttpGetAndGetProxyInfo()
		{
			_ = RobustNetworkOperation.DoHttpGetAndGetProxyInfo(
				"http://hg.palaso.org/", out _, out _, out _, s => Debug.WriteLine(s));
		}

	}
}
