using System.Xml;
using NUnit.Framework;
using Palaso.IO;
using System.Linq;
using Palaso.UI.WindowsForms.ClearShare;

namespace PalasoUIWindowsForms.Tests.ClearShare
{
	/// <summary>
	/// High-level examples of using ClearShare
	/// </summary>
	public class ClearShareUsage
	{
		[Test, Ignore("not yet")]
		public void TypicalEmbedInMyXmlDocument()
		{
			var system = new OlacSystem();
			var work = new Work();
			work.Licenses.Add(License.CreativeCommons_Attribution_ShareAlike);
			work.Contributions.Add(new Contribution("Charlie Brown", system.GetRoleByCodeOrThrow("author")));
			work.Contributions.Add(new Contribution("Linus", system.GetRoleByCodeOrThrow("editor")));

			string metaData = system.GetXmlForWork(work);

			//Embed that data in our own file
			using (var f = new TempFile(@"<doc>
				<metadata>" + metaData + @"</metadata>
				<ourDocumentContents>blah blah<ourDocumentContents/></doc>"))
			{
				//Then when it comes time to read the file, we can extract out the work again
				var dom = new XmlDocument();
				dom.Load(f.Path);

				var node = dom.SelectSingleNode("//metadata");
				var work2 = new Work();
				system.LoadWorkFromXml(work2, node.InnerXml);

				Assert.AreEqual(2,work2.Contributions.Count());
			}
		}
	}
}
