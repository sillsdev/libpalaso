// Copyright (c) 2011-2015 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.Text;

namespace SIL.Reporting
{
	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// Use this exception when an error occurs from which a user may continue. This will encourage
	/// error reporting (stack and log provided for user to report error to development team).
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	public class ContinuableErrorException : ApplicationException
	{
		/// -----------------------------------------------------------------------------------
		/// <summary>
		/// Initializes a new instance of the <see cref="ContinuableErrorException"/> class.
		/// </summary>
		/// -----------------------------------------------------------------------------------
		public ContinuableErrorException(string message)
			: base(message)
		{
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Initializes a new instance of the <see cref="ContinuableErrorException"/> class.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="innerException">The inner exception.</param>
		/// ------------------------------------------------------------------------------------
		public ContinuableErrorException(string message, Exception innerException) :
			base(message, innerException)
		{
		}
	}
}
