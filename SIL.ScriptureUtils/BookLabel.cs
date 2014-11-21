// --------------------------------------------------------------------------------------------
#region // Copyright (c) 2014, SIL International.
// <copyright from='2008' to='2014' company='SIL International'>
//		Copyright (c) 2014, SIL International.   
//    
//		Distributable under the terms of the MIT License (http://sil.mit-license.org/)
// </copyright> 
#endregion
// 
// File: MultilingScrBooks.cs
// --------------------------------------------------------------------------------------------
namespace SIL.ScriptureUtils
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
