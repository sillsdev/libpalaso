using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using NUnit.Framework;
using SIL.IO;

namespace SIL.Tests.IO
{
	/// <summary>
	/// Class to hold tests of RobustIO routines.
	/// </summary>
	public class RobustIOTests
	{
		[Test]
		public void SaveXml()
		{
			using (var temp = new TempFile())
			{
				var doc = new XmlDocument();
				doc.LoadXml("<?xml version='1.0' encoding='utf-8'?><root>This is a test</root>");
				RobustIO.SaveXml(doc, temp.Path);
				// Not a great test, since it captures some non-essential features of how XmlDocument writes its content.
				// But this is at least a reasonable result to get from doing the above.
				Assert.That(File.ReadAllText(temp.Path, Encoding.UTF8), Is.EqualTo("<?xml version=\"1.0\" encoding=\"utf-8\"?>" + Environment.NewLine + "<root>This is a test</root>"));
			}
		}
	}
}
