using System.Windows.Forms.Design;
using System.Windows.Forms;

namespace SIL.Windows.Forms.Scripture
{
	[ToolStripItemDesignerAvailability
	  (ToolStripItemDesignerAvailability.ToolStrip
		| ToolStripItemDesignerAvailability.StatusStrip
		| ToolStripItemDesignerAvailability.MenuStrip)]
	public class ToolStripVerseControl : ToolStripControlHost
	{
		public ToolStripVerseControl() : base(new VerseControl())
		{
		}

		public VerseControl VerseControl => Control as VerseControl;
	}
}
