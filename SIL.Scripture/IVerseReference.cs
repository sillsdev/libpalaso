// ---------------------------------------------------------------------------------------------
#region // Copyright (c) 2014, SIL International. All Rights Reserved.
// <copyright from='2008' to='2014' company='SIL International'>
//		Copyright (c) 2014, SIL International. All Rights Reserved.   
//    
//		Distributable under the terms of the MIT License (http://sil.mit-license.org/)
// </copyright> 
#endregion
// 
// This class originated in FieldWorks (under the GNU Lesser General Public License), but we
// have decided to make it avaialble in SIL.ScriptureUtils as part of Palaso so it will be more
// readily available to other projects.
// ---------------------------------------------------------------------------------------------

namespace SIL.Scripture
{
	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// Represents a scripture reference
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	public interface IVerseReference
	{
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Returns the verse reference as a string
		/// </summary>
		/// ------------------------------------------------------------------------------------
		string ToString();

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets and sets the current versification scheme
		/// </summary>
		/// ------------------------------------------------------------------------------------
		ScrVers Versification { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Parses Scripture reference string.
		/// </summary>
		/// <param name="sTextToBeParsed">Reference string the user types in.</param>
		/// <remarks>This method is pretty similar to MultilingScrBooks.ParseRefString, but
		/// it deals only with SIL codes.</remarks>
		/// ------------------------------------------------------------------------------------
		void Parse(string sTextToBeParsed);
	}
}
