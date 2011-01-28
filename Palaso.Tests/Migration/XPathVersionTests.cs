using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Palaso.Migration;
using Palaso.TestUtilities;

using NUnit.Framework;

namespace Palaso.Tests.Migration
{
	[TestFixture]
	public class XPathVersionTests
	{
		[Test]
		public void GetFileVersion_WithVersionAttribute_CorrectVersion()
		{
			string xml = @"<?xml version='1.0' encoding='UTF-8' ?>
<configuration version='3'>
  <blah />
</configuration>
".Replace("'", "\"");

			using (var file = new TempFile(xml))
			{
				var xPathVersion = new XPathVersion(10, "configuration/@version");
				int result = xPathVersion.GetFileVersion(file.Path);
				Assert.That(result, Is.EqualTo(3));
			}
		}
	}
}
