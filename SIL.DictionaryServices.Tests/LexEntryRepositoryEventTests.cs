using NUnit.Framework;
using SIL.DictionaryServices.Model;
using SIL.TestUtilities;

namespace SIL.DictionaryServices.Tests
{
	[TestFixture]
	public class LiftLexEntryRepositoryEventTests
	{
		private bool _gotEventCall;

		[SetUp]
		public void Setup()
		{
			_gotEventCall = false;
		}


		[Test]
		public void NewEntry_ByEntry_TriggersModifiedEntryAdded()
		{
			using (var f = new TemporaryFolder("eventTests"))
			{
				using (var r = new LiftLexEntryRepository(f.GetPathForNewTempFile(true)))
				{
					r.AfterEntryModified += OnEvent;
					LexEntry entry = r.CreateItem();
					r.SaveItem(entry);
					Assert.IsTrue(_gotEventCall);
				}
			}
		}

		[Test]
		public void ModifiedEntry_ByEntry_TriggersModifiedEntryAdded()
		{
			using (TemporaryFolder f = new TemporaryFolder("eventTests"))
			{
				using (LiftLexEntryRepository r = new LiftLexEntryRepository(f.GetPathForNewTempFile(true)))
				{
					LexEntry entry = r.CreateItem();
					r.SaveItem(entry);
					r.AfterEntryModified += OnEvent;
					entry.Senses.Add(new LexSense());
					r.SaveItem(entry);
					Assert.IsTrue(_gotEventCall);
				}
			}
		}

		[Test]
		public void DeleteEntry_ById_TriggersAfterEntryDeleted()
		{
			using (TemporaryFolder f = new TemporaryFolder("eventTests"))
			{
				using (LiftLexEntryRepository r = new LiftLexEntryRepository(f.GetPathForNewTempFile(true)))
				{
					r.AfterEntryDeleted +=OnEvent;

					LexEntry entry = r.CreateItem();
					r.SaveItem(entry);

					r.DeleteItem(r.GetId(entry));
					Assert.IsTrue(_gotEventCall);
				}
			}
		}

		[Test]
		public void DeleteEntry_ByEntry_TriggersAfterEntryDeleted()
		{
			using (TemporaryFolder f = new TemporaryFolder("eventTests"))
			{
				using (LiftLexEntryRepository r = new LiftLexEntryRepository(f.GetPathForNewTempFile(true)))
				{
					r.AfterEntryDeleted += OnEvent;

					LexEntry entry = r.CreateItem();
					r.SaveItem(entry);

					r.DeleteItem(entry);
					Assert.IsTrue(_gotEventCall);
				}
			}
		}


		void OnEvent(object sender, LiftLexEntryRepository.EntryEventArgs e)
		{
			_gotEventCall = true;
		}
	}
}