using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using SIL.Keyboarding;

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
			Assert.That(_writingSystemConflatedEventArgs.OldID, Is.EqualTo("en"));
			Assert.That(_writingSystemConflatedEventArgs.NewID, Is.EqualTo("de"));
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
			RepositoryUnderTest.CreateNew();
			Assert.AreEqual(0, RepositoryUnderTest.Count);
		}

		[Test]
		public void SetDefinitionTwice_OnlySetOnce()
		{
			_writingSystem.Language = "one";
			RepositoryUnderTest.Set(_writingSystem);
			Assert.AreEqual(1, RepositoryUnderTest.Count);
			var ws = new WritingSystemDefinition {StoreID = _writingSystem.StoreID};
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
			Assert.AreEqual("de", ws.ID);
		}

		[Test]
		public void CreateNewDefinitionThenSet_CountEquals1()
		{
			RepositoryUnderTest.Set(RepositoryUnderTest.CreateNew());
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
			RepositoryUnderTest.Remove(_writingSystem.ID);
			Assert.AreEqual(0, RepositoryUnderTest.Count);
		}

		[Test]
		public void NewerThanEmpty_ReturnsNoneNewer()
		{
			var ws1 = new WritingSystemDefinition {Language = "en"};
			RepositoryUnderTest.Set(ws1);

			IWritingSystemRepository repository = CreateNewStore();
			int count = repository.WritingSystemsNewerIn(RepositoryUnderTest.AllWritingSystems).Count();
			Assert.AreEqual(0, count);
		}

		[Test]
		public void NewerThanOlder_ReturnsOneNewer()
		{
			var ws1 = new WritingSystemDefinition {Language = "en", DateModified = new DateTime(2008, 1, 15)};
			RepositoryUnderTest.Set(ws1);

			IWritingSystemRepository repository = CreateNewStore();
			var ws2 = RepositoryUnderTest.MakeDuplicate(ws1);
			ws2.DateModified = new DateTime(2008, 1, 14);
			repository.Set(ws2);

			int count = repository.WritingSystemsNewerIn(RepositoryUnderTest.AllWritingSystems).Count();
			Assert.AreEqual(1, count);
		}

		[Test]
		public void NewerThanNewer_ReturnsNoneNewer()
		{
			var ws1 = new WritingSystemDefinition {Language = "en", DateModified = new DateTime(2008, 1, 15)};
			RepositoryUnderTest.Set(ws1);

			IWritingSystemRepository repository = CreateNewStore();
			var ws2 = RepositoryUnderTest.MakeDuplicate(ws1);
			ws2.DateModified = new DateTime(2008, 1, 16);
			repository.Set(ws2);

			int count = repository.WritingSystemsNewerIn(RepositoryUnderTest.AllWritingSystems).Count();
			Assert.AreEqual(0, count);
		}

		[Test]
		public void NewerThanCheckedAlready_ReturnsNoneNewer()
		{
			var ws1 = new WritingSystemDefinition {Language = "en", DateModified = new DateTime(2008, 1, 15)};
			RepositoryUnderTest.Set(ws1);

			IWritingSystemRepository repository = CreateNewStore();
			var ws2 = RepositoryUnderTest.MakeDuplicate(ws1);
			ws2.DateModified = new DateTime(2008, 1, 14);
			repository.Set(ws2);
			repository.LastChecked("en", new DateTime(2008, 1, 16));

			int count = repository.WritingSystemsNewerIn(RepositoryUnderTest.AllWritingSystems).Count();
			Assert.AreEqual(0, count);
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
		public void GetNewStoreIDWhenSet_Null_Throws()
		{
			Assert.Throws<ArgumentNullException>(
				() => RepositoryUnderTest.GetNewStoreIDWhenSet(null)
			);
		}

		[Test]
		public void GetNewStoreIDWhenSet_NewWritingSystem_ReturnsSameStoreIDAsSet()
		{
			var ws = new WritingSystemDefinition("de");
			string newID = RepositoryUnderTest.GetNewStoreIDWhenSet(ws);
			RepositoryUnderTest.Set(ws);
			Assert.AreEqual(ws.StoreID, newID);
		}

		[Test]
		public void GetNewStoreIDWhenSet_WritingSystemIsAlreadyRegisteredWithRepo_ReturnsSameStoreIDAsSet()
		{
			var ws = new WritingSystemDefinition("de");
			RepositoryUnderTest.Set(ws);
			ws.Language = "en";
			string newID = RepositoryUnderTest.GetNewStoreIDWhenSet(ws);
			Assert.AreEqual(ws.StoreID, newID);
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
			_writingSystem = RepositoryUnderTest.CreateNew();
			Assert.IsFalse(RepositoryUnderTest.CanSet(_writingSystem));
		}

		[Test]
		public void CanSetNull_False()
		{
			Assert.IsFalse(RepositoryUnderTest.CanSet(null));
		}

		[Test]
		public void MakeDuplicate_ReturnsNewObject()
		{
			RepositoryUnderTest.Set(_writingSystem);
			var ws2 = RepositoryUnderTest.MakeDuplicate(_writingSystem);
			Assert.AreNotSame(_writingSystem, ws2);
		}

		[Test]
		public void MakeDuplicate_FieldsAreEqual()
		{
			var ws1 = new WritingSystemDefinition("en", "Zxxx", "US", "x-audio", "abbrev", false)
			{
				Keyboard = "keyboard",
				VersionDescription = "description of this version",
				VersionNumber = "1.0"
			};
			RepositoryUnderTest.Set(ws1);
			WritingSystemDefinition ws2 = RepositoryUnderTest.MakeDuplicate(ws1);
			Assert.AreEqual(ws1.Language, ws2.Language);
			Assert.AreEqual(ws1.Script, ws2.Script);
			Assert.AreEqual(ws1.Region, ws2.Region);
			Assert.That(ws1.Variants, Is.EqualTo(ws2.Variants));
			Assert.AreEqual(ws1.Abbreviation, ws2.Abbreviation);
			Assert.AreEqual(ws1.RightToLeftScript, ws2.RightToLeftScript);
			Assert.AreEqual(ws1.Keyboard, ws2.Keyboard);
			Assert.AreEqual(ws1.VersionDescription, ws2.VersionDescription);
			Assert.AreEqual(ws1.VersionNumber, ws2.VersionNumber);
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
		public void NewerInNull_Throws()
		{
			Assert.Throws<ArgumentNullException>(
				() => RepositoryUnderTest.WritingSystemsNewerIn(null)
			);
		}

		[Test]
		public void NewerInNullDefinition_Throws()
		{
			var list = new WritingSystemDefinition[] {null};
			Assert.Throws<ArgumentNullException>(
				() => RepositoryUnderTest.WritingSystemsNewerIn(list)
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
		public void MakeDuplicateNull_Throws()
		{
			Assert.Throws<ArgumentNullException>(
				() => RepositoryUnderTest.MakeDuplicate(null)
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
		public void OnWritingSystemIDChange_DifferentId_OldIdIsRemoved()
		{
			var ws = new WritingSystemDefinition("fr");
			RepositoryUnderTest.Set(ws);
			Assert.IsTrue(RepositoryUnderTest.Contains("fr"));
			ws.Language = "th";
			ws.Script = "Thai";
			RepositoryUnderTest.OnWritingSystemIDChange(ws, "fr");
			Assert.IsFalse(RepositoryUnderTest.Contains("fr"));
			Assert.IsTrue(RepositoryUnderTest.Contains("th"));
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
			Assert.IsTrue(RepositoryUnderTest.VoiceWritingSystems.Count() == 1);
		}

		[Test]
		public void TextWritingSystems_HasAllWritingSystems_ReturnsTextWritingSystems()
		{
			var ws1 = new WritingSystemDefinition("fr");
			ws1.IsVoice = true;
			RepositoryUnderTest.Set(ws1);
			RepositoryUnderTest.Set(new WritingSystemDefinition("de"));
			RepositoryUnderTest.Set(new WritingSystemDefinition("es"));
			Assert.IsTrue(RepositoryUnderTest.TextWritingSystems.Count() == 2);
		}

		[Test]
		public void TextWritingSystems_IdIsNotText_ReturnsEmpty()
		{
			var ws = new WritingSystemDefinition("en");
			ws.IsVoice = true;
			RepositoryUnderTest.Set(ws);
			var wsIdsToFilter = new List<string> {ws.ID};
			var textIds = new List<string> (RepositoryUnderTest.FilterForTextIDs(wsIdsToFilter));
			Assert.IsEmpty(textIds);
		}

		[Test]
		public void TextWritingSystems_IdIsText_ReturnsId()
		{
			var ws = new WritingSystemDefinition("en");
			RepositoryUnderTest.Set(ws);
			var wsIdsToFilter = new List<string> {ws.ID};
			var textIds = new List<string>(RepositoryUnderTest.FilterForTextIDs(wsIdsToFilter));
			Assert.AreEqual(1, textIds.Count);
			Assert.AreEqual("en", textIds[0]);
		}

		[Test]
		public void TextWritingSystems_IdsAreMixOfTextAndNotText_ReturnsOnlyTextIds()
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

			var wsIdsToFilter = RepositoryUnderTest.AllWritingSystems.Select(wsinRepo => wsinRepo.ID);

			var textIds = new List<string>(RepositoryUnderTest.FilterForTextIDs(wsIdsToFilter));

			Assert.AreEqual(2, textIds.Count);
			Assert.AreEqual("en", textIds[0]);
			Assert.AreEqual("de", textIds[1]);
		}

		[Test]
		public void FilterForTextIds_PreservesOrderGivenByParameter()
		{
			RepositoryUnderTest.Set(new WritingSystemDefinition("ar"));
			RepositoryUnderTest.Set(new WritingSystemDefinition("en"));
			RepositoryUnderTest.Set(new WritingSystemDefinition("th"));

			var textIds = new List<string>(RepositoryUnderTest.FilterForTextIDs(new []{"en","ar","th"}));

			Assert.AreEqual("en", textIds[0]);
			Assert.AreEqual("ar", textIds[1]);
			Assert.AreEqual("th", textIds[2]);

			 textIds = new List<string>(RepositoryUnderTest.FilterForTextIDs(new[] { "th", "ar", "en" }));

			Assert.AreEqual("th", textIds[0]);
			Assert.AreEqual("ar", textIds[1]);
			Assert.AreEqual("en", textIds[2]);
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
			Assert.That(_writingSystemIdChangedEventArgs.OldID, Is.EqualTo("en"));
			Assert.That(_writingSystemIdChangedEventArgs.NewID, Is.EqualTo("de"));
		}

		[Test]
		public void Set_IdOfNewWritingSystemIsSetToOldStoreIdOfOtherWritingSystem_GetReturnsCorrectWritingSystems()
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
			RepositoryUnderTest.Remove(ws.ID);
			Assert.That(_writingSystemDeletedEventArgs.ID, Is.EqualTo(ws.ID));
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
			Assert.That(RepositoryUnderTest.WritingSystemIDHasChanged("en"), Is.True);
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
			Assert.That(RepositoryUnderTest.WritingSystemIDHasChanged("en"), Is.True);
		}

		[Test]
		[Ignore("WritingSystemIdHasBeenChanged has not been implmented on LdmlInStreamWritingSystemRepository. A copy of this test exists in LdmlInFolderWritingSystemRepositoryTests")]
		public void WritingSystemIdHasBeenChanged_IdExistsAndHasNeverChanged_ReturnsFalse()
		{
			//Add a writing system to the repo
			var ws = new WritingSystemDefinition("en");
			RepositoryUnderTest.Set(ws);
			RepositoryUnderTest.Save();
			Assert.That(RepositoryUnderTest.WritingSystemIDHasChanged("en"), Is.False);
		}

		[Test]
		[Ignore("WritingSystemIdHasChangedTo has not been implmented on LdmlInStreamWritingSystemRepository. A copy of this test exists in LdmlInFolderWritingSystemRepositoryTests")]
		public void WritingSystemIdHasChangedTo_IdNeverExisted_ReturnsNull()
		{
			//Add a writing system to the repo
			Assert.That(RepositoryUnderTest.WritingSystemIDHasChangedTo("en"), Is.Null);
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
			Assert.That(RepositoryUnderTest.WritingSystemIDHasChangedTo("en"), Is.EqualTo("en-x-bogus"));
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
			Assert.That(RepositoryUnderTest.WritingSystemIDHasChangedTo("en"), Is.Null);
		}

		[Test]
		[Ignore("WritingSystemIdHasChangedTo has not been implmented on LdmlInStreamWritingSystemRepository. A copy of this test exists in LdmlInFolderWritingSystemRepositoryTests")]
		public void WritingSystemIdHasChangedTo_IdExistsAndHasNeverChanged_ReturnsId()
		{
			//Add a writing system to the repo
			var ws = new WritingSystemDefinition("en");
			RepositoryUnderTest.Set(ws);
			RepositoryUnderTest.Save();
			Assert.That(RepositoryUnderTest.WritingSystemIDHasChangedTo("en"), Is.EqualTo("en"));
		}

		[Test]
		public void GetWsForInputLanguage_GetsMatchingWsByCulture()
		{
			var wsEn = new WritingSystemDefinition("en");
			RepositoryUnderTest.Set(wsEn);
			var wsFr = new WritingSystemDefinition("fr");
			RepositoryUnderTest.Set(wsFr);
			DefaultKeyboardDefinition kbdEn = CreateKeyboard("English", "en-US");
			wsEn.LocalKeyboard = kbdEn;
			DefaultKeyboardDefinition kbdFr = CreateKeyboard("French", "fr-FR");
			wsFr.LocalKeyboard = kbdFr;

			Assert.That(RepositoryUnderTest.GetWsForInputLanguage("", new CultureInfo("en-US"), wsEn, new[] {wsEn, wsFr}), Is.EqualTo(wsEn));
			Assert.That(RepositoryUnderTest.GetWsForInputLanguage("", new CultureInfo("en-US"), wsFr, new[] { wsEn, wsFr }), Is.EqualTo(wsEn));
			Assert.That(RepositoryUnderTest.GetWsForInputLanguage("", new CultureInfo("en-US"), wsEn, new[] { wsFr, wsEn }), Is.EqualTo(wsEn));
			Assert.That(RepositoryUnderTest.GetWsForInputLanguage("", new CultureInfo("en-US"), wsFr, new[] { wsFr, wsEn }), Is.EqualTo(wsEn));
			Assert.That(RepositoryUnderTest.GetWsForInputLanguage("", new CultureInfo("fr-FR"), wsEn, new[] { wsFr, wsEn }), Is.EqualTo(wsFr));
			Assert.That(RepositoryUnderTest.GetWsForInputLanguage("", new CultureInfo("fr-FR"), wsEn, new[] { wsEn, wsFr }), Is.EqualTo(wsFr));
			Assert.That(RepositoryUnderTest.GetWsForInputLanguage("", new CultureInfo("fr-FR"), null, new[] { wsEn, wsFr }), Is.EqualTo(wsFr));
		}

		[Test]
		public void GetWsForInputLanguage_PrefersCurrentCultureIfTwoMatch()
		{
			var wsEn = new WritingSystemDefinition("en");
			RepositoryUnderTest.Set(wsEn);
			var wsFr = new WritingSystemDefinition("fr");
			RepositoryUnderTest.Set(wsFr);
			DefaultKeyboardDefinition kbdEn = CreateKeyboard("English", "en-US");
			wsEn.LocalKeyboard = kbdEn;
			DefaultKeyboardDefinition kbdFr = CreateKeyboard("French", "en-US");
			wsFr.LocalKeyboard = kbdFr;

			Assert.That(RepositoryUnderTest.GetWsForInputLanguage("", new CultureInfo("en-US"), wsEn, new[] { wsEn, wsFr }), Is.EqualTo(wsEn));
			Assert.That(RepositoryUnderTest.GetWsForInputLanguage("", new CultureInfo("en-US"), wsFr, new[] { wsEn, wsFr }), Is.EqualTo(wsFr));
			Assert.That(RepositoryUnderTest.GetWsForInputLanguage("", new CultureInfo("en-US"), wsEn, new[] { wsFr, wsEn }), Is.EqualTo(wsEn));
			Assert.That(RepositoryUnderTest.GetWsForInputLanguage("", new CultureInfo("en-US"), wsFr, new[] { wsFr, wsEn }), Is.EqualTo(wsFr));
		}

		[Test]
		public void GetWsForInputLanguage_PrefersCurrentLayoutIfTwoMatch()
		{
			var wsEn = new WritingSystemDefinition("en");
			RepositoryUnderTest.Set(wsEn);
			var wsFr = new WritingSystemDefinition("fr");
			RepositoryUnderTest.Set(wsFr);
			DefaultKeyboardDefinition kbdEn = CreateKeyboard("English", "en-US");
			wsEn.LocalKeyboard = kbdEn;
			DefaultKeyboardDefinition kbdFr = CreateKeyboard("English", "fr-US");
			wsFr.LocalKeyboard = kbdFr;

			Assert.That(RepositoryUnderTest.GetWsForInputLanguage("English", new CultureInfo("de-DE"), wsEn, new[] { wsEn, wsFr }), Is.EqualTo(wsEn));
			Assert.That(RepositoryUnderTest.GetWsForInputLanguage("English", new CultureInfo("de-DE"), wsFr, new[] { wsEn, wsFr }), Is.EqualTo(wsFr));
			Assert.That(RepositoryUnderTest.GetWsForInputLanguage("English", new CultureInfo("de-DE"), wsEn, new[] { wsFr, wsEn }), Is.EqualTo(wsEn));
			Assert.That(RepositoryUnderTest.GetWsForInputLanguage("English", new CultureInfo("de-DE"), wsFr, new[] { wsFr, wsEn }), Is.EqualTo(wsFr));
		}

		[Test]
		public void GetWsForInputLanguage_CorrectlyPrioritizesLayoutAndCulture()
		{
			var wsEn = new WritingSystemDefinition("en");
			RepositoryUnderTest.Set(wsEn);
			var wsEnIpa = new WritingSystemDefinition("en-fonipa");
			RepositoryUnderTest.Set(wsEnIpa);
			var wsFr = new WritingSystemDefinition("fr");
			RepositoryUnderTest.Set(wsFr);
			var wsDe = new WritingSystemDefinition("de");
			RepositoryUnderTest.Set(wsDe);
			DefaultKeyboardDefinition kbdEn = CreateKeyboard("English", "en-US");
			wsEn.LocalKeyboard = kbdEn;
			DefaultKeyboardDefinition kbdEnIpa = CreateKeyboard("English-IPA", "en-US");
			wsEnIpa.LocalKeyboard = kbdEnIpa;
			DefaultKeyboardDefinition kbdFr = CreateKeyboard("French", "fr-FR");
			wsFr.LocalKeyboard = kbdFr;
			DefaultKeyboardDefinition kbdDe = CreateKeyboard("English", "de-DE");
			wsDe.LocalKeyboard = kbdDe;

			WritingSystemDefinition[] wss = {wsEn, wsFr, wsDe, wsEnIpa};

			// Exact match selects correct one, even though there are other matches for layout and/or culture
			Assert.That(RepositoryUnderTest.GetWsForInputLanguage("English", new CultureInfo("en-US"), wsFr, wss), Is.EqualTo(wsEn));
			Assert.That(RepositoryUnderTest.GetWsForInputLanguage("English-IPA", new CultureInfo("en-US"), wsEn, wss), Is.EqualTo(wsEnIpa));
			Assert.That(RepositoryUnderTest.GetWsForInputLanguage("French", new CultureInfo("fr-FR"), wsDe, wss), Is.EqualTo(wsFr));
			Assert.That(RepositoryUnderTest.GetWsForInputLanguage("English", new CultureInfo("de-DE"), wsEn, wss), Is.EqualTo(wsDe));

			// If there is no exact match, but there are matches by both layout and culture, we prefer layout (even though there is a
			// culture match for the default WS)
			Assert.That(RepositoryUnderTest.GetWsForInputLanguage("English", new CultureInfo("fr-FR"), wsFr, wss), Is.EqualTo(wsEn)); // first of two equally good matches
		}

		[Test]
		public void GetWsForInputLanguage_PrefersWsCurrentIfEqualMatches()
		{
			var wsEn = new WritingSystemDefinition("en");
			RepositoryUnderTest.Set(wsEn);
			var wsEnUS = new WritingSystemDefinition("en-US");
			RepositoryUnderTest.Set(wsEnUS);
			var wsEnIpa = new WritingSystemDefinition("en-fonipa");
			RepositoryUnderTest.Set(wsEnIpa);
			var wsFr = new WritingSystemDefinition("fr");
			RepositoryUnderTest.Set(wsFr);
			var wsDe = new WritingSystemDefinition("de");
			RepositoryUnderTest.Set(wsDe);
			DefaultKeyboardDefinition kbdEn = CreateKeyboard("English", "en-US");
			wsEn.LocalKeyboard = kbdEn;
			DefaultKeyboardDefinition kbdEnIpa = CreateKeyboard("English-IPA", "en-US");
			wsEnIpa.LocalKeyboard = kbdEnIpa;
			wsEnUS.LocalKeyboard = kbdEn; // exact same keyboard used!
			DefaultKeyboardDefinition kbdFr = CreateKeyboard("French", "fr-FR");
			wsFr.LocalKeyboard = kbdFr;
			DefaultKeyboardDefinition kbdDe = CreateKeyboard("English", "de-DE");
			wsDe.LocalKeyboard = kbdDe;

			WritingSystemDefinition[] wss = {wsEn, wsFr, wsDe, wsEnIpa, wsEnUS};

			// Exact matches
			Assert.That(RepositoryUnderTest.GetWsForInputLanguage("English", new CultureInfo("en-US"), wsFr, wss), Is.EqualTo(wsEn)); // first of 2
			Assert.That(RepositoryUnderTest.GetWsForInputLanguage("English", new CultureInfo("en-US"), wsEn, wss), Is.EqualTo(wsEn)); // prefer default
			Assert.That(RepositoryUnderTest.GetWsForInputLanguage("English", new CultureInfo("en-US"), wsEnUS, wss), Is.EqualTo(wsEnUS)); // prefer default

			// Match on Layout only
			Assert.That(RepositoryUnderTest.GetWsForInputLanguage("English", new CultureInfo("fr-FR"), wsFr, wss), Is.EqualTo(wsEn)); // first of 3
			Assert.That(RepositoryUnderTest.GetWsForInputLanguage("English", new CultureInfo("fr-FR"), wsEn, wss), Is.EqualTo(wsEn)); // prefer default
			Assert.That(RepositoryUnderTest.GetWsForInputLanguage("English", new CultureInfo("fr-FR"), wsEnUS, wss), Is.EqualTo(wsEnUS)); // prefer default
			Assert.That(RepositoryUnderTest.GetWsForInputLanguage("English", new CultureInfo("fr-FR"), wsDe, wss), Is.EqualTo(wsDe)); // prefer default

			// Match on culture only
			Assert.That(RepositoryUnderTest.GetWsForInputLanguage("Nonsence", new CultureInfo("en-US"), wsDe, wss), Is.EqualTo(wsEn)); // first of 3
			Assert.That(RepositoryUnderTest.GetWsForInputLanguage("Nonsence", new CultureInfo("en-US"), wsEn, wss), Is.EqualTo(wsEn)); // prefer default
			Assert.That(RepositoryUnderTest.GetWsForInputLanguage("Nonsence", new CultureInfo("en-US"), wsEnUS, wss), Is.EqualTo(wsEnUS)); // prefer default
			Assert.That(RepositoryUnderTest.GetWsForInputLanguage("Nonsence", new CultureInfo("en-US"), wsEnIpa, wss), Is.EqualTo(wsEnIpa)); // prefer default
		}

		[Test]
		public void GetWsForInputLanguage_ReturnsCurrentIfNoneMatches()
		{
			var wsEn = new WritingSystemDefinition("en");
			RepositoryUnderTest.Set(wsEn);
			var wsFr = new WritingSystemDefinition("fr");
			RepositoryUnderTest.Set(wsFr);
			DefaultKeyboardDefinition kbdEn = CreateKeyboard("English", "en-US");
			wsEn.LocalKeyboard = kbdEn;
			DefaultKeyboardDefinition kbdFr = CreateKeyboard("French", "en-US");
			wsFr.LocalKeyboard = kbdFr;

			Assert.That(RepositoryUnderTest.GetWsForInputLanguage("", new CultureInfo("fr-FR"), wsEn, new[] { wsEn, wsFr }), Is.EqualTo(wsEn));
			Assert.That(RepositoryUnderTest.GetWsForInputLanguage("", new CultureInfo("fr-FR"), wsFr, new[] { wsEn, wsFr }), Is.EqualTo(wsFr));
			Assert.That(RepositoryUnderTest.GetWsForInputLanguage("", new CultureInfo("fr-FR"), wsEn, new[] { wsFr, wsEn }), Is.EqualTo(wsEn));
			Assert.That(RepositoryUnderTest.GetWsForInputLanguage("", new CultureInfo("fr-FR"), wsFr, new[] { wsFr, wsEn }), Is.EqualTo(wsFr));
			Assert.That(RepositoryUnderTest.GetWsForInputLanguage("", new CultureInfo("fr-FR"), null, new[] { wsFr, wsEn }), Is.Null);
		}

		private static DefaultKeyboardDefinition CreateKeyboard(string layout, string locale)
		{
			return new DefaultKeyboardDefinition(string.Format("{1}_{0}", layout, locale), layout, layout, locale, true);
		}
#endif
	}
}
