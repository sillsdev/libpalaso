// Copyright (c) 2019 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Xml;
using System.Xml.Linq;
using NUnit.Framework.Constraints;

namespace SIL.TestUtilities.NUnitExtensions
{
	/// <summary>
	/// Checks if the actual XML node is equal to the expected
	/// XML string with the help of System.Xml.Linq.XNodeEqualityComparer.
	/// </summary>
	public class XmlEquatableConstraint: Constraint
	{
		private string _expectedXml;

		public XmlEquatableConstraint(string expectedXml)
		{
			_expectedXml = expectedXml;
		}

		public override bool Matches(object actualParam)
		{
			this.actual = actualParam;
			var actualXml = actualParam as XNode;
			if (actualXml == null)
			{
				var actualXmlNode = actualParam as XmlNode;
				if (actualXmlNode != null)
				{
					actualXml = XElement.Parse(actualXmlNode.OuterXml);
				}
				else if (actualParam is string)
				{
					actualXml = XElement.Parse((string)actualParam);
				}
				else if (actualParam == null)
				{
					return string.IsNullOrEmpty(_expectedXml);
				}
				else
				{
					throw new ArgumentException("Don't know how to convert value to XML",
						nameof(actualParam));
				}
			}

			var equalityComparer = new XNodeEqualityComparer();
			return equalityComparer.Equals(XElement.Parse(_expectedXml), actualXml);
		}

		public override void WriteDescriptionTo(MessageWriter writer)
		{
			writer.WriteExpectedValue(_expectedXml);
		}
	}

	public static class XmlEquatableConstraintExtensionMethods
	{
		public static XmlEquatableConstraint XmlEqualTo(
			this ConstraintExpression expression, string expected)
		{
			var constraint = new XmlEquatableConstraint(expected);
			expression.Append(constraint);
			return constraint;
		}
	}

}