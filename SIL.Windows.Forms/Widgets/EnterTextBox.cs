using System.Windows.Forms;

namespace SIL.Windows.Forms.Widgets
{
	/// <summary>
	/// Subclass of a TextBox which captures the enter key.
	/// Normal text boxes cannot process enter keys, and the 
	/// default button on the form is selected instead.
	/// </summary>
	public class EnterTextBox : TextBox
	{
		protected override bool IsInputKey(Keys key)
		{
			if (key == Keys.Enter)
				return true;
			return base.IsInputKey(key);
		}
	}
}
