using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace SIL.WritingSystems.Tests
{
	public abstract class WritingSystemRepositoryTests
	{
		private IWritingSystemRepository _repositoryUnderTest;
		private WritingSystemDefinition _writingSystem;
		private WritingSystemIdChangedEventArgs _writingSystemIdChangedEventArgs;
		private WritingSystemDeletedEventArgs _writingSystemDeletedEventArgs;
		private WritingSystemConflatedEventArgs _writingSystemConflatedEventArgs;

		public IWritingSystemRepository RepositoryUnderTest
		{
			get
			{
				if (_repositoryUnderTest == null)
				{
					throw new InvalidOperationException("RepositoryUnderTest must be set before the tests are run.");
				}
				return _repositoryUnderTest;
			}
			set { _repositoryUnderTest = value; }
		}

		public abstract IWritingSystemRepository CreateNewStore();

		[SetUp]
		public virtual void SetUp()
		{
			_writingSystem = new WritingSystemDefinition();
			RepositoryUnderTest = CreateNewStore();
			_writingSystemIdChangedEventArgs = null;
			_writingSystemDeletedEventArgs = null;
			_writingSystemConflatedEventArgs = null;
		}

		[TearDown]
		public virtual void TearDown() { }

// Disabled because linux nunit-test runner can't handle Tests in abastract base class
// TODO: refactor or fix nunit-runner
#if !MONO
		[Test]
		public void SetTwoDefinitions_CountEquals2()
		{
			_writingSystem.Language = "one";
			RepositoryUnderTest.Set(_writingSystem);
			var ws2 = new WritingSystemDefinition();
			ws2.Language = "two";
			RepositoryUnderTest.Set(ws2);

			Assert.AreEqual(2, RepositoryUnderTest.Count);
		}

		[Test]
		public void Conflate_TwoWritingSystemsOneIsConflatedIntoOther_OneWritingSystemRemains()
		{
			RepositoryUnderTest.Set(new WritingSystemDefinition("en"));
			RepositoryUnderTest.Set(new WritingSystemDefinition("de"));
			RepositoryUnderTest.Conflate("en", "de");
			Assert.That(RepositoryUnderTest.Contains("de"), Is.True);
			Assert.That(RepositoryUnderTest.Contains("en"), Is.False);
		}

		[Test]
		public void Conflate_WritingSystemsIsConflated_FiresWritingSystemsIsConflatedEvent()
		{
			RepositoryUnderTest.WritingSystemConflated += OnWritingSystemConflated;
			RepositoryUnderTest.Set(new WritingSystemDefinition("en"));
			RepositoryUnderTest.Set(new WritingSystemDefinition("de"));
			RepositoryUnderTest.Conflate("en", "de");
			Assert.That(RepositoryUnderTest.Contains("de"), Is.True);
			Assert.That(RepositoryUnderTest.Contains("en"), Is.False);
			Assert.That(_writingSystemConflatedEventArgs, Is.Not.Null);
			Assert.That(_writingSystemConflatedEventArgs.OldId, Is.EqualTo("en"));
			Assert.That(_writingSystemConflatedEventArgs.NewId, Is.EqualTo("de"));
		}

		[Test]
		public void Conflate_WritingSystemsIsConflated_FiresWritingSystemsIsDeletedEventIsNotFired()
		{
			RepositoryUnderTest.WritingSystemDeleted += OnWritingsystemDeleted;
			RepositoryUnderTest.Set(new WritingSystemDefinition("en"));
			RepositoryUnderTest.Set(new WritingSystemDefinition("de"));
			RepositoryUnderTest.Conflate("en", "de");
			Assert.That(RepositoryUnderTest.Contains("de"), Is.True);
			Assert.That(RepositoryUnderTest.Contains("en"), Is.False);
			Assert.That(_writingSystemDeletedEventArgs, Is.Null);
		}

		private void OnWritingSystemConflated(object sender, WritingSystemConflatedEventArgs e)
		{
			_writingSystemConflatedEventArgs = e;
		}

		[Test]
		public void Remove_TwoWritingSystemsOneIsDeleted_OneWritingSystemRemains()
		{
			RepositoryUnderTest.Set(new WritingSystemDefinition("en"));
			RepositoryUnderTest.Set(new WritingSystemDefinition("de"));
			RepositoryUnderTest.Remove("en");
			Assert.That(RepositoryUnderTest.Contains("de"), Is.True);
			Assert.That(RepositoryUnderTest.Contains("en"), Is.False);
		}

		[Test]
		public void CreateNewDefinition_CountEquals0()
		{
			RepositoryUnderTest.WritingSystemFactory.Create();
			Assert.AreEqual(0, RepositoryUnderTest.Count);
		}

		[Test]
		public void SetDefinitionTwice_OnlySetOnce()
		{
			_writingSystem.Language = "one";
			RepositoryUnderTest.Set(_writingSystem);
			Assert.AreEqual(1, RepositoryUnderTest.Count);
			var ws = new WritingSystemDefinition {Id = _writingSystem.Id};
			RepositoryUnderTest.Set(ws);
			Assert.AreEqual(1, RepositoryUnderTest.Count);
		}

		[Test]
		public void Set_ClonedWritingSystemWithChangedId_CanGetWithNewId()
		{
			_writingSystem.Language = "one";
			RepositoryUnderTest.Set(_writingSystem);
			Assert.AreEqual(1, RepositoryUnderTest.Count);
			var ws = new WritingSystemDefinition(_writingSystem);
			ws.Language = "de";
			RepositoryUnderTest.Set(ws);
			Assert.AreEqual(2, RepositoryUnderTest.Count);
			Assert.AreEqual("de", ws.IetfLanguageTag);
		}

		[Test]
		public void CreateNewDefinitionThenSet_CountEquals1()
		{
			RepositoryUnderTest.Set(RepositoryUnderTest.WritingSystemFactory.Create());
			Assert.AreEqual(1, RepositoryUnderTest.Count);
		}

		[Test]
		public void SetSameDefinitionTwice_UpdatesStore()
		{
			_writingSystem.Language = "one";
			RepositoryUnderTest.Set(_writingSystem);
			Assert.AreEqual(1, RepositoryUnderTest.Count);
			Assert.AreNotEqual("lang1", _writingSystem.Language.Name);
			_writingSystem.Language = new LanguageSubtag((LanguageSubtag) "two", "lang1");
			RepositoryUnderTest.Set(_writingSystem);
			WritingSystemDefinition ws2 = RepositoryUnderTest.Get("two");
			Assert.AreEqual("lang1", ws2.Language.Name);
			Assert.AreEqual(1, RepositoryUnderTest.Count);
		}


		/// <summary>
		/// Language tags should be case insensitive, as required by RFC 5646
		/// </summary>
		[Test]
		public void Get_StoredWithUpperCaseButRequestedUsingLowerCase_Finds()
		{
			_writingSystem.Language = "sr";
			_writingSystem.Script = "Latn";
			_writingSystem.Variants.Add("RS");
			RepositoryUnderTest.Set(_writingSystem);
			Assert.IsNotNull(RepositoryUnderTest.Get("sr-Latn-x-rs"));
		}

		/// <summary>
		/// Language tags should be case insensitive, as required by RFC 5646
		/// </summary>
		[Test]
		public void Get_StoredWithLowerCaseButRequestedUsingUpperCase_Finds()
		{

			_writingSystem.Language = "sR";
			_writingSystem.Script = "LaTn";
			_writingSystem.Variants.Add("rs");
			RepositoryUnderTest.Set(_writingSystem);
			Assert.IsNotNull(RepositoryUnderTest.Get("sr-Latn-x-RS"));
		}

		[Test]
		public void Contains_FalseThenTrue()
		{
			Assert.IsFalse(RepositoryUnderTest.Contains("one"));
			_writingSystem.Language = "one";
			RepositoryUnderTest.Set(_writingSystem);
			Assert.IsTrue(RepositoryUnderTest.Contains("one"));
		}

		[Test]
		public void Remove_CountDecreases()
		{
			_writingSystem.Language = "one";
			RepositoryUnderTest.Set(_writingSystem);
			Assert.AreEqual(1, RepositoryUnderTest.Count);
			RepositoryUnderTest.Remove(_writingSystem.IetfLanguageTag);
			Assert.AreEqual(0, RepositoryUnderTest.Count);
		}

		[Test]
		public void CanStoreVariants_CountTwo()
		{
			var ws1 = new WritingSystemDefinition();
			ws1.Language = "en";
			Assert.AreEqual("en", ws1.IetfLanguageTag);
			WritingSystemDefinition ws2 = ws1.Clone();
			ws2.Variants.Add("1901");
			Assert.AreEqual("en-1901", ws2.IetfLanguageTag);

			RepositoryUnderTest.Set(ws1);
			Assert.AreEqual(1, RepositoryUnderTest.Count);
			RepositoryUnderTest.Set(ws2);
			Assert.AreEqual(2, RepositoryUnderTest.Count);
		}

		[Test]
		public void StoreTwoOfSame_Throws()
		{
			var ws1 = new WritingSystemDefinition("en");
			var ws2 = new WritingSystemDefinition("en");
			RepositoryUnderTest.Set(ws1);
			Assert.Throws<ArgumentException>(
				() => RepositoryUnderTest.Set(ws2)
			);
		}

		 [Test]
		public void GetNewIdWhenSet_Null_Throws()
		{
			Assert.Throws<ArgumentNullException>(
				() => RepositoryUnderTest.GetNewIdWhenSet(null)
			);
		}

		[Test]
		public void GetNewIdWhenSet_NewWritingSystem_ReturnsSameIdAsSet()
		{
			var ws = new WritingSystemDefinition("de");
			string newId = RepositoryUnderTest.GetNewIdWhenSet(ws);
			RepositoryUnderTest.Set(ws);
			Assert.AreEqual(ws.Id, newId);
		}

		[Test]
		public void GetNewIdWhenSet_WritingSystemIsAlreadyRegisteredWithRepo_ReturnsSameIdAsSet()
		{
			var ws = new WritingSystemDefinition("de");
			RepositoryUnderTest.Set(ws);
			ws.Language = "en";
			string newId = RepositoryUnderTest.GetNewIdWhenSet(ws);
			Assert.AreEqual(ws.Id, newId);
		}

		[Test]
		public void CanSetAfterSetting_True()
		{
			RepositoryUnderTest.Set(_writingSystem);
			Assert.IsTrue(RepositoryUnderTest.CanSet(_writingSystem));
		}

		[Test]
		public void CanSetNewWritingSystem_True()
		{
			Assert.IsTrue(RepositoryUnderTest.CanSet(_writingSystem));
		}

		[Test]
		public void CanSetSecondNew_False()
		{
			RepositoryUnderTest.Set(_writingSystem);
			_writingSystem = RepositoryUnderTest.WritingSystemFactory.Create();
			Assert.IsFalse(RepositoryUnderTest.CanSet(_writingSystem));
		}

		[Test]
		public void CanSetNull_False()
		{
			Assert.IsFalse(RepositoryUnderTest.CanSet(null));
		}

		[Test]
		public void GetNull_Throws()
		{
			Assert.Throws<ArgumentNullException>(
				() => RepositoryUnderTest.Get(null)
			);
		}

		[Test]
		public void Get_NotInStore_Throws()
		{
			Assert.Throws<ArgumentOutOfRangeException>(
				() => RepositoryUnderTest.Get("I sure hope this isn't in the store.")
			);
		}

		[Test]
		public void SetNull_Throws()
		{
			Assert.Throws<ArgumentNullException>(
				() => RepositoryUnderTest.Set(null)
			);
		}

		[Test]
		public void RemoveNull_Throws()
		{
			Assert.Throws<ArgumentNullException>(
				() => RepositoryUnderTest.Remove(null)
			);
		}

		[Test]
		public void Remove_NotInStore_Throws()
		{
			Assert.Throws<ArgumentOutOfRangeException>(
				() => RepositoryUnderTest.Remove("This isn't in the store!")
			);
		}

		[Test]
		public void AllWritingSystems_HasAllWritingSystems_ReturnsAllWritingSystems()
		{
			var ws1 = new WritingSystemDefinition("fr");
			ws1.IsVoice = true;
			RepositoryUnderTest.Set(ws1);
			RepositoryUnderTest.Set(new WritingSystemDefinition("de"));
			RepositoryUnderTest.Set(new WritingSystemDefinition("es"));
			Assert.IsTrue(RepositoryUnderTest.AllWritingSystems.Count() == 3);
		}

		[Test]
		public void VoiceWritingSystems_HasAllWritingSystems_ReturnsVoiceWritingSystems()
		{
			var ws1 = new WritingSystemDefinition("fr");
			ws1.IsVoice = true;
			RepositoryUnderTest.Set(ws1);
			RepositoryUnderTest.Set(new WritingSystemDefinition("de"));
			RepositoryUnderTest.Set(new WritingSystemDefinition("es"));
			Assert.IsTrue(RepositoryUnderTest.VoiceWritingSystems().Count() == 1);
		}

		[Test]
		public void TextWritingSystems_HasAllWritingSystems_ReturnsTextWritingSystems()
		{
			var ws1 = new WritingSystemDefinition("fr");
			ws1.IsVoice = true;
			RepositoryUnderTest.Set(ws1);
			RepositoryUnderTest.Set(new WritingSystemDefinition("de"));
			RepositoryUnderTest.Set(new WritingSystemDefinition("es"));
			Assert.IsTrue(RepositoryUnderTest.TextWritingSystems().Count() == 2);
		}

		[Test]
		public void TextWritingSystems_IetfLanguageTagIsNotText_ReturnsEmpty()
		{
			var ws = new WritingSystemDefinition("en") {IsVoice = true};
			RepositoryUnderTest.Set(ws);
			Assert.That(RepositoryUnderTest.FilterForTextIetfLanguageTags(new[] {ws.IetfLanguageTag}), Is.Empty);
		}

		[Test]
		public void TextWritingSystems_IetfLanguageTagIsText_ReturnsIetfLanguageTag()
		{
			var ws = new WritingSystemDefinition("en");
			RepositoryUnderTest.Set(ws);
			Assert.That(RepositoryUnderTest.FilterForTextIetfLanguageTags(new[] {ws.IetfLanguageTag}), Is.EqualTo(new[] {"en"}));
		}

		[Test]
		public void TextWritingSystems_IetfLanguageTagsAreMixOfTextAndNotText_ReturnsOnlyTextIetfLanguageTags()
		{
			var ws = new WritingSystemDefinition("en");
			var ws1 = new WritingSystemDefinition("de");
			var ws2 = new WritingSystemDefinition("th");
			ws2.IsVoice = true;
			var ws3 = new WritingSystemDefinition("pt");
			ws3.IsVoice = true;

			RepositoryUnderTest.Set(ws);
			RepositoryUnderTest.Set(ws1);
			RepositoryUnderTest.Set(ws2);
			RepositoryUnderTest.Set(ws3);

			IEnumerable<string> langTagsToFilter = RepositoryUnderTest.AllWritingSystems.Select(wsinRepo => wsinRepo.IetfLanguageTag);
			Assert.That(RepositoryUnderTest.FilterForTextIetfLanguageTags(langTagsToFilter), Is.EqualTo(new[] {"en", "de"}));
		}

		[Test]
		public void FilterForTextIetfLanguageTags_PreservesOrderGivenByParameter()
		{
			RepositoryUnderTest.Set(new WritingSystemDefinition("ar"));
			RepositoryUnderTest.Set(new WritingSystemDefinition("en"));
			RepositoryUnderTest.Set(new WritingSystemDefinition("th"));

			string[] langTags = {"en", "ar", "th"};
			Assert.That(RepositoryUnderTest.FilterForTextIetfLanguageTags(langTags), Is.EqualTo(langTags));

			langTags = new[] {"th", "ar", "en"};
			Assert.That(RepositoryUnderTest.FilterForTextIetfLanguageTags(langTags), Is.EqualTo(langTags));
		}

		private void OnWritingSystemIdChanged(object sender, WritingSystemIdChangedEventArgs e)
		{
			_writingSystemIdChangedEventArgs = e;
		}

		[Test]
		public void Set_IdOfWritingSystemChanged_EventArgsAreCorrect()
		{
			RepositoryUnderTest.WritingSystemIdChanged += OnWritingSystemIdChanged;
			var ws = new WritingSystemDefinition("en");
			RepositoryUnderTest.Set(ws);
			ws.Language = "de";
			RepositoryUnderTest.Set(ws);
			Assert.That(_writingSystemIdChangedEventArgs.OldId, Is.EqualTo("en"));
			Assert.That(_writingSystemIdChangedEventArgs.NewId, Is.EqualTo("de"));
		}

		[Test]
		public void Set_IdOfNewWritingSystemIsSetToOldIdOfOtherWritingSystem_GetReturnsCorrectWritingSystems()
		{
			var ws = new WritingSystemDefinition("en");
			RepositoryUnderTest.Set(ws);
			RepositoryUnderTest.Save();
			ws.Variants.Add("orig");
			RepositoryUnderTest.Set(ws);
			var newWs = new WritingSystemDefinition("en");
			RepositoryUnderTest.Set(newWs);
			Assert.That(RepositoryUnderTest.Get("en-x-orig"), Is.EqualTo(ws));
		}

		[Test]
		public void Set_IdOfWritingSystemIsUnChanged_EventIsNotFired()
		{
			RepositoryUnderTest.WritingSystemIdChanged += OnWritingSystemIdChanged;
			var ws = new WritingSystemDefinition("en");
			RepositoryUnderTest.Set(ws);
			ws.Language = "en";
			RepositoryUnderTest.Set(ws);
			Assert.That(_writingSystemIdChangedEventArgs, Is.Null);
		}

		[Test]
		public void Set_NewWritingSystem_EventIsNotFired()
		{
			RepositoryUnderTest.WritingSystemIdChanged += OnWritingSystemIdChanged;
			var ws = new WritingSystemDefinition("en");
			RepositoryUnderTest.Set(ws);
			Assert.That(_writingSystemIdChangedEventArgs, Is.Null);
		}

		public WritingSystemDeletedEventArgs WritingSystemDeletedEventArgs
		{
			get { return _writingSystemDeletedEventArgs; }
		}

		public void OnWritingsystemDeleted(object sender, WritingSystemDeletedEventArgs args)
		{
			_writingSystemDeletedEventArgs = args;
		}

		[Test]
		public void Remove_WritingsystemIdExists_FiresEventAndEventArgIsSetToIdOfDeletedWritingSystem()
		{
			RepositoryUnderTest.WritingSystemDeleted += OnWritingsystemDeleted;
			var ws = new WritingSystemDefinition("en");
			RepositoryUnderTest.Set(ws);
			RepositoryUnderTest.Remove(ws.IetfLanguageTag);
			Assert.That(_writingSystemDeletedEventArgs.Id, Is.EqualTo(ws.IetfLanguageTag));
		}


		[Test]
		[Ignore("WritingSystemIdHasBeenChanged has not been implmented on LdmlInStreamWritingSystemRepository. A copy of this test exists in LdmlInFolderWritingSystemRepositoryTests")]
		public void WritingSystemIdHasBeenChanged_IdChanged_ReturnsTrue()
		{
			//Add a writing system to the repo
			var ws = new WritingSystemDefinition("en");
			RepositoryUnderTest.Set(ws);
			RepositoryUnderTest.Save();
			//Now change the Id
			ws.Variants.Add("bogus");
			RepositoryUnderTest.Save();
			Assert.That(RepositoryUnderTest.WritingSystemIdHasChanged("en"), Is.True);
		}

		[Test]
		[Ignore("WritingSystemIdHasBeenChanged has not been implmented on LdmlInStreamWritingSystemRepository. A copy of this test exists in LdmlInFolderWritingSystemRepositoryTests")]
		public void WritingSystemIdHasBeenChanged_IdChangedToMultipleDifferentNewIds_ReturnsTrue()
		{
			//Add a writing system to the repo
			var wsEn = new WritingSystemDefinition("en");
			RepositoryUnderTest.Set(wsEn);
			RepositoryUnderTest.Save();
			//Now change the Id and create a duplicate of the original Id
			wsEn.Variants.Add("bogus");
			RepositoryUnderTest.Set(wsEn);
			var wsEnDup = new WritingSystemDefinition("en");
			RepositoryUnderTest.Set(wsEnDup);
			RepositoryUnderTest.Save();
			//Now change the duplicate's Id as well
			wsEnDup.Variants.Add("bogus2");
			RepositoryUnderTest.Set(wsEnDup);
			RepositoryUnderTest.Save();
			Assert.That(RepositoryUnderTest.WritingSystemIdHasChanged("en"), Is.True);
		}

		[Test]
		[Ignore("WritingSystemIdHasBeenChanged has not been implmented on LdmlInStreamWritingSystemRepository. A copy of this test exists in LdmlInFolderWritingSystemRepositoryTests")]
		public void WritingSystemIdHasBeenChanged_IdExistsAndHasNeverChanged_ReturnsFalse()
		{
			//Add a writing system to the repo
			var ws = new WritingSystemDefinition("en");
			RepositoryUnderTest.Set(ws);
			RepositoryUnderTest.Save();
			Assert.That(RepositoryUnderTest.WritingSystemIdHasChanged("en"), Is.False);
		}

		[Test]
		[Ignore("WritingSystemIdHasChangedTo has not been implmented on LdmlInStreamWritingSystemRepository. A copy of this test exists in LdmlInFolderWritingSystemRepositoryTests")]
		public void WritingSystemIdHasChangedTo_IdNeverExisted_ReturnsNull()
		{
			//Add a writing system to the repo
			Assert.That(RepositoryUnderTest.WritingSystemIdHasChangedTo("en"), Is.Null);
		}

		[Test]
		[Ignore("WritingSystemIdHasChangedTo has not been implmented on LdmlInStreamWritingSystemRepository. A copy of this test exists in LdmlInFolderWritingSystemRepositoryTests")]
		public void WritingSystemIdHasChangedTo_IdChanged_ReturnsNewId()
		{
			//Add a writing system to the repo
			var ws = new WritingSystemDefinition("en");
			RepositoryUnderTest.Set(ws);
			RepositoryUnderTest.Save();
			//Now change the Id
			ws.Variants.Add("bogus");
			RepositoryUnderTest.Save();
			Assert.That(RepositoryUnderTest.WritingSystemIdHasChangedTo("en"), Is.EqualTo("en-x-bogus"));
		}

		[Test]
		[Ignore("WritingSystemIdHasChangedTo has not been implmented on LdmlInStreamWritingSystemRepository. A copy of this test exists in LdmlInFolderWritingSystemRepositoryTests")]
		public void WritingSystemIdHasChangedTo_IdChangedToMultipleDifferentNewIds_ReturnsNull()
		{
			//Add a writing system to the repo
			var wsEn = new WritingSystemDefinition("en");
			RepositoryUnderTest.Set(wsEn);
			RepositoryUnderTest.Save();
			//Now change the Id and create a duplicate of the original Id
			wsEn.Variants.Add("bogus");
			RepositoryUnderTest.Set(wsEn);
			var wsEnDup = new WritingSystemDefinition("en");
			RepositoryUnderTest.Set(wsEnDup);
			RepositoryUnderTest.Save();
			//Now change the duplicate's Id as well
			wsEnDup.Variants.Add("bogus2");
			RepositoryUnderTest.Set(wsEnDup);
			RepositoryUnderTest.Save();
			Assert.That(RepositoryUnderTest.WritingSystemIdHasChangedTo("en"), Is.Null);
		}

		[Test]
		[Ignore("WritingSystemIdHasChangedTo has not been implmented on LdmlInStreamWritingSystemRepository. A copy of this test exists in LdmlInFolderWritingSystemRepositoryTests")]
		public void WritingSystemIdHasChangedTo_IdExistsAndHasNeverChanged_ReturnsId()
		{
			//Add a writing system to the repo
			var ws = new WritingSystemDefinition("en");
			RepositoryUnderTest.Set(ws);
			RepositoryUnderTest.Save();
			Assert.That(RepositoryUnderTest.WritingSystemIdHasChangedTo("en"), Is.EqualTo("en"));
		}
#endif
	}
}
