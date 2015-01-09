using System;
using System.Collections.Generic;
using NUnit.Framework;
using SIL.WritingSystems;
using Palaso.Data;

namespace Palaso.Tests.Data
{
	[TestFixture]
	public class RecordTokenComparerTests
	{
		[Test]
		public void Compare_EnglishWordsUsingInvariantCulture_ComparesNotEqual()
		{
			using (var e = new TestEnvironment())
			{
				var sd1 = new SortDefinition("Form", StringComparer.InvariantCulture);
				var results1 = new Dictionary<string, object>();
				results1.Add("Form", "form1");
				var results2 = new Dictionary<string, object>();
				results2.Add("Form", "form2");
				var rt1 = new RecordToken<PalasoTestItem>(e.DataMapper, results1, new TestRepositoryId(8));
				var rt2 = new RecordToken<PalasoTestItem>(e.DataMapper, results2, new TestRepositoryId(8));

				var rtc = new RecordTokenComparer<PalasoTestItem>(new[] {sd1});

				int order = rtc.Compare(rt1, rt2);
				Assert.That(order, Is.Not.EqualTo(0));
			}
		}

		[Test]
		public void Compare_KhmerWordsUsingInvariantCulture_ComparesNotEqual()
		{
			using (var e = new TestEnvironment())
			{
				var sd1 = new SortDefinition("Form", StringComparer.InvariantCulture);
				var results1 = new Dictionary<string, object>();
				results1.Add("Form", "សង្ឃនៃអំបូរអឺរ៉ុន");
				var results2 = new Dictionary<string, object>();
				results2.Add("Form", "បូជាចារ្យនៃអំបូរអឺរ៉ុន");
				var rt1 = new RecordToken<PalasoTestItem>(e.DataMapper, results1, new TestRepositoryId(8));
				var rt2 = new RecordToken<PalasoTestItem>(e.DataMapper, results2, new TestRepositoryId(8));

				var rtc = new RecordTokenComparer<PalasoTestItem>(new[] { sd1 });

				int order = rtc.Compare(rt1, rt2);
				Assert.That(order, Is.Not.EqualTo(0));
			}
		}

		[Test]
		public void Compare_KhmerWordsUsingOrdinal_ComparesNotEqual()
		{
			using (var e = new TestEnvironment())
			{
				var sd1 = new SortDefinition("Form", StringComparer.Ordinal);
				var results1 = new Dictionary<string, object>();
				results1.Add("Form", "សង្ឃនៃអំបូរអឺរ៉ុន");
				var results2 = new Dictionary<string, object>();
				results2.Add("Form", "បូជាចារ្យនៃអំបូរអឺរ៉ុន");
				var rt1 = new RecordToken<PalasoTestItem>(e.DataMapper, results1, new TestRepositoryId(8));
				var rt2 = new RecordToken<PalasoTestItem>(e.DataMapper, results2, new TestRepositoryId(8));

				var rtc = new RecordTokenComparer<PalasoTestItem>(new[] { sd1 });

				int order = rtc.Compare(rt1, rt2);
				Assert.That(order, Is.Not.EqualTo(0));
			}
		}

		[Test]
		public void Compare_KhmerWordsUsingWritingSystemSameAsOtherLanguage_ComparesNotEqual()
		{
			using (var e = new TestEnvironment())
			{
				var ws = new WritingSystemDefinition("en") {DefaultCollation = new InheritedCollationDefinition("standard") {BaseLanguageTag = "km-KH"}};
				var sd1 = new SortDefinition("Form", ws.DefaultCollation.Collator);
				var results1 = new Dictionary<string, object> {{"Form", "សង្ឃនៃអំបូរអឺរ៉ុន"}};
				var results2 = new Dictionary<string, object> {{"Form", "បូជាចារ្យនៃអំបូរអឺរ៉ុន"}};
				var rt1 = new RecordToken<PalasoTestItem>(e.DataMapper, results1, new TestRepositoryId(8));
				var rt2 = new RecordToken<PalasoTestItem>(e.DataMapper, results2, new TestRepositoryId(8));

				var rtc = new RecordTokenComparer<PalasoTestItem>(new[] { sd1 });

				int order = rtc.Compare(rt1, rt2);
				Assert.That(order, Is.Not.EqualTo(0));
			}
		}

		private class TestEnvironment : IDisposable
		{
			public TestEnvironment()
			{

				DataMapper = new MemoryDataMapper<PalasoTestItem>();
			}

			public void Dispose()
			{
				DataMapper.Dispose();
			}

			public IDataMapper<PalasoTestItem> DataMapper { get; private set; }
		}
	}
}
