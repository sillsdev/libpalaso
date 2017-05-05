using System;
using System.IO;
using NUnit.Framework;
using SIL.Lift.Merging;
using SIL.Lift.Validation;

namespace SIL.Lift.Tests.Merging
{
	[TestFixture]
	public class LiftChangeReportTests
	{
		[Test]
		public void OriginalHasNoEntries()
		{
			string orig = @"";
			string mod = @"<entry id='three'>
							</entry>
							<entry id='one' guid='0ae89610-fc01-4bfd-a0d6-1125b7281dd1' greeting='hello'>
							</entry>";
			LiftChangeReport report = GetChanges(orig, mod);
			Assert.AreEqual(LiftChangeReport.ChangeType.New, report.GetChangeType("one"));
			Assert.AreEqual(LiftChangeReport.ChangeType.New, report.GetChangeType("three"));
		}

		[Test]
		public void AllEntriesDeleted()
		{
			string orig = @"<entry id='one' guid='0ae89610-fc01-4bfd-a0d6-1125b7281dd1' greeting='hi'>
							</entry>
							<entry id='two' guid='0ae89610-fc01-4bfd-a0d6-1125b7281d22'></entry>";
			string mod = @"";
			LiftChangeReport report = GetChanges(orig, mod);
			Assert.IsTrue(report.IdsOfDeletedEntries.Contains("one"));
			Assert.IsTrue(report.IdsOfDeletedEntries.Contains("two"));
		}

		[Test]
		public void ModifiedEntryDetected()
		{
			string orig = @"<entry id='one' guid='0ae89610-fc01-4bfd-a0d6-1125b7281dd1' greeting='hi'>
							</entry>
							<entry id='two' guid='0ae89610-fc01-4bfd-a0d6-1125b7281d22'></entry>";
			string mod =  @"<entry id='three'>
							</entry>
							<entry id='one' guid='0ae89610-fc01-4bfd-a0d6-1125b7281dd1' greeting='hello'>
							</entry>";
			LiftChangeReport report = GetChanges(orig, mod);
			Assert.AreEqual(LiftChangeReport.ChangeType.Editted, report.GetChangeType("one"));
		}

		public void DeletedEntryDetected()
		{
			string orig = @"<entry id='one' guid='0ae89610-fc01-4bfd-a0d6-1125b7281dd1' greeting='hi'>
							</entry>
							<entry id='two' guid='0ae89610-fc01-4bfd-a0d6-1125b7281d22'></entry>";
			string mod = @"<entry id='three'>
							</entry>
							<entry id='one' guid='0ae89610-fc01-4bfd-a0d6-1125b7281dd1' greeting='hello'>
							</entry>";
			LiftChangeReport report = GetChanges(orig, mod);
			Assert.IsTrue(report.IdsOfDeletedEntries.Contains("two"));
		}

		[Test]
		public void AddedEntryDetected()
		{
			string orig = @"<entry id='one' guid='0ae89610-fc01-4bfd-a0d6-1125b7281dd1' greeting='hi'>
							</entry>
							<entry id='two' guid='0ae89610-fc01-4bfd-a0d6-1125b7281d22'></entry>";
			string mod = @"<entry id='three'>
							</entry>
							<entry id='one' guid='0ae89610-fc01-4bfd-a0d6-1125b7281dd1' greeting='hello'>
							</entry>";
			LiftChangeReport report = GetChanges(orig, mod);
			Assert.AreEqual(LiftChangeReport.ChangeType.New, report.GetChangeType("three"));
		}

		private LiftChangeReport GetChanges(string origEntriesContent, string modifiedEntriesContent)
		{
			string origDocContent = String.Format("<?xml version=\"1.0\" encoding=\"utf-8\"?><lift version ='{0}' producer='test'>{1}</lift>", Validator.LiftVersion, origEntriesContent);
			string modifiedDocContent = String.Format("<?xml version=\"1.0\" encoding=\"utf-8\"?><lift version ='{0}' producer='test'>{1}</lift>", Validator.LiftVersion, modifiedEntriesContent);

			StringReader o = new StringReader(origDocContent);
			StringReader m = new StringReader(modifiedDocContent);

/*experiment with rhino mocks
 * LiftChangeReport report=null;
 * With.Mocks(delegate
						   {
							   IProgress p = (IProgress) Mocker.Current.DynamicMock(typeof (IProgress));
							   Mocker.Current.ReplayAll();
							   report= LiftChangeReport.DetermineChanges(o, m, p);
						   });
			return report;
 */

	/* same with moq
	 var p = new Moq.Mock<IProgress>();
			p.ExpectSet(t => t.Status).Equals("working...");
		   return LiftChangeReport.DetermineChanges(o, m, p.Object);
	 */
		   return LiftChangeReport.DetermineChanges(o, m, null);
		}
	}
}