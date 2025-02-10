using System.IO;
using NUnit.Framework;
using SIL.IO;

namespace SIL.DictionaryServices.Tests
{
	[TestFixture]
	public class LiftLexEntryRepositoryStateUninitializedTests
	{

		[Test]
		public void Constructor_FileIsNotWriteableWhenRepositoryIsCreated_Throws()
		{
			using (TempFile t = TempFile.CreateAndGetPathButDontMakeTheFile())
			{
				using (File.OpenWrite(t.Path))
				{
					Assert.Throws<IOException>(() =>
											   new LiftLexEntryRepository(t.Path));
				}
			}
		}
	}
}