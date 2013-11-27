using System.Windows.Forms;

namespace Palaso.UI.WindowsForms.Miscellaneous
{
	public interface IUserInterfaceMemory
	{
		IUserInterfaceMemory CreateNewSection(string sectionName);
		void Set(string key, string value);
		void Set(string key, int value);
		string Get(string key, string defaultValue);
		int Get(string key, int defaultValue);
		void TrackSplitContainer(SplitContainer container, string key);
	}
}
