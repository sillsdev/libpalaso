using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using NUnit.Framework;
using SIL.IO;
using SIL.TestUtilities;

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

		// This attempts to be a regression test for a problem (Bloom BL-5416) in using
		// RobustIO.LoadXElement() with a file name that contained some RTL script.
		// However, I could not come up with a unit test in which the old code actually failed,
		// though it definitely and repeatably failed in Bloom using as far as I can tell
		// these exact strings in the path (though in Bloom's document folder, not Temp).
		[Test]
		public void LoadXElement_WithNRFileName_DoesNotCrash()
		{
			using (var folder = new TemporaryFolder("Balangao کتابونه"))
			{
				var input = new XElement("root");
				var fileName = Path.Combine(folder.Path, "Balangao کتابونه.xml");
				input.Save(fileName);
				Assert.DoesNotThrow(() => RobustIO.LoadXElement(fileName));
			}
		}
	}
}
