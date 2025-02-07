using System.IO;
using System.Xml;
using NUnit.Framework;
using SIL.DictionaryServices.Lift;
using SIL.DictionaryServices.Model;
using SIL.IO;
using SIL.Progress;
using SIL.Tests.Data;

namespace SIL.DictionaryServices.Tests.Lift
{
	internal static class LiftFileInitializer
	{
		public static string MakeFile(string liftFileName)
		{
			File.WriteAllText(liftFileName,
							  @"<?xml version='1.0' encoding='utf-8'?>
				<lift
					version='0.13'
					producer='WeSay 1.0.0.0'>
					<entry
						id='Sonne_c753f6cc-e07c-4bb1-9e3c-013d09629111'
						dateCreated='2008-07-01T06:29:23Z'
						dateModified='2008-07-01T06:29:57Z'
						guid='c753f6cc-e07c-4bb1-9e3c-013d09629111'>
						<lexical-unit>
							<form
								lang='v'>
								<text>Sonne</text>
							</form>
						</lexical-unit>
						<sense
							id='33d60091-ba96-4204-85fe-9d15a24bd5ff'>
							<trait
								name='SemanticDomainDdp4'
								value='1 Universe, creation' />
						</sense>
					</entry>
				</lift>");
			return liftFileName;
		}
	}

	[TestFixture]
	public class LiftRepositoryStateUninitializedTests: IRepositoryStateUninitializedTests<LexEntry>
	{
		private string _persistedFilePath;
		private TempFile _tempFile;

		[SetUp]
		public override void SetUp()
		{
			_tempFile = new TempFile();
			_persistedFilePath = _tempFile.Path;
			DataMapperUnderTest = LiftRepositoryStateUninitializedTests.CreateDataMapper(_persistedFilePath);
		}

		[TearDown]
		public override void TearDown()
		{
			DataMapperUnderTest.Dispose();
			_tempFile.Dispose();
		}

		public static LiftDataMapper CreateDataMapper(string filePath)
		{
			return new LiftDataMapper(
				filePath, null, new string[] {}, new ProgressState()
			);
		}

		[Test]
		public void Constructor_FileIsNotWriteableWhenRepositoryIsCreated_Throws()
		{
			using (File.OpenWrite(_persistedFilePath))
			{
				Assert.Throws<IOException>(() => CreateDataMapper(_persistedFilePath));
			}
		}

		[Test]
		public void Constructor_FileDoesNotExist_EmptyLiftFileIsCreated()
		{
			string nonExistentFileToBeCreated = Path.GetTempPath() + Path.GetRandomFileName();
			using (LiftRepositoryStateUninitializedTests.CreateDataMapper(nonExistentFileToBeCreated))
			{
			}
			XmlDocument dom = new XmlDocument();
			dom.Load(nonExistentFileToBeCreated);
			Assert.AreEqual(2, dom.ChildNodes.Count);
			Assert.AreEqual("lift", dom.ChildNodes[1].Name);
			Assert.AreEqual(0, dom.ChildNodes[1].ChildNodes.Count);
			File.Delete(nonExistentFileToBeCreated);
		}

		[Test]
		public void Constructor_FileIsEmpty_MakeFileAnEmptyLiftFile()
		{
			string emptyFileToBeFilled = Path.GetTempFileName();
			using (LiftRepositoryStateUninitializedTests.CreateDataMapper(emptyFileToBeFilled))
			{
			}
			XmlDocument doc = new XmlDocument();
			doc.Load(emptyFileToBeFilled);
			XmlNode root = doc.DocumentElement;
			Assert.AreEqual("lift", root.Name);
			File.Delete(emptyFileToBeFilled);
		}

		[Test]
		public void UnlockedLiftFile_ConstructorDoesNotThrow()
		{
			using (var persistedFile = new TempFile())
			{
				var persistedFilePath = persistedFile.Path;

				// Confirm that the file is writable.
				FileStream fileStream = File.OpenWrite(persistedFilePath);
				Assert.IsTrue(fileStream.CanWrite);

				// Close it before creating the LiftDataMapper.
				fileStream.Close();

				LiftDataMapper liftDataMapper = null;
				Assert.That(() => liftDataMapper = CreateDataMapper(persistedFilePath), Throws.Nothing);
				liftDataMapper.Dispose();
			}
		}
	}

	[TestFixture]
	public class LiftRepositoryCreatedFromPersistedData:
		IRepositoryPopulateFromPersistedTests<LexEntry>
	{
		private string _persistedFilePath;
		private TempFile _tempFile;

		[SetUp]
		public override void SetUp()
		{
			_tempFile = new TempFile();
			_persistedFilePath = _tempFile.Path;
			LiftFileInitializer.MakeFile(_persistedFilePath);
			DataMapperUnderTest = LiftRepositoryStateUninitializedTests.CreateDataMapper(_persistedFilePath);
		}

		[TearDown]
		public override void TearDown()
		{
			DataMapperUnderTest.Dispose();
			_tempFile.Dispose();
		}

		protected override void  LastModified_IsSetToMostRecentItemInPersistedDatasLastModifiedTime_v()
		{
			Assert.AreEqual(Item.ModificationTime, DataMapperUnderTest.LastModified);
		}

		protected override void CreateNewRepositoryFromPersistedData()
		{
			DataMapperUnderTest.Dispose();
			DataMapperUnderTest = LiftRepositoryStateUninitializedTests.CreateDataMapper(_persistedFilePath);
		}

	}

	[TestFixture]
	public class LiftRepositoryCreateItemTransitionTests:
		IRepositoryCreateItemTransitionTests<LexEntry>
	{
		private string _persistedFilePath;
		private TempFile _tempFile;

		public LiftRepositoryCreateItemTransitionTests()
		{
			_hasPersistOnCreate = false;
		}

		[SetUp]
		public override void SetUp()
		{
			_tempFile = new TempFile();
			_persistedFilePath = _tempFile.Path;
			DataMapperUnderTest = LiftRepositoryStateUninitializedTests.CreateDataMapper(_persistedFilePath);
		}

		[TearDown]
		public override void TearDown()
		{
			DataMapperUnderTest.Dispose();
			_tempFile.Dispose();
		}

		protected override void CreateNewRepositoryFromPersistedData()
		{
			DataMapperUnderTest.Dispose();
			DataMapperUnderTest = LiftRepositoryStateUninitializedTests.CreateDataMapper(_persistedFilePath);
		}

	}

	[TestFixture]
	public class LiftRepositoryDeleteItemTransitionTests:
		IRepositoryDeleteItemTransitionTests<LexEntry>
	{
		private string _persistedFilePath;
		private TempFile _tempFile;

		[SetUp]
		public override void SetUp()
		{
			_tempFile = new TempFile();
			_persistedFilePath = _tempFile.Path;
			DataMapperUnderTest = LiftRepositoryStateUninitializedTests.CreateDataMapper(_persistedFilePath);
		}

		[TearDown]
		public override void TearDown()
		{
			DataMapperUnderTest.Dispose();
			_tempFile.Dispose();
		}

		protected override void CreateNewRepositoryFromPersistedData()
		{
			DataMapperUnderTest.Dispose();
			DataMapperUnderTest = LiftRepositoryStateUninitializedTests.CreateDataMapper(_persistedFilePath);
		}

	}

	[TestFixture]
	public class LiftRepositoryDeleteIdTransitionTests: IRepositoryDeleteIdTransitionTests<LexEntry>
	{
		private string _persistedFilePath;
		private TempFile _tempFile;

		[SetUp]
		public override void SetUp()
		{
			_tempFile = new TempFile();
			_persistedFilePath = _tempFile.Path;
			DataMapperUnderTest = LiftRepositoryStateUninitializedTests.CreateDataMapper(_persistedFilePath);
		}

		[TearDown]
		public override void TearDown()
		{
			DataMapperUnderTest.Dispose();
			_tempFile.Dispose();
		}

		protected override void CreateNewRepositoryFromPersistedData()
		{
			DataMapperUnderTest.Dispose();
			DataMapperUnderTest = LiftRepositoryStateUninitializedTests.CreateDataMapper(_persistedFilePath);
		}


	}

	[TestFixture]
	public class LiftRepositoryDeleteAllItemsTransitionTests:
		IRepositoryDeleteAllItemsTransitionTests<LexEntry>
	{
		private string _persistedFilePath;
		private TempFile _tempFile;

		[SetUp]
		public override void SetUp()
		{
			_tempFile = new TempFile();
			_persistedFilePath = _tempFile.Path;
			DataMapperUnderTest = LiftRepositoryStateUninitializedTests.CreateDataMapper(_persistedFilePath);
		}

		[TearDown]
		public override void TearDown()
		{
			DataMapperUnderTest.Dispose();
			_tempFile.Dispose();
		}

		protected override void RepopulateRepositoryFromPersistedData()
		{
			DataMapperUnderTest.Dispose();
			DataMapperUnderTest = LiftRepositoryStateUninitializedTests.CreateDataMapper(_persistedFilePath);
		}


	}

	[TestFixture]
	public class LiftFileAlreadyLockedTest
	{
		private string _persistedFilePath;
		private FileStream _fileStream;

		[SetUp]
		public void SetUp()
		{
			_persistedFilePath = Path.GetRandomFileName();
			_persistedFilePath = Path.GetFullPath(_persistedFilePath);
			_fileStream = File.OpenWrite(_persistedFilePath);
		}

		[TearDown]
		public void TearDown()
		{
			_fileStream.Close();
			File.Delete(_persistedFilePath);
		}

		[Test]
		public void LockedFile_Throws()
		{
			Assert.IsTrue(_fileStream.CanWrite);
			Assert.Throws<IOException>(() =>
				LiftRepositoryStateUninitializedTests.CreateDataMapper(_persistedFilePath));
		}
	}
}