using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;

namespace SIL.TestUtilities
{
	/// <summary>
	/// Comparer for two objects that might implement the IValueEquatable&lt;T&gt; interface.
	/// </summary>
	public class ValueEquatableComparer : IComparer
	{
		public static ValueEquatableComparer Instance { get; } = new ValueEquatableComparer();
		
		/// <summary>
		/// Determines whether the two objects are equal, searching reflectively for the IValueEquatable&lt;T&gt;.ValueEquals(T) method
		/// (there is no easy way because templates need to know their types at compile time)
		/// </summary>
		public int Compare(object x, object y)
		{
			if (Is.EqualTo(y).ApplyTo(x).IsSuccess)
				return 0;
			if (x?.GetType() != y?.GetType())
				return 1;

			// search reflectively for ValueEquals
			// ReSharper disable once PossibleNullReferenceException - Is.EqualTo handles if both are null; checking types handles if one is null.
			var type = x.GetType();
			MethodInfo valueEquals = null;
			while (valueEquals == null && type != null)
			{
				valueEquals = type.GetMethod("ValueEquals", BindingFlags.Public | BindingFlags.Instance);
				type = type.BaseType;
			}

			return (bool?) valueEquals?.Invoke(x, new[] {y}) ?? EqualityComparer<object>.Default.Equals(x, y) ? 0 : 1;
		}
	}
}
