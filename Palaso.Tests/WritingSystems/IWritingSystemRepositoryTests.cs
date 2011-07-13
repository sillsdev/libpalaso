using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Palaso.WritingSystems;

namespace Palaso.Tests.WritingSystems
{
	public abstract class IWritingSystemRepositoryTests
	{
		private IWritingSystemRepository _repositoryUnderTest;
		private WritingSystemDefinition _writingSystem;
		private WritingSystemIdChangedEventArgs _writingSystemIdChangedEventArgs;
		private WritingSystemDeletedEventArgs _writingSystemDeletedEventArgs;

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
		}

		[TearDown]
		public virtual void TearDown() { }

		[Test]
		public void SetTwoDefinitions_CountEquals2()
		{
			_writingSystem.Language = "one";
			RepositoryUnderTest.Set(_writingSystem);
			WritingSystemDefinition ws2 = new WritingSystemDefinition();
			ws2.Language = "two";
			RepositoryUnderTest.Set(ws2);

			Assert.AreEqual(2, RepositoryUnderTest.Count);
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
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.StoreID = _writingSystem.StoreID;
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
			Assert.AreEqual("de", ws.Id);
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
			Assert.AreNotEqual("one font", _writingSystem.DefaultFontName);
			_writingSystem.Language = "two";
			_writingSystem.DefaultFontName = "one font";
			RepositoryUnderTest.Set(_writingSystem);
			WritingSystemDefinition ws2 = RepositoryUnderTest.Get("two");
			Assert.AreEqual("one font", ws2.DefaultFontName);
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
			_writingSystem.Variant = "x-RS";
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
			_writingSystem.Variant = "x-rs";
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
			RepositoryUnderTest.Remove(_writingSystem.Id);
			Assert.AreEqual(0, RepositoryUnderTest.Count);
		}

		[Test]
		public void NewerThanEmpty_ReturnsNoneNewer()
		{
			var ws1 = new WritingSystemDefinition();
			ws1.Language = "en";
			RepositoryUnderTest.Set(ws1);

			IWritingSystemRepository repository = CreateNewStore();
			int count = 0;
			foreach (WritingSystemDefinition ws in repository.WritingSystemsNewerIn(RepositoryUnderTest.WritingSystemDefinitions))
			{
				count++;
			}
			Assert.AreEqual(0, count);
		}

		[Test]
		public void NewerThanOlder_ReturnsOneNewer()
		{
			var ws1 = new WritingSystemDefinition();
			ws1.Language = "en";
			ws1.DateModified = new DateTime(2008, 1, 15);
			RepositoryUnderTest.Set(ws1);

			IWritingSystemRepository repository = CreateNewStore();
			WritingSystemDefinition ws2 = RepositoryUnderTest.MakeDuplicate(ws1);
			ws2.DateModified = new DateTime(2008, 1, 14);
			repository.Set(ws2);

			int count = 0;
			foreach (WritingSystemDefinition ws in repository.WritingSystemsNewerIn(RepositoryUnderTest.WritingSystemDefinitions))
			{
				count++;
			}
			Assert.AreEqual(1, count);
		}

		[Test]
		public void NewerThanNewer_ReturnsNoneNewer()
		{
			var ws1 = new WritingSystemDefinition();
			ws1.Language = "en";
			ws1.DateModified = new DateTime(2008, 1, 15);
			RepositoryUnderTest.Set(ws1);

			IWritingSystemRepository repository = CreateNewStore();
			WritingSystemDefinition ws2 = RepositoryUnderTest.MakeDuplicate(ws1);
			ws2.DateModified = new DateTime(2008, 1, 16);
			repository.Set(ws2);

			int count = 0;
			foreach (WritingSystemDefinition ws in repository.WritingSystemsNewerIn(RepositoryUnderTest.WritingSystemDefinitions))
			{
				count++;
			}
			Assert.AreEqual(0, count);
		}

		[Test]
		public void NewerThanCheckedAlready_ReturnsNoneNewer()
		{
			var ws1 = new WritingSystemDefinition();
			ws1.Language = "en";
			ws1.DateModified = new DateTime(2008, 1, 15);
			RepositoryUnderTest.Set(ws1);

			IWritingSystemRepository repository = CreateNewStore();
			WritingSystemDefinition ws2 = RepositoryUnderTest.MakeDuplicate(ws1);
			ws2.DateModified = new DateTime(2008, 1, 14);
			repository.Set(ws2);
			repository.LastChecked("en", new DateTime(2008, 1, 16));

			int count = 0;
			foreach (var ws in repository.WritingSystemsNewerIn(RepositoryUnderTest.WritingSystemDefinitions))
			{
				count++;
			}
			Assert.AreEqual(0, count);
		}

		[Test]
		public void CanStoreVariants_CountTwo()
		{
			var ws1 = new WritingSystemDefinition();
			ws1.Language = "en";
			Assert.AreEqual("en", ws1.RFC5646);
			WritingSystemDefinition ws2 = ws1.Clone();
			ws2.Variant = "1901";
			Assert.AreEqual("en-1901", ws2.RFC5646);

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
			WritingSystemDefinition ws2 = RepositoryUnderTest.MakeDuplicate(_writingSystem);
			Assert.AreNotSame(_writingSystem, ws2);
		}

		[Test]
		public void MakeDuplicate_FieldsAreEqual()
		{
			WritingSystemDefinition ws1 = new WritingSystemDefinition("en", "Zxxx", "US", "x-audio",
																	  "abbrev", false);
			ws1.Keyboard = "keyboard";
			ws1.NativeName = "native name";
			ws1.DefaultFontName = "font";
			ws1.VersionDescription = "description of this version";
			ws1.VersionNumber = "1.0";
			RepositoryUnderTest.Set(ws1);
			WritingSystemDefinition ws2 = RepositoryUnderTest.MakeDuplicate(ws1);
			Assert.AreEqual(ws1.Language, ws2.Language);
			Assert.AreEqual(ws1.Script, ws2.Script);
			Assert.AreEqual(ws1.Region, ws2.Region);
			Assert.AreEqual(ws1.Variant, ws2.Variant);
			Assert.AreEqual(ws1.LanguageName, ws2.LanguageName);
			Assert.AreEqual(ws1.Abbreviation, ws2.Abbreviation);
			Assert.AreEqual(ws1.RightToLeftScript, ws2.RightToLeftScript);
			Assert.AreEqual(ws1.Keyboard, ws2.Keyboard);
			Assert.AreEqual(ws1.NativeName, ws2.NativeName);
			Assert.AreEqual(ws1.DefaultFontName, ws2.DefaultFontName);
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
			var ws = WritingSystemDefinition.FromLanguage("en");
			ws.IsVoice = true;
			RepositoryUnderTest.Set(ws);
			var wsIdsToFilter = new List<string> {ws.Id};
			var textIds = new List<string> (RepositoryUnderTest.FilterForTextIds(wsIdsToFilter));
			Assert.IsEmpty(textIds);
		}

		[Test]
		public void TextWritingSystems_IdIsText_ReturnsId()
		{
			var ws = WritingSystemDefinition.FromLanguage("en");
			RepositoryUnderTest.Set(ws);
			var wsIdsToFilter = new List<string> { ws.Id };
			var textIds = new List<string>(RepositoryUnderTest.FilterForTextIds(wsIdsToFilter));
			Assert.AreEqual(1, textIds.Count);
			Assert.AreEqual("en", textIds[0]);
		}

		[Test]
		public void TextWritingSystems_IdsAreMixOfTextAndNotText_ReturnsOnlyTextIds()
		{
			var ws = WritingSystemDefinition.FromLanguage("en");
			var ws1 = WritingSystemDefinition.FromLanguage("de");
			var ws2 = WritingSystemDefinition.FromLanguage("th");
			ws2.IsVoice = true;
			var ws3 = WritingSystemDefinition.FromLanguage("pt");
			ws3.IsVoice = true;

			RepositoryUnderTest.Set(ws);
			RepositoryUnderTest.Set(ws1);
			RepositoryUnderTest.Set(ws2);
			RepositoryUnderTest.Set(ws3);

			var wsIdsToFilter = RepositoryUnderTest.AllWritingSystems.Select(wsinRepo => wsinRepo.Id);

			var textIds = new List<string>(RepositoryUnderTest.FilterForTextIds(wsIdsToFilter));

			Assert.AreEqual(2, textIds.Count);
			Assert.AreEqual("en", textIds[0]);
			Assert.AreEqual("de", textIds[1]);
		}

		private void OnWritingSystemIdChanged(object sender, WritingSystemIdChangedEventArgs e)
		{
			_writingSystemIdChangedEventArgs = e;
		}

		[Test]
		public void Set_IdOfWritingSystemChanged_EventArgsAreCorrect()
		{
			RepositoryUnderTest.WritingSystemIdChanged += OnWritingSystemIdChanged;
			var ws = WritingSystemDefinition.FromLanguage("en");
			RepositoryUnderTest.Set(ws);
			ws.Language = "de";
			RepositoryUnderTest.Set(ws);
			Assert.That(_writingSystemIdChangedEventArgs.OldId, Is.EqualTo("en"));
			Assert.That(_writingSystemIdChangedEventArgs.NewId, Is.EqualTo("de"));
		}

		[Test]
		public void Set_IdOfNewWritingSystemIsSetToOldStoreIdOfOtherWritingSystem_GetReturnsCorrectWritingSystems()
		{
			var ws = new WritingSystemDefinition("en");
			RepositoryUnderTest.Set(ws);
			RepositoryUnderTest.Save();
			ws.Variant = "x-orig";
			RepositoryUnderTest.Set(ws);
			var newWs = new WritingSystemDefinition("en");
			RepositoryUnderTest.Set(newWs);
			Assert.That(RepositoryUnderTest.Get("en-x-orig"), Is.EqualTo(ws));
		}

		[Test]
		public void Set_IdOfWritingSystemIsUnChanged_EventIsNotFired()
		{
			RepositoryUnderTest.WritingSystemIdChanged += OnWritingSystemIdChanged;
			var ws = WritingSystemDefinition.FromLanguage("en");
			RepositoryUnderTest.Set(ws);
			ws.Language = "en";
			RepositoryUnderTest.Set(ws);
			Assert.That(_writingSystemIdChangedEventArgs, Is.Null);
		}

		[Test]
		public void Set_NewWritingSystem_EventIsNotFired()
		{
			RepositoryUnderTest.WritingSystemIdChanged += OnWritingSystemIdChanged;
			var ws = WritingSystemDefinition.FromLanguage("en");
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
			var ws = WritingSystemDefinition.FromLanguage("en");
			RepositoryUnderTest.Set(ws);
			RepositoryUnderTest.Remove(ws.Id);
			Assert.That(_writingSystemDeletedEventArgs.Id, Is.EqualTo(ws.Id));
		}
	}
}
