using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace SIL.Archiving.Tests
{
	[TestFixture]
	public class JSONUtilsTests
	{
		/// ------------------------------------------------------------------------------------
		[Test]
		public void MakeKeyValuePair_BothAreEmtpy_ReturnsNull()
		{
			Assert.IsNull(JSONUtils.MakeKeyValuePair("", ""));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void MakeKeyValuePair_KeyIsNull_ReturnsNull()
		{
			Assert.IsNull(JSONUtils.MakeKeyValuePair(null, "blah"));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void MakeKeyValuePair_ValueIsNull_ReturnsNull()
		{
			Assert.IsNull(JSONUtils.MakeKeyValuePair("blah", null));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void MakeKeyValuePair_KeyAndValueAreGood_ReturnsCorrectString()
		{
			Assert.AreEqual("\"key\":\"value\"", JSONUtils.MakeKeyValuePair("key", "value"));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void MakeKeyValuePair_BracketValue_ReturnsBracketedValue()
		{
			Assert.AreEqual("\"key\":[\"value\"]", JSONUtils.MakeKeyValuePair("key", "value", true));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void MakeArrayFromValues_GoodValues_MakesArray()
		{
			var list = new List<string>();
			list.Add(JSONUtils.MakeKeyValuePair("corvette", "car"));
			list.Add(JSONUtils.MakeKeyValuePair("banana", "fruit"));
			list.Add(JSONUtils.MakeKeyValuePair("spot", "dog"));

			Assert.AreEqual("\"stuff\":{\"0\":{\"corvette\":\"car\"},\"1\":{\"banana\":\"fruit\"},\"2\":{\"spot\":\"dog\"}}",
				JSONUtils.MakeArrayFromValues("stuff", list));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void MakeBracketedListFromValues_NullKey_ReturnsNull()
		{
			Assert.IsNull(JSONUtils.MakeBracketedListFromValues(null, new[] { "blah" }));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void MakeBracketedListFromValues_EmptyKey_ReturnsNull()
		{
			Assert.IsNull(JSONUtils.MakeBracketedListFromValues(string.Empty, new[] { "blah" }));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void MakeBracketedListFromValues_NullList_ReturnsNull()
		{
			Assert.IsNull(JSONUtils.MakeBracketedListFromValues("blah", null));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void MakeBracketedListFromValues_EmptyList_ReturnsNull()
		{
			Assert.IsNull(JSONUtils.MakeBracketedListFromValues("blah", new string[] { }));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void MakeBracketedListFromValues_GoodKeyAndList_ReturnsBracketedList()
		{
			Assert.AreEqual("\"key\":[\"red\",\"green\",\"blue\"]",
				JSONUtils.MakeBracketedListFromValues("key", new[] { "red", "green", "blue" }));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void EncodeData_RoundtripOfArbitraryString_DecodedDataMatchesOriginal()
		{
			string original = "drs;gkljsdr;lgsd";
			string decoded = JSONUtils.DecodeData(JSONUtils.EncodeData(original));
			Assert.AreEqual(original, decoded);
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void DecodeData_JsonData_ReturnsDecodedString()
		{
			const string encoded =
			//	"eyJpZCI6Ijk2Z282MXEzYmgiLCJyYW1wLmlzX3JlYWR5IjoiWSIsImRjLnRpdGxlIjoiU3RhZ2VzIiwiYnJvYWRfdHlwZSI6InZlcm5hY3VsYXIiLCJkYy50eXBlLm1vZGUiOlsiVGV4dCJdLCJyYW1wLnZlcm5hY3VsYXJtYXRlcmlhbHN0eXBlIjoiZ2VuZXJhbFZlcm5hY3VsYXIiLCJkYy5zdWJqZWN0LnZlcm5hY3VsYXJDb250ZW50IjpbIlNvbmdzIiwiQ2FsZW5kYXIiLCJDb21tdW5pdHkgbmV3cyIsIkNpdmljIGVkdWNhdGlvbiIsIkN1bHR1cmUgYW5kIGZvbGtsb3JlIiwiUHJvdmVyYnMgYW5kIG1heGltcyIsIkFJRFMgYW5kIEhJViIsIkF2aWFuIGZsdSIsIkhlYWx0aCBhbmQgaHlnaWVuZSIsIk1hbGFyaWEiLCJQcmVuYXRhbCBhbmQgaW5mYW50IGNhcmUiLCJBbHBoYWJldCIsIlByZXJlYWRpbmciLCJQcmV3cml0aW5nIiwiUHJpbWVyIiwiUmVhZGVyIiwiUmlkZGxlcyIsIlNwZWxsaW5nIiwiVG9uZSBwcmltZXIiLCJUcmFuc2l0aW9uIHByaW1lciIsIlZvY2FidWxhcnkiLCJXcml0aW5nIiwiQXJpdGhtZXRpYyIsIkFydHMiLCJFdGhpY3MiLCJOdW1iZXJzIiwiU2NpZW5jZSIsIlNvY2lhbCBzdHVkaWVzIiwiV29ya2Jvb2siLCJXYWxsIGNoYXJ0IiwiVGV4dGJvb2siLCJUZWFjaGVyJ3MgZ3VpZGUiLCJBY3Rpdml0eSBib29rIiwiV29yc2hpcCBhbmQgbGl0dXJneSIsIlRoZW9sb2d5IiwiRXZhbmdlbGlzdGljIiwiQ2hyaXN0aWFuIGxpdmluZyIsIkRldm90aW9uYWwiLCJDYXRlY2hpc20iLCJQaHJhc2UgYm9vayIsIkxhbmd1YWdlIGluc3RydWN0aW9uIiwiR3JhbW1hciBpbnN0cnVjdGlvbiIsIkVuZ2xpc2ggbGFuZ3VhZ2UgaW5zdHJ1Y3Rpb24iLCJBZ3JpY3VsdHVyZSBhbmQgZm9vZCBwcm9kdWN0aW9uIiwiQnVzaW5lc3MgYW5kIGluY29tZSBnZW5lcmF0aW9uIiwiRW52aXJvbm1lbnRhbCBjYXJlIiwiSW5zdHJ1Y3Rpb25hbCBtYW51YWwiLCJWb2NhdGlvbmFsIGVkdWNhdGlvbiJdLCJzaWwuc2Vuc2l0aXZpdHkubWV0YWRhdGEiOiJFbnRpdHkgY3VyYXRvcnMiLCJmaWxlcyI6eyIwIjp7IiAiOiJTaG9lX1JlcGFpci50eHQiLCJkZXNjcmlwdGlvbiI6Ikp1bmsiLCJyZWxhdGlvbnNoaXAiOiJQcmVzZW50YXRpb24iLCJpc19wcmltYXJ5IjoiWSJ9fSwiZGMuZGVzY3JpcHRpb24uc3RhZ2UiOiJmaW5pc2hlZCIsInN0YXR1cyI6ImV4cG9ydGVkIiwiZGMudHlwZS5zY2hvbGFybHlXb3JrIjoiUG9zaXRpb24gcGFwZXIifQ==";
				"eyJpZCI6InUzZDJ1aG5mcnUiLCJjcmVhdGVkX2F0IjoiVHVlLCAxMyBBdWcgMjAxMyAxNDozMDo0NSBHTVQiLCJyYW1wLmlzX3JlYWR5IjoiWSIsImRjLnRpdGxlIjoiRmllbGRXb3JrcyBUZXN0IExhbmd1YWdlIFByb2plY3QiLCJicm9hZF90eXBlIjoid2lkZXJfYXVkaWVuY2UiLCJkYy50eXBlLnNjaG9sYXJseVdvcmsiOiJEYXRhIHNldCIsImRjLnN1YmplY3Quc2lsRG9tYWluIjpbIkxJTkc6TGluZ3Vpc3RpY3MiXSwidHlwZS5kb21haW5TdWJ0eXBlLkxJTkciOlsibGV4aWNvbiAoTElORykiXSwiZGMuZGF0ZS5jcmVhdGVkIjoiOC8xMy8yMDEzIiwiZmlsZXMiOnsiMCI6eyIgIjoiVGVzdExhbmdQcm9qKzIwMTMtMDgtMTMrMDkxNy5md2JhY2t1cCIsImRlc2NyaXB0aW9uIjoiRmllbGRXb3JrcyBiYWNrdXAiLCJyZWxhdGlvbnNoaXAiOiJzb3VyY2UifX0sImRjLnR5cGUubW9kZSI6WyJNdXNpY2FsIG5vdGF0aW9uIiwiUHJlc2VudGF0aW9uIiwiU29mdHdhcmUgYXBwbGljYXRpb24iLCJEYXRhc2V0Il0sImRjLmRlc2NyaXB0aW9uLnN0YWdlIjoicm91Z2hfZHJhZnQiLCJzaWwuc2Vuc2l0aXZpdHkubWV0YWRhdGEiOiJFbnRpdHkgY3VyYXRvcnMiLCJzdGF0dXMiOiJyZWFkeSJ9";
			string decoded = JSONUtils.DecodeData(encoded);
			Console.Out.WriteLine(decoded);
			Assert.AreEqual("{\"id\":\"u3d2uhnfru\",\"created_at\":\"Tue, 13 Aug 2013 14:30:45 GMT\",\"ramp.is_ready\":\"Y\",\"dc.title\":\"FieldWorks Test Language Project\",\"broad_type\":\"wider_audience\",\"dc.type.scholarlyWork\":\"Data set\",\"dc.subject.silDomain\":[\"LING:Linguistics\"],\"type.domainSubtype.LING\":[\"lexicon (LING)\"],\"dc.date.created\":\"8/13/2013\",\"files\":{\"0\":{\" \":\"TestLangProj+2013-08-13+0917.fwbackup\",\"description\":\"FieldWorks backup\",\"relationship\":\"source\"}},\"dc.type.mode\":[\"Musical notation\",\"Presentation\",\"Software application\",\"Dataset\"],\"dc.description.stage\":\"rough_draft\",\"sil.sensitivity.metadata\":\"Entity curators\",\"status\":\"ready\"}",
				decoded);
		}
	}
}
