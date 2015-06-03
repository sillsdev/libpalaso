using System;
using System.IO;
using NUnit.Framework;
using SIL.WritingSystems.Migration;

namespace SIL.WritingSystems.Tests.Migration
{
	[TestFixture]
	public class WritingSystemLdmlVersionGetterTests
	{
		private class TestEnvironment : IDisposable
		{
			private readonly string _pathToLdml;

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
		public void WritingSystemLdmlVersionGetterGetFileVersion_FileHasNoVersion_ReturnsminusOne()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlContentForTests.Version0("en", "", "", ""));
				var versionGetter = new WritingSystemLdmlVersionGetter();
				Assert.That(versionGetter.GetFileVersion(_environment.PathToWritingSystemLdmlFile), Is.EqualTo(-1));
			}
		}

		[Test]
		public void WritingSystemLdmlVersionGetterGetFileVersion_FileIsVersion1_Returns1()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlContentForTests.Version1("en", "", "", ""));
				var versionGetter = new WritingSystemLdmlVersionGetter();
				Assert.That(versionGetter.GetFileVersion(_environment.PathToWritingSystemLdmlFile), Is.EqualTo(1));
			}
		}

		[Test]
		public void WritingSystemLdmlVerisonGetterGetFileVerison_FileIsVersion3_Returns3()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlContentForTests.Version3("en", "", "", ""));
				var versionGetter = new WritingSystemLdmlVersionGetter();
				Assert.That(versionGetter.GetFileVersion(_environment.PathToWritingSystemLdmlFile), Is.EqualTo(3));
			}
		}
	}
}
