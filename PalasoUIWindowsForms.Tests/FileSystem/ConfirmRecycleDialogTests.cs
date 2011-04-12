using NUnit.Framework;
using Palaso.UI.WindowsForms.FileSystem;

namespace PalasoUIWindowsForms.Tests.FileSystem
{
	/// ----------------------------------------------------------------------------------------
	[TestFixture]
	public class ConfirmRecycleDialogTests
	{
		/// ------------------------------------------------------------------------------------
		[Test]
		public void Recycle_FileDoesNotExist_ReturnsFalse()
		{
			Assert.IsFalse(ConfirmRecycleDialog.Recycle("blahblah.blah"));
		}
	}
}
