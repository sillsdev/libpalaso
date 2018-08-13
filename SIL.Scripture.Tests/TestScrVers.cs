// --------------------------------------------------------------------------------------------
#region // Copyright (c) 2014, SIL International.
// <copyright from='2013' to='2014' company='SIL International'>
//		Copyright (c) 2014, SIL International.   
//	
//		Distributable under the terms of the MIT License (http://sil.mit-license.org/)
// </copyright> 
#endregion
// 
// File: TestScrVers.cs
// --------------------------------------------------------------------------------------------
using Rhino.Mocks;

namespace SIL.Scripture.Tests
{
	public class TestScrVers : IScrVers
	{
		private IScrVers m_vers;

		/// ------------------------------------------------------------------------------------
		public TestScrVers()
		{
			m_vers = MockRepository.GenerateMock<IScrVers>();
			m_vers.Stub(v => v.GetLastChapter(1)).Return(50);
			m_vers.Stub(v => v.GetLastVerse(1, 1)).Return(31);
			m_vers.Stub(v => v.GetLastVerse(1, 2)).Return(25);
			m_vers.Stub(v => v.GetLastChapter(5)).Return(34);
			m_vers.Stub(v => v.GetLastVerse(5, 1)).Return(46);
			m_vers.Stub(v => v.GetLastVerse(5, 17)).Return(20);
			m_vers.Stub(v => v.GetLastChapter(6)).Return(24);
			m_vers.Stub(v => v.GetLastVerse(6, 1)).Return(18);
			m_vers.Stub(v => v.GetLastChapter(7)).Return(21);
			m_vers.Stub(v => v.GetLastVerse(7, 21)).Return(25);
			m_vers.Stub(v => v.GetLastChapter(57)).Return(1);
			m_vers.Stub(v => v.GetLastVerse(57, 1)).Return(25);
			m_vers.Stub(v => v.GetLastChapter(59)).Return(5);
			m_vers.Stub(v => v.GetLastVerse(59, 1)).Return(27);
			m_vers.Stub(v => v.GetLastChapter(66)).Return(22);
			m_vers.Stub(v => v.GetLastVerse(66, 1)).Return(20);
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

		public VerseRef ChangeVersification(VerseRef reference)
		{
			throw new System.NotImplementedException();
		}

		public bool ChangeVersificationWithRanges(VerseRef reference, out VerseRef newReference)
		{
			throw new System.NotImplementedException();
		}
	}
}
