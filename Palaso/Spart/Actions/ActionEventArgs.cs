// Spart License (zlib/png)
//
//
// Copyright (c) 2003 Jonathan de Halleux
//
// This software is provided 'as-is', without any express or implied warranty.
// In no event will the authors be held liable for any damages arising from
// the use of this software.
//
// Permission is granted to anyone to use this software for any purpose,
// including commercial applications, and to alter it and redistribute it
// freely, subject to the following restrictions:
//
// 1. The origin of this software must not be misrepresented; you must not
// claim that you wrote the original software. If you use this software in a
// product, an acknowledgment in the product documentation would be
// appreciated but is not required.
//
// 2. Altered source versions must be plainly marked as such, and must not be
// misrepresented as being the original software.
//
// 3. This notice may not be removed or altered from any source distribution.
//
// Author: Jonathan de Halleuxnamespace Spart.Actions
using System;
using Spart.Parsers;

namespace Spart.Actions
{

	/// <summary>
	/// Action event argument class
	/// </summary>
	public class ActionEventArgs : EventArgs
	{
		private ParserMatch m_Match;
		private Object m_TypeValue;

		/// <summary>
		/// Construct a new event argument instance
		/// </summary>
		/// <param name="match"></param>
		public ActionEventArgs(ParserMatch match)
		{
			if (match == null)
				throw new ArgumentNullException("match is null");
			if (!match.Success)
				throw new ArgumentException("Match is not successfull");
			m_Match = match;
			m_TypeValue = null;
		}

		/// <summary>
		/// Construct a new event argument instance
		/// </summary>
		/// <param name="match"></param>
		/// <param name="typedValue"></param>
		public ActionEventArgs(ParserMatch match, Object typedValue)
		{
			if (match == null)
				throw new ArgumentNullException("match is null");
			if (!match.Success)
				throw new ArgumentException("Match is not successfull");
			if (typedValue==null)
				throw new ArgumentNullException("typed value");
			m_Match = match;
			m_TypeValue = typedValue;
		}

		/// <summary>
		/// The parser match
		/// </summary>
		public ParserMatch Match
		{
			get
			{
				return m_Match;
			}
		}

		/// <summary>
		/// The parser match value
		/// </summary>
		public String Value
		{
			get
			{
				return Match.Value;
			}
		}

		/// <summary>
		/// The typed parse result
		/// </summary>
		public Object TypeValue
		{
			get
			{
				return m_TypeValue;
			}
		}
	}
}
