using SIL.DblBundle.Text;

namespace SIL.DblBundle
{
	public interface IProjectInfo
	{
		string Name { get; }
		string Id { get; }
		DblMetadataLanguage Language { get; }
	}
}
