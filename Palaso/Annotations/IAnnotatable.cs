using System.Xml.Serialization;

namespace Palaso.Annotations
{
	public interface IAnnotatable
	{
		bool IsStarred { get; set; }
	}
}