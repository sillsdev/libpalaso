using System;
using NUnit.Framework;
using SIL.WritingSystems;

namespace SIL.TestUtilities
{
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Assembly, AllowMultiple = true)]
	public class OfflineSldrAttribute : Attribute, ITestAction
	{
		private TemporaryFolder _sldrCacheFolder;

		public void BeforeTest(TestDetails testDetails)
		{
			_sldrCacheFolder = new TemporaryFolder("SldrCacheTest");
			Sldr.SldrCachePath = _sldrCacheFolder.Path;
			Sldr.OfflineMode = true;
			Sldr.ResetLanguageTags();
		}

		public void AfterTest(TestDetails testDetails)
		{
			Sldr.OfflineMode = false;
			Sldr.ResetLanguageTags();
			_sldrCacheFolder.Dispose();
			_sldrCacheFolder = null;
			Sldr.SldrCachePath = Sldr.DefaultSldrCachePath;
		}

		public ActionTargets Targets
		{
			get { return ActionTargets.Test; }
		}
	}
}
