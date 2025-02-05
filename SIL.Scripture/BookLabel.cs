// --------------------------------------------------------------------------------------------
#region // Copyright (c) 2025 SIL Global
// <copyright from='2008' to='2024' company='SIL Global'>
//		Copyright (c) 2025 SIL Global
//
//		Distributable under the terms of the MIT License (http://sil.mit-license.org/)
// </copyright>
#endregion
// --------------------------------------------------------------------------------------------
namespace SIL.Scripture
{
	#region class BookLabel
	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// Class to associate a book label (name or abbreviation) with a canonical book number.
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	public class BookLabel
	{
		/// <summary></summary>
		public string Label;
		/// <summary></summary>
		public int BookNum;

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sLabel">The s label.</param>
		/// <param name="nBookNum">The n book num.</param>
		/// ------------------------------------------------------------------------------------
		public BookLabel(string sLabel, int nBookNum)
		{
			Label = sLabel;
			BookNum = nBookNum;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Returns a <see cref="System.String"/> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/> that represents this instance.
		/// </returns>
		/// ------------------------------------------------------------------------------------
		public override string ToString()
		{
			return Label;
		}
	}
	#endregion
}
