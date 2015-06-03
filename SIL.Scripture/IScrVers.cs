// --------------------------------------------------------------------------------------------
#region // Copyright (c) 2014, SIL International.
// <copyright from='2008' to='2014' company='SIL International'>
//		Copyright (c) 2014, SIL International.   
//    
//		Distributable under the terms of the MIT License (http://sil.mit-license.org/)
// </copyright> 
#endregion
// 
// File: IScrVers.cs
// --------------------------------------------------------------------------------------------
namespace SIL.Scripture
{
    public interface IScrVers
    {
        int GetLastChapter(int bookNum);

        int GetLastVerse(int bookNum, int chapterNum);

        int ChangeVersification(int reference, IScrVers scrVersSource);
    }
}