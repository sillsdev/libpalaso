// Copyright (c) 2015 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Drawing;

namespace SIL
{
	public static class RectExtensions
	{
//		/// ------------------------------------------------------------------------------------
//		/// <summary>
//		/// Converts a Rect struct to a .NET Rectangle
//		/// </summary>
//		/// <param name="rc">Windows rectangle</param>
//		/// <returns>.NET rectangle</returns>
//		/// ------------------------------------------------------------------------------------
//		public static implicit operator Rectangle(Rect rc)
//		{
//			return Rectangle.FromLTRB(rc.left, rc.top, rc.right, rc.bottom);
//		}
//
//		/// ------------------------------------------------------------------------------------
//		/// <summary>
//		/// Converts a .NET rectangle to a windows rectangle
//		/// </summary>
//		/// <param name="rc">.NET rectangle</param>
//		/// <returns>Windows rectangle</returns>
//		/// ------------------------------------------------------------------------------------
//		public static implicit operator Rect(Rectangle rc)
//		{
//			return new Rect(rc.Left, rc.Top, rc.Right, rc.Bottom);
//		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Test whether the rectangle contains the specified point.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static bool Contains(this Rect pt1, Point pt2)
		{
			if (pt2.X < pt1.left)
				return false;
			if (pt2.X > pt1.right)
				return false;
			if (pt2.Y < pt1.top)
				return false;
			if (pt2.Y > pt1.bottom)
				return false;
			return true;
		}
	}
}

