// TODO Can this be moved up to SIL.Lift.Tests?
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using SIL.DictionaryServices.Model;
using SIL.Lift;
using SIL.TestUtilities;

namespace SIL.DictionaryServices.Tests.Model
{
	[TestFixture]
	public class LexRelationCloneableTests : CloneableTests<IPalasoDataObjectProperty>
	{
		public override IPalasoDataObjectProperty CreateNewCloneable()
		{
			return new LexRelation("Id", "targetId", null);
		}

		public override string ExceptionList
		{
			//PropertyChanged: No good way to clone eventhandlers
			//_parent: We are doing top down clones. Children shouldn't make clones of their parents, but parents of their children.
			get { return "|_parent|PropertyChanged|"; }
		}

		protected override List<ValuesToSet> DefaultValuesForTypes
		{
			get
			{
				return new List<ValuesToSet>
							 {
								new ValuesToSet("to be", "!(to be)"),
								new ValuesToSet(
									new List<LexTrait> { new LexTrait("one", "eins"), new LexTrait("two", "zwei") },
									new List<LexTrait> { new LexTrait("three", "drei"), new LexTrait("four", "vier") }),
								new ValuesToSet(
									new List<LexField> { new LexField("one"), new LexField("two") },
									new List<LexField> { new LexField("three"), new LexField("four") }),
								new ValuesToSet(new List<string>{"to", "be"}, new List<string>{"!","to","be"})
							 };
			}
		}
	}

	[TestFixture]
	public class LexRelationTests
	{
		private string _filePath;

		[SetUp]
		public void Setup()
		{
			_filePath = Path.GetTempFileName();
		}

		[TearDown]
		public void TearDown()
		{
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