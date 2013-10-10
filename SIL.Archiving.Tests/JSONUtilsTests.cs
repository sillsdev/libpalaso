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
				"eyJpZCI6InBmcXJxa2F1MW4iLCJyYW1wLmlzX3JlYWR5IjoiWSIsImRjLnRpdGxlIjoiTmV3IFNlc3Npb24gMDIiLCJicm9hZF90eXBlIjoid2lkZXJfYXVkaWVuY2UiLCJkYy50eXBlLnNjaG9sYXJseVdvcmsiOiJEYXRhIHNldCIsInR5cGUuZG9tYWluU3VidHlwZS5MSU5HIjpbImxhbmd1YWdlIGRvY3VtZW50YXRpb24gKExJTkcpIl0sImRjLmRlc2NyaXB0aW9uLmFic3RyYWN0Ijp7IjAiOnsiICI6IlRoaXMgc2hvdWxkIGFwcGVhciBhcyB0aGUgUkFNUCBwYWNrYWdlJ3MgRGVzY3JpcHRpb24uIiwibGFuZyI6IiJ9fSwiZmlsZXMiOnsiMCI6eyIgIjoiQW50aStkYXRlLm1vdiIsImRlc2NyaXB0aW9uIjoiU2F5TW9yZSBTZXNzaW9uIEZpbGUiLCJyZWxhdGlvbnNoaXAiOiJzb3VyY2UifSwiMSI6eyIgIjoiQW50aStkYXRlI21vdi5tZXRhIiwiZGVzY3JpcHRpb24iOiJTYXlNb3JlIEZpbGUgTWV0YWRhdGEgKFhNTCkiLCJyZWxhdGlvbnNoaXAiOiJzb3VyY2UifSwiMiI6eyIgIjoiQW50aStkYXRlX1N0YW5kYXJkQXVkaW8ud2F2IiwiZGVzY3JpcHRpb24iOiJTYXlNb3JlIFNlc3Npb24gRmlsZSIsInJlbGF0aW9uc2hpcCI6InNvdXJjZSJ9LCIzIjp7IiAiOiJBbnRpK2RhdGVfU3RhbmRhcmRBdWRpbyN3YXYjYW5ub3RhdGlvbnMuZWFmIiwiZGVzY3JpcHRpb24iOiJTYXlNb3JlIFNlc3Npb24gRmlsZSIsInJlbGF0aW9uc2hpcCI6InNvdXJjZSJ9LCI0Ijp7IiAiOiJBbnRpK2RhdGVfU3RhbmRhcmRBdWRpbyN3YXYubWV0YSIsImRlc2NyaXB0aW9uIjoiU2F5TW9yZSBGaWxlIE1ldGFkYXRhIChYTUwpIiwicmVsYXRpb25zaGlwIjoic291cmNlIn0sIjUiOnsiICI6IkFudGkrZGF0ZV9TdGFuZGFyZEF1ZGlvI3dhdiNvcmFsQW5ub3RhdGlvbnMud2F2IiwiZGVzY3JpcHRpb24iOiJTYXlNb3JlIFNlc3Npb24gRmlsZSIsInJlbGF0aW9uc2hpcCI6InNvdXJjZSJ9LCI2Ijp7IiAiOiJOZXcrU2Vzc2lvbiswMi5zZXNzaW9uIiwiZGVzY3JpcHRpb24iOiJTYXlNb3JlIFNlc3Npb24gTWV0YWRhdGEgKFhNTCkiLCJyZWxhdGlvbnNoaXAiOiJzb3VyY2UifSwiNyI6eyIgIjoiX19Db250cmlidXRvcnNfX0RvZG8ucGVyc29uIiwiZGVzY3JpcHRpb24iOiJTYXlNb3JlIENvbnRyaWJ1dG9yIE1ldGFkYXRhIChYTUwpIiwicmVsYXRpb25zaGlwIjoic291cmNlIn0sIjgiOnsiICI6Il9fQ29udHJpYnV0b3JzX19Eb2RvX0NvbnNlbnQud2F2IiwiZGVzY3JpcHRpb24iOiJTYXlNb3JlIENvbnRyaWJ1dG9yIEZpbGUiLCJyZWxhdGlvbnNoaXAiOiJzb3VyY2UifSwiOSI6eyIgIjoiX19Db250cmlidXRvcnNfX0RvZG9fQ29uc2VudCN3YXYjYW5ub3RhdGlvbnMuZWFmIiwiZGVzY3JpcHRpb24iOiJTYXlNb3JlIENvbnRyaWJ1dG9yIEZpbGUiLCJyZWxhdGlvbnNoaXAiOiJzb3VyY2UifSwiMTAiOnsiICI6Il9fQ29udHJpYnV0b3JzX19Eb2RvX0NvbnNlbnQjd2F2Lm1ldGEiLCJkZXNjcmlwdGlvbiI6IlNheU1vcmUgRmlsZSBNZXRhZGF0YSAoWE1MKSIsInJlbGF0aW9uc2hpcCI6InNvdXJjZSJ9LCIxMSI6eyIgIjoiX19Db250cmlidXRvcnNfX0RvZG9fUGhvdG8uSlBHIiwiZGVzY3JpcHRpb24iOiJTYXlNb3JlIENvbnRyaWJ1dG9yIEZpbGUiLCJyZWxhdGlvbnNoaXAiOiJzb3VyY2UifSwiMTIiOnsiICI6Il9fQ29udHJpYnV0b3JzX19Eb2RvX1Bob3RvI0pQRy5tZXRhIiwiZGVzY3JpcHRpb24iOiJTYXlNb3JlIEZpbGUgTWV0YWRhdGEgKFhNTCkiLCJyZWxhdGlvbnNoaXAiOiJzb3VyY2UifSwiMTMiOnsiICI6Il9fQ29udHJpYnV0b3JzX19OdWV2YStwZXJzb25hKzAxLnBlcnNvbiIsImRlc2NyaXB0aW9uIjoiU2F5TW9yZSBDb250cmlidXRvciBNZXRhZGF0YSAoWE1MKSIsInJlbGF0aW9uc2hpcCI6InNvdXJjZSJ9LCIxNCI6eyIgIjoiX19Db250cmlidXRvcnNfX1RpbStTdGVlbnd5ay5wZXJzb24iLCJkZXNjcmlwdGlvbiI6IlNheU1vcmUgQ29udHJpYnV0b3IgTWV0YWRhdGEgKFhNTCkiLCJyZWxhdGlvbnNoaXAiOiJzb3VyY2UifX0sImRjLnN1YmplY3Quc2lsRG9tYWluIjpbIkxJTkc6TGluZ3Vpc3RpY3MiLCJBTlRIOkFudGhyb3BvbG9neSJdLCJ0eXBlLmRvbWFpblN1YnR5cGUuQU5USCI6W10sInJlbGF0aW9uLnJlcXVpcmVzLmhhcyI6IlkiLCJkYy5sYW5ndWFnZS5pc28iOnsiMCI6eyJkaWFsZWN0IjoiIiwiICI6InNlaDpTZW5hIn0sIjEiOnsiZGlhbGVjdCI6IiIsIiAiOiJoaW46SGluZGkifSwiMiI6eyJkaWFsZWN0IjoiIn19LCJkYy50eXBlLm1vZGUiOlsiVmlkZW8iLCJTcGVlY2giLCJQaG90b2dyYXBoIiwiVGV4dCJdLCJkZXNjcmlwdGlvbi5oYXMiOiJZIiwiZGMuZGVzY3JpcHRpb24iOnsiMCI6eyIgIjoiVGhpcyBpcyBhIGdlbmVyYWwgZGVzY3JpcHRpb24sIHlvdSBnb29mYmFsLiJ9fSwiZGMuZGVzY3JpcHRpb24uc3RhZ2UiOiJyb3VnaF9kcmFmdCIsInNpbC5zZW5zaXRpdml0eS5tZXRhZGF0YSI6IlB1YmxpYyIsInN0YXR1cyI6InJlYWR5IiwiZGMuY29udHJpYnV0b3IiOnsiMCI6eyIgIjoiVG9tIEJvZ2xlIiwicm9sZSI6ImF1dGhvciJ9fX0=";
			string decoded = JSONUtils.DecodeData(encoded);
			Console.Out.WriteLine(decoded);
			Assert.AreEqual("{\"id\":\"u3d2uhnfru\",\"created_at\":\"Tue, 13 Aug 2013 14:30:45 GMT\",\"ramp.is_ready\":\"Y\",\"dc.title\":\"FieldWorks Test Language Project\",\"broad_type\":\"wider_audience\",\"dc.type.scholarlyWork\":\"Data set\",\"dc.subject.silDomain\":[\"LING:Linguistics\"],\"type.domainSubtype.LING\":[\"lexicon (LING)\"],\"dc.date.created\":\"8/13/2013\",\"files\":{\"0\":{\" \":\"TestLangProj+2013-08-13+0917.fwbackup\",\"description\":\"FieldWorks backup\",\"relationship\":\"source\"}},\"dc.type.mode\":[\"Musical notation\",\"Presentation\",\"Software application\",\"Dataset\"],\"dc.description.stage\":\"rough_draft\",\"sil.sensitivity.metadata\":\"Entity curators\",\"status\":\"ready\"}",
				decoded);
		}
	}
}
