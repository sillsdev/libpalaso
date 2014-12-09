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
// Author: Jonathan de Halleux
using System;
using System.IO;
using Spart.Parsers.NonTerminal;

namespace Spart.Debug
{
	/// <summary>
	/// A Debugger
	/// </summary>
	public class Debugger
	{
		private readonly IParserContext m_Context;
		/// <summary>
		/// constructor
		/// </summary>
		/// <param name="context"></param>
		public Debugger(IParserContext context)
		{
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}
			m_Context = context;
		}
		/// <summary>
		/// constructor
		/// </summary>
		/// <param name="output"></param>
		public Debugger(TextWriter output)
		{
			if (output == null)
			{
				throw new ArgumentNullException("output");
			}
			m_Context = new DebugContext(output);
		}

		/// <summary>
		/// The current parser context
		/// </summary>
		public IParserContext Context
		{
			get { return m_Context; }
		}

		/// <summary>
		/// Adds the debug context to the given rule
		/// </summary>
		/// <param name="debug"></param>
		/// <param name="rule"></param>
		/// <returns></returns>
		public static Debugger operator +(Debugger debug, Rule rule)
		{
			rule.AddContext(debug.Context);
			return debug;
		}
	}
}