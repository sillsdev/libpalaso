using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Palaso.WritingSystems.Migration.WritingSystemsLdmlV0To1Migration;

namespace Palaso.Tests.WritingSystems.Migration
{
	[TestFixture]
	public class LdmlAdaptorV0Tests
	{
		[Test]
		//WS-33992
		public void Read_LdmlContainsEmptyCollationElement_SortUsingIsSetToSameAsIfNoCollationElementExisted()
		{
			string ldmlWithEmptyCollationElement =
				"<ldml><!--Comment--><identity><version number=\"\" /><generation date=\"0001-01-01T00:00:00\" /><language type=\"qaa\" /></identity><dates /><collations><collation></collation></collations><special xmlns:palaso=\"urn://palaso.org/ldmlExtensions/v1\" /><special></special></ldml>";
			string ldmlwithNoCollationElement =
				"<ldml><!--Comment--><identity><version number=\"\" /><generation date=\"0001-01-01T00:00:00\" /><language type=\"qaa\" /></identity><dates /><collations/><special xmlns:palaso=\"urn://palaso.org/ldmlExtensions/v1\" /><special></special></ldml>";

			string pathToLdmlWithEmptyCollationElement = Path.GetTempFileName();
			try
			{
				File.WriteAllText(pathToLdmlWithEmptyCollationElement, ldmlWithEmptyCollationElement);
				string pathToLdmlWithNoCollationElement = Path.GetTempFileName();
				try
				{
					File.WriteAllText(pathToLdmlWithNoCollationElement, ldmlwithNoCollationElement);


					var adaptor = new LdmlAdaptorV0();
					var wsFromEmptyCollationElement = new WritingSystemDefinitionV0();
					adaptor.Read(pathToLdmlWithEmptyCollationElement, wsFromEmptyCollationElement);
					var wsFromNoCollationElement = new WritingSystemDefinitionV0();
					adaptor.Read(pathToLdmlWithNoCollationElement, wsFromNoCollationElement);

					Assert.AreEqual(wsFromNoCollationElement.SortUsing, wsFromEmptyCollationElement.SortUsing);
				}
				finally
				{
					File.Delete(pathToLdmlWithNoCollationElement);
				}
			}
			finally
			{
				File.Delete(pathToLdmlWithEmptyCollationElement);
			}
		}
	}
}
