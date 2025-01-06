using System.Collections.Generic;

namespace SIL.Data
{
	/// -----------------------------------------------------------------------------------------
	/// <summary>
	/// Class to compare strings and sort by length
	/// </summary>
	/// -----------------------------------------------------------------------------------------
	public class StrLengthComparer : IComparer<string>
	{
		private readonly int m_asc;

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Initializes a new instance of the <see cref="StrLengthComparer"/> class that sorts
		/// from shortest to longest.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public StrLengthComparer() : this(true)
		{
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Initializes a new instance of the <see cref="StrLengthComparer"/> class.
		/// </summary>
		/// <param name="ascending">if set to <c>true</c>, strings will be sorted from shortest
		/// to longest; otherwise, from longest to shortest.</param>
		/// ------------------------------------------------------------------------------------
		public StrLengthComparer(bool ascending)
		{
			m_asc = ascending ? 1 : -1;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Comparison method
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public int Compare(string obj1, string obj2)
		{
			return m_asc * ((obj1?.Length ?? -1) - (obj2?.Length ?? - 1));
		}
	}
}
