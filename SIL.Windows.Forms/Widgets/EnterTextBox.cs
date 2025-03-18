using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace SIL.Windows.Forms.Widgets
{
	/// <summary>
	/// Subclass of a TextBox which captures the Enter key. Additionally, if supplied, other
	/// keys (e.g., Tab) can also be specified to be treated as input keys.
	/// </summary>
	/// <remarks>Normal text boxes cannot process Enter keys, and the default button on the form
	/// (if any) is selected instead.</remarks>
	public class EnterTextBox: TextBox
	{
		public IReadOnlyCollection<Keys> OtherKeysToTreatAsInputKeys { get; set; }

		protected override bool IsInputKey(Keys key)
		{
			return key == Keys.Enter || (OtherKeysToTreatAsInputKeys != null &&
				OtherKeysToTreatAsInputKeys.Contains(key)) || base.IsInputKey(key);
		}
	}
}
