using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace Palaso.DictionaryService.Client.Tests
{
	[TestFixture]
	public class ServiceFinderTests
	{
		private ServiceFinder _finder;

		[SetUp]
		public void Setup()
		{
			_finder = new ServiceFinder();
			_finder.Init();
			_finder.LoadTestDictionary("qPretend");
		}

		[TearDown]
		public void TearDown()
		{
			_finder.Dispose();
		}

		[Test]
		public void GetDictionaryService_UnmatchedWritingSystem_ReturnNull()
		{
			Assert.IsNull(_finder.GetDictionaryService("x12"));
		}

		[Test]
		public void GetDictionaryService_ReturnsSameDictionaryInstanceEachTime()
		{
			Assert.IsNotNull(_finder.GetDictionaryService("qPretend"));
			Assert.AreEqual(_finder.GetDictionaryService("qPretend"), _finder.GetDictionaryService("qPretend"));
		}
	}
}
