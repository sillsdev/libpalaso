using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Palaso.DictionaryServices.Model;
using Palaso.Lift;
using Palaso.TestUtilities;

// TODO Can this be moved up to Palaso.Lift.Tests?

namespace Palaso.DictionaryServices.Tests.Model
{
	[TestFixture]
	public class LexRelationIClonableGenericTests : IClonableGenericTests<IPalasoDataObjectProperty>
	{
		public override IPalasoDataObjectProperty CreateNewClonable()
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
	public class LexRelationCollectionIClonableGenericTests : IClonableGenericTests<IPalasoDataObjectProperty>
	{
		public override IPalasoDataObjectProperty CreateNewClonable()
		{
			return new LexRelationCollection();
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
								 new ValuesToSet(
									new List<LexRelation> { new LexRelation("id", "target", null) },
									new List<LexRelation> { new LexRelation("!id", "!target", null) }),
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