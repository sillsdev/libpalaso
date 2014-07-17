using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using NUnit.Framework;
using Palaso.Xml;

namespace Palaso.Tests.Xml
{
	[TestFixture]
	public class XmlNodeExtensionsTests
	{
		[Test]
		public void SelectSingleNodeHonoringDefaultNS_XHtml_CanFindTextArea()
		{
			var dom = new XmlDocument();
			dom.LoadXml(@"<?xml version='1.0' encoding='utf-8'?>
<html xmlns='http://www.w3.org/1999/xhtml'>
  <body>
	  <div>
<textarea></textarea>
		</div>
  </body></html>");

			Assert.IsNotNull(dom.SelectSingleNodeHonoringDefaultNS("//textarea"));


			Assert.IsNotNull(dom.SelectSingleNodeHonoringDefaultNS("html/body/div/textarea"));
		}

		[Test]
		public void SelectSingleNodeHonoringDefaultNS_XHtml_CanFindAncestorDiv()
		{
			var dom = new XmlDocument();
			dom.LoadXml(@"<?xml version='1.0' encoding='utf-8'?>
<html xmlns='http://www.w3.org/1999/xhtml'>
  <body>
	  <div>
<textarea></textarea>
		</div>
  </body></html>");

			var textarea = dom.SelectSingleNodeHonoringDefaultNS("//textarea");
			var div = textarea.SelectSingleNodeHonoringDefaultNS("ancestor::div");
			Assert.IsNotNull(div);
		}

		[Test]
		public void DeleteNodes()
		{
			var dom = new XmlDocument();
			dom.LoadXml(@"<?xml version='1.0' encoding='utf-8'?>
<html xmlns='http://www.w3.org/1999/xhtml'>
	<body>
		<div class='A'><textarea></textarea></div>
		<div class='B'><textarea></textarea></div>
		<div class='A'><textarea></textarea></div>
	</body>
</html>");

			dom.DeleteNodes("descendant-or-self::*[contains(@class, 'A')]");
			Assert.AreEqual(1, dom.SafeSelectNodes("html/body/div").Count);
		}

		[Test]
		public void DeleteNodes_NestedNodes()
		{
			var dom = new XmlDocument();
			dom.LoadXml(@"<?xml version='1.0' encoding='utf-8'?>
<html>
	<body>
		<div class='A'><div class='A'>child</div></div>
		<div class='B'><div class='A'>child</div><textarea></textarea></div>
		<div class='A'><textarea></textarea></div>
	</body>
</html>");

			dom.DeleteNodes("descendant-or-self::*[contains(@class, 'A')]");

			Assert.AreEqual(1, dom.SafeSelectNodes("html/body/div").Count);
			Assert.AreEqual("<textarea></textarea>", dom.SafeSelectNodes("html/body/div")[0].InnerXml);
		}
	}
}
