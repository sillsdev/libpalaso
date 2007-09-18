/// Spart License (zlib/png)
///
///
/// Copyright (c) 2003 Jonathan de Halleux
///
/// This software is provided 'as-is', without any express or implied warranty.
/// In no event will the authors be held liable for any damages arising from
/// the use of this software.
///
/// Permission is granted to anyone to use this software for any purpose,
/// including commercial applications, and to alter it and redistribute it
/// freely, subject to the following restrictions:
///
/// 1. The origin of this software must not be misrepresented; you must not
/// claim that you wrote the original software. If you use this software in a
/// product, an acknowledgment in the product documentation would be
/// appreciated but is not required.
///
/// 2. Altered source versions must be plainly marked as such, and must not be
/// misrepresented as being the original software.
///
/// 3. This notice may not be removed or altered from any source distribution.
///
/// Author: Jonathan de Halleux
namespace Spart.Scanners
{
	using System;
	using Spart.Parsers;


	/// <summary>
	/// Input scanner interface
	/// </summary>
	public interface IScanner
	{
		/// <summary>
		/// Return true if all input is consummed
		/// </summary>
		bool AtEnd {get;}

		/// <summary>
		/// Reads one character of the input
		/// </summary>
		/// <returns>true if not at end</returns>
		bool Read();

		/// <summary>
		/// Current character
		/// </summary>
		/// <returns></returns>
		char Peek();

		/// <summary>
		/// Scanner cursor position
		/// </summary>
		long Offset{get;set;}

		/// <summary>
		/// Move cursor position to the offset
		/// </summary>
		/// <param name="offset"></param>
		void Seek(long offset);

		/// <summary>
		/// Extracts a substring of the input
		/// </summary>
		/// <param name="offset"></param>
		/// <param name="length"></param>
		/// <returns></returns>
		String Substring(long offset, int length);

		/// <summary>
		/// Sets the input filter
		/// </summary>
		IFilter Filter{get;set;}

		/// <summary>
		/// Create a failure match
		/// </summary>
		ParserMatch NoMatch      {get;}

		/// <summary>
		/// Create an empty match
		/// </summary>
		ParserMatch EmptyMatch   {get;}

		/// <summary>
		/// Create a match out of the intput
		/// </summary>
		/// <param name="offset"></param>
		/// <param name="length"></param>
		/// <returns></returns>
		ParserMatch CreateMatch(long offset, int length);
	}
}