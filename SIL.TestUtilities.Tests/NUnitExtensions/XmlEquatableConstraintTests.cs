// Copyright (c) 2019 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.Xml;
using System.Xml.Linq;
using NUnit.Framework;
using SIL.TestUtilities.NUnitExtensions;
using Is = NUnit.Framework.Is;

namespace SIL.TestUtilities.Tests.NUnitExtensions
{
	[TestFixture]
	public class XmlEquatableConstraintTests
	{
		[Test]
		public void XmlEquatableConstraint_TrueIfBothMatch()
		{
			var xmlString = @"<root><foo id='abc'/></root>";
			Assert.That(new XmlEquatableConstraint(xmlString)
				.Matches(XElement.Parse(xmlString)), Is.True);
		}

		[TestCase(null)]
		[TestCase("")]
		public void XmlEquatableConstraint_NullValue(string expectedXml)
		{
			Assert.That(new XmlEquatableConstraint(expectedXml).Matches(null), Is.True);
		}

		[Test]
		public void XmlEquatableConstraint_FailsIfDifferent()
		{
			Assert.That(new XmlEquatableConstraint(@"<root><foo id='abc'/></root>")
				.Matches(XElement.Parse(@"<root><somethingOther/></root>")), Is.False);
		}

		[Test]
		public void XmlEquatableConstraint_FailsIfActualNull()
		{
			Assert.That(new XmlEquatableConstraint(@"<root><foo id='abc'/></root>")
				.Matches(null), Is.False);
		}

		[Test]
		public void XmlEquatableConstraint_WorksWithXmlDocument()
		{
			var xmlString = @"<root><foo id='abc'/></root>";
			var doc = new XmlDocument();
			doc.LoadXml(xmlString);
			Assert.That(new XmlEquatableConstraint(xmlString).Matches(doc), Is.True);
		}

		[Test]
		public void XmlEquatableConstraint_WorksWithXmlString()
		{
			var xmlString = @"<root><foo id='abc'/></root>";
			Assert.That(new XmlEquatableConstraint(xmlString).Matches(xmlString), Is.True);
		}

		[Test]
		public void XmlEquatableConstraint_FailsWithRandomString()
		{
			Assert.That(() => new XmlEquatableConstraint(@"<root><foo id='abc'/></root>")
				.Matches("some text"), Throws.Exception.TypeOf<XmlException>());
		}

		[Test]
		public void XmlEquatableConstraint_FailsWithNonXmlObject()
		{
			Assert.That(() => new XmlEquatableConstraint(@"<root><foo id='abc'/></root>")
				.Matches(new object()), Throws.ArgumentException);
		}

		[Test]
		public void XmlEquatableConstraint_WorksWithXElement()
		{
			var xmlString = @"<root><foo id='abc'/></root>";
			var xmlElement = XElement.Parse(xmlString);
			Assert.That(new XmlEquatableConstraint(xmlString).Matches(xmlElement), Is.True);
		}
	}
}