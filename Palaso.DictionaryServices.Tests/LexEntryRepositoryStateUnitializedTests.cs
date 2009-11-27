using System.IO;
using NUnit.Framework;
using Palaso.DictionaryServices;
using Palaso.TestUtilities;

namespace Palaso.DictionaryServices.Tests
{
	[TestFixture]
	public class LiftLexEntryRepositoryStateUnitializedTests
	{
		/* NOMORELOCKING
		[Test]
		[ExpectedException(typeof(IOException))]
		public void Constructor_FileIsWriteableAfterRepositoryIsCreated_Throws()
		{
			using (File.OpenWrite(_persistedFilePath))
			{
			}
		}
*/

		[Test]
		[ExpectedException(typeof(IOException))]
		public void Constructor_FileIsNotWriteableWhenRepositoryIsCreated_Throws()
		{
			using (TempFile t = TempFile.CreateAndGetPathButDontMakeTheFile())
			{
				using (File.OpenWrite(t.Path))
				{
					// Note: Will throw => Dispose will not be called.
					using (var dm = new LiftLexEntryRepository(t.Path))
					{
					}
				}
			}
		}
	}
}