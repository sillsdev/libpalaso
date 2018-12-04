using System;
using System.IO;
using NUnit.Framework;
using SIL.IO;
using SIL.TestUtilities;
using Is = SIL.TestUtilities.NUnitExtensions.Is;

namespace SIL.WritingSystems.Tests
{
	[TestFixture]
	public class WritingSystemChangeLogDataMapperTests
	{
		[Test]
		public void Write_NullChangeLog_Throws()
		{
			var dataMapper = new WritingSystemChangeLogDataMapper("whatever");
			Assert.Throws<ArgumentNullException>(
				() => dataMapper.Write(null)
			);
		}

		[Test]
		public void Read_SampleLogFile_PopulatesChanges()
		{
			using (var e = new TestEnvironment())
			{
				var log = new WritingSystemChangeLog(new WritingSystemChangeLogDataMapper(e.GetSampleLogFilePath()));
				Assert.That(log.HasChangeFor("aaa"), Is.True);
				Assert.That(log.GetChangeFor("aaa"), Is.EqualTo("ddd"));
			}
		}

		[Test]
		public void Write_NewEmptyFile_WritesModelToLogFile()
		{
			using (var e = new TestEnvironment())
			{
				string tempFilePath = Path.Combine(e._tempFolder.Path, "testchangelog.xml");
				var log = new WritingSystemChangeLog(new WritingSystemChangeLogDataMapper(tempFilePath));
				log.LogChange("aab", "bba");
				log.LogAdd("aab");
				log.LogDelete("aab");
				log.LogConflate("aab","bba");
				AssertThatXmlIn.File(tempFilePath).HasAtLeastOneMatchForXpath("/WritingSystemChangeLog/Changes/Change/From[text()='aab']");
				AssertThatXmlIn.File(tempFilePath).HasAtLeastOneMatchForXpath("/WritingSystemChangeLog/Changes/Change/To[text()='bba']");
				AssertThatXmlIn.File(tempFilePath).HasAtLeastOneMatchForXpath("/WritingSystemChangeLog/Changes/Add/Id[text()='aab']");
				AssertThatXmlIn.File(tempFilePath).HasAtLeastOneMatchForXpath("/WritingSystemChangeLog/Changes/Delete/Id[text()='aab']");
				AssertThatXmlIn.File(tempFilePath).HasAtLeastOneMatchForXpath("/WritingSystemChangeLog/Changes/Merge/From[text()='aab']");
				AssertThatXmlIn.File(tempFilePath).HasAtLeastOneMatchForXpath("/WritingSystemChangeLog/Changes/Merge/To[text()='bba']");
			}
		}

		public class TestEnvironment : IDisposable
		{
			public TemporaryFolder _tempFolder = new TemporaryFolder("writingSystemChangeLogTests");
			private TempFile _tempFile = new TempFile();

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
	<Delete Producer='WeSay' ProducerVersion='1.1' TimeStamp='1994-11-06T13:15:30Z'>
		<id>eee</id>
	</Delete>
	<Add Producer='WeSay' ProducerVersion='1.1' TimeStamp='1994-11-06T13:15:30Z'>
		<id>fff</id>
	</Add>
	<Merge Producer='WeSay' ProducerVersion='1.1' TimeStamp='1994-11-07T13:15:30Z'>
		<From>ccc</From>
		<To>ddd</To>
	</Merge>
</Changes>
</WritingSystemChangeLog>
").Replace("'", "\"");
				File.WriteAllText(_tempFile.Path, contents);
				return _tempFile.Path;
			}

			public WritingSystemChangeLog Log { get; set; }

			public void Dispose()
			{
				_tempFolder.Dispose();
				_tempFile.Dispose();
			}

			public WritingSystemChangeLog GetSampleWritingSystemChangeLog()
			{
				var log = new WritingSystemChangeLog();
				log.LogChange("aab", "bba");
				log.LogChange("ccc", "ddd");
				return log;
			}
		}
	}
}
