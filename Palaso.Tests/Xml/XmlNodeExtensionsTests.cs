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
	}
}
