using System.IO;
using NUnit.Framework;
using Palaso.Lift.Model;

// TODO Can this be moved up to Palaso.Lift.Tests?

namespace Palaso.DictionaryServices.Tests.Model
{
	[TestFixture]
	public class LexRelationTests
	{
		private string _filePath;
		//private LexEntryRepository _lexEntryRepository;

		[SetUp]
		public void Setup()
		{
			//WeSayWordsProject.InitializeForTests();

			_filePath = Path.GetTempFileName();
			//_lexEntryRepository = new LexEntryRepository(_filePath);
		}

		[TearDown]
		public void TearDown()
		{
			//_lexEntryRepository.Dispose();
			File.Delete(_filePath);
		}

		[Test]
		public void Construct_TargetIdNull_TargetIdIsEmptyString()
		{
			LexSense sense = new LexSense();
			LexRelationType synonymRelationType = new LexRelationType("synonym",
																	  LexRelationType.Multiplicities
																			  .Many,
																	  LexRelationType.TargetTypes.
																			  Sense);

			LexRelation relation = new LexRelation(synonymRelationType.ID, null, sense);
			Assert.AreEqual(string.Empty, relation.Key);
		}

		[Test]
		public void TargetId_SetNull_GetStringEmpty()
		{
			LexSense sense = new LexSense();
			LexRelationType synonymRelationType = new LexRelationType("synonym",
																	  LexRelationType.Multiplicities
																			  .Many,
																	  LexRelationType.TargetTypes.
																			  Sense);

			LexRelation relation = new LexRelation(synonymRelationType.ID, "something", sense);
			relation.Key = null;
			Assert.AreEqual(string.Empty, relation.Key);
		}
	}
}