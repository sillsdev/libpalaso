using System.IO;
using System.Text;
using System.Xml;
using NUnit.Framework;
using SIL.WindowsForms.Progress;

namespace SIL.WindowsForms.Tests.Progress
{
	[TestFixture]
	public class TransformWithProgressDialogTests
	{
		[Test]
		[Category("DesktopRequired")] // not run on Jenkins
		public void TranformIsAppliedCorrectly()
		{
			string outputPath = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());
			XmlWriterSettings settings = new XmlWriterSettings();
			settings.Indent = true;
			settings.NewLineOnAttributes = true;

			string xslt =
				@"<xsl:transform xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='1.0' >
					  <xsl:output method='xml' indent='yes' encoding='utf-8'/>


					<xsl:template match='/'>
						<xsl:message/>
						<xsl:value-of select='count(//number)'/>
					</xsl:template>

				   </xsl:transform>";

			string sourceFilePath = Path.GetTempFileName();
			StringBuilder xmlBuilder  = new StringBuilder();

			xmlBuilder.AppendLine(@"<?xml version='1.0' encoding='utf-8'?><numbers>");
			for (int i = 0; i < 1000; i++)
			{
				xmlBuilder.AppendFormat("<number>{0}</number>", i);
			}
			xmlBuilder.AppendLine("</numbers>");

			File.WriteAllText(sourceFilePath, xmlBuilder.ToString());
			try
			{
				using (MemoryStream xsltStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(xslt)))
				{
					TransformWithProgressDialog transformer =
						new TransformWithProgressDialog(sourceFilePath, outputPath, xsltStream, "//number");
					transformer.TaskMessage = "Testing...";
					transformer.Transform(true);
					string output = File.ReadAllText(outputPath);
					Assert.IsTrue(output.Contains("1000"));
				}
			}
			finally
			{
				File.Delete(outputPath);
				File.Delete(sourceFilePath);
			}
		}
	}
}
