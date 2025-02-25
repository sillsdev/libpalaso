using System;
using System.Collections.Generic;
using NUnit.Framework;
using SIL.Data;
using SIL.DictionaryServices.Model;
using SIL.IO;
using SIL.Lift;
using SIL.Lift.Options;
using SIL.TestUtilities;
using SIL.Text;
using SIL.WritingSystems;

namespace SIL.DictionaryServices.Tests
{
	[TestFixture]
	[OfflineSldr]
	public class LiftLexEntryRepositoryTests
	{
		private class TestEnvironment : IDisposable
		{
			private readonly TempFile _tempFile;
			private readonly LiftLexEntryRepository _repository;
			private readonly WritingSystemDefinition _headwordWritingSystem;

			public TestEnvironment()
			{
				_tempFile = new TempFile();
				_repository = new LiftLexEntryRepository(_tempFile.Path);
				_headwordWritingSystem = new WritingSystemDefinition("th") {DefaultCollation = new IcuRulesCollationDefinition("standard")};
			}

			public LiftLexEntryRepository Repository
			{
				get { return _repository; }
			}

			public WritingSystemDefinition HeadwordWritingSystem
			{
				get { return _headwordWritingSystem; }
			}

			public void CreateThreeDifferentLexEntries(GetMultiTextFromLexEntryDelegate getMultiTextFromLexEntryDelegate)
			{
				LexEntry[] lexEntriesToSort = new LexEntry[3];
				MultiText[] propertyOfLexEntry = new MultiText[3];

				lexEntriesToSort[0] = _repository.CreateItem();
				propertyOfLexEntry[0] = getMultiTextFromLexEntryDelegate(lexEntriesToSort[0]);
				propertyOfLexEntry[0].SetAlternative("de", "de Word2");

				lexEntriesToSort[1] = _repository.CreateItem();
				propertyOfLexEntry[1] = getMultiTextFromLexEntryDelegate(lexEntriesToSort[1]);
				propertyOfLexEntry[1].SetAlternative("de", "de Word3");

				lexEntriesToSort[2] = _repository.CreateItem();
				propertyOfLexEntry[2] = getMultiTextFromLexEntryDelegate(lexEntriesToSort[2]);
				propertyOfLexEntry[2].SetAlternative("de", "de Word1");

				_repository.SaveItem(lexEntriesToSort[0]);
				_repository.SaveItem(lexEntriesToSort[1]);
				_repository.SaveItem(lexEntriesToSort[2]);
			}

			public LexEntry CreateLexEntryWithSemanticDomain(string semanticDomain)
			{
				LexEntry lexEntryWithSemanticDomain = _repository.CreateItem();
				lexEntryWithSemanticDomain.Senses.Add(new LexSense());
				OptionRefCollection o =
					lexEntryWithSemanticDomain.Senses[0].GetOrCreateProperty<OptionRefCollection>(
						LexSense.WellKnownProperties.SemanticDomainDdp4);
				o.Add(semanticDomain);
				_repository.SaveItem(lexEntryWithSemanticDomain);
				return lexEntryWithSemanticDomain;
			}

			public void AddSenseWithSemanticDomainToLexEntry(LexEntry entry, string semanticDomain)
			{
				entry.Senses.Add(new LexSense());
				OptionRefCollection o =
					entry.Senses[1].GetOrCreateProperty<OptionRefCollection>(
						LexSense.WellKnownProperties.SemanticDomainDdp4);
				o.Add(semanticDomain);
				_repository.SaveItem(entry);
			}

			public void CreateLexEntryWithLexicalForm(string lexicalForm)
			{
				LexEntry lexEntryWithLexicalForm = _repository.CreateItem();
				lexEntryWithLexicalForm.LexicalForm["de"] = lexicalForm;
				_repository.SaveItem(lexEntryWithLexicalForm);
			}

			public void CreateEntryWithLexicalFormAndGloss(LanguageForm glossToMatch, string lexicalFormWritingSystem, string lexicalForm)
			{
				LexEntry entryWithGlossAndLexicalForm = _repository.CreateItem();
				entryWithGlossAndLexicalForm.Senses.Add(new LexSense());
				entryWithGlossAndLexicalForm.Senses[0].Gloss.SetAlternative(glossToMatch.WritingSystemId, glossToMatch.Form);
				entryWithGlossAndLexicalForm.LexicalForm.SetAlternative(lexicalFormWritingSystem, lexicalForm);
			}

			public void AddEntryWithGloss(string gloss)
			{
				LexEntry entry = _repository.CreateItem();
				LexSense sense = new LexSense();
				entry.Senses.Add(sense);
				sense.Gloss["en"] = gloss;
				_repository.SaveItem(entry);
			}

			public LexEntry MakeEntryWithLexemeForm(string writingSystemId, string lexicalUnit)
			{
				LexEntry entry = _repository.CreateItem();
				entry.LexicalForm.SetAlternative(writingSystemId, lexicalUnit);
				_repository.SaveItem(entry);
				return entry;
			}

			public void Dispose()
			{
				_repository.Dispose();
				_tempFile.Dispose();
			}
		}

		private delegate MultiText GetMultiTextFromLexEntryDelegate(LexEntry entry);

		private static WritingSystemDefinition WritingSystemDefinitionForTest(string languageISO)
		{
			var retval = new WritingSystemDefinition();
			retval.Language = languageISO;
			retval.DefaultCollation = new IcuRulesCollationDefinition("standard");
			return retval;
		}

		private static WritingSystemDefinition WritingSystemDefinitionForTest()
		{
			return WritingSystemDefinitionForTest("th");
		}

		[Test]
		public void GetAllEntriesSortedByHeadword_RepositoryIsEmpty_ReturnsEmptyList()
		{
			using (var env = new TestEnvironment())
			{
				Assert.AreEqual(0, env.Repository.GetAllEntriesSortedByHeadword(WritingSystemDefinitionForTest()).Count);
			}
		}

		[Test]
		public void GetAllEntriesSortedByHeadword_Null_Throws()
		{
			using (var env = new TestEnvironment())
			{
				Assert.Throws<ArgumentNullException>(() =>
					env.Repository.GetAllEntriesSortedByHeadword(null));
			}
		}

		[Test]
		public void GetAllEntriesSortedByHeadword_CitationFormExistsInWritingSystemForAllEntries_ReturnsListSortedByCitationForm()
		{
			using (var env = new TestEnvironment())
			{
				env.CreateThreeDifferentLexEntries(delegate(LexEntry e) { return e.CitationForm; });
				var german = WritingSystemDefinitionForTest("de");
				ResultSet<LexEntry> listOfLexEntriesSortedByHeadWord = env.Repository.GetAllEntriesSortedByHeadword(german);
				Assert.AreEqual("de Word1", listOfLexEntriesSortedByHeadWord[0]["Form"]);
				Assert.AreEqual("de Word2", listOfLexEntriesSortedByHeadWord[1]["Form"]);
				Assert.AreEqual("de Word3", listOfLexEntriesSortedByHeadWord[2]["Form"]);
			}
		}

		[Test]
		public void GetAllEntriesSortedByHeadword_CitationAndLexicalFormInWritingSystemDoNotExist_ReturnsNullForThatEntry()
		{
			using (var env = new TestEnvironment())
			{
				LexEntry lexEntryWithOutFrenchHeadWord = env.Repository.CreateItem();
				lexEntryWithOutFrenchHeadWord.LexicalForm.SetAlternative("de", "de Word1");
				lexEntryWithOutFrenchHeadWord.CitationForm.SetAlternative("de", "de Word1");
				var french = WritingSystemDefinitionForTest("fr");
				ResultSet<LexEntry> listOfLexEntriesSortedByHeadWord = env.Repository.GetAllEntriesSortedByHeadword(french);
				Assert.AreEqual(null, listOfLexEntriesSortedByHeadWord[0]["Form"]);
			}
		}

		[Test]
		public void GetAllEntriesSortedByHeadword_CitationFormInWritingSystemDoesNotExistButLexicalFormDoes_SortsByLexicalFormForThatEntry()
		{
			using (var env = new TestEnvironment())
			{
				env.CreateThreeDifferentLexEntries(delegate(LexEntry e) { return e.CitationForm; });
				LexEntry lexEntryWithOutGermanCitationForm = env.Repository.CreateItem();
				lexEntryWithOutGermanCitationForm.CitationForm.SetAlternative("fr", "fr Word4");
				lexEntryWithOutGermanCitationForm.LexicalForm.SetAlternative("de", "de Word0");
				var german = WritingSystemDefinitionForTest("de");
				ResultSet<LexEntry> listOfLexEntriesSortedByHeadWord = env.Repository.GetAllEntriesSortedByHeadword(german);
				Assert.AreEqual(4, listOfLexEntriesSortedByHeadWord.Count);
				Assert.AreEqual("de Word0", listOfLexEntriesSortedByHeadWord[0]["Form"]);
				Assert.AreEqual("de Word1", listOfLexEntriesSortedByHeadWord[1]["Form"]);
				Assert.AreEqual("de Word2", listOfLexEntriesSortedByHeadWord[2]["Form"]);
				Assert.AreEqual("de Word3", listOfLexEntriesSortedByHeadWord[3]["Form"]);
			}
		}

		[Test]
		public void GetAllEntriesSortedByHeadword_CitationFormAndLexicalFormAreIdenticalInAnEntry_EntryOnlyAppearsOnce()
		{
			using (var env = new TestEnvironment())
			{
				LexEntry lexEntryWithIdenticalCitationAndLexicalForm = env.Repository.CreateItem();
				lexEntryWithIdenticalCitationAndLexicalForm.CitationForm.SetAlternative("de", "de Word1");
				lexEntryWithIdenticalCitationAndLexicalForm.LexicalForm.SetAlternative("de", "de Word1");
				var german = WritingSystemDefinitionForTest("de");
				ResultSet<LexEntry> listOfLexEntriesSortedByHeadWord = env.Repository.GetAllEntriesSortedByHeadword(german);
				Assert.AreEqual(1, listOfLexEntriesSortedByHeadWord.Count);
			}
		}

		[Test]
		public void GetAllEntriesSortedByLexicalFormOrAlternative_RepositoryIsEmpty_ReturnsEmptyList()
		{
			using (var env = new TestEnvironment())
			{
				Assert.AreEqual(0, env.Repository.GetAllEntriesSortedByLexicalFormOrAlternative(WritingSystemDefinitionForTest()).Count);
			}
		}

		[Test]
		public void GetAllEntriesSortedByLexicalFormOrAlternative_Null_Throws()
		{
			using (var env = new TestEnvironment())
			{
				Assert.Throws<ArgumentNullException>(() =>
					env.Repository.GetAllEntriesSortedByLexicalFormOrAlternative(null));
			}
		}

		[Test]
		public void GetAllEntriesSortedByLexicalFormOrAlternative_LexicalFormExistsInWritingSystemForAllEntries_ReturnsListSortedByLexicalForm()
		{
			using (var env = new TestEnvironment())
			{
				env.CreateThreeDifferentLexEntries(delegate(LexEntry e) { return e.LexicalForm; });
				var german = WritingSystemDefinitionForTest("de");
				ResultSet<LexEntry> listOfLexEntriesSortedByLexicalForm = env.Repository.GetAllEntriesSortedByLexicalFormOrAlternative(german);
				Assert.AreEqual("de Word1", listOfLexEntriesSortedByLexicalForm[0]["Form"]);
				Assert.AreEqual("de Word2", listOfLexEntriesSortedByLexicalForm[1]["Form"]);
				Assert.AreEqual("de Word3", listOfLexEntriesSortedByLexicalForm[2]["Form"]);
			}
		}

		[Test]
		public void GetAllEntriesSortedByLexicalFormOrAlternative_LexicalFormDoesNotExistInAnyWritingSystem_ReturnsNullForThatEntry()
		{
			using (var env = new TestEnvironment())
			{
				LexEntry lexEntryWithOutFrenchHeadWord = env.Repository.CreateItem();
				env.Repository.SaveItem(lexEntryWithOutFrenchHeadWord);
				var french = WritingSystemDefinitionForTest("fr");
				ResultSet<LexEntry> listOfLexEntriesSortedByLexicalForm = env.Repository.GetAllEntriesSortedByLexicalFormOrAlternative(french);
				Assert.AreEqual(null, listOfLexEntriesSortedByLexicalForm[0]["Form"]);
			}
		}

		[Test]
		public void GetAllEntriesSortedByLexicalFormOrAlternative_LexicalFormDoesNotExistInPrimaryWritingSystemButDoesInAnother_ReturnsAlternativeThatEntry()
		{
			using (var env = new TestEnvironment())
			{
				LexEntry lexEntryWithOutFrenchHeadWord = env.Repository.CreateItem();
				lexEntryWithOutFrenchHeadWord.LexicalForm.SetAlternative("de", "de word1");
				env.Repository.SaveItem(lexEntryWithOutFrenchHeadWord);
				var french = WritingSystemDefinitionForTest("fr");
				ResultSet<LexEntry> listOfLexEntriesSortedByLexicalForm = env.Repository.GetAllEntriesSortedByLexicalFormOrAlternative(french);
				Assert.AreEqual("de word1", listOfLexEntriesSortedByLexicalForm[0]["Form"]);
			}
		}

		[Test]
		public void GetAllEntriesSortedByLexicalFormOrAlternative_LexicalFormExistsInWritingSystem_ReturnsWritingSystem()
		{
			using (var env = new TestEnvironment())
			{
				LexEntry lexEntryWithOutFrenchHeadWord = env.Repository.CreateItem();
				lexEntryWithOutFrenchHeadWord.LexicalForm.SetAlternative("fr", "fr word1");
				env.Repository.SaveItem(lexEntryWithOutFrenchHeadWord);
				var french = WritingSystemDefinitionForTest("fr");
				ResultSet<LexEntry> listOfLexEntriesSortedByLexicalForm = env.Repository.GetAllEntriesSortedByLexicalFormOrAlternative(french);
				Assert.AreEqual("fr", listOfLexEntriesSortedByLexicalForm[0]["WritingSystem"]);
			}
		}

		[Test]
		public void GetAllEntriesSortedByLexicalFormOrAlternative_LexicalFormDoesNotExistInPrimaryWritingSystemButDoesInAnother_ReturnsWritingSystem()
		{
			using (var env = new TestEnvironment())
			{
				LexEntry lexEntryWithOutFrenchHeadWord = env.Repository.CreateItem();
				lexEntryWithOutFrenchHeadWord.LexicalForm.SetAlternative("de", "de word1");
				env.Repository.SaveItem(lexEntryWithOutFrenchHeadWord);
				var french = WritingSystemDefinitionForTest("fr");
				ResultSet<LexEntry> listOfLexEntriesSortedByLexicalForm = env.Repository.GetAllEntriesSortedByLexicalFormOrAlternative(french);
				Assert.AreEqual("de", listOfLexEntriesSortedByLexicalForm[0]["WritingSystem"]);
			}
		}

		[Test]
		public void GetAllEntriesSortedByDefinition_RepositoryIsEmpty_ReturnsEmptyList()
		{
			using (var env = new TestEnvironment())
			{
				Assert.AreEqual(0, env.Repository.GetAllEntriesSortedByDefinitionOrGloss(WritingSystemDefinitionForTest()).Count);
			}
		}

		[Test]
		public void GetAllEntriesSortedByDefinition_Null_Throws()
		{
			using (var env = new TestEnvironment())
			{
				Assert.Throws<ArgumentNullException>(() =>
					env.Repository.GetAllEntriesSortedByDefinitionOrGloss(null));
			}
		}

		[Test]
		public void GetAllEntriesSortedByDefinition_DefinitionExistsInWritingSystemForAllEntries_ReturnsListSortedByDefinition()
		{
			using (var env = new TestEnvironment())
			{
				env.CreateThreeDifferentLexEntries(delegate(LexEntry e)
				{
					e.Senses.Add(new LexSense());
					return e.Senses[0].Definition;
				});
				var german = WritingSystemDefinitionForTest("de");
				ResultSet<LexEntry> listOfLexEntriesSortedByDefinition = env.Repository.GetAllEntriesSortedByDefinitionOrGloss(german);
				Assert.AreEqual(3, listOfLexEntriesSortedByDefinition.Count);
				Assert.AreEqual("de Word1", listOfLexEntriesSortedByDefinition[0]["Form"]);
				Assert.AreEqual("de Word2", listOfLexEntriesSortedByDefinition[1]["Form"]);
				Assert.AreEqual("de Word3", listOfLexEntriesSortedByDefinition[2]["Form"]);
			}
		}

		[Test]
		public void GetAllEntriesSortedByDefinition_DefinitionsDoesNotExistInWritingSystemButGlossDoesForAllEntries_ReturnsListSortedByGloss()
		{
			using (var env = new TestEnvironment())
			{
				env.CreateThreeDifferentLexEntries(delegate(LexEntry e)
				{
					e.Senses.Add(new LexSense());
					return e.Senses[0].Gloss;
				});
				var german = WritingSystemDefinitionForTest("de");
				ResultSet<LexEntry> listOfLexEntriesSortedByGloss = env.Repository.GetAllEntriesSortedByDefinitionOrGloss(german);
				Assert.AreEqual(3, listOfLexEntriesSortedByGloss.Count);
				Assert.AreEqual("de Word1", listOfLexEntriesSortedByGloss[0]["Form"]);
				Assert.AreEqual("de Word2", listOfLexEntriesSortedByGloss[1]["Form"]);
				Assert.AreEqual("de Word3", listOfLexEntriesSortedByGloss[2]["Form"]);
			}
		}

		[Test]
		public void GetAllEntriesSortedByDefinition_DefinitionAndGlossExistInWritingSystem_ReturnsSortedListWithBothDefinitionAndGloss()
		{
			using (var env = new TestEnvironment())
			{
				LexEntry lexEntryWithBothDefinitionAndAGloss = env.Repository.CreateItem();
				lexEntryWithBothDefinitionAndAGloss.Senses.Add(new LexSense());
				lexEntryWithBothDefinitionAndAGloss.Senses[0].Definition.SetAlternative("de", "de Word2");
				lexEntryWithBothDefinitionAndAGloss.Senses[0].Gloss.SetAlternative("de", "de Word1");
				var german = WritingSystemDefinitionForTest("de");
				ResultSet<LexEntry> listOfLexEntriesSortedByDefinition = env.Repository.GetAllEntriesSortedByDefinitionOrGloss(german);
				Assert.AreEqual("de Word1", listOfLexEntriesSortedByDefinition[0]["Form"]);
				Assert.AreEqual("de Word2", listOfLexEntriesSortedByDefinition[1]["Form"]);
			}
		}

		[Test]
		public void GetAllEntriesSortedByDefinition_DefinitionIsIdenticalInWritingSystemInMultipleSenses_ReturnsSortedListWithBothDefinitions()
		{
			using (var env = new TestEnvironment())
			{
				LexEntry lexEntryWithBothDefinitionAndAGloss = env.Repository.CreateItem();
				lexEntryWithBothDefinitionAndAGloss.Senses.Add(new LexSense());
				lexEntryWithBothDefinitionAndAGloss.Senses.Add(new LexSense());
				lexEntryWithBothDefinitionAndAGloss.Senses[0].Definition.SetAlternative("de", "de Word1");
				lexEntryWithBothDefinitionAndAGloss.Senses[1].Definition.SetAlternative("de", "de Word1");
				var german = WritingSystemDefinitionForTest("de");
				ResultSet<LexEntry> listOfLexEntriesSortedByDefinition = env.Repository.GetAllEntriesSortedByDefinitionOrGloss(german);
				Assert.AreEqual(2, listOfLexEntriesSortedByDefinition.Count);
				Assert.AreEqual("de Word1", listOfLexEntriesSortedByDefinition[0]["Form"]);
				Assert.AreEqual("de Word1", listOfLexEntriesSortedByDefinition[1]["Form"]);
			}
		}

		[Test]
		public void GetAllEntriesSortedByDefinition_GlossIsIdenticalInWritingSystemInMultipleSenses_ReturnsSortedListWithBothDefinitions()
		{
			using (var env = new TestEnvironment())
			{
				LexEntry lexEntryWithBothDefinitionAndAGloss = env.Repository.CreateItem();
				lexEntryWithBothDefinitionAndAGloss.Senses.Add(new LexSense());
				lexEntryWithBothDefinitionAndAGloss.Senses.Add(new LexSense());
				lexEntryWithBothDefinitionAndAGloss.Senses[0].Gloss.SetAlternative("de", "de Word1");
				lexEntryWithBothDefinitionAndAGloss.Senses[1].Gloss.SetAlternative("de", "de Word1");
				var german = WritingSystemDefinitionForTest("de");
				ResultSet<LexEntry> listOfLexEntriesSortedByDefinition = env.Repository.GetAllEntriesSortedByDefinitionOrGloss(german);
				Assert.AreEqual(2, listOfLexEntriesSortedByDefinition.Count);
				Assert.AreEqual("de Word1", listOfLexEntriesSortedByDefinition[0]["Form"]);
				Assert.AreEqual("de Word1", listOfLexEntriesSortedByDefinition[1]["Form"]);
			}
		}

		[Test]
		public void GetAllEntriesSortedByDefinition_DefinitionAndGlossAreNullInWritingSystemInMultipleSenses_ReturnsSortedListWithBothDefinitions()
		{
			using (var env = new TestEnvironment())
			{
				LexEntry lexEntryWithBothDefinitionAndAGloss = env.Repository.CreateItem();
				lexEntryWithBothDefinitionAndAGloss.Senses.Add(new LexSense());
				lexEntryWithBothDefinitionAndAGloss.Senses.Add(new LexSense());
				var german = WritingSystemDefinitionForTest("de");
				ResultSet<LexEntry> listOfLexEntriesSortedByDefinition = env.Repository.GetAllEntriesSortedByDefinitionOrGloss(german);
				Assert.AreEqual(2, listOfLexEntriesSortedByDefinition.Count);
				Assert.AreEqual(null, listOfLexEntriesSortedByDefinition[0]["Form"]);
				Assert.AreEqual(null, listOfLexEntriesSortedByDefinition[1]["Form"]);
			}
		}

		[Test]
		public void GetAllEntriesSortedByDefinition_DefinitionAndGlossAreIdenticalInWritingSystemInMultipleSenses_ReturnsSortedListWithBothDefinitions()
		{
			using (var env = new TestEnvironment())
			{
				LexEntry lexEntryWithBothDefinitionAndAGloss = env.Repository.CreateItem();
				lexEntryWithBothDefinitionAndAGloss.Senses.Add(new LexSense());
				lexEntryWithBothDefinitionAndAGloss.Senses.Add(new LexSense());
				lexEntryWithBothDefinitionAndAGloss.Senses[0].Definition.SetAlternative("de", "de Word1");
				lexEntryWithBothDefinitionAndAGloss.Senses[0].Gloss.SetAlternative("de", "de Word1");
				lexEntryWithBothDefinitionAndAGloss.Senses[1].Definition.SetAlternative("de", "de Word1");
				lexEntryWithBothDefinitionAndAGloss.Senses[1].Gloss.SetAlternative("de", "de Word1");
				var german = WritingSystemDefinitionForTest("de");
				ResultSet<LexEntry> listOfLexEntriesSortedByDefinition = env.Repository.GetAllEntriesSortedByDefinitionOrGloss(german);
				Assert.AreEqual(2, listOfLexEntriesSortedByDefinition.Count);
				Assert.AreEqual("de Word1", listOfLexEntriesSortedByDefinition[0]["Form"]);
				Assert.AreEqual("de Word1", listOfLexEntriesSortedByDefinition[1]["Form"]);
			}
		}

		[Test]
		public void GetAllEntriesSortedByDefinition_DefinitionContainsOnlySemiColon_ReturnsListWithNullRecord()
		{
			using (var env = new TestEnvironment())
			{
				LexEntry lexEntryWithDefinition = env.Repository.CreateItem();
				lexEntryWithDefinition.Senses.Add(new LexSense());
				lexEntryWithDefinition.Senses[0].Definition.SetAlternative("de", ";");
				var german = WritingSystemDefinitionForTest("de");
				ResultSet<LexEntry> listOfLexEntriesSortedByDefinition = env.Repository.GetAllEntriesSortedByDefinitionOrGloss(german);
				Assert.AreEqual(1, listOfLexEntriesSortedByDefinition.Count);
				Assert.AreEqual(null, listOfLexEntriesSortedByDefinition[0]["Form"]);
			}
		}

		[Test]
		public void GetAllEntriesSortedByDefinition_GlossContainsOnlySemiColon_ReturnsListWithNullRecord()
		{
			using (var env = new TestEnvironment())
			{
				LexEntry lexEntryWithGloss = env.Repository.CreateItem();
				lexEntryWithGloss.Senses.Add(new LexSense());
				lexEntryWithGloss.Senses[0].Gloss.SetAlternative("de", ";");
				var german = WritingSystemDefinitionForTest("de");
				ResultSet<LexEntry> listOfLexEntriesSortedByDefinition = env.Repository.GetAllEntriesSortedByDefinitionOrGloss(german);
				Assert.AreEqual(1, listOfLexEntriesSortedByDefinition.Count);
				Assert.AreEqual(null, listOfLexEntriesSortedByDefinition[0]["Form"]);
			}
		}

		[Test]
		public void GetAllEntriesSortedByDefinition_DefinitionAndGlossAreIdenticalContainsOnlySemiColon_ReturnsListWithOneNullRecord()
		{
			using (var env = new TestEnvironment())
			{
				LexEntry lexEntryWithGloss = env.Repository.CreateItem();
				lexEntryWithGloss.Senses.Add(new LexSense());
				lexEntryWithGloss.Senses[0].Definition.SetAlternative("de", ";");
				lexEntryWithGloss.Senses[0].Gloss.SetAlternative("de", ";");
				var german = WritingSystemDefinitionForTest("de");
				ResultSet<LexEntry> listOfLexEntriesSortedByDefinition = env.Repository.GetAllEntriesSortedByDefinitionOrGloss(german);
				Assert.AreEqual(1, listOfLexEntriesSortedByDefinition.Count);
				Assert.AreEqual(null, listOfLexEntriesSortedByDefinition[0]["Form"]);
			}
		}

		[Test]
		public void GetAllEntriesSortedByDefinition_DefinitionAndGlossAreIdenticalAndHaveTwoIdenticalWordsSeparatedBySemiColon_ReturnsListWithOneNullRecord()
		{
			using (var env = new TestEnvironment())
			{
				LexEntry lexEntryWithGloss = env.Repository.CreateItem();
				lexEntryWithGloss.Senses.Add(new LexSense());
				lexEntryWithGloss.Senses[0].Definition.SetAlternative("de", "de Word1;de Word2");
				lexEntryWithGloss.Senses[0].Gloss.SetAlternative("de", "de Word1;de Word2");
				var german = WritingSystemDefinitionForTest("de");
				ResultSet<LexEntry> listOfLexEntriesSortedByDefinition = env.Repository.GetAllEntriesSortedByDefinitionOrGloss(german);
				Assert.AreEqual(2, listOfLexEntriesSortedByDefinition.Count);
				Assert.AreEqual("de Word1", listOfLexEntriesSortedByDefinition[0]["Form"]);
				Assert.AreEqual("de Word2", listOfLexEntriesSortedByDefinition[1]["Form"]);
			}
		}

		[Test]
		public void GetAllEntriesSortedByDefinition_DefinitionHasTwoIdenticalWordsSeparatedBySemiColon_ReturnsListWithOnlyOneRecord()
		{
			using (var env = new TestEnvironment())
			{
				LexEntry lexEntryWithDefinition = env.Repository.CreateItem();
				lexEntryWithDefinition.Senses.Add(new LexSense());
				lexEntryWithDefinition.Senses[0].Definition.SetAlternative("de", "de Word1;de Word1");
				var german = WritingSystemDefinitionForTest("de");
				ResultSet<LexEntry> listOfLexEntriesSortedByDefinition = env.Repository.GetAllEntriesSortedByDefinitionOrGloss(german);
				Assert.AreEqual(1, listOfLexEntriesSortedByDefinition.Count);
				Assert.AreEqual("de Word1", listOfLexEntriesSortedByDefinition[0]["Form"]);
			}
		}

		[Test]
		public void GetAllEntriesSortedByDefinition_GlossHasTwoIdenticalWordsSeparatedBySemiColon_ReturnsListWithOnlyOneRecord()
		{
			using (var env = new TestEnvironment())
			{
				LexEntry lexEntryWithGloss = env.Repository.CreateItem();
				lexEntryWithGloss.Senses.Add(new LexSense());
				lexEntryWithGloss.Senses[0].Gloss.SetAlternative("de", "de Word1;de Word1");
				var german = WritingSystemDefinitionForTest("de");
				ResultSet<LexEntry> listOfLexEntriesSortedByDefinition = env.Repository.GetAllEntriesSortedByDefinitionOrGloss(german);
				Assert.AreEqual(1, listOfLexEntriesSortedByDefinition.Count);
				Assert.AreEqual("de Word1", listOfLexEntriesSortedByDefinition[0]["Form"]);
			}
		}

		[Test]
		public void GetAllEntriesSortedByDefinition_DefinitionContainsSemiColon_DefinitionIsSplitAtSemiColonAndEachPartReturned()
		{
			using (var env = new TestEnvironment())
			{
				LexEntry lexEntryWithBothDefinitionAndAGloss = env.Repository.CreateItem();
				lexEntryWithBothDefinitionAndAGloss.Senses.Add(new LexSense());
				lexEntryWithBothDefinitionAndAGloss.Senses[0].Definition.SetAlternative("de", "de Word2;de Word1");
				var german = WritingSystemDefinitionForTest("de");
				ResultSet<LexEntry> listOfLexEntriesSortedByDefinition = env.Repository.GetAllEntriesSortedByDefinitionOrGloss(german);
				Assert.AreEqual(2, listOfLexEntriesSortedByDefinition.Count);
				Assert.AreEqual("de Word1", listOfLexEntriesSortedByDefinition[0]["Form"]);
				Assert.AreEqual("de Word2", listOfLexEntriesSortedByDefinition[1]["Form"]);
			}
		}

		[Test]
		public void GetAllEntriesSortedByDefinition_GlossContainsSemiColon_GlossIsSplitAtSemiColonAndEachPartReturned()
		{
			using (var env = new TestEnvironment())
			{
				LexEntry lexEntryWithBothDefinitionAndAGloss = env.Repository.CreateItem();
				lexEntryWithBothDefinitionAndAGloss.Senses.Add(new LexSense());
				lexEntryWithBothDefinitionAndAGloss.Senses[0].Gloss.SetAlternative("de", "de Word2;de Word1");
				var german = WritingSystemDefinitionForTest("de");
				ResultSet<LexEntry> listOfLexEntriesSortedByDefinition = env.Repository.GetAllEntriesSortedByDefinitionOrGloss(german);
				Assert.AreEqual(2, listOfLexEntriesSortedByDefinition.Count);
				Assert.AreEqual("de Word1", listOfLexEntriesSortedByDefinition[0]["Form"]);
				Assert.AreEqual("de Word2", listOfLexEntriesSortedByDefinition[1]["Form"]);
			}
		}

		[Test]
		public void GetAllEntriesSortedByDefinition_DefinitionContainsSemiColon_DefinitionIsSplitAtSemiColonAndExtraWhiteSpaceStrippedAndEachPartReturned()
		{
			using (var env = new TestEnvironment())
			{
				LexEntry lexEntryWithBothDefinitionAndAGloss = env.Repository.CreateItem();
				lexEntryWithBothDefinitionAndAGloss.Senses.Add(new LexSense());
				lexEntryWithBothDefinitionAndAGloss.Senses[0].Definition.SetAlternative("de", "de Word2; de Word1");
				var german = WritingSystemDefinitionForTest("de");
				ResultSet<LexEntry> listOfLexEntriesSortedByDefinition = env.Repository.GetAllEntriesSortedByDefinitionOrGloss(german);
				Assert.AreEqual(2, listOfLexEntriesSortedByDefinition.Count);
				Assert.AreEqual("de Word1", listOfLexEntriesSortedByDefinition[0]["Form"]);
				Assert.AreEqual("de Word2", listOfLexEntriesSortedByDefinition[1]["Form"]);
			}
		}

		[Test]
		public void GetAllEntriesSortedByDefinition_GlossContainsSemiColon_GlossIsSplitAtSemiColonAndExtraWhiteSpaceStrippedAndEachPartReturned()
		{
			using (var env = new TestEnvironment())
			{
				LexEntry lexEntryWithBothDefinitionAndAGloss = env.Repository.CreateItem();
				lexEntryWithBothDefinitionAndAGloss.Senses.Add(new LexSense());
				lexEntryWithBothDefinitionAndAGloss.Senses[0].Gloss.SetAlternative("de", "de Word1; de Word2");
				var german = WritingSystemDefinitionForTest("de");
				ResultSet<LexEntry> listOfLexEntriesSortedByDefinition = env.Repository.GetAllEntriesSortedByDefinitionOrGloss(german);
				Assert.AreEqual(2, listOfLexEntriesSortedByDefinition.Count);
				Assert.AreEqual("de Word1", listOfLexEntriesSortedByDefinition[0]["Form"]);
				Assert.AreEqual("de Word2", listOfLexEntriesSortedByDefinition[1]["Form"]);
			}
		}

		[Test]
		public void GetAllEntriesSortedByDefinition_DefinitionAndGlossAreIdenticalAndContainSemiColon_DefinitionIsSplitAtSemiColonAndExtraWhiteSpaceStrippedAndEachPartReturned()
		{
			using (var env = new TestEnvironment())
			{
				LexEntry lexEntryWithBothDefinitionAndAGloss = env.Repository.CreateItem();
				lexEntryWithBothDefinitionAndAGloss.Senses.Add(new LexSense());
				lexEntryWithBothDefinitionAndAGloss.Senses[0].Definition.SetAlternative("de", "de Word1; de Word2");
				lexEntryWithBothDefinitionAndAGloss.Senses[0].Gloss.SetAlternative("de", "de Word1; de Word2");
				var german = WritingSystemDefinitionForTest("de");
				ResultSet<LexEntry> listOfLexEntriesSortedByDefinition = env.Repository.GetAllEntriesSortedByDefinitionOrGloss(german);
				Assert.AreEqual(2, listOfLexEntriesSortedByDefinition.Count);
				Assert.AreEqual("de Word1", listOfLexEntriesSortedByDefinition[0]["Form"]);
				Assert.AreEqual("de Word2", listOfLexEntriesSortedByDefinition[1]["Form"]);
			}
		}

		[Test]
		public void GetAllEntriesSortedByDefinition_DefinitionAndGlossAreIdenticalAndContainSemiColon_DefinitionIsSplitAtSemiColonAndEachPartReturned()
		{
			using (var env = new TestEnvironment())
			{
				LexEntry lexEntryWithBothDefinitionAndAGloss = env.Repository.CreateItem();
				lexEntryWithBothDefinitionAndAGloss.Senses.Add(new LexSense());
				lexEntryWithBothDefinitionAndAGloss.Senses[0].Definition.SetAlternative("de", "de Word1; de Word2");
				lexEntryWithBothDefinitionAndAGloss.Senses[0].Gloss.SetAlternative("de", "de Word1; de Word2");
				var german = WritingSystemDefinitionForTest("de");
				ResultSet<LexEntry> listOfLexEntriesSortedByDefinition = env.Repository.GetAllEntriesSortedByDefinitionOrGloss(german);
				Assert.AreEqual(2, listOfLexEntriesSortedByDefinition.Count);
				Assert.AreEqual("de Word1", listOfLexEntriesSortedByDefinition[0]["Form"]);
				Assert.AreEqual("de Word2", listOfLexEntriesSortedByDefinition[1]["Form"]);
			}
		}

		[Test]
		public void GetAllEntriesSortedByDefinition_DefinitionContainSemiColonAndOneElementIsIdenticalToGloss_IdenticalElementIsReturnedOnlyOnce()
		{
			using (var env = new TestEnvironment())
			{
				LexEntry lexEntryWithBothDefinitionAndAGloss = env.Repository.CreateItem();
				lexEntryWithBothDefinitionAndAGloss.Senses.Add(new LexSense());
				lexEntryWithBothDefinitionAndAGloss.Senses[0].Definition.SetAlternative("de", "de Word1; de Word2");
				lexEntryWithBothDefinitionAndAGloss.Senses[0].Gloss.SetAlternative("de", "de Word1");
				var german = WritingSystemDefinitionForTest("de");
				ResultSet<LexEntry> listOfLexEntriesSortedByDefinition = env.Repository.GetAllEntriesSortedByDefinitionOrGloss(german);
				Assert.AreEqual(2, listOfLexEntriesSortedByDefinition.Count);
				Assert.AreEqual("de Word1", listOfLexEntriesSortedByDefinition[0]["Form"]);
				Assert.AreEqual("de Word2", listOfLexEntriesSortedByDefinition[1]["Form"]);
			}
		}

		[Test]
		public void GetAllEntriesSortedByDefinition_GlossContainSemiColonAndOneElementIsIdenticalToDefinition_IdenticalElementIsReturnedOnlyOnce()
		{
			using (var env = new TestEnvironment())
			{
				LexEntry lexEntryWithBothDefinitionAndAGloss = env.Repository.CreateItem();
				lexEntryWithBothDefinitionAndAGloss.Senses.Add(new LexSense());
				lexEntryWithBothDefinitionAndAGloss.Senses[0].Definition.SetAlternative("de", "de Word1");
				lexEntryWithBothDefinitionAndAGloss.Senses[0].Gloss.SetAlternative("de", "de Word1; de Word2");
				var german = WritingSystemDefinitionForTest("de");
				ResultSet<LexEntry> listOfLexEntriesSortedByDefinition = env.Repository.GetAllEntriesSortedByDefinitionOrGloss(german);
				Assert.AreEqual(2, listOfLexEntriesSortedByDefinition.Count);
				Assert.AreEqual("de Word1", listOfLexEntriesSortedByDefinition[0]["Form"]);
				Assert.AreEqual("de Word2", listOfLexEntriesSortedByDefinition[1]["Form"]);
			}
		}

		[Test]
		public void GetAllEntriesSortedByDefinition_DefinitionAndGlossContainSemiColonAndSomeElementsAreIdentical_IdenticalElementsAreReturnedOnlyOnce()
		{
			using (var env = new TestEnvironment())
			{
				LexEntry lexEntryWithBothDefinitionAndAGloss = env.Repository.CreateItem();
				lexEntryWithBothDefinitionAndAGloss.Senses.Add(new LexSense());
				lexEntryWithBothDefinitionAndAGloss.Senses[0].Definition.SetAlternative("de", "de Word1;de Word2; de Word3");
				lexEntryWithBothDefinitionAndAGloss.Senses[0].Gloss.SetAlternative("de", "de Word1; de Word3; de Word4");
				var german = WritingSystemDefinitionForTest("de");
				ResultSet<LexEntry> listOfLexEntriesSortedByDefinition = env.Repository.GetAllEntriesSortedByDefinitionOrGloss(german);
				Assert.AreEqual(4, listOfLexEntriesSortedByDefinition.Count);
				Assert.AreEqual("de Word1", listOfLexEntriesSortedByDefinition[0]["Form"]);
				Assert.AreEqual("de Word2", listOfLexEntriesSortedByDefinition[1]["Form"]);
				Assert.AreEqual("de Word3", listOfLexEntriesSortedByDefinition[2]["Form"]);
				Assert.AreEqual("de Word4", listOfLexEntriesSortedByDefinition[3]["Form"]);
			}
		}

		[Test]
		public void GetAllEntriesSortedByDefinition_DefinitionAndGlossInWritingSystemDoNotExist_ReturnsNullForThatEntry()
		{
			using (var env = new TestEnvironment())
			{
				LexEntry lexEntryWithOutFrenchGloss = env.Repository.CreateItem();
				lexEntryWithOutFrenchGloss.Senses.Add(new LexSense());
				lexEntryWithOutFrenchGloss.Senses[0].Definition.SetAlternative("de", "de Word1");
				var french = WritingSystemDefinitionForTest("fr");
				ResultSet<LexEntry> listOfLexEntriesSortedByDefinition = env.Repository.GetAllEntriesSortedByDefinitionOrGloss(french);
				Assert.AreEqual(1, listOfLexEntriesSortedByDefinition.Count);
				Assert.AreEqual(null, listOfLexEntriesSortedByDefinition[0]["Form"]);
			}
		}

		[Test]
		public void GetAllEntriesSortedByDefinition_DefinitionAndGlossOfOneEntryAreIdentical_ReturnsOnlyOneRecordToken()
		{
			using (var env = new TestEnvironment())
			{
				env.CreateThreeDifferentLexEntries(delegate(LexEntry e)
				{
					e.Senses.Add(new LexSense());
					return e.Senses[0].Definition;
				});
				LexEntry lexEntryWithBothDefinitionAndAGloss = env.Repository.CreateItem();
				lexEntryWithBothDefinitionAndAGloss.Senses.Add(new LexSense());
				lexEntryWithBothDefinitionAndAGloss.Senses[0].Definition.SetAlternative("de", "de Word4");
				lexEntryWithBothDefinitionAndAGloss.Senses[0].Gloss.SetAlternative("de", "de Word4");
				var german = WritingSystemDefinitionForTest("de");
				ResultSet<LexEntry> listOfLexEntriesSortedByDefinition = env.Repository.GetAllEntriesSortedByDefinitionOrGloss(german);
				Assert.AreEqual(4, listOfLexEntriesSortedByDefinition.Count);
				Assert.AreEqual("de Word1", listOfLexEntriesSortedByDefinition[0]["Form"]);
				Assert.AreEqual("de Word2", listOfLexEntriesSortedByDefinition[1]["Form"]);
				Assert.AreEqual("de Word3", listOfLexEntriesSortedByDefinition[2]["Form"]);
				Assert.AreEqual("de Word4", listOfLexEntriesSortedByDefinition[3]["Form"]);
			}
		}

		[Test]
		public void GetAllEntriesSortedByDefinition_DefinitionOfTwoEntriesAreIdentical_ReturnsTwoRecordTokens()
		{
			using (var env = new TestEnvironment())
			{
				LexEntry lexEntryWithBothDefinition = env.Repository.CreateItem();
				lexEntryWithBothDefinition.Senses.Add(new LexSense());
				lexEntryWithBothDefinition.Senses[0].Definition.SetAlternative("de", "de Word1");
				LexEntry lexEntryTwoWithBothDefinition = env.Repository.CreateItem();
				lexEntryTwoWithBothDefinition.Senses.Add(new LexSense());
				lexEntryTwoWithBothDefinition.Senses[0].Definition.SetAlternative("de", "de Word1");
				var german = WritingSystemDefinitionForTest("de");
				ResultSet<LexEntry> listOfLexEntriesSortedByDefinition = env.Repository.GetAllEntriesSortedByDefinitionOrGloss(german);
				Assert.AreEqual(2, listOfLexEntriesSortedByDefinition.Count);
				Assert.AreEqual("de Word1", listOfLexEntriesSortedByDefinition[0]["Form"]);
				Assert.AreEqual("de Word1", listOfLexEntriesSortedByDefinition[1]["Form"]);
			}
		}

		[Test]
		public void GetEntriesWithSemanticDomainSortedBySemanticDomain_EntriesWithDifferingSemanticDomains_EntriesAreSortedBySemanticDomain()
		{
			using (var env = new TestEnvironment())
			{
				env.CreateLexEntryWithSemanticDomain("SemanticDomain2");
				env.CreateLexEntryWithSemanticDomain("SemanticDomain1");
				ResultSet<LexEntry> sortedResults = env.Repository.GetEntriesWithSemanticDomainSortedBySemanticDomain(LexSense.WellKnownProperties.SemanticDomainDdp4);
				Assert.AreEqual(2, sortedResults.Count);
				Assert.AreEqual("SemanticDomain1", sortedResults[0]["SemanticDomain"]);
				Assert.AreEqual("SemanticDomain2", sortedResults[1]["SemanticDomain"]);
			}
		}

		[Test]
		public void GetEntriesWithSemanticDomainSortedBySemanticDomain_OneEntryWithTwoSensesThatHaveIdenticalSemanticDomains_OnlyOneTokenIsReturned()
		{
			using (var env = new TestEnvironment())
			{
				LexEntry entryWithTwoIdenticalSenses = env.CreateLexEntryWithSemanticDomain("SemanticDomain1");
				env.AddSenseWithSemanticDomainToLexEntry(entryWithTwoIdenticalSenses, "SemanticDomain1");
				ResultSet<LexEntry> sortedResults = env.Repository.GetEntriesWithSemanticDomainSortedBySemanticDomain(LexSense.WellKnownProperties.SemanticDomainDdp4);
				Assert.AreEqual(1, sortedResults.Count);
				Assert.AreEqual("SemanticDomain1", sortedResults[0]["SemanticDomain"]);
			}
		}

		[Test]
		public void GetEntriesWithSemanticDomainSortedBySemanticDomain_EntryWithoutSemanticDomain_ReturnEmpty()
		{
			using (var env = new TestEnvironment())
			{
				// Create a LexEntry with no semantic domain.
				env.Repository.CreateItem();
				ResultSet<LexEntry> sortedResults = env.Repository.GetEntriesWithSemanticDomainSortedBySemanticDomain(LexSense.WellKnownProperties.SemanticDomainDdp4);
				Assert.AreEqual(0, sortedResults.Count);
			}
		}

		[Test]
		public void GetEntriesWithSimilarLexicalForm_WritingSystemNull_Throws()
		{
			using (var env = new TestEnvironment())
			{
				WritingSystemDefinition ws = null;
				Assert.Throws<ArgumentNullException>(() =>
					env.Repository.GetEntriesWithSimilarLexicalForm(
						"",
						ws,
						ApproximateMatcherOptions.IncludePrefixedForms));
			}
		}

		[Test]
		public void GetEntriesWithSimilarLexicalForm_MultipleEntriesWithEqualAndLowestMatchingDistance_ReturnsThoseEntries()
		{
			using (var env = new TestEnvironment())
			{
				env.CreateThreeDifferentLexEntries(delegate(LexEntry e) { return e.LexicalForm; });
				var ws = WritingSystemDefinitionForTest("de");
				var matches = env.Repository.GetEntriesWithSimilarLexicalForm(
					"",
					ws,
					ApproximateMatcherOptions.IncludePrefixedForms);
				Assert.AreEqual(3, matches.Count);
			}
		}

		[Test]
		public void GetEntriesWithSimilarLexicalForm_MultipleEntriesWithDifferentMatchingDistanceAndAnyEntriesBeginningWithWhatWeAreMatchingHaveZeroDistance_ReturnsAllEntriesBeginningWithTheFormToMatch()
		{
			using (var env = new TestEnvironment())
			{
				//Matching distance as compared to Empty string
				LexEntry lexEntryWithMatchingDistance1 = env.Repository.CreateItem();
				lexEntryWithMatchingDistance1.LexicalForm.SetAlternative("de", "d");
				LexEntry lexEntryWithMatchingDistance2 = env.Repository.CreateItem();
				lexEntryWithMatchingDistance2.LexicalForm.SetAlternative("de", "de");
				LexEntry lexEntryWithMatchingDistance3 = env.Repository.CreateItem();
				lexEntryWithMatchingDistance3.LexicalForm.SetAlternative("de", "de_");
				var ws = WritingSystemDefinitionForTest("de");
				ResultSet<LexEntry> matches = env.Repository.GetEntriesWithSimilarLexicalForm(
					"",
					ws,
					ApproximateMatcherOptions.IncludePrefixedForms);
				Assert.AreEqual(3, matches.Count);
				Assert.AreEqual("d", matches[0]["Form"]);
				Assert.AreEqual("de", matches[1]["Form"]);
				Assert.AreEqual("de_", matches[2]["Form"]);
			}
		}

		[Test]
		public void GetEntriesWithSimilarLexicalForm_MultipleEntriesWithDifferentMatchingDistanceAndAnyEntriesBeginningWithWhatWeAreMatchingHaveZeroDistanceAndWeWantTheTwoClosestMatches_ReturnsAllEntriesBeginningWithTheFormToMatchAsWellAsTheNextClosestMatch()
		{
			using (var env = new TestEnvironment())
			{
				LexEntry lexEntryWithMatchingDistance0 = env.Repository.CreateItem();
				lexEntryWithMatchingDistance0.LexicalForm.SetAlternative("de", "de Word1");
				LexEntry lexEntryWithMatchingDistance1 = env.Repository.CreateItem();
				lexEntryWithMatchingDistance1.LexicalForm.SetAlternative("de", "fe");
				LexEntry lexEntryWithMatchingDistance2 = env.Repository.CreateItem();
				lexEntryWithMatchingDistance2.LexicalForm.SetAlternative("de", "ft");
				var ws = WritingSystemDefinitionForTest("de");
				var matches = env.Repository.GetEntriesWithSimilarLexicalForm(
					"de",
					ws,
					ApproximateMatcherOptions.IncludePrefixedAndNextClosestForms);
				Assert.AreEqual(2, matches.Count);
				Assert.AreEqual("de Word1", matches[0]["Form"]);
				Assert.AreEqual("fe", matches[1]["Form"]);
			}
		}

		[Test]
		public void GetEntriesWithSimilarLexicalForm_MultipleEntriesWithDifferentMatchingDistanceAndWeWantTheTwoClosestForms_ReturnsTwoEntriesWithLowestMatchingDistance()
		{
			using (var env = new TestEnvironment())
			{
				//Matching distance as compared to Empty string
				LexEntry lexEntryWithMatchingDistance1 = env.Repository.CreateItem();
				lexEntryWithMatchingDistance1.LexicalForm.SetAlternative("de", "d");
				LexEntry lexEntryWithMatchingDistance2 = env.Repository.CreateItem();
				lexEntryWithMatchingDistance2.LexicalForm.SetAlternative("de", "de");
				LexEntry lexEntryWithMatchingDistance3 = env.Repository.CreateItem();
				lexEntryWithMatchingDistance3.LexicalForm.SetAlternative("de", "de_");
				var ws = WritingSystemDefinitionForTest("de");
				var matches = env.Repository.GetEntriesWithSimilarLexicalForm(
					"",
					ws,
					ApproximateMatcherOptions.IncludeNextClosestForms);
				Assert.AreEqual(2, matches.Count);
				Assert.AreEqual("d", matches[0]["Form"]);
				Assert.AreEqual("de", matches[1]["Form"]);
			}
		}

		[Test]
		public void GetEntriesWithSimilarLexicalForm_EntriesWithDifferingWritingSystems_OnlyFindEntriesMatchingTheGivenWritingSystem()
		{
			using (var env = new TestEnvironment())
			{
				env.CreateThreeDifferentLexEntries(delegate(LexEntry e) { return e.LexicalForm; });
				LexEntry lexEntryWithFrenchLexicalForm = env.Repository.CreateItem();
				lexEntryWithFrenchLexicalForm.LexicalForm.SetAlternative("fr", "de Word2");
				var ws = WritingSystemDefinitionForTest("de");

				var matches = env.Repository.GetEntriesWithSimilarLexicalForm(
					"de Wor",
					ws,
					ApproximateMatcherOptions.IncludePrefixedForms);
				Assert.AreEqual(3, matches.Count);
				Assert.AreEqual("de Word1", matches[0]["Form"]);
				Assert.AreEqual("de Word2", matches[1]["Form"]);
				Assert.AreEqual("de Word3", matches[2]["Form"]);
			}
		}

		[Test]
		public void GetEntriesWithMatchingLexicalForm_RepositoryContainsTwoEntriesWithDifferingLexicalForms_OnlyEntryWithMatchingLexicalFormIsFound()
		{
			using (var env = new TestEnvironment())
			{
				LexEntry entryToFind = env.Repository.CreateItem();
				entryToFind.LexicalForm["en"] = "find me";
				env.Repository.SaveItem(entryToFind);
				//don't want to find this one
				LexEntry entryToIgnore = env.Repository.CreateItem();
				entryToIgnore.LexicalForm["en"] = "don't find me";
				env.Repository.SaveItem(entryToIgnore);
				var writingSystem = WritingSystemDefinitionForTest("en");
				var list = env.Repository.GetEntriesWithMatchingLexicalForm("find me", writingSystem);
				Assert.AreEqual(1, list.Count);
			}
		}

		[Test]
		public void GetEntriesWithMatchingLexicalForm_WritingSystemNull_Throws()
		{
			using (var env = new TestEnvironment())
			{
				WritingSystemDefinition lexicalFormWritingSystem = null;
				Assert.Throws<ArgumentNullException>(() =>
					env.Repository.GetEntriesWithMatchingLexicalForm("de Word1", lexicalFormWritingSystem));
			}
		}

		[Test]
		public void GetEntriesWithMatchingLexicalForm_MultipleMatchingEntries_ReturnsMatchingEntries()
		{
			using (var env = new TestEnvironment())
			{
				env.CreateLexEntryWithLexicalForm("de Word1");
				env.CreateLexEntryWithLexicalForm("de Word1");
				var lexicalFormWritingSystem = WritingSystemDefinitionForTest("de");
				var matches = env.Repository.GetEntriesWithMatchingLexicalForm("de Word1", lexicalFormWritingSystem);
				Assert.AreEqual(2, matches.Count);
			}
		}

		[Test]
		public void GetEntriesWithMatchingLexicalForm_NoMatchingEntries_ReturnsEmpty()
		{
			using (var env = new TestEnvironment())
			{
				env.CreateLexEntryWithLexicalForm("de Word1");
				env.CreateLexEntryWithLexicalForm("de Word1");
				var lexicalFormWritingSystem = WritingSystemDefinitionForTest("de");
				var matches = env.Repository.GetEntriesWithMatchingLexicalForm("de Word2", lexicalFormWritingSystem);
				Assert.AreEqual(0, matches.Count);
			}
		}

		[Test]
		public void GetEntriesWithMatchingLexicalForm_NoMatchesInWritingSystem_ReturnsEmpty()
		{
			using (var env = new TestEnvironment())
			{
				env.CreateLexEntryWithLexicalForm("de Word1");
				env.CreateLexEntryWithLexicalForm("de Word1");
				var lexicalFormWritingSystem = WritingSystemDefinitionForTest("fr");
				var matches = env.Repository.GetEntriesWithMatchingLexicalForm("de Word2", lexicalFormWritingSystem);
				Assert.AreEqual(0, matches.Count);
			}
		}

		[Test]
		public void GetLexEntryWithMatchingGuid_GuidIsEmpty_Throws()
		{
			using (var env = new TestEnvironment())
			{
				Assert.Throws<ArgumentOutOfRangeException>(() =>
					env.Repository.GetLexEntryWithMatchingGuid(Guid.Empty));
			}
		}

		[Test]
		public void GetLexEntryWithMatchingGuid_GuidExists_ReturnsEntryWithGuid()
		{
			using (var env = new TestEnvironment())
			{
				LexEntry lexEntryWithGuid = env.Repository.CreateItem();
				Guid guidToFind = Guid.NewGuid();
				lexEntryWithGuid.Guid = guidToFind;
				LexEntry found = env.Repository.GetLexEntryWithMatchingGuid(guidToFind);
				Assert.AreSame(lexEntryWithGuid, found);
			}
		}

		[Test]
		public void GetLexEntryWithMatchingGuid_GuidDoesNotExist_ReturnsNull()
		{
			using (var env = new TestEnvironment())
			{
				LexEntry found = env.Repository.GetLexEntryWithMatchingGuid(Guid.NewGuid());
				Assert.IsNull(found);
			}
		}

		[Test]
		public void GetLexEntryWithMatchingGuid_MultipleGuidMatchesInRepo_Throws()
		{
			using (var env = new TestEnvironment())
			{
				LexEntry lexEntryWithGuid = env.Repository.CreateItem();
				Guid guidToFind = Guid.NewGuid();
				lexEntryWithGuid.Guid = guidToFind;
				LexEntry lexEntryWithConflictingGuid = env.Repository.CreateItem();
				lexEntryWithConflictingGuid.Guid = guidToFind;
				Assert.Throws<ApplicationException>(() =>
					env.Repository.GetLexEntryWithMatchingGuid(guidToFind));
			}
		}

		[Test]
		public void GetLexEntryWithMatchingId_IdIsEmpty_Throws()
		{
			using (var env = new TestEnvironment())
			{
				Assert.Throws<ArgumentOutOfRangeException>(() =>
					env.Repository.GetLexEntryWithMatchingId(""));
			}
		}

		[Test]
		public void GetLexEntryWithMatchingId_IdExists_ReturnsEntryWithId()
		{
			using (var env = new TestEnvironment())
			{
				LexEntry lexEntryWithId = env.Repository.CreateItem();
				string idToFind = "This is the id.";
				lexEntryWithId.Id = idToFind;
				LexEntry found = env.Repository.GetLexEntryWithMatchingId(idToFind);
				Assert.AreSame(lexEntryWithId, found);
			}
		}

		[Test]
		public void GetLexEntryWithMatchingId_IdDoesNotExist_ReturnsNull()
		{
			using (var env = new TestEnvironment())
			{
				LexEntry found = env.Repository.GetLexEntryWithMatchingId("This is a nonexistent Id.");
				Assert.IsNull(found);
			}
		}

		[Test]
		public void GetLexEntryWithMatchingId_MultipleIdMatchesInRepo_Throws()
		{
			using (var env = new TestEnvironment())
			{
				LexEntry lexEntryWithId = env.Repository.CreateItem();
				string idToFind = "This is an id";
				lexEntryWithId.Id = idToFind;
				LexEntry lexEntryWithConflictingId = env.Repository.CreateItem();
				lexEntryWithConflictingId.Id = idToFind;
				Assert.Throws<ApplicationException>(() =>
					env.Repository.GetLexEntryWithMatchingId(idToFind));
			}
		}

		[Test]
		public void GetEntriesWithMatchingGlossSortedByLexicalForm_WritingSystemNull_Throws()
		{
			using (var env = new TestEnvironment())
			{
				var writingSystem = WritingSystemDefinitionForTest("en");
				Assert.Throws<ArgumentNullException>(() =>
					env.Repository.GetEntriesWithMatchingGlossSortedByLexicalForm(null, writingSystem));
			}
		}

		[Test]
		public void GetEntriesWithMatchingGlossSortedByLexicalForm_LanguageFormNull_Throws()
		{
			using (var env = new TestEnvironment())
			{
				var glossLanguageForm = new LanguageForm("en", "en Gloss", new MultiText());
				Assert.Throws<ArgumentNullException>(() =>
					env.Repository.GetEntriesWithMatchingGlossSortedByLexicalForm(
						glossLanguageForm, null
						));
			}
		}

		[Test]
		public void GetEntriesWithMatchingGlossSortedByLexicalForm_TwoEntriesWithDifferingGlosses_OnlyEntryWithMatchingGlossIsFound()
		{
			using (var env = new TestEnvironment())
			{
				const string glossToFind = "Gloss To Find.";
				env.AddEntryWithGloss(glossToFind);
				env.AddEntryWithGloss("Gloss Not To Find.");
				LanguageForm glossLanguageForm = new LanguageForm("en", glossToFind, new MultiText());
				var writingSystem = WritingSystemDefinitionForTest("en");
				var list = env.Repository.GetEntriesWithMatchingGlossSortedByLexicalForm(
					glossLanguageForm, writingSystem
					);
				Assert.AreEqual(1, list.Count);
				Assert.AreSame(glossToFind, list[0].RealObject.Senses[0].Gloss["en"]);
			}
		}

		[Test]
		public void GetEntriesWithMatchingGlossSortedByLexicalForm_GlossDoesNotExist_ReturnsEmpty()
		{
			using (var env = new TestEnvironment())
			{
				var ws = WritingSystemDefinitionForTest("en");
				LanguageForm glossThatDoesNotExist = new LanguageForm("en", "I don't exist!", new MultiText());
				var matches = env.Repository.GetEntriesWithMatchingGlossSortedByLexicalForm(glossThatDoesNotExist, ws);
				Assert.AreEqual(0, matches.Count);
			}
		}

		[Test]
		public void GetEntriesWithMatchingGlossSortedByLexicalForm_TwoEntriesWithSameGlossButDifferentLexicalForms_ReturnsListSortedByLexicalForm()
		{
			using (var env = new TestEnvironment())
			{
				LanguageForm glossToMatch = new LanguageForm("de", "de Gloss", new MultiText());
				env.CreateEntryWithLexicalFormAndGloss(glossToMatch, "en", "en LexicalForm2");
				env.CreateEntryWithLexicalFormAndGloss(glossToMatch, "en", "en LexicalForm1");
				var lexicalFormWritingSystem = WritingSystemDefinitionForTest("en");
				var matches = env.Repository.GetEntriesWithMatchingGlossSortedByLexicalForm(glossToMatch, lexicalFormWritingSystem);
				Assert.AreEqual("en LexicalForm1", matches[0]["Form"]);
				Assert.AreEqual("en LexicalForm2", matches[1]["Form"]);
			}
		}

		[Test]
		public void GetEntriesWithMatchingGlossSortedByLexicalForm_EntryHasNoLexicalFormInWritingSystem_ReturnsNullForThatEntry()
		{
			using (var env = new TestEnvironment())
			{
				LanguageForm glossToMatch = new LanguageForm("de", "de Gloss", new MultiText());
				env.CreateEntryWithLexicalFormAndGloss(glossToMatch, "en", "en LexicalForm2");
				var lexicalFormWritingSystem = WritingSystemDefinitionForTest("fr");
				var matches = env.Repository.GetEntriesWithMatchingGlossSortedByLexicalForm(glossToMatch, lexicalFormWritingSystem);
				Assert.AreEqual(null, matches[0]["Form"]);
			}
		}

		[Test]
		public void GetEntriesWithMatchingGlossSortedByLexicalForm_EntryHasIdenticalSenses_ReturnsBoth()
		{
			using (var env = new TestEnvironment())
			{
				LanguageForm identicalGloss = new LanguageForm("de", "de Gloss", new MultiText());

				LexEntry entryWithGlossAndLexicalForm = env.Repository.CreateItem();
				entryWithGlossAndLexicalForm.LexicalForm.SetAlternative("en", "en Word1");
				entryWithGlossAndLexicalForm.Senses.Add(new LexSense());
				entryWithGlossAndLexicalForm.Senses[0].Gloss.SetAlternative(identicalGloss.WritingSystemId, identicalGloss.Form);
				entryWithGlossAndLexicalForm.Senses.Add(new LexSense());
				entryWithGlossAndLexicalForm.Senses[1].Gloss.SetAlternative(identicalGloss.WritingSystemId, identicalGloss.Form);

				var lexicalFormWritingSystem = WritingSystemDefinitionForTest("en");
				var matches = env.Repository.GetEntriesWithMatchingGlossSortedByLexicalForm(identicalGloss, lexicalFormWritingSystem);
				Assert.AreEqual(2, matches.Count);
				Assert.AreEqual("en Word1", matches[0]["Form"]);
				Assert.AreEqual("en Word1", matches[1]["Form"]);
			}
		}

		[Test]
		public void GetHomographNumber_OnlyOneEntry_Returns0()
		{
			using (var env = new TestEnvironment())
			{
				LexEntry entry1 = env.MakeEntryWithLexemeForm(env.HeadwordWritingSystem.LanguageTag, "blue");
				Assert.AreEqual(0,
					env.Repository.GetHomographNumber(entry1, env.HeadwordWritingSystem));
			}
		}

		[Test]
		public void GetHomographNumber_FirstEntryWithFollowingHomograph_Returns1()
		{
			using (var env = new TestEnvironment())
			{
				LexEntry entry1 = env.MakeEntryWithLexemeForm(env.HeadwordWritingSystem.LanguageTag, "blue");
				env.MakeEntryWithLexemeForm(env.HeadwordWritingSystem.LanguageTag, "blue");
				Assert.AreEqual(1,
					env.Repository.GetHomographNumber(entry1, env.HeadwordWritingSystem));
			}
		}

		[Test]
		public void GetHomographNumber_SecondEntry_Returns2()
		{
			using (var env = new TestEnvironment())
			{
				env.MakeEntryWithLexemeForm(env.HeadwordWritingSystem.LanguageTag, "blue");
				LexEntry entry2 = env.MakeEntryWithLexemeForm(env.HeadwordWritingSystem.LanguageTag, "blue");
				Assert.AreEqual(2,
					env.Repository.GetHomographNumber(entry2, env.HeadwordWritingSystem));
			}
		}

		[Test]
		public void GetHomographNumber_AssignsUniqueNumbers()
		{
			using (var env = new TestEnvironment())
			{
				env.MakeEntryWithLexemeForm("en", "blue");
				Assert.AreNotEqual("en", env.HeadwordWritingSystem.LanguageTag);
				LexEntry[] entries = new LexEntry[3];
				entries[0] = env.MakeEntryWithLexemeForm(env.HeadwordWritingSystem.LanguageTag, "blue");
				entries[1] = env.MakeEntryWithLexemeForm(env.HeadwordWritingSystem.LanguageTag, "blue");
				entries[2] = env.MakeEntryWithLexemeForm(env.HeadwordWritingSystem.LanguageTag, "blue");
				List<int> ids = new List<int>(entries.Length);
				foreach (LexEntry entry in entries)
				{
					int homographNumber = env.Repository.GetHomographNumber(entry,
						env.HeadwordWritingSystem);
					Assert.IsFalse(ids.Contains(homographNumber));
					ids.Add(homographNumber);
				}
			}
		}

		[Test]
		public void GetHomographNumber_ThirdEntry_Returns3()
		{
			using (var env = new TestEnvironment())
			{
				env.MakeEntryWithLexemeForm("en", "blue");
				Assert.AreNotEqual("en", env.HeadwordWritingSystem.LanguageTag);
				LexEntry entry1 = env.MakeEntryWithLexemeForm(env.HeadwordWritingSystem.LanguageTag, "blue");
				LexEntry entry2 = env.MakeEntryWithLexemeForm(env.HeadwordWritingSystem.LanguageTag, "blue");
				LexEntry entry3 = env.MakeEntryWithLexemeForm(env.HeadwordWritingSystem.LanguageTag, "blue");
				Assert.AreEqual(3,
					env.Repository.GetHomographNumber(entry3, env.HeadwordWritingSystem));
				Assert.AreEqual(2,
					env.Repository.GetHomographNumber(entry2, env.HeadwordWritingSystem));
				Assert.AreEqual(1,
					env.Repository.GetHomographNumber(entry1, env.HeadwordWritingSystem));
			}
		}

		[Test]
		public void GetHomographNumber_3SameLexicalForms_Returns123()
		{
			using (var env = new TestEnvironment())
			{
				LexEntry entry1 = env.MakeEntryWithLexemeForm(env.HeadwordWritingSystem.LanguageTag, "blue");
				LexEntry entry2 = env.MakeEntryWithLexemeForm(env.HeadwordWritingSystem.LanguageTag, "blue");
				LexEntry entry3 = env.MakeEntryWithLexemeForm(env.HeadwordWritingSystem.LanguageTag, "blue");
				Assert.AreEqual(1,
					env.Repository.GetHomographNumber(entry1, env.HeadwordWritingSystem));
				Assert.AreEqual(3,
					env.Repository.GetHomographNumber(entry3, env.HeadwordWritingSystem));
				Assert.AreEqual(2,
					env.Repository.GetHomographNumber(entry2, env.HeadwordWritingSystem));
			}
		}

		[Test]
		public void GetHomographNumber_3SameLexicalFormsAnd3OtherLexicalForms_Returns123()
		{
			using (var env = new TestEnvironment())
			{
				LexEntry red1 = env.MakeEntryWithLexemeForm(env.HeadwordWritingSystem.LanguageTag, "red");
				LexEntry blue1 = env.MakeEntryWithLexemeForm(env.HeadwordWritingSystem.LanguageTag, "blue");
				LexEntry red2 = env.MakeEntryWithLexemeForm(env.HeadwordWritingSystem.LanguageTag, "red");
				LexEntry blue2 = env.MakeEntryWithLexemeForm(env.HeadwordWritingSystem.LanguageTag, "blue");
				LexEntry red3 = env.MakeEntryWithLexemeForm(env.HeadwordWritingSystem.LanguageTag, "red");
				LexEntry blue3 = env.MakeEntryWithLexemeForm(env.HeadwordWritingSystem.LanguageTag, "blue");
				Assert.AreEqual(1, env.Repository.GetHomographNumber(blue1, env.HeadwordWritingSystem));
				Assert.AreEqual(3, env.Repository.GetHomographNumber(blue3, env.HeadwordWritingSystem));
				Assert.AreEqual(2, env.Repository.GetHomographNumber(blue2, env.HeadwordWritingSystem));
				Assert.AreEqual(1, env.Repository.GetHomographNumber(red1, env.HeadwordWritingSystem));
				Assert.AreEqual(3, env.Repository.GetHomographNumber(red3, env.HeadwordWritingSystem));
				Assert.AreEqual(2, env.Repository.GetHomographNumber(red2, env.HeadwordWritingSystem));
			}
		}

		[Test]
		[Ignore("not implemented")]
		public void GetHomographNumber_HonorsOrderAttribute() { }

		[Test]
		public void GetAllEntriesSortedByHeadword_3EntriesWithLexemeForms_TokensAreSorted()
		{
			using (var env = new TestEnvironment())
			{
				LexEntry e1 = env.Repository.CreateItem();
				e1.LexicalForm.SetAlternative(env.HeadwordWritingSystem.LanguageTag, "bank");
				env.Repository.SaveItem(e1);
				RepositoryId bankId = env.Repository.GetId(e1);

				LexEntry e2 = env.Repository.CreateItem();
				e2.LexicalForm.SetAlternative(env.HeadwordWritingSystem.LanguageTag, "apple");
				env.Repository.SaveItem(e2);
				RepositoryId appleId = env.Repository.GetId(e2);

				LexEntry e3 = env.Repository.CreateItem();
				e3.LexicalForm.SetAlternative(env.HeadwordWritingSystem.LanguageTag, "xa");
				//has to be something low in the alphabet to test a bug we had
				env.Repository.SaveItem(e3);
				RepositoryId xaId = env.Repository.GetId(e3);

				ResultSet<LexEntry> list =
					env.Repository.GetAllEntriesSortedByHeadword(env.HeadwordWritingSystem);

				Assert.AreEqual(3, list.Count);
				Assert.AreEqual(appleId, list[0].Id);
				Assert.AreEqual(bankId, list[1].Id);
				Assert.AreEqual(xaId, list[2].Id);
			}
		}
	}
}
