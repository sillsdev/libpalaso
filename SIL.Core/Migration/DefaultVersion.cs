namespace SIL.Migration
{
	public class DefaultVersion : IFileVersion
	{
		private readonly int _defaultVersion;

		public DefaultVersion(int goodToVersion, int defaultVersion)
		{
			StrategyGoodToVersion = goodToVersion;
			_defaultVersion = defaultVersion;
		}

		public int StrategyGoodToVersion { get; private set; }

		public int GetFileVersion(string filePath)
		{
			return _defaultVersion;
		}

	}
}
