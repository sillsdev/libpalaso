using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Palaso.IO;
using Palaso.TestUtilities;
using Palaso.WritingSystems;

namespace Palaso.Tests.WritingSystems
{
	[TestFixture]
	public class WritingSystemChangeLogDataMapperTests
	{
		[Test]
		public void Read_NullFileName_Throws()
		{
			string emptyFilePath = null;
			Assert.Throws<ArgumentNullException>(
				() => WritingSystemChangeLogDataMapper.Read(emptyFilePath)
			);
		}

		[Test]
		public void Read_NonExistentFile_Throws()
		{
			string nonexistentFilePath = "tempfiledoesntexist1432";
			Assert.Throws<System.IO.FileNotFoundException>(
				() => WritingSystemChangeLogDataMapper.Read(nonexistentFilePath)
			);
		}

		[Test]
		public void Write_NullFileName_Throws()
		{
			var log = new WritingSystemChangeLog();
			Assert.Throws<ArgumentNullException>(
				() => WritingSystemChangeLogDataMapper.Write(null, log)
			);
		}

		[Test]
		public void Write_NullChangeLog_Throws()
		{
			Assert.Throws<ArgumentNullException>(
				() => WritingSystemChangeLogDataMapper.Write("a file path", null)
			);
		}

		[Test]
		public void Read_SampleLogFile_PopulatesModel()
		{
			using (var e = new TestEnvironment())
			{
				WritingSystemChangeLog log = WritingSystemChangeLogDataMapper.Read(e.GetSampleLogFilePath());
				Assert.That(log.HasChangeFor("aaa"));
				Assert.That(log.GetChangeFor("aaa"), Is.EqualTo("ccc"));
			}
		}

		[Test]
		public void Write_Model_WritesToLogFile()
		{
			using (var e = new TestEnvironment())
			{
				var tempFile = new TempFile();
				WritingSystemChangeLogDataMapper.Write(tempFile.Path, e.GetSampleWritingSystemChangeLog());
				AssertThatXmlIn.File(tempFile.Path).HasAtLeastOneMatchForXpath("/WritingSystemChangeLog/Changes/Change/From[text()='aab']");
			}
		}

		public class TestEnvironment : IDisposable
		{
			public TestEnvironment()
			{
				TempFile file = new TempFile();
			}

			public string GetSampleLogFilePath()
			{
				string contents = String.Format(@"<WritingSystemChangeLog Version='1'>
<Changes>
	<Change Producer='WeSay' ProducerVersion='1.1' TimeStamp='1994-11-05T13:15:30Z'>
		<From>aaa</From>
		<To>ccc</To>
	</Change>
	<Change Producer='WeSay' ProducerVersion='1.1' TimeStamp='1994-11-06T13:15:30Z'>
		<From>bbb</From>
		<To>ddd</To>
	</Change>
</Changes>
</WritingSystemChangeLog>
").Replace("'", "\"");
				var tempFile = new TempFile(contents);
				return tempFile.Path;
			}

			public WritingSystemChangeLog Log { get; set; }
			public void Dispose()
			{
			}

			public WritingSystemChangeLog GetSampleWritingSystemChangeLog()
			{
				var log = new WritingSystemChangeLog();
				log.Set("aab", "bba", "WeSay", "1.1");
				log.Set("ccc", "ddd", "FLEx", "7.1");
				return log;
			}
		}
	}
}
