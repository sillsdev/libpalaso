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
// Author: Jonathan de Halleuxnamespace Spart.Parsers
using System;
using Spart.Scanners;

namespace Spart.Parsers
{

	/// <summary>
	/// A parser match
	/// </summary>
	public class ParserMatch
	{
		private IScanner m_Scanner;
		private long m_Offset;
		private int m_Length;

		/// <summary>
		/// Builds a new match
		/// </summary>
		/// <param name="scanner"></param>
		/// <param name="offset"></param>
		/// <param name="length"></param>
		public ParserMatch(IScanner scanner, long offset, int length)
		{
			if (scanner == null)
				throw new ArgumentNullException("scanner");

			m_Scanner = scanner;
			m_Offset = offset;
			m_Length = length;
		}

		/// <summary>
		/// Scanner
		/// </summary>
		public IScanner Scanner
		{
			get
			{
				return m_Scanner;
			}
		}

		/// <summary>
		/// Offset
		/// </summary>
		public long Offset
		{
			get
			{
				return m_Offset;
			}
		}

		/// <summary>
		/// Length
		/// </summary>
		public int Length
		{
			get
			{
				return m_Length;
			}
		}

		/// <summary>
		/// Extracts the match value
		/// </summary>
		public String Value
		{
			get
			{
				if (Length<0)
					throw new InvalidOperationException("no match");
				return Scanner.Substring(Offset, Length);
			}
		}

		/// <summary>
		/// True if match successfull
		/// </summary>
		public bool Success
		{
			get
			{
				return Length >= 0;
			}
		}

		/// <summary>
		/// True if match empty
		/// </summary>
		public bool Empty
		{
			get
			{
				if (Length<0)
					throw new InvalidOperationException("no match");
				return Length == 0;
			}
		}

		/// <summary>
		/// Concatenates match with m
		/// </summary>
		/// <param name="m"></param>
		public void Concat(ParserMatch m)
		{
			if(m==null)
				throw new ArgumentNullException("Cannot concatenate null match");
			if(!m.Success)
				throw new ArgumentException("Non-successful matches cannot be concatenated onto matches");

			if(!Success)
			{
				throw new InvalidOperationException("Matches cannot be concatenated onto non-successful matches");
			}

			// if other is empty, return this
			if(m.Empty)
				return;

			if (m.Offset < Offset)
				throw new ArgumentException("match reserved ?");

			m_Length = (int)(m.Offset-Offset) + m.Length;
		}
	}
}