namespace SIL.Migration
{
	public interface IFileVersion
	{
		int GetFileVersion(string filePath);
		int StrategyGoodToVersion { get; }
	}
}
