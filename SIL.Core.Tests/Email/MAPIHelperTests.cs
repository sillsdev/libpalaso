// Copyright (c) 2018 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using NUnit.Framework;
using SIL.Email;

namespace SIL.Tests.Email
{
	public class MAPIHelperTests
	{
		[Test]
		[Ignore("by hand only")]
		[Platform(Include = "Windows", Reason = "Windows specific test")]
		public void TestSendEmail()
		{
			MAPI x = new MAPI();
			x.AddRecipientTo("pretend@8ksdfj83jls8.com");
			x.SendMailDirect("test", "testbody");
		}
	}
}
