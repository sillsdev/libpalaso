// --------------------------------------------------------------------------------------------
#region // Copyright 2024 SIL Global
// <copyright from='2013' to='2024' company='SIL Global'>
//		Copyright (c) 2024 SIL Global
//	
//		Distributable under the terms of the MIT License (http://sil.mit-license.org/)
// </copyright> 
#endregion
// 
// File: TestScrVers.cs
// --------------------------------------------------------------------------------------------
using Moq;

namespace SIL.Scripture.Tests
{
	public class TestScrVers : IScrVers
	{
		private IScrVers m_vers;

		/// ------------------------------------------------------------------------------------
		public TestScrVers()
		{
			var mock = new Mock<IScrVers>();
			mock.Setup(v => v.GetLastChapter(1)).Returns(50);
			mock.Setup(v => v.GetLastVerse(1, 1)).Returns(31);
			mock.Setup(v => v.GetLastVerse(1, 2)).Returns(25);
			mock.Setup(v => v.GetLastChapter(5)).Returns(34);
			mock.Setup(v => v.GetLastVerse(5, 1)).Returns(46);
			mock.Setup(v => v.GetLastVerse(5, 17)).Returns(20);
			mock.Setup(v => v.GetLastChapter(6)).Returns(24);
			mock.Setup(v => v.GetLastVerse(6, 1)).Returns(18);
			mock.Setup(v => v.GetLastChapter(7)).Returns(21);
			mock.Setup(v => v.GetLastVerse(7, 21)).Returns(25);
			mock.Setup(v => v.GetLastChapter(57)).Returns(1);
			mock.Setup(v => v.GetLastVerse(57, 1)).Returns(25);
			mock.Setup(v => v.GetLastChapter(59)).Returns(5);
			mock.Setup(v => v.GetLastVerse(59, 1)).Returns(27);
			mock.Setup(v => v.GetLastChapter(66)).Returns(22);
			mock.Setup(v => v.GetLastVerse(66, 1)).Returns(20);
			m_vers = mock.Object;
		}

		public int GetLastChapter(int bookNum)
		{
			return m_vers.GetLastChapter(bookNum);
		}

		public int GetLastVerse(int bookNum, int chapterNum)
		{
			return m_vers.GetLastVerse(bookNum, chapterNum);
		}

		public int ChangeVersification(int reference, IScrVers scrVersSource)
		{
			return m_vers.ChangeVersification(reference, scrVersSource);
		}

		public string Name
		{
			get { return "dummy"; }
		}

		public int GetLastBook()
		{
			throw new System.NotImplementedException();
		}

		public bool IsExcluded(int bbbcccvvv)
		{
			throw new System.NotImplementedException();
		}

		public VerseRef? FirstIncludedVerse(int bookNum, int chapterNum)
		{
			throw new System.NotImplementedException();
		}

		public string[] VerseSegments(int bbbcccvvv)
		{
			throw new System.NotImplementedException();
		}

		public void ChangeVersification(ref VerseRef reference)
		{
			throw new System.NotImplementedException();
		}

		public bool ChangeVersificationWithRanges(VerseRef reference, out VerseRef newReference)
		{
			throw new System.NotImplementedException();
		}
	}
}
