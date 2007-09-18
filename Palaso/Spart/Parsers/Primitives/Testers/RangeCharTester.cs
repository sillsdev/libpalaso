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
/// Author: Jonathan de Halleuxusing System;

namespace Spart.Parsers.Primitives.Testers
{
	using System;
	public class RangeCharTester : ICharTester
	{
		private Char m_First;
		private Char m_Last;

		public RangeCharTester(Char first, Char last)
		{
			SetRange(first,last);
		}

		public Char First
		{
			get
			{
				return m_First;
			}
		}

		public  Char Last
		{
			get
			{
				return m_Last;
			}
		}

		public void SetRange(Char first, Char last)
		{
			if (last < first)
				throw new ArgumentException("last character < first character");
			m_First = first;
			m_Last = last;
		}

		public bool Test(Char c)
		{
			return c>=First && c <= Last;
		}
	}
}
