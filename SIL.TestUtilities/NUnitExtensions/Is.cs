// Copyright (c) 2025 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using SIL.ObjectModel;
using JetBrains.Annotations;

namespace SIL.TestUtilities.NUnitExtensions
{
	/// <inheritdoc />
	/// <summary>
	/// Some extensions to NUnits constraints. To use this class add the following line to
	/// the top of your test class:
	/// <c>using Is = SIL.TestUtilities.NUnitExtensions.Is;</c>
	/// </summary>
	// ReSharper disable once ClassNeverInstantiated.Global
	public class Is : NUnit.Framework.Is
	{
		public static ValueEquatableConstraint<T> ValueEqualTo<T>(IValueEquatable<T> expected)
			where T : class
		{
			return new ValueEquatableConstraint<T>(expected);
		}

		/// <summary>
		/// Checks if the actual XML node is equal to the expected
		/// XML string.
		/// </summary>
		/// <example>Assert.That(actual, Is.XmlEqualTo("<root/>"));</example>
		[PublicAPI]
		public static XmlEquatableConstraint XmlEqualTo(string expected)
		{
			return new XmlEquatableConstraint(expected);
		}
	}

}