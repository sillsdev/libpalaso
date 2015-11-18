using System;
using NUnit.Framework;
using SIL.WritingSystems;

namespace SIL.TestUtilities
{
	/// <summary>
	/// A NUnit action attribute that enables offline-mode for the SLDR during unit tests. This attribute can be
	/// placed on a test fixture, test assembly, or test to disable online access to the live SLDR server.
	/// </summary>
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

		/// <summary>
		/// Provides the target for the action attribute. This action will be run on all tests.
		/// </summary>
		public ActionTargets Targets
		{
			get { return ActionTargets.Test; }
		}
	}
}
