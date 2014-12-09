using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using WritingSystems.WritingSystems;

namespace WritingSystems.Tests.WritingSystems
{
	[TestFixture]
	public class Iso15924ScriptTests
	{
		[Test]
		public void ShortLabel_HasParens_RemovesParens()
		{
			Assert.AreEqual("Korean", new Iso15924Script("Korean (Hangul + Han)", "Kore").ShortLabel());
		}

		[Test]
		public void ShortLabel_Normal_Same()
		{
			Assert.AreEqual("Pahawh Hmong", new Iso15924Script("Pahawh Hmong", "Hmng").ShortLabel());
		}
	}
}
