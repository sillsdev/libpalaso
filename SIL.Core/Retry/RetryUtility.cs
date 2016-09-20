﻿using System;
using System.Collections.Generic;
using System.Threading;

namespace SIL.Retry
{
	/// <summary>
	/// This utility can be used to wrap any call and retry it in a loop
	/// until specific exceptions no longer occur.
	///
	/// Number of retry attempts, pause time between attempts, and exception types
	/// can all be specified or left to the defaults.
	///
	/// This class came about as an attempt to mitigate certain IO issues, so the
	/// defaults are specified along those lines.
	/// </summary>
	public static class RetryUtility
	{
		// Nothing special about these numbers. Just attempting to balance issues of user experience.
		public const int kDefaultMaxRetryAttempts = 10;
		public const int kDefaultRetryDelay = 200;
		private static readonly ISet<Type> kDefaultExceptionTypesToRetry = new HashSet<Type> { Type.GetType("System.IO.IOException") };

		public static void Retry(Action action, int maxRetryAttempts = kDefaultMaxRetryAttempts, int retryDelay = kDefaultRetryDelay, ISet<Type> exceptionTypesToRetry = null)
		{
			Retry<object>(() =>
			{
				action();
				return null;
			}, maxRetryAttempts, retryDelay, exceptionTypesToRetry);
		}

		public static T Retry<T>(Func<T> action, int maxRetryAttempts = kDefaultMaxRetryAttempts, int retryDelay = kDefaultRetryDelay, ISet<Type> exceptionTypesToRetry = null)
		{
			if (exceptionTypesToRetry == null)
				exceptionTypesToRetry = kDefaultExceptionTypesToRetry;

			for (int attempt = 1; attempt <= maxRetryAttempts; attempt++)
			{
				try
				{
					var result = action();
					//Debug.WriteLine("Successful after {0} attempts", attempt);
					return result;
				}
				catch (Exception e)
				{
					if (exceptionTypesToRetry.Contains(e.GetType()))
					{
						if (attempt == maxRetryAttempts)
						{
							//Debug.WriteLine("Failed after {0} attempts", attempt);
							throw;
						}
						Thread.Sleep(retryDelay);
						continue;
					}
					throw;
				}
			}
			return default(T);
		}
	}
}
