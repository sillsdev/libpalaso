// Copyright (c) 2018 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using SIL.ObjectModel;

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
	}

}