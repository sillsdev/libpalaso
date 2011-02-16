using System;
using System.IO;
using NUnit.Framework;
using Palaso.Tests.WritingSystems.Migration.WritingSystemsLdmlV0To1Migration;
using Palaso.WritingSystems.Migration;

namespace Palaso.Tests.WritingSystems.Migration
{
	[TestFixture]
	public class WritingSystemLdmlVersionGetterTests
	{
		private class TestEnvironment : IDisposable
		{
			readonly string _pathToLdml = Path.GetTempFileName();

			public TestEnvironment()
			{
				_pathToLdml = Path.GetTempFileName();
			}

			public string PathToWritingSystemLdmlFile
			{
				get { return _pathToLdml; }
			}

			public void WriteContentToWritingSystemLdmlFile(string contentToWrite)
			{
				File.WriteAllText(_pathToLdml, contentToWrite);
			}

			public void Dispose()
			{
				File.Delete(_pathToLdml);
			}
		}

		private TestEnvironment _environment;

		[Test]
		public void WritingSystemLdmlVersionGetterGetFileVersion_FileHasNoVersion_Returns0()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.Version0LdmlFile);
				var versionGetter = new WritingSystemLdmlVersionGetter();
				Assert.AreEqual(0, versionGetter.GetFileVersion(_environment.PathToWritingSystemLdmlFile));
			}
		}

		[Test]
		public void WritingSystemLdmlVersionGetterGetFileVersion_FileIsVersion1_Returns1()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.Version1LdmlFile);
				var versionGetter = new WritingSystemLdmlVersionGetter();
				Assert.AreEqual(1, versionGetter.GetFileVersion(_environment.PathToWritingSystemLdmlFile));
			}
		}
	}
}
