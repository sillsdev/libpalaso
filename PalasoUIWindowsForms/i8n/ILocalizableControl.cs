using System;
using System.Collections.Generic;
using System.Text;

namespace Palaso.UI.WindowsForms.i8n
{
	/// <summary>
	/// A control can implement this to override some default localization behavior
	/// </summary>
	public interface ILocalizableControl
	{
		bool ShouldModifyFont { get;}
	}
}
