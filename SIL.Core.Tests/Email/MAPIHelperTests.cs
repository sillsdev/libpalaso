// Copyright (c) 2018 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using NUnit.Framework;
using SIL.Email;
using SIL.IO;

namespace SIL.Tests.Email
{
	public class MAPIHelperTests
	{
		[Test]
		[Explicit("by hand only")]
		[Platform(Include = "Win", Reason = "Windows specific test")]
		public void TestSendEmail()
		{
			using (var file = new TempFile("TestSendEmail test"))
			{
				var mapi = new MAPI();
				mapi.AddRecipientTo("pretend@8ksdfj83jls8.com");
				mapi.AddAttachment(file.Path);
				Assert.That(() => mapi.SendMailDirect("test", "testbody"), Throws.Nothing);
			}
		}
	}
}
