using System;
using System.Drawing;
using SIL.Reporting;
using SIL.WritingSystems;

namespace SIL.WindowsForms.Widgets
{
	public static class WidgetExtensions
	{
		public static Font CreateDefaultFont(this WritingSystemDefinition writingSystem)
		{
			float size = writingSystem.DefaultFontSize > 0 ? writingSystem.DefaultFontSize : 12;
			try
			{
				return new Font(writingSystem.DefaultFont.Name, size);
			}
			catch (Exception e) //I could never get the above to fail, but a user did (WS-34091), so now we catch it here.
			{
				ErrorReport.NotifyUserOfProblem(new ShowOncePerSessionBasedOnExactMessagePolicy(),
					e,
					"There is something wrong with the font {0} on this computer. Try re-installing it. Meanwhile, the font {1} will be used instead.",
					writingSystem.DefaultFont.Name, SystemFonts.DefaultFont.Name);
				return new Font(SystemFonts.DefaultFont.Name, size);
			}
		}
	}
}
