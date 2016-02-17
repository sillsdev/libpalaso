using SIL.ObjectModel;
using SIL.WritingSystems;

namespace SIL.TestUtilities
{
	/// <summary>
	/// This is a disposable class that enables offline mode for the SLDR. It is used for unit tests, specifically
	/// in fixture or test setup methods. The OfflineSldrAttribute is the easier method for enabling offline mode,
	/// but it is run after the fixture and test setup methods. This class can be used in fixture and test setup
	/// methods so that offline mode can be enabled for the SLDR before other code is executed in these methods.
	/// </summary>
	public class OfflineSldr : DisposableBase
	{
		private readonly TemporaryFolder _sldrCacheFolder;

		public OfflineSldr()
		{
			_sldrCacheFolder = new TemporaryFolder("SldrCacheTest");
			Sldr.Initialize(true, _sldrCacheFolder.Path);
		}

		protected override void DisposeManagedResources()
		{
			Sldr.Cleanup();
			_sldrCacheFolder.Dispose();
		}
	}
}
