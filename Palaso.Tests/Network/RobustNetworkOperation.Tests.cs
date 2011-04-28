using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Palaso.Network;
using Palaso.Reporting;

namespace Palaso.Tests.reporting
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
