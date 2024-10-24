// Copyright (c) 2024 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Xml;
using System.Xml.Linq;
using JetBrains.Annotations;
using NUnit.Framework.Constraints;

namespace SIL.TestUtilities.NUnitExtensions
{
	/// <summary>
	/// Checks if the actual XML node is equal to the expected
	/// XML string with the help of System.Xml.Linq.XNodeEqualityComparer.
	/// </summary>
	public class XmlEquatableConstraint: Constraint
	{
		private readonly string _expectedXml;

		public XmlEquatableConstraint(string expectedXml)
		{
			_expectedXml = expectedXml;
		}

		public bool Matches(object actualParam)
		{
			var actualXml = actualParam as XNode;
			if (actualXml == null)
			{
				if (actualParam is XmlNode actualXmlNode)
					actualXml = XElement.Parse(actualXmlNode.OuterXml);
				else if (actualParam is string s)
					actualXml = XElement.Parse(s);
				else if (actualParam == null)
					return string.IsNullOrEmpty(_expectedXml);
				else
				{
					throw new ArgumentException("Don't know how to convert value to XML",
						nameof(actualParam));
				}
			}

			var equalityComparer = new XNodeEqualityComparer();
			return equalityComparer.Equals(XElement.Parse(_expectedXml), actualXml);
		}

		public override ConstraintResult ApplyTo<TActual>(TActual actual)
		{
			return new ConstraintResult(this, actual,
				Matches(actual) ? ConstraintStatus.Success : ConstraintStatus.Failure);
		}

		public override string Description => _expectedXml;
	}

	[PublicAPI]
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