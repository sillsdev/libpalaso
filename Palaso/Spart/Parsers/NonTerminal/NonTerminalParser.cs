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
/// Author: Jonathan de Halleuxnamespace Spart.Parsers.NonTerminal


namespace Spart.Parsers.NonTerminal
{
	using System;
	using Spart.Scanners;

	/// <summary>
	/// NonTerminal parser abstract class
	/// </summary>
	public abstract class NonTerminalParser : Parser
	{
		private String m_ID;

		/// <summary>
		/// Default constructor
		/// </summary>
		public NonTerminalParser()
			:base()
		{
			ID = GetHashCode().ToString();
		}

		/// <summary>
		/// Rule ID, used for debugging
		/// </summary>
		public string ID
		{
			get
			{
				return m_ID;
			}
			set
			{
				if( m_ID != value )
					m_ID = value;
			}
		}

		/// <summary>
		/// Pre parse event
		/// </summary>
		public event PreParseEventHandler PreParse;

		/// <summary>
		/// Post parse event
		/// </summary>
		public event PostParseEventHandler PostParse;

		/// <summary>
		/// Preparse event caller
		/// </summary>
		/// <param name="scan"></param>
		public virtual void OnPreParse(IScanner scan)
		{
			if (PreParse != null)
				PreParse(this, new PreParseEventArgs(this,scan) );
		}

		/// <summary>
		/// Post parse event caller
		/// </summary>
		/// <param name="match"></param>
		/// <param name="scan"></param>
		public virtual void OnPostParse(ParserMatch match, IScanner scan)
		{
			if (PostParse != null)
				PostParse(this,new PostParseEventArgs(match,this,scan));
		}

		/// <summary>
		/// Adds event handlers
		/// </summary>
		/// <param name="context"></param>
		public void AddContext(IParserContext context)
		{
			PreParse+=new PreParseEventHandler(context.PreParse);
			PostParse+=new PostParseEventHandler(context.PostParse);
		}

		/// <summary>
		/// Removes event handlers
		/// </summary>
		/// <param name="context"></param>
		public void RemoveContext(IParserContext context)
		{
			PreParse-=new PreParseEventHandler(context.PreParse);
			PostParse-=new PostParseEventHandler(context.PostParse);
		}
	}
}
