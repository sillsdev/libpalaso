using SIL.DblBundle.Text;

namespace SIL.DblBundle
{
	/// <summary>
	/// Project information for a Digital Bible Library text bundle.
	/// </summary>
	public interface IProjectInfo
	{
		/// <summary>Name of the project</summary>
		string Name { get; }
		/// <summary>Project Id</summary>
		string Id { get; }
		/// <summary>Language used in the project</summary>
		DblMetadataLanguage Language { get; }
	}
}
