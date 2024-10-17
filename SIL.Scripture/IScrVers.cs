// --------------------------------------------------------------------------------------------
#region // Copyright (c) 20124 SIL Global.
// <copyright from='2008' to='2014' company='SIL Global'>
//		Copyright (c) 2024 SIL Global
//	
//		Distributable under the terms of the MIT License (http://sil.mit-license.org/)
// </copyright> 
#endregion
// 
// File: IScrVers.cs
// --------------------------------------------------------------------------------------------
using JetBrains.Annotations;

namespace SIL.Scripture
{
	public interface IScrVers
	{
		string Name { get; }

		/// <summary>
		/// Gets last book in this versification
		/// </summary>
		[PublicAPI]
		int GetLastBook();

		/// <summary>
		/// Gets last chapter number in the given book.
		/// </summary>
		int GetLastChapter(int bookNum);

		/// <summary>
		/// Gets last verse number in the given book/chapter.
		/// </summary>
		int GetLastVerse(int bookNum, int chapterNum);

		int ChangeVersification(int reference, IScrVers scrVersSource);

		/// <summary>
		/// Determines whether the specified verse is excluded in the versification.
		/// </summary>
		[PublicAPI]
		bool IsExcluded(int bbbcccvvv);

		/// <summary>
		/// Gets first reference starting at the specified book/chapter considering excluded verses.
		/// </summary>
		/// <returns>first verse in the specified book and chapter that is not excluded or
		/// returns <c>null</c> if no included verse left in book</returns>
		[PublicAPI]
		VerseRef? FirstIncludedVerse(int bookNum, int chapterNum);

		/// <summary>
		/// Gets a list of verse segments for the specified reference or null if the specified
		/// reference does not have segments defined in the versification.
		/// </summary>
		string[] VerseSegments(int bbbcccvvv);

		/// <summary>
		/// Change the passed VerseRef to be this versification applying any necessary mappings.
		/// </summary>
		void ChangeVersification(ref VerseRef reference);

		/// <summary>
		/// Change the versification of an entry with Verse like 1-3 or 1,3a applying any necessary mappings to each part.
		/// Can't really work in the most general case because the verse parts could become separate chapters.
		/// </summary>
		/// <returns>true if successful (i.e. all verses were in the same the same chapter in the new versification),
		/// false if the changing resulted in the reference spanning chapters (which makes the results undefined)</returns>
		[PublicAPI]
		bool ChangeVersificationWithRanges(VerseRef reference, out VerseRef newReference);
	}
}