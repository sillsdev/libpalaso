using System;

//two of these methods originally from RhinoCommons

namespace SIL.Code
{
	/// <summary>
	/// Helper class for guard statements, which allow prettier
	/// code for guard clauses
	/// </summary>
	public static class Guard
	{
		/// <summary>
		/// Will throw a <see cref="InvalidOperationException"/> if the assertion
		/// is true, with the supplied message.
		/// </summary>
		/// <param name="expressionThatIsFalseIfEverythingIsOk">if set to <c>true</c> [assertion].</param>
		/// <param name="message">The message.</param>
		/// <example>
		/// Sample usage:
		/// <code>
		/// Guard.Against(string.IsNullOrEmpty(name), "Name must have a value");
		/// </code>
		/// </example>
		public static void Against(bool expressionThatIsFalseIfEverythingIsOk, string message)
		{
			if(expressionThatIsFalseIfEverythingIsOk == false)
				return;
			throw new InvalidOperationException(message);
		}
		/// <summary>
		/// Will throw if assertion is false
		/// </summary>
		/// <param name="assertion"></param>
		/// <param name="message"></param>
		/// <exception cref="InvalidOperationException"></exception>
		public static void AssertThat(bool assertion, string message)
		{
			if (!assertion)
			{
				throw new InvalidOperationException(message);
			}
		}

		public static void AgainstNull(object value, string valueName)
		{
			if (value == null)
				throw new ArgumentNullException(valueName);
		}


		public static void AgainstNullOrEmptyString(string value, string valueName)
		{
			if (string.IsNullOrEmpty(value))
				throw new ArgumentNullException(valueName);
		}

		/// <summary>
		/// Will throw exception of type <typeparamref name="TException"/>
		/// with the specified message if the assertion is true
		/// </summary>
		/// <typeparam name="TException"></typeparam>
		/// <param name="assertion">if set to <c>true</c> [assertion].</param>
		/// <param name="message">The message.</param>
		/// <example>
		/// Sample usage:
		/// <code>
		/// <![CDATA[
		/// Guard.Against<ArgumentException>(string.IsNullOrEmpty(name), "Name must have a value");
		/// ]]>
		/// </code>
		/// </example>
		public static void Against<TException>(bool assertion, string message) where TException : Exception
		{
			if (assertion == false)
				return;
			throw (TException)Activator.CreateInstance(typeof(TException), message);
		}

		[Obsolete("Use Detect.Reentry instead")]
		public static GuardAgainstReentry AgainstReEntry(GuardAgainstReentry guard)
		{
			if (guard != null)
			{
				guard.EnterNotExpected();
				return guard;
			}
			return new GuardAgainstReentry();
		}

		[Obsolete("Use Detect.Reentry instead")]
		public static GuardAgainstReentry AgainstReEntryExpected(GuardAgainstReentry guard)
		{
			if (guard != null)
			{
				guard.EnterExpected();
				return guard;
			}
			return new GuardAgainstReentry();
		}
	}
}