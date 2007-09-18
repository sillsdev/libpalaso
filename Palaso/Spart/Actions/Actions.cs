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

using System;
using System.Collections;
using Spart.Actions.Actors;

namespace Spart.Actions
{
	/// <summary>
	/// Static helper class that creates actors
	/// </summary>
	public class Actor
	{
		/// <summary>
		/// Create an actor that append the parse result to a <see cref="IList"/>.
		/// </summary>
		/// <param name="list"></param>
		/// <returns></returns>
		public static ActionHandler Append(IList list)
		{
			AppendActor a =  new AppendActor(list);
			return new ActionHandler( a.DoAction );
		}

		/// <summary>
		/// Create an actor that assign the parse result to an object
		/// </summary>
		/// <param name="o"></param>
		/// <returns></returns>
		public static ActionHandler Assign(Object o)
		{
			AssignActor a =  new AssignActor(o);
			return new ActionHandler( a.DoAction );
		}

		/// <summary>
		/// Creates an actor that throws an exception
		/// </summary>
		/// <param name="ex"></param>
		/// <returns></returns>
		public static ActionHandler Throw(Exception ex)
		{
			ThrowActor a = new ThrowActor(ex);
			return new ActionHandler( a.DoAction );
		}
	}
}
