using System.IO;
using NUnit.Framework;
using Palaso.DictionaryServices;
using Palaso.TestUtilities;

namespace Palaso.DictionaryServices.Tests
{
	[TestFixture]
	public class LiftLexEntryRepositoryStateUnitializedTests
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