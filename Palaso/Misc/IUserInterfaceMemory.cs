using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Palaso.Misc
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
