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

using System;
using System.Collections;

namespace Spart.Actions.Actors
{
	/// <summary>
	/// Actor that appends parse result to <see cref="IList"/>.
	/// </summary>
	public class AppendActor
	{
		private IList m_List;

		public AppendActor(IList list)
		{
			if (list == null)
			   throw new ArgumentNullException("list");
			m_List = list;
		}

		public IList List
		{
			get
			{
				return m_List;
			}
		}

		public void DoAction(Object sender, ActionEventArgs args)
		{
			List.Add(args.Value);
		}
	}
}
