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
			string host, userName, password;
			bool gotProxy = RobustNetworkOperation.DoHttpGetAndGetProxyInfo("http://hg.palaso.org/", out host, out userName, out password, s=>Debug.WriteLine(s));
		}

	}
}
