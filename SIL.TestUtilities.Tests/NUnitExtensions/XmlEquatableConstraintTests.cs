// Copyright (c) 2025 SIL Global
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
		public void ApplyTo_ActualIsEquivalentXElement_Succeeds()
		{
			var xmlString = @"<root><foo id='abc'/></root>";
			Assert.That(XElement.Parse(xmlString), new XmlEquatableConstraint(xmlString));
		}

		[TestCase(null)]
		[TestCase("")]
		public void ApplyTo_NullOrEmpty_MatchesNull(string expectedXml)
		{
			Assert.That(null, new XmlEquatableConstraint(expectedXml));
		}

		[Test]
		public void ApplyTo_ActualIsDifferentXElement_Fails()
		{
			var actual = XElement.Parse(@"<root><somethingOther/></root>");
			var expected = @"<root><foo id='abc'/></root>";
			var result = new XmlEquatableConstraint(expected).ApplyTo(actual);
			Assert.False(result.IsSuccess);
			Assert.AreEqual(actual, result.ActualValue);
			Assert.AreEqual(expected, result.Description);
		}

		[Test]
		public void Matches_ActualNull_Fails()
		{
			Assert.That(new XmlEquatableConstraint(@"<root><foo id='abc'/></root>")
				.Matches(null), Is.False);
		}

		[Test]
		public void Matches_ActualIsEquivalentXmlDocument_Succeeds()
		{
			var xmlString = @"<root><foo id='abc'/></root>";
			var doc = new XmlDocument();
			doc.LoadXml(xmlString);
			Assert.That(new XmlEquatableConstraint(xmlString).Matches(doc), Is.True);
		}

		[Test]
		public void ApplyTo_ActualIsDifferentXmlDocument_Fails()
		{
			var xmlString = @"<root><myElement id='abc'/></root>";
			var doc = new XmlDocument();
			doc.LoadXml(xmlString);

			var expected = @"<root><foo id='abc'/></root>";
			var result = new XmlEquatableConstraint(expected).ApplyTo(doc);
			Assert.False(result.IsSuccess);
			Assert.AreEqual(doc, result.ActualValue);
			Assert.AreEqual(expected, result.Description);
		}

		[Test]
		public void Matches_ActualIsEquivalentString_Succeeds()
		{
			var xmlString = @"<root><foo id='abc'/></root>";
			Assert.That(new XmlEquatableConstraint(xmlString).Matches(xmlString), Is.True);
		}

		[Test]
		public void Matches_ActualIsNonXmlString_Fails()
		{
			Assert.That(() => new XmlEquatableConstraint(@"<root><foo id='abc'/></root>")
				.Matches("some text"), Throws.Exception.TypeOf<XmlException>());
		}

		[Test]
		public void Matches_ActualIsNonXmlObject_Fails()
		{
			Assert.That(() => new XmlEquatableConstraint(@"<root><foo id='abc'/></root>")
				.Matches(new object()), Throws.ArgumentException);
		}

		[Test]
		public void Matches_ActualIsEquivalentXElement_Succeeds()
		{
			var xmlString = @"<root><foo id='abc'/></root>";
			var xmlElement = XElement.Parse(xmlString);
			Assert.That(new XmlEquatableConstraint(xmlString).Matches(xmlElement), Is.True);
		}
	}
}