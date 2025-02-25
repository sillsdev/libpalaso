using System;
using System.Diagnostics;
using System.IO;
using System.Xml;
using NUnit.Framework;
using SIL.Lift.Merging;

namespace SIL.Lift.Tests.Merging
{
	[TestFixture]
	public class LiftChangeDetectorTests
	{
		private string _originalLift = "<lift version='0.12'><entry id='one'/><entry id='two'/></lift>";
		private string _modifiedLift = "<lift version='0.12'><entry id='one'><lexical-unit/></entry></lift>";

		[Test]
		public void CanProvideChangeRecord_AfterClear_False()
		{
			using (TempFile working = new TempFile("<lift version='0.12'/>"))
			{
				using (TempFolder cache = new TempFolder("LiftChangeDetectorTestsCache"))
				{
					LiftChangeDetector detector = new LiftChangeDetector(working.Path, cache.Path);
					detector.ClearCache();
					Assert.IsFalse(detector.CanProvideChangeRecord);
				}
			}
		}

		[Test]
		public void CanProvideChangeRecord_AfterReset_True()
		{
			using (TempFile working = new TempFile("<lift version='0.12'/>"))
			{
				using (TempFolder cache = new TempFolder("LiftChangeDetectorTestsCache"))
				{
					LiftChangeDetector detector = new LiftChangeDetector(working.Path, cache.Path);
					detector.Reset();
					Assert.IsTrue(detector.CanProvideChangeRecord);
				}
			}
		}

		[Test]
		public void GetChangeReport_NoChanges_PerformanceTest()
		{
			using (TempFile working = new TempFile(_originalLift))
			{
				int howManyEntries = 10000;
				Debug.WriteLine("running test using " + howManyEntries.ToString() + " entries");
				using (XmlWriter w = XmlWriter.Create(working.Path))
				{
					w.WriteStartElement("lift");
					for (int i = 0; i < howManyEntries; i++)
					{
						w.WriteStartElement("entry");
						w.WriteAttributeString("id", i.ToString());
						w.WriteElementString("lexical-unit", "<form lang='x'><text>"
															 + Path.GetRandomFileName()
															 //just a way to get some random text
															 + "</text></form>");
						w.WriteElementString("gloss", "<form lang='y'><text>"
													  + Path.GetRandomFileName() //just a way to get some random text
													  + "</text></form>");
						w.WriteEndElement();
					}
					w.WriteEndElement();
				}
				using (TempFolder cache = new TempFolder("LiftChangeDetectorTestsCache"))
				{
					LiftChangeDetector detector = new LiftChangeDetector(working.Path, cache.Path);

					System.Diagnostics.Stopwatch timer = new Stopwatch();
					timer.Start();
					detector.Reset();
					timer.Stop();
					Debug.WriteLine("reset took " + timer.Elapsed.TotalSeconds + " seconds");

					timer.Reset();
					timer.Start();
					ILiftChangeReport report = detector.GetChangeReport(null);
					timer.Stop();
					Debug.WriteLine("getting report took " + timer.Elapsed.TotalSeconds + " seconds");

					timer.Reset();
					timer.Start();
					for (int i = 0; i < howManyEntries; i++)
					{
						report.GetChangeType(i.ToString());
					}
					timer.Stop();
					Debug.WriteLine("Time to inquire about each entry " + timer.Elapsed.TotalSeconds + " seconds");

				}
			}
		}

		[Test]
		public void GetChangeReport_AfterReset_NoChanges()
		{
			using (TempFile working = new TempFile(_originalLift))
			{
				using (TempFolder cache = new TempFolder("LiftChangeDetectorTestsCache"))
				{
					LiftChangeDetector detector = new LiftChangeDetector(working.Path, cache.Path);
					File.WriteAllText(working.Path, _modifiedLift);
					detector.Reset();


					ILiftChangeReport report = detector.GetChangeReport(null);
					Assert.AreEqual(LiftChangeReport.ChangeType.None, report.GetChangeType("one"));
					Assert.AreEqual(LiftChangeReport.ChangeType.None, report.GetChangeType("two"));
					Assert.AreEqual(0, report.IdsOfDeletedEntries.Count);
				}
			}
		}

		[Test]
		public void GetChangeReport_AfterChange_Reasonable()
		{
			using (TempFile working = new TempFile(_originalLift))
			{
				using (TempFolder cache = new TempFolder("LiftChangeDetectorTestsCache"))
				{
					LiftChangeDetector detector = new LiftChangeDetector(working.Path, cache.Path);
					detector.Reset();
					File.WriteAllText(working.Path, _modifiedLift);
					ILiftChangeReport report = detector.GetChangeReport(new NullProgress());
					Assert.AreEqual(LiftChangeReport.ChangeType.Editted, report.GetChangeType("one"));
					Assert.AreEqual(LiftChangeReport.ChangeType.Deleted, report.GetChangeType("two"));
					Assert.AreEqual(1, report.IdsOfDeletedEntries.Count);
				}
			}
		}

		[Test]
		public void GetChangeReport_AfterClear_Throws()
		{
			using (TempFile working = new TempFile("<lift version='0.12'/>"))
			{
				using (TempFolder cache = new TempFolder("LiftChangeDetectorTestsCache"))
				{
					LiftChangeDetector detector = new LiftChangeDetector(working.Path, cache.Path);
					detector.ClearCache();
					Assert.Throws<ApplicationException>(() => detector.GetChangeReport(new NullProgress()));
				}
			}
		}

		[Test]
		public void Constructor_CacheFolderDoesNotExist_DoesNothing()
		{
			string cache = Path.Combine(Path.GetTempPath(), "CreateForTest");
			if (Directory.Exists(cache))
			{
				Directory.Delete(cache, true);
			}
			using (TempFile working = new TempFile("<lift version='0.12'/>"))
			{
				_ = new LiftChangeDetector(working.Path, cache);
				Assert.IsFalse(Directory.Exists(cache));
			}
		}

		[Test]
		public void Reset_WorkingLiftFolderDoesNotExist_Throws()
		{
			using (TempFolder cache = new TempFolder("LiftChangeDetectorTestsCache"))
			{
				LiftChangeDetector detector = new LiftChangeDetector("notgonnafindme", cache.Path);
				Assert.Throws<ApplicationException>(() => detector.Reset());
			}
		}
	}

	internal class NullProgress : IProgress
	{
		public string Status
		{
			get => string.Empty;
			set { }
		}
	}
}