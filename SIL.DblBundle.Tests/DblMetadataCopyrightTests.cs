using System;
using System.Linq;
using NUnit.Framework;
using SIL.DblBundle.Text;

namespace SIL.DblBundle.Tests
{
	[TestFixture]
	class DblMetadataCopyrightTests
	{
		[Test]
		public void ConstructorFromString_SimpleString_StatementHasSingleNode()
		{
			var copyrightObject =
				new DblMetadataCopyright("Do not copy this or you will go to prison forever!");
			Assert.AreEqual("Do not copy this or you will go to prison forever!",
				copyrightObject.Statement.InternalNodes.Single().InnerText);
			Assert.AreEqual("Do not copy this or you will go to prison forever!", copyrightObject.ToString());
		}

		[Test]
		public void ConstructorFromString_XHtmlStringWithTwoParagraphs_StatementHasTwoNodesWithCorrectContent()
		{
			var copyrightObject =
				new DblMetadataCopyright("<p>Do not copy this!</p><p>Copyright strictly enforced until 2315.</p>");
			var nodes = copyrightObject.Statement.InternalNodes;
			Assert.AreEqual(2, nodes.Length);
			Assert.AreEqual("Do not copy this!", nodes.First().InnerText);
			Assert.AreEqual("Copyright strictly enforced until 2315.", nodes.Last().InnerText);
			Assert.AreEqual("Do not copy this!" + Environment.NewLine +
				"Copyright strictly enforced until 2315.", copyrightObject.ToString());
		}

		[Test]
		public void ToString_AsXHtml_ResultIsStatementXhtml()
		{
			var copyrightObject =
				new DblMetadataCopyright("<p>Do not copy this!</p><p>Copyright strictly enforced until 2315.</p>");
			Assert.AreEqual(copyrightObject.Statement.Xhtml, copyrightObject.ToString(true));
		}

		[Test]
		public void ToString_SeparatorSpecified_SpecifiedSeparatorUsed()
		{
			var copyrightObject =
				new DblMetadataCopyright("<p>Do not copy this!</p><p>Copyright strictly enforced until 2315.</p>");
			Assert.AreEqual("Do not copy this!###Copyright strictly enforced until 2315.", copyrightObject.ToString("###"));
		}
	}
}
